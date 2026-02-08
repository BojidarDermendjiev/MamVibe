namespace MomVibe.Infrastructure.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Stripe;
using Stripe.Checkout;

using Domain.Enums;
using Application.Interfaces;
using Application.DTOs.Payments;
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

    public PaymentService(IApplicationDbContext context, IMapper mapper, IConfiguration configuration, ITakeANapService takeANapService)
    {
        this._context = context;
        this._mapper = mapper;
        this._configuration = configuration;
        this._takeANapService = takeANapService;
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
