namespace MomVibe.Infrastructure.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Stripe;
using Stripe.Checkout;

using Microsoft.Extensions.Options;

using Domain.Enums;
using Application.Interfaces;
using Application.DTOs.Payments;
using Application.DTOs.Shipping;
using Infrastructure.Configuration;
using PaymentEntity = Domain.Entities.Payment;

/// <summary>
/// Service for processing payments: creates Stripe Checkout sessions for sellable items,
/// handles Stripe webhook events to persist completed card payments, supports on-spot payments,
/// and retrieves payments associated with a user. Integrates Stripe SDK, EF Core, AutoMapper,
/// and configuration; validates item availability and sets appropriate payment status and metadata.
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly ITakeANapService _takeANapService;
    private readonly IN8nWebhookService _webhook;
    private readonly N8nSettings _n8nSettings;
    private readonly IShippingService _shippingService;
    private readonly IEBillService _eBillService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IApplicationDbContext context,
        IMapper mapper,
        IConfiguration configuration,
        ITakeANapService takeANapService,
        IN8nWebhookService webhook,
        IOptions<N8nSettings> n8nSettings,
        IShippingService shippingService,
        IEBillService eBillService,
        ILogger<PaymentService> logger)
    {
        this._context = context;
        this._mapper = mapper;
        this._configuration = configuration;
        this._takeANapService = takeANapService;
        this._webhook = webhook;
        this._n8nSettings = n8nSettings.Value;
        this._shippingService = shippingService;
        this._eBillService = eBillService;
        this._logger = logger;
        StripeConfiguration.ApiKey = this._configuration["Stripe:SecretKey"];
    }

    private static string MaskEmail(string? email)
    {
        if (string.IsNullOrEmpty(email)) return "***";
        var at = email.IndexOf('@');
        if (at <= 0) return "***";
        var local = email[..at];
        var domain = email[at..];
        return (local.Length <= 2 ? "***" : local[..2] + "***") + domain;
    }

    private bool IsStripeConfigured()
    {
        var stripeKey = this._configuration["Stripe:SecretKey"];
        return !string.IsNullOrWhiteSpace(stripeKey) && !stripeKey.Contains("YOUR_STRIPE");
    }

    public async Task<string> CreateCheckoutSessionAsync(Guid itemId, string buyerId, string successUrl, string cancelUrl, PaymentDeliveryRequest? delivery = null)
    {
        var item = await this._context.Items.Include(i => i.Photos).FirstOrDefaultAsync(i => i.Id == itemId);
        if (item == null) throw new KeyNotFoundException("Item not found.");
        if (item.ListingType != ListingType.Sell)
            throw new InvalidOperationException("This item is not for sale.");

        // Test mode: simulate payment when Stripe is not configured
        if (!IsStripeConfigured())
        {
            var payment = new PaymentEntity
            {
                ItemId = itemId,
                BuyerId = buyerId,
                SellerId = item.UserId,
                Amount = item.Price ?? 0,
                PaymentMethod = Domain.Enums.PaymentMethod.Card,
                StripeSessionId = $"test_{Guid.NewGuid()}",
                Status = PaymentStatus.Completed
            };
            this._context.Payments.Add(payment);
            await this._context.SaveChangesAsync();

            try
            {
                this._webhook.Send(this._n8nSettings.PaymentCompleted, new
                {
                    Event = "payment.completed",
                    Timestamp = DateTime.UtcNow,
                    PaymentId = payment.Id,
                    ItemId = item.Id,
                    ItemTitle = item.Title,
                    Amount = payment.Amount,
                    TestMode = true
                });
            }
            catch { /* Webhook failure must not break payment flow */ }

            // Auto-create shipment in test mode if delivery info provided
            if (delivery != null)
            {
                try
                {
                    await this._shippingService.CreateShipmentAsync(new CreateShipmentDto
                    {
                        PaymentId = payment.Id,
                        CourierProvider = delivery.CourierProvider,
                        DeliveryType = delivery.DeliveryType,
                        RecipientName = delivery.RecipientName,
                        RecipientPhone = delivery.RecipientPhone,
                        City = delivery.City,
                        DeliveryAddress = delivery.Address,
                        OfficeId = delivery.OfficeId,
                        OfficeName = delivery.OfficeName,
                        Weight = delivery.Weight,
                        IsCod = false,
                        CodAmount = 0,
                        IsInsured = false,
                        InsuredAmount = 0
                    });
                }
                catch (Exception ex) { this._logger.LogError(ex, "Auto-shipment creation failed for payment {PaymentId}. Buyer's delivery details were recorded but no waybill was generated.", payment.Id); }
            }

            try { await CompletePurchaseRequestAsync(itemId, buyerId); }
            catch (Exception ex) { this._logger.LogError(ex, "Failed to complete purchase request for item {ItemId}, buyer {BuyerId}.", itemId, buyerId); }

            return successUrl + "?session_id=test_simulated";
        }

        var metadata = new Dictionary<string, string>
        {
            { "itemId", item.Id.ToString() },
            { "buyerId", buyerId },
            { "sellerId", item.UserId }
        };

        // Embed delivery info as Stripe metadata so the webhook can reconstruct it
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

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = ["card"],
            LineItems =
            [
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price!.Value * 100),
                        Currency = "bgn",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Title,
                            Description = item.Description.Length > 500 ? item.Description[..500] : item.Description
                        }
                    },
                    Quantity = 1
                }
            ],
            Mode = "payment",
            SuccessUrl = successUrl + "?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = cancelUrl,
            Metadata = metadata
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);
        return session.Url!;
    }

    public async Task HandleWebhookAsync(string json, string stripeSignature)
    {
        var webhookSecret = _configuration["Stripe:WebhookSecret"];
        if (string.IsNullOrWhiteSpace(webhookSecret))
            throw new InvalidOperationException("Stripe:WebhookSecret is not configured.");

        var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);

        if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
        {
            var session = stripeEvent.Data.Object as Session;
            if (session == null) return;

            // Idempotency guard: Stripe delivers webhooks at-least-once; skip if already processed.
            if (await this._context.Payments.AnyAsync(p => p.StripeSessionId == session.Id))
            {
                this._logger.LogInformation("Duplicate Stripe webhook for session {SessionId} — skipping.", session.Id);
                return;
            }

            var isBulk = session.Metadata.ContainsKey("isBulk") && session.Metadata["isBulk"] == "true";

            if (isBulk)
            {
                var itemIdStrings = session.Metadata["itemIds"].Split(',');
                var sellerIdStrings = session.Metadata["sellerIds"].Split(',');
                var buyerId = session.Metadata["buyerId"];
                var itemIds = itemIdStrings.Select(Guid.Parse).ToList();
                var items = await this._context.Items.Where(i => itemIds.Contains(i.Id)).ToListAsync();

                for (var idx = 0; idx < itemIds.Count; idx++)
                {
                    var currentItem = items.FirstOrDefault(i => i.Id == itemIds[idx]);
                    if (currentItem == null) continue;
                    var payment = new PaymentEntity
                    {
                        ItemId = itemIds[idx],
                        BuyerId = buyerId,
                        SellerId = idx < sellerIdStrings.Length ? sellerIdStrings[idx] : currentItem.UserId,
                        Amount = currentItem.Price ?? 0,
                        PaymentMethod = Domain.Enums.PaymentMethod.Card,
                        StripeSessionId = session.Id,
                        Status = PaymentStatus.Completed
                    };
                    this._context.Payments.Add(payment);
                }

                await this._context.SaveChangesAsync();

                // Fire webhook for each bulk payment
                try
                {
                    var buyer = await this._context.Payments
                        .Where(p => p.BuyerId == buyerId)
                        .Select(p => p.Buyer)
                        .FirstOrDefaultAsync();

                    foreach (var item in items)
                    {
                        this._webhook.Send(this._n8nSettings.PaymentCompleted, new
                        {
                            Event = "payment.completed",
                            Timestamp = DateTime.UtcNow,
                            ItemId = item.Id,
                            ItemTitle = item.Title,
                            BuyerEmail = MaskEmail(buyer?.Email),
                            BuyerName = buyer?.DisplayName,
                            SellerEmail = MaskEmail(item.User?.Email),
                            SellerName = item.User?.DisplayName,
                            Amount = item.Price ?? 0
                        });
                    }
                }
                catch { /* Webhook failure must not break payment flow */ }
            }
            else
            {
                var payment = new PaymentEntity
                {
                    ItemId = Guid.Parse(session.Metadata["itemId"]),
                    BuyerId = session.Metadata["buyerId"],
                    SellerId = session.Metadata["sellerId"],
                    Amount = session.AmountTotal!.Value / 100m,
                    PaymentMethod = Domain.Enums.PaymentMethod.Card,
                    StripeSessionId = session.Id,
                    Status = PaymentStatus.Completed
                };

                this._context.Payments.Add(payment);
                await this._context.SaveChangesAsync();

                // Auto-create shipment if delivery metadata was embedded at checkout
                if (session.Metadata.ContainsKey("delivery_name") && !string.IsNullOrEmpty(session.Metadata["delivery_name"]))
                {
                    try
                    {
                        await this._shippingService.CreateShipmentAsync(new CreateShipmentDto
                        {
                            PaymentId = payment.Id,
                            CourierProvider = Enum.Parse<Domain.Enums.CourierProvider>(session.Metadata["delivery_courier"]),
                            DeliveryType = Enum.Parse<Domain.Enums.DeliveryType>(session.Metadata["delivery_type"]),
                            RecipientName = session.Metadata["delivery_name"],
                            RecipientPhone = session.Metadata["delivery_phone"],
                            City = string.IsNullOrEmpty(session.Metadata.GetValueOrDefault("delivery_city")) ? null : session.Metadata["delivery_city"],
                            DeliveryAddress = string.IsNullOrEmpty(session.Metadata.GetValueOrDefault("delivery_address")) ? null : session.Metadata["delivery_address"],
                            OfficeId = string.IsNullOrEmpty(session.Metadata.GetValueOrDefault("delivery_office_id")) ? null : session.Metadata["delivery_office_id"],
                            OfficeName = string.IsNullOrEmpty(session.Metadata.GetValueOrDefault("delivery_office_name")) ? null : session.Metadata["delivery_office_name"],
                            Weight = decimal.TryParse(session.Metadata.GetValueOrDefault("delivery_weight"), out var w) ? w : 1m,
                            IsCod = false,
                            CodAmount = 0,
                            IsInsured = false,
                            InsuredAmount = 0
                        });
                    }
                    catch (Exception ex) { this._logger.LogError(ex, "Auto-shipment creation failed for Stripe payment session {SessionId}.", session.Id); }
                }

                // Fire webhook for single payment
                try
                {
                    var paidItem = await this._context.Items.Include(i => i.User).FirstOrDefaultAsync(i => i.Id == payment.ItemId);
                    var payBuyer = await this._context.Payments
                        .Include(p => p.Buyer)
                        .Where(p => p.Id == payment.Id)
                        .Select(p => p.Buyer)
                        .FirstOrDefaultAsync();

                    this._webhook.Send(this._n8nSettings.PaymentCompleted, new
                    {
                        Event = "payment.completed",
                        Timestamp = DateTime.UtcNow,
                        PaymentId = payment.Id,
                        ItemId = payment.ItemId,
                        ItemTitle = paidItem?.Title,
                        BuyerEmail = MaskEmail(payBuyer?.Email),
                        BuyerName = payBuyer?.DisplayName,
                        SellerEmail = MaskEmail(paidItem?.User?.Email),
                        SellerName = paidItem?.User?.DisplayName,
                        payment.Amount
                    });
                }
                catch { /* Webhook failure must not break payment flow */ }

                // Create digital receipt via Take a NAP (non-blocking on failure)
                try
                {
                    var item = await this._context.Items.FindAsync(payment.ItemId);
                    payment.Item = item!;
                    var receiptUrl = await this._takeANapService.CreateOrderAndGetReceiptAsync(payment);
                    if (receiptUrl != null)
                    {
                        payment.ReceiptUrl = receiptUrl;
                        await this._context.SaveChangesAsync();
                    }
                }
                catch
                {
                    // Receipt failure should not break payment flow
                }

                // Issue e-bill and send receipt email to buyer (non-blocking on failure)
                try
                {
                    var buyerEmail = await this._context.Payments
                        .Include(p => p.Buyer)
                        .Where(p => p.Id == payment.Id)
                        .Select(p => p.Buyer!.Email)
                        .FirstOrDefaultAsync();

                    if (buyerEmail != null)
                        await this._eBillService.IssueEBillAsync(payment.Id, buyerEmail);
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "E-bill issuance failed for payment {PaymentId}.", payment.Id);
                }

                try { await CompletePurchaseRequestAsync(payment.ItemId, payment.BuyerId); }
                catch (Exception ex) { this._logger.LogError(ex, "Failed to complete purchase request for item {ItemId}, buyer {BuyerId}.", payment.ItemId, payment.BuyerId); }
            }
        }
    }

    public async Task<PaymentDto> CreateOnSpotPaymentAsync(Guid itemId, string buyerId, PaymentDeliveryRequest? delivery = null)
    {
        var item = await this._context.Items.FindAsync(itemId);
        if (item == null) throw new KeyNotFoundException("Item not found.");

        var payment = new PaymentEntity
        {
            ItemId = itemId,
            BuyerId = buyerId,
            SellerId = item.UserId,
            Amount = item.Price ?? 0,
            PaymentMethod = Domain.Enums.PaymentMethod.OnSpot,
            Status = PaymentStatus.Pending
        };

        this._context.Payments.Add(payment);
        await this._context.SaveChangesAsync();

        if (delivery != null)
        {
            try
            {
                await this._shippingService.CreateShipmentAsync(new CreateShipmentDto
                {
                    PaymentId = payment.Id,
                    CourierProvider = delivery.CourierProvider,
                    DeliveryType = delivery.DeliveryType,
                    RecipientName = delivery.RecipientName,
                    RecipientPhone = delivery.RecipientPhone,
                    City = delivery.City,
                    DeliveryAddress = delivery.Address,
                    OfficeId = delivery.OfficeId,
                    OfficeName = delivery.OfficeName,
                    Weight = delivery.Weight,
                    IsCod = false,
                    CodAmount = 0,
                    IsInsured = false,
                    InsuredAmount = 0
                });
            }
            catch { /* Shipment creation failure must not break payment flow */ }
        }

        try { await CompletePurchaseRequestAsync(itemId, buyerId); }
        catch (Exception ex) { this._logger.LogError(ex, "Failed to complete purchase request for item {ItemId}, buyer {BuyerId}.", itemId, buyerId); }

        return this._mapper.Map<PaymentDto>(payment);
    }

    public async Task<PaymentDto> CreateBookingAsync(Guid itemId, string buyerId, PaymentDeliveryRequest? delivery = null)
    {
        var item = await this._context.Items.FindAsync(itemId);
        if (item == null) throw new KeyNotFoundException("Item not found.");
        if (item.ListingType != ListingType.Donate)
            throw new InvalidOperationException("Only donate items can be booked.");

        var payment = new PaymentEntity
        {
            ItemId = itemId,
            BuyerId = buyerId,
            SellerId = item.UserId,
            Amount = 0,
            PaymentMethod = Domain.Enums.PaymentMethod.Booking,
            Status = PaymentStatus.Completed
        };

        this._context.Payments.Add(payment);
        await this._context.SaveChangesAsync();

        if (delivery != null)
        {
            try
            {
                await this._shippingService.CreateShipmentAsync(new CreateShipmentDto
                {
                    PaymentId = payment.Id,
                    CourierProvider = delivery.CourierProvider,
                    DeliveryType = delivery.DeliveryType,
                    RecipientName = delivery.RecipientName,
                    RecipientPhone = delivery.RecipientPhone,
                    City = delivery.City,
                    DeliveryAddress = delivery.Address,
                    OfficeId = delivery.OfficeId,
                    OfficeName = delivery.OfficeName,
                    Weight = delivery.Weight,
                    IsCod = false,
                    CodAmount = 0,
                    IsInsured = false,
                    InsuredAmount = 0
                });
            }
            catch { /* Shipment creation failure must not break payment flow */ }
        }

        try { await CompletePurchaseRequestAsync(itemId, buyerId); }
        catch (Exception ex) { this._logger.LogError(ex, "Failed to complete purchase request for item {ItemId}, buyer {BuyerId}.", itemId, buyerId); }

        return this._mapper.Map<PaymentDto>(payment);
    }

    /// <summary>
    /// Finds the accepted PurchaseRequest for this item+buyer and marks it Completed.
    /// Called after any successful payment so the buyer's "My Requests" tab reflects the true state.
    /// </summary>
    private async Task CompletePurchaseRequestAsync(Guid itemId, string buyerId)
    {
        var request = await this._context.PurchaseRequests
            .FirstOrDefaultAsync(r => r.ItemId == itemId
                                   && r.BuyerId == buyerId
                                   && r.Status == PurchaseRequestStatus.Accepted);
        if (request != null)
        {
            request.Status = PurchaseRequestStatus.Completed;
            await this._context.SaveChangesAsync();
        }
    }

    public async Task<List<PaymentDto>> GetPaymentsByUserAsync(string userId)
    {
        var payments = await this._context.Payments
            .Include(p => p.Item)
            .Where(p => p.BuyerId == userId || p.SellerId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return this._mapper.Map<List<PaymentDto>>(payments);
    }

    public async Task<List<PaymentDto>> GetAllPaymentsAsync()
    {
        var payments = await this._context.Payments
            .AsNoTracking()
            .Include(p => p.Item)
            .OrderByDescending(p => p.CreatedAt)
            .Take(1000)
            .ToListAsync();

        return this._mapper.Map<List<PaymentDto>>(payments);
    }

    public async Task<string> CreateBulkCheckoutSessionAsync(List<Guid> itemIds, string buyerId, string successUrl, string cancelUrl)
    {
        var items = await this._context.Items.Where(i => itemIds.Contains(i.Id)).ToListAsync();
        if (items.Count != itemIds.Count)
            throw new KeyNotFoundException("One or more items not found.");

        var saleItems = items.Where(i => i.ListingType == ListingType.Sell).ToList();
        if (saleItems.Count == 0)
            throw new InvalidOperationException("No sale items in the cart.");

        // Test mode: simulate bulk payment when Stripe is not configured
        if (!IsStripeConfigured())
        {
            var testSessionId = $"test_{Guid.NewGuid()}";
            foreach (var item in saleItems)
            {
                var payment = new PaymentEntity
                {
                    ItemId = item.Id,
                    BuyerId = buyerId,
                    SellerId = item.UserId,
                    Amount = item.Price ?? 0,
                    PaymentMethod = Domain.Enums.PaymentMethod.Card,
                    StripeSessionId = testSessionId,
                    Status = PaymentStatus.Completed
                };
                this._context.Payments.Add(payment);
            }
            await this._context.SaveChangesAsync();

            try
            {
                foreach (var item in saleItems)
                {
                    this._webhook.Send(this._n8nSettings.PaymentCompleted, new
                    {
                        Event = "payment.completed",
                        Timestamp = DateTime.UtcNow,
                        ItemId = item.Id,
                        ItemTitle = item.Title,
                        Amount = item.Price ?? 0,
                        TestMode = true
                    });
                }
            }
            catch { /* Webhook failure must not break payment flow */ }

            return successUrl + "?session_id=test_simulated";
        }

        var lineItems = saleItems.Select(item => new SessionLineItemOptions
        {
            PriceData = new SessionLineItemPriceDataOptions
            {
                UnitAmount = (long)(item.Price!.Value * 100),
                Currency = "bgn",
                ProductData = new SessionLineItemPriceDataProductDataOptions
                {
                    Name = item.Title,
                    Description = item.Description.Length > 500 ? item.Description[..500] : item.Description
                }
            },
            Quantity = 1
        }).ToList();

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = ["card"],
            LineItems = lineItems,
            Mode = "payment",
            SuccessUrl = successUrl + "?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = cancelUrl,
            Metadata = new Dictionary<string, string>
            {
                { "itemIds", string.Join(",", saleItems.Select(i => i.Id)) },
                { "buyerId", buyerId },
                { "sellerIds", string.Join(",", saleItems.Select(i => i.UserId)) },
                { "isBulk", "true" }
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);
        return session.Url!;
    }

    public async Task<List<PaymentDto>> CreateBulkBookingAsync(List<Guid> itemIds, string buyerId)
    {
        var items = await this._context.Items.Where(i => itemIds.Contains(i.Id)).ToListAsync();
        if (items.Count != itemIds.Count)
            throw new KeyNotFoundException("One or more items not found.");

        var donateItems = items.Where(i => i.ListingType == ListingType.Donate).ToList();
        var payments = new List<PaymentEntity>();

        foreach (var item in donateItems)
        {
            var payment = new PaymentEntity
            {
                ItemId = item.Id,
                BuyerId = buyerId,
                SellerId = item.UserId,
                Amount = 0,
                PaymentMethod = Domain.Enums.PaymentMethod.Booking,
                Status = PaymentStatus.Completed
            };
            this._context.Payments.Add(payment);
            payments.Add(payment);
        }

        await this._context.SaveChangesAsync();
        return this._mapper.Map<List<PaymentDto>>(payments);
    }

    public async Task<List<PaymentDto>> CreateBulkOnSpotPaymentAsync(List<Guid> itemIds, string buyerId)
    {
        var items = await this._context.Items.Where(i => itemIds.Contains(i.Id)).ToListAsync();
        if (items.Count != itemIds.Count)
            throw new KeyNotFoundException("One or more items not found.");

        var payments = new List<PaymentEntity>();

        foreach (var item in items)
        {
            var payment = new PaymentEntity
            {
                ItemId = item.Id,
                BuyerId = buyerId,
                SellerId = item.UserId,
                Amount = item.Price ?? 0,
                PaymentMethod = Domain.Enums.PaymentMethod.OnSpot,
                Status = PaymentStatus.Pending
            };
            this._context.Payments.Add(payment);
            payments.Add(payment);
        }

        await this._context.SaveChangesAsync();
        return this._mapper.Map<List<PaymentDto>>(payments);
    }

    public async Task<string> CreatePaymentIntentAsync(Guid itemId, string buyerId)
    {
        if (!IsStripeConfigured())
            return "test_simulated_client_secret";

        var item = await this._context.Items.FirstOrDefaultAsync(i => i.Id == itemId);
        if (item == null) throw new KeyNotFoundException("Item not found.");
        if (item.ListingType != ListingType.Sell)
            throw new InvalidOperationException("This item is not for sale.");

        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(item.Price!.Value * 100),
            Currency = "bgn",
            Metadata = new Dictionary<string, string>
            {
                { "itemId", item.Id.ToString() },
                { "buyerId", buyerId },
                { "sellerId", item.UserId }
            }
        };

        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options);
        return paymentIntent.ClientSecret;
    }

    public async Task<string> CreateDonationCheckoutAsync(decimal amount, string successUrl, string cancelUrl)
    {
        if (amount < 1.00m || amount > 10000.00m)
            throw new InvalidOperationException("Donation amount must be between 1.00 and 10,000.00 BGN.");

        if (!IsStripeConfigured())
            return successUrl + "?session_id=test_donation_simulated";

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = ["card"],
            LineItems =
            [
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(amount * 100),
                        Currency = "bgn",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "MamVibe Donation 💛",
                            Description = "Thank you for supporting MamVibe!"
                        }
                    },
                    Quantity = 1
                }
            ],
            Mode = "payment",
            SuccessUrl = successUrl + "?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = cancelUrl,
            Metadata = new Dictionary<string, string>
            {
                { "type", "donation" },
                { "amount", amount.ToString("F2") }
            }
        };

        var sessionService = new SessionService();
        var session = await sessionService.CreateAsync(options);
        return session.Url!;
    }

    public async Task<string> CreateDonationIntentAsync(decimal amount)
    {
        if (amount < 1.00m || amount > 10000.00m)
            throw new InvalidOperationException("Donation amount must be between 1.00 and 10,000.00 BGN.");

        if (!IsStripeConfigured())
            return "test_simulated_donation_client_secret";

        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100),
            Currency = "bgn",
            Metadata = new Dictionary<string, string>
            {
                { "type", "donation" },
                { "amount", amount.ToString("F2") }
            }
        };

        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(options);
        return intent.ClientSecret;
    }
}
