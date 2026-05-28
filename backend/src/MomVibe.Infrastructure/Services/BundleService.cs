namespace MomVibe.Infrastructure.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Stripe;
using Stripe.Checkout;

using Domain.Entities;
using Domain.Enums;
using Application.Interfaces;
using Application.DTOs.Bundles;
using Application.DTOs.Payments;
using Application.DTOs.Shipping;

using PaymentEntity = Domain.Entities.Payment;

/// <summary>
/// Implements seller bundle management and all bundle payment flows.
/// Mirrors the patterns established in <see cref="PaymentService"/> for consistency.
/// </summary>
public class BundleService : IBundleService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly IShippingService _shippingService;
    private readonly ILogger<BundleService> _logger;

    public BundleService(
        IApplicationDbContext context,
        IMapper mapper,
        IConfiguration configuration,
        IShippingService shippingService,
        ILogger<BundleService> logger)
    {
        this._context = context;
        this._mapper = mapper;
        this._configuration = configuration;
        this._shippingService = shippingService;
        this._logger = logger;
        StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
    }

    // ------------------------------------------------------------------ //
    //  CRUD
    // ------------------------------------------------------------------ //

    /// <inheritdoc/>
    public async Task<BundleDto> CreateAsync(string sellerId, CreateBundleDto dto)
    {
        if (dto.ItemIds.Count < 2 || dto.ItemIds.Count > 10)
            throw new InvalidOperationException("A bundle must contain between 2 and 10 items.");

        // All items must belong to the seller, be active, and not already part of another active bundle
        var items = await this._context.Items
            .Where(i => dto.ItemIds.Contains(i.Id))
            .ToListAsync();

        if (items.Count != dto.ItemIds.Count)
            throw new KeyNotFoundException("One or more items were not found.");

        var invalidItems = items.Where(i => i.UserId != sellerId || !i.IsActive).ToList();
        if (invalidItems.Count > 0)
            throw new InvalidOperationException("All items must be your own active listings.");

        // Check none of the items are already in an active bundle
        var existingBundleItemIds = await this._context.BundleItems
            .Where(bi => dto.ItemIds.Contains(bi.ItemId))
            .Join(this._context.Bundles,
                bi => bi.BundleId,
                b => b.Id,
                (bi, b) => new { bi.ItemId, b.IsActive, b.IsSold })
            .Where(x => x.IsActive && !x.IsSold)
            .Select(x => x.ItemId)
            .ToListAsync();

        if (existingBundleItemIds.Count > 0)
            throw new InvalidOperationException("One or more items are already part of an active bundle.");

        var bundle = new Bundle
        {
            Title = dto.Title,
            Description = dto.Description,
            Price = dto.Price,
            SellerId = sellerId,
            IsActive = true,
            IsSold = false,
            BundleItems = dto.ItemIds.Select(id => new BundleItem { ItemId = id }).ToList()
        };

        this._context.Bundles.Add(bundle);
        await this._context.SaveChangesAsync();

        return await GetByIdAsync(bundle.Id);
    }

    /// <inheritdoc/>
    public async Task<BundleDto> GetByIdAsync(Guid id)
    {
        var bundle = await this._context.Bundles
            .AsNoTracking()
            .Include(b => b.Seller)
            .Include(b => b.BundleItems)
                .ThenInclude(bi => bi.Item)
                    .ThenInclude(i => i.Photos)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bundle == null)
            throw new KeyNotFoundException("Bundle not found.");

        return MapToDto(bundle);
    }

    /// <inheritdoc/>
    public async Task<List<BundleDto>> GetMyAsync(string sellerId)
    {
        var bundles = await this._context.Bundles
            .AsNoTracking()
            .Include(b => b.Seller)
            .Include(b => b.BundleItems)
                .ThenInclude(bi => bi.Item)
                    .ThenInclude(i => i.Photos)
            .Where(b => b.SellerId == sellerId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return bundles.Select(MapToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id, string sellerId)
    {
        var bundle = await this._context.Bundles
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bundle == null)
            throw new KeyNotFoundException("Bundle not found.");
        if (bundle.SellerId != sellerId)
            throw new UnauthorizedAccessException("You are not the seller of this bundle.");
        if (bundle.IsSold)
            throw new InvalidOperationException("Cannot delete a sold bundle.");

        this._context.Bundles.Remove(bundle);
        await this._context.SaveChangesAsync();
    }

    // ------------------------------------------------------------------ //
    //  Payment
    // ------------------------------------------------------------------ //

    /// <inheritdoc/>
    public async Task<string> CreateCheckoutSessionAsync(
        Guid bundleId,
        string buyerId,
        string successUrl,
        string cancelUrl,
        PaymentDeliveryRequest? delivery = null)
    {
        var bundle = await LoadBundleForPaymentAsync(bundleId);

        if (!IsStripeConfigured())
        {
            // Test mode: simulate completed payment immediately
            var payment = new PaymentEntity
            {
                BundleId = bundleId,
                ItemId = null,
                BuyerId = buyerId,
                SellerId = bundle.SellerId,
                Amount = bundle.Price,
                PaymentMethod = Domain.Enums.PaymentMethod.Card,
                StripeSessionId = $"test_{Guid.NewGuid()}",
                Status = PaymentStatus.Completed
            };
            this._context.Payments.Add(payment);
            await this._context.SaveChangesAsync();

            if (delivery != null)
            {
                try
                {
                    await this._shippingService.CreateShipmentAsync(BuildShipmentDto(payment.Id, delivery, isCod: false));
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "Auto-shipment creation failed for bundle payment {PaymentId}.", payment.Id);
                }
            }

            try { await CompleteBundlePurchaseAsync(bundleId, buyerId); }
            catch (Exception ex) { this._logger.LogError(ex, "Failed to complete bundle purchase request for bundle {BundleId}, buyer {BuyerId}.", bundleId, buyerId); }

            return successUrl + "?session_id=test_simulated";
        }

        // Live Stripe session
        var metadata = new Dictionary<string, string>
        {
            { "bundleId", bundleId.ToString() },
            { "buyerId", buyerId },
            { "sellerId", bundle.SellerId },
            { "isBundle", "true" }
        };

        if (delivery != null)
        {
            metadata["delivery_courier"] = delivery.CourierProvider.ToString();
            metadata["delivery_type"] = delivery.DeliveryType.ToString();
            metadata["delivery_name"] = delivery.RecipientName;
            metadata["delivery_phone"] = delivery.RecipientPhone;
            metadata["delivery_city"] = delivery.City ?? "";
            metadata["delivery_address"] = delivery.Address ?? "";
            metadata["delivery_office_id"] = delivery.OfficeId ?? "";
            metadata["delivery_office_name"] = delivery.OfficeName ?? "";
            metadata["delivery_weight"] = delivery.Weight.ToString();
        }

        decimal shippingFee = 0;
        if (delivery != null)
        {
            try
            {
                var calc = await this._shippingService.CalculatePriceAsync(new CalculateShippingDto
                {
                    CourierProvider = delivery.CourierProvider,
                    DeliveryType = delivery.DeliveryType,
                    ToCity = delivery.City,
                    OfficeId = delivery.OfficeId,
                    Weight = delivery.Weight,
                    IsCod = false,
                    CodAmount = 0,
                    IsInsured = false,
                    InsuredAmount = 0
                });
                shippingFee = calc.Price;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Shipping price calculation failed for bundle {BundleId} card checkout.", bundleId);
            }
        }

        var lineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(bundle.Price * 100),
                    Currency = "bgn",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = bundle.Title,
                        Description = bundle.Description != null
                            ? (bundle.Description.Length > 500 ? bundle.Description[..500] : bundle.Description)
                            : null
                    }
                },
                Quantity = 1
            }
        };

        if (shippingFee > 0)
        {
            lineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(shippingFee * 100),
                    Currency = "bgn",
                    ProductData = new SessionLineItemPriceDataProductDataOptions { Name = "Доставка / Shipping" }
                },
                Quantity = 1
            });
            metadata["delivery_shipping_fee"] = shippingFee.ToString("F2");
        }

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = ["card"],
            LineItems = lineItems,
            Mode = "payment",
            SuccessUrl = successUrl + "?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = cancelUrl,
            Metadata = metadata
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);
        return session.Url!;
    }

    /// <inheritdoc/>
    public async Task<PaymentDto> CreateOnSpotPaymentAsync(Guid bundleId, string buyerId, PaymentDeliveryRequest? delivery = null)
    {
        var bundle = await LoadBundleForPaymentAsync(bundleId);

        var payment = new PaymentEntity
        {
            BundleId = bundleId,
            ItemId = null,
            BuyerId = buyerId,
            SellerId = bundle.SellerId,
            Amount = bundle.Price,
            PaymentMethod = Domain.Enums.PaymentMethod.OnSpot,
            Status = PaymentStatus.Pending
        };

        this._context.Payments.Add(payment);
        await this._context.SaveChangesAsync();

        if (delivery != null)
        {
            try
            {
                await this._shippingService.CreateShipmentAsync(BuildShipmentDto(payment.Id, delivery, isCod: false));
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Auto-shipment creation failed for on-spot bundle payment {PaymentId}.", payment.Id);
            }
        }

        try { await CompleteBundlePurchaseAsync(bundleId, buyerId); }
        catch (Exception ex) { this._logger.LogError(ex, "Failed to complete bundle purchase request for bundle {BundleId}, buyer {BuyerId}.", bundleId, buyerId); }

        return MapPaymentToDto(payment);
    }

    /// <inheritdoc/>
    public async Task<PaymentDto> CreateCashOnDeliveryAsync(Guid bundleId, string buyerId, PaymentDeliveryRequest delivery)
    {
        var bundle = await LoadBundleForPaymentAsync(bundleId);

        decimal shippingFee = 0;
        try
        {
            var calc = await this._shippingService.CalculatePriceAsync(new CalculateShippingDto
            {
                CourierProvider = delivery.CourierProvider,
                DeliveryType = delivery.DeliveryType,
                ToCity = delivery.City,
                OfficeId = delivery.OfficeId,
                Weight = delivery.Weight,
                IsCod = true,
                CodAmount = bundle.Price,
                IsInsured = false,
                InsuredAmount = 0
            });
            shippingFee = calc.Price;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Shipping price calculation failed for COD bundle payment on bundle {BundleId}. Defaulting to 0.", bundleId);
        }

        var totalAmount = bundle.Price + shippingFee;

        var payment = new PaymentEntity
        {
            BundleId = bundleId,
            ItemId = null,
            BuyerId = buyerId,
            SellerId = bundle.SellerId,
            Amount = totalAmount,
            PaymentMethod = Domain.Enums.PaymentMethod.CashOnDelivery,
            Status = PaymentStatus.Pending
        };

        this._context.Payments.Add(payment);
        await this._context.SaveChangesAsync();

        try
        {
            await this._shippingService.CreateShipmentAsync(BuildShipmentDto(payment.Id, delivery, isCod: true, codAmount: totalAmount));
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Auto-shipment creation failed for COD bundle payment {PaymentId}.", payment.Id);
        }

        try { await CompleteBundlePurchaseAsync(bundleId, buyerId); }
        catch (Exception ex) { this._logger.LogError(ex, "Failed to complete bundle purchase request for bundle {BundleId}, buyer {BuyerId}.", bundleId, buyerId); }

        return MapPaymentToDto(payment);
    }

    // ------------------------------------------------------------------ //
    //  Private helpers
    // ------------------------------------------------------------------ //

    private bool IsStripeConfigured()
    {
        var stripeKey = this._configuration["Stripe:SecretKey"];
        return !string.IsNullOrWhiteSpace(stripeKey) && !stripeKey.Contains("YOUR_STRIPE");
    }

    private async Task<Bundle> LoadBundleForPaymentAsync(Guid bundleId)
    {
        var bundle = await this._context.Bundles
            .Include(b => b.BundleItems)
                .ThenInclude(bi => bi.Item)
            .FirstOrDefaultAsync(b => b.Id == bundleId);

        if (bundle == null) throw new KeyNotFoundException("Bundle not found.");
        if (!bundle.IsActive || bundle.IsSold) throw new InvalidOperationException("This bundle is not available.");

        return bundle;
    }

    /// <summary>
    /// Finds the accepted PurchaseRequest for this bundle + buyer and marks it Completed.
    /// Also marks all bundle items as sold and the bundle itself as sold.
    /// </summary>
    private async Task CompleteBundlePurchaseAsync(Guid bundleId, string buyerId)
    {
        var request = await this._context.PurchaseRequests
            .FirstOrDefaultAsync(r => r.BundleId == bundleId
                                   && r.BuyerId == buyerId
                                   && r.Status == PurchaseRequestStatus.Accepted);

        if (request != null)
        {
            request.Status = PurchaseRequestStatus.Completed;
        }

        // Mark all bundle items as sold
        var bundleItems = await this._context.BundleItems
            .Include(bi => bi.Item)
            .Where(bi => bi.BundleId == bundleId)
            .ToListAsync();

        foreach (var bi in bundleItems)
        {
            bi.Item.IsActive = false;
            bi.Item.IsReserved = false;
            bi.Item.IsSold = true;
        }

        // Mark the bundle as sold
        var bundle = await this._context.Bundles.FirstOrDefaultAsync(b => b.Id == bundleId);
        if (bundle != null)
        {
            bundle.IsSold = true;
            bundle.IsActive = false;
        }

        await this._context.SaveChangesAsync();
    }

    private static CreateShipmentDto BuildShipmentDto(
        Guid paymentId,
        PaymentDeliveryRequest delivery,
        bool isCod,
        decimal codAmount = 0)
    {
        return new CreateShipmentDto
        {
            PaymentId = paymentId,
            CourierProvider = delivery.CourierProvider,
            DeliveryType = delivery.DeliveryType,
            RecipientName = delivery.RecipientName,
            RecipientPhone = delivery.RecipientPhone,
            City = delivery.City,
            DeliveryAddress = delivery.Address,
            OfficeId = delivery.OfficeId,
            OfficeName = delivery.OfficeName,
            Weight = delivery.Weight,
            IsCod = isCod,
            CodAmount = codAmount,
            IsInsured = false,
            InsuredAmount = 0
        };
    }

    private static BundleDto MapToDto(Bundle bundle)
    {
        return new BundleDto
        {
            Id = bundle.Id,
            Title = bundle.Title,
            Description = bundle.Description,
            Price = bundle.Price,
            SellerId = bundle.SellerId,
            SellerDisplayName = bundle.Seller?.DisplayName,
            SellerAvatarUrl = bundle.Seller?.AvatarUrl,
            IsActive = bundle.IsActive,
            IsSold = bundle.IsSold,
            CreatedAt = bundle.CreatedAt,
            Items = bundle.BundleItems
                .Select(bi => new BundleItemDto
                {
                    ItemId = bi.ItemId,
                    Title = bi.Item?.Title ?? "",
                    Price = bi.Item?.Price,
                    PhotoUrl = bi.Item?.Photos?
                        .OrderBy(p => p.DisplayOrder)
                        .Select(p => p.Url)
                        .FirstOrDefault()
                })
                .ToList()
        };
    }

    private static PaymentDto MapPaymentToDto(PaymentEntity payment)
    {
        return new PaymentDto
        {
            Id = payment.Id,
            ItemId = payment.ItemId ?? Guid.Empty,
            ItemTitle = null,
            BuyerId = payment.BuyerId,
            SellerId = payment.SellerId,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            Status = payment.Status,
            CreatedAt = payment.CreatedAt,
            ReceiptUrl = payment.ReceiptUrl
        };
    }
}
