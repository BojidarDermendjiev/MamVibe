namespace MomVibe.Infrastructure.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Stripe;
using Stripe.Checkout;

using Microsoft.Extensions.Options;

using Domain.Enums;
using Application.Interfaces;
using Application.DTOs.Payments;
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

    public PaymentService(
        IApplicationDbContext context,
        IMapper mapper,
        IConfiguration configuration,
        ITakeANapService takeANapService,
        IN8nWebhookService webhook,
        IOptions<N8nSettings> n8nSettings)
    {
        this._context = context;
        this._mapper = mapper;
        this._configuration = configuration;
        this._takeANapService = takeANapService;
        this._webhook = webhook;
        this._n8nSettings = n8nSettings.Value;
        StripeConfiguration.ApiKey = this._configuration["Stripe:SecretKey"];
    }

    public async Task<string> CreateCheckoutSessionAsync(Guid itemId, string buyerId, string successUrl, string cancelUrl)
    {
        var stripeKey = this._configuration["Stripe:SecretKey"];
        if (string.IsNullOrWhiteSpace(stripeKey) || stripeKey.Contains("YOUR_STRIPE"))
            throw new InvalidOperationException("Stripe is not configured. Please set a valid Stripe SecretKey in appsettings.json.");

        var item = await this._context.Items.Include(i => i.Photos).FirstOrDefaultAsync(i => i.Id == itemId);
        if (item == null) throw new KeyNotFoundException("Item not found.");
        if (item.ListingType != ListingType.Sell)
            throw new InvalidOperationException("This item is not for sale.");

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
            Metadata = new Dictionary<string, string>
            {
                { "itemId", item.Id.ToString() },
                { "buyerId", buyerId },
                { "sellerId", item.UserId }
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);
        return session.Url!;
    }

    public async Task HandleWebhookAsync(string json, string stripeSignature)
    {
        var webhookSecret = _configuration["Stripe:WebhookSecret"]!;
        var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);

        if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
        {
            var session = stripeEvent.Data.Object as Session;
            if (session == null) return;

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
                    var currentItem = items.First(i => i.Id == itemIds[idx]);
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
                            BuyerEmail = buyer?.Email,
                            BuyerName = buyer?.DisplayName,
                            SellerEmail = item.User?.Email,
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
                        BuyerEmail = payBuyer?.Email,
                        BuyerName = payBuyer?.DisplayName,
                        SellerEmail = paidItem?.User?.Email,
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
            }
        }
    }

    public async Task<PaymentDto> CreateOnSpotPaymentAsync(Guid itemId, string buyerId)
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

        return this._mapper.Map<PaymentDto>(payment);
    }

    public async Task<PaymentDto> CreateBookingAsync(Guid itemId, string buyerId)
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

        return this._mapper.Map<PaymentDto>(payment);
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
            .Include(p => p.Item)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return this._mapper.Map<List<PaymentDto>>(payments);
    }

    public async Task<string> CreateBulkCheckoutSessionAsync(List<Guid> itemIds, string buyerId, string successUrl, string cancelUrl)
    {
        var stripeKey = this._configuration["Stripe:SecretKey"];
        if (string.IsNullOrWhiteSpace(stripeKey) || stripeKey.Contains("YOUR_STRIPE"))
            throw new InvalidOperationException("Stripe is not configured. Please set a valid Stripe SecretKey in appsettings.json.");

        var items = await this._context.Items.Where(i => itemIds.Contains(i.Id)).ToListAsync();
        if (items.Count != itemIds.Count)
            throw new KeyNotFoundException("One or more items not found.");

        var saleItems = items.Where(i => i.ListingType == ListingType.Sell).ToList();
        if (saleItems.Count == 0)
            throw new InvalidOperationException("No sale items in the cart.");

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
        var stripeKey = this._configuration["Stripe:SecretKey"];
        if (string.IsNullOrWhiteSpace(stripeKey) || stripeKey.Contains("YOUR_STRIPE"))
            throw new InvalidOperationException("Stripe is not configured. Please set a valid Stripe SecretKey in appsettings.json.");

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
}
