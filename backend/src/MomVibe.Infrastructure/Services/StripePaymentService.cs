namespace MomVibe.Infrastructure.Services;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Stripe;
using Stripe.Checkout;

using Domain.Enums;
using Application.Events;
using Application.Interfaces;
using Application.DTOs.Payments;
using Application.DTOs.Shipping;
using Infrastructure.Configuration;

using PaymentEntity = Domain.Entities.Payment;

/// <summary>
/// Handles all Stripe-specific payment flows: Checkout sessions, PaymentIntents, bulk/bundle
/// sessions, donation sessions, and Stripe webhook event processing.
/// </summary>
public class StripePaymentService : IStripePaymentService
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IShippingService _shippingService;
    private readonly IPublisher _publisher;
    private readonly IBusinessSubscriptionService _businessSubscriptions;
    private readonly ILogger<StripePaymentService> _logger;

    public StripePaymentService(
        IApplicationDbContext context,
        IConfiguration configuration,
        IShippingService shippingService,
        IPublisher publisher,
        IBusinessSubscriptionService businessSubscriptions,
        ILogger<StripePaymentService> logger)
    {
        _context = context;
        _configuration = configuration;
        _shippingService = shippingService;
        _publisher = publisher;
        _businessSubscriptions = businessSubscriptions;
        _logger = logger;
    }

    private bool IsStripeConfigured()
    {
        var key = _configuration["Stripe:SecretKey"];
        return !string.IsNullOrWhiteSpace(key) && !key.Contains("YOUR_STRIPE");
    }

    private static RequestOptions? StripeOptionsForKey(string? key) =>
        string.IsNullOrWhiteSpace(key) ? null : new RequestOptions { IdempotencyKey = key };

    private async Task<PaymentEntity?> FindRecentByIdempotencyKeyAsync(string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        var cutoff = DateTime.UtcNow.AddHours(-24);
        return await _context.Payments
            .AsNoTracking()
            .Include(p => p.Item)
            .Where(p => p.IdempotencyKey == key && p.CreatedAt > cutoff)
            .FirstOrDefaultAsync();
    }

    private async Task<List<PaymentEntity>> FindBulkByIdempotencyKeyAsync(string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return [];
        var prefix = key + "#";
        var cutoff = DateTime.UtcNow.AddHours(-24);
        return await _context.Payments
            .AsNoTracking()
            .Include(p => p.Item)
            .Where(p => p.IdempotencyKey != null
                     && p.IdempotencyKey.StartsWith(prefix)
                     && p.CreatedAt > cutoff)
            .ToListAsync();
    }

    private static string? BulkKeyFor(string? key, Guid itemId) =>
        string.IsNullOrWhiteSpace(key) ? null : $"{key}#{itemId}";

    public async Task<string> CreateCheckoutSessionAsync(
        Guid itemId, string buyerId, string successUrl, string cancelUrl,
        PaymentDeliveryRequest? delivery = null,
        string? idempotencyKey = null)
    {
        var item = await _context.Items.Include(i => i.Photos).FirstOrDefaultAsync(i => i.Id == itemId);
        if (item == null) throw new KeyNotFoundException("Item not found.");
        if (item.ListingType != ListingType.Sell)
            throw new InvalidOperationException("This item is not for sale.");
        if (item.UserId == buyerId)
            throw new InvalidOperationException("You cannot purchase your own item.");

        if (!IsStripeConfigured())
        {
            var existing = await FindRecentByIdempotencyKeyAsync(idempotencyKey);
            if (existing != null) return successUrl + "?session_id=test_simulated";

            var payment = new PaymentEntity
            {
                ItemId = itemId,
                BuyerId = buyerId,
                SellerId = item.UserId,
                Amount = item.Price ?? 0,
                PaymentMethod = Domain.Enums.PaymentMethod.Card,
                StripeSessionId = $"test_{Guid.NewGuid()}",
                Status = PaymentStatus.Completed,
                IdempotencyKey = idempotencyKey
            };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            await _publisher.Publish(new PaymentCompletedEvent(payment.Id, IsTestMode: true));

            if (delivery != null)
            {
                try
                {
                    await _shippingService.CreateShipmentAsync(new CreateShipmentDto
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
                        IsCod = false, CodAmount = 0, IsInsured = false, InsuredAmount = 0
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Auto-shipment creation failed for payment {PaymentId}.", payment.Id);
                }
            }

            return successUrl + "?session_id=test_simulated";
        }

        var metadata = new Dictionary<string, string>
        {
            { "itemId", item.Id.ToString() },
            { "buyerId", buyerId },
            { "sellerId", item.UserId }
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
                var calc = await _shippingService.CalculatePriceAsync(new CalculateShippingDto
                {
                    CourierProvider = delivery.CourierProvider,
                    DeliveryType = delivery.DeliveryType,
                    ToCity = delivery.City,
                    OfficeId = delivery.OfficeId,
                    Weight = delivery.Weight,
                    IsCod = false, CodAmount = 0, IsInsured = false, InsuredAmount = 0
                });
                shippingFee = calc.Price;
            }
            catch (Exception ex) { _logger.LogError(ex, "Shipping price calculation failed for item {ItemId}.", itemId); }
        }

        var lineItems = new List<SessionLineItemOptions>
        {
            new()
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(item.Price!.Value * 100),
                    Currency = "eur",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.Title,
                        Description = item.Description.Length > 500 ? item.Description[..500] : item.Description
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
                    Currency = "eur",
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

        var sessionService = new SessionService();
        var session = await sessionService.CreateAsync(options, StripeOptionsForKey(idempotencyKey));
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
            if (stripeEvent.Data.Object is not Session session) return;
            await HandleCheckoutSessionAsync(session);
            return;
        }

        if (stripeEvent.Type == EventTypes.PaymentIntentSucceeded)
        {
            if (stripeEvent.Data.Object is not PaymentIntent intent) return;
            await HandlePaymentIntentSucceededAsync(intent);
            return;
        }

        // Business-vertical subscription lifecycle events — route to BusinessSubscriptionService,
        // which writes the dedup ledger row and reconciles the local subscription state.
        if (stripeEvent.Type == EventTypes.CustomerSubscriptionCreated
            || stripeEvent.Type == EventTypes.CustomerSubscriptionUpdated
            || stripeEvent.Type == EventTypes.CustomerSubscriptionDeleted
            || stripeEvent.Type == EventTypes.InvoicePaymentSucceeded
            || stripeEvent.Type == EventTypes.InvoicePaymentFailed)
        {
            await _businessSubscriptions.HandleStripeEventAsync(stripeEvent);
            return;
        }
    }

    private async Task HandleCheckoutSessionAsync(Session session)
    {
        if (await _context.Payments.AnyAsync(p => p.StripeSessionId == session.Id))
        {
            _logger.LogInformation("Duplicate Stripe webhook for session {SessionId} — skipping.", session.Id);
            return;
        }

        var isBulk   = session.Metadata.ContainsKey("isBulk")   && session.Metadata["isBulk"]   == "true";
        var isBundle = session.Metadata.ContainsKey("isBundle") && session.Metadata["isBundle"] == "true";

        if (isBundle)
        {
            await HandleBundleWebhookAsync(session);
        }
        else if (isBulk)
        {
            await HandleBulkWebhookAsync(session);
        }
        else
        {
            await HandleSingleWebhookAsync(session);
        }
    }

    private async Task HandlePaymentIntentSucceededAsync(PaymentIntent intent)
    {
        if (intent.Metadata is null
            || !intent.Metadata.TryGetValue("itemId", out var itemIdRaw)
            || !Guid.TryParse(itemIdRaw, out var itemId))
        {
            _logger.LogWarning("PaymentIntent {IntentId} missing or malformed itemId metadata — ignoring.", intent.Id);
            return;
        }

        // Idempotency on the PaymentIntent identifier — Stripe will retry on transient failures.
        if (await _context.Payments.AnyAsync(p => p.StripeSessionId == intent.Id))
        {
            _logger.LogInformation("Duplicate PaymentIntent webhook {IntentId} — skipping.", intent.Id);
            return;
        }

        var buyerId  = intent.Metadata.GetValueOrDefault("buyerId")  ?? string.Empty;
        var sellerId = intent.Metadata.GetValueOrDefault("sellerId") ?? string.Empty;
        if (string.IsNullOrEmpty(buyerId) || string.IsNullOrEmpty(sellerId))
        {
            _logger.LogWarning("PaymentIntent {IntentId} missing buyerId/sellerId metadata — ignoring.", intent.Id);
            return;
        }

        Guid paymentId;
        await using var tx = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        try
        {
            // Double-sell guard: re-check item is still available inside the serializable tx.
            var item = await _context.Items
                .Where(i => i.Id == itemId && !i.IsSold)
                .FirstOrDefaultAsync();
            if (item is null)
            {
                _logger.LogWarning("PaymentIntent {IntentId} resolved an already-sold item {ItemId} — refund required.", intent.Id, itemId);
                await tx.RollbackAsync();
                return;
            }

            var payment = new PaymentEntity
            {
                ItemId          = itemId,
                BuyerId         = buyerId,
                SellerId        = sellerId,
                Amount          = intent.AmountReceived / 100m,
                PaymentMethod   = Domain.Enums.PaymentMethod.Card,
                StripeSessionId = intent.Id,
                Status          = PaymentStatus.Completed
            };
            _context.Payments.Add(payment);

            item.IsActive   = false;
            item.IsReserved = false;
            item.IsSold     = true;

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            paymentId = payment.Id;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        await _publisher.Publish(new PaymentCompletedEvent(paymentId));
    }

    private async Task HandleBundleWebhookAsync(Session session)
    {
        var bundleId = Guid.Parse(session.Metadata["bundleId"]);
        var buyerId  = session.Metadata["buyerId"];
        var sellerId = session.Metadata["sellerId"];

        Guid paymentId;

        // Single atomic transaction: payment record + bundle completion in one commit.
        // If either save fails the transaction rolls back, the payment row is gone,
        // and Stripe's retry will find no existing payment and process it again.
        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var bundlePayment = new PaymentEntity
            {
                BundleId = bundleId,
                ItemId   = null,
                BuyerId  = buyerId,
                SellerId = sellerId,
                Amount   = session.AmountTotal!.Value / 100m,
                PaymentMethod   = Domain.Enums.PaymentMethod.Card,
                StripeSessionId = session.Id,
                Status   = PaymentStatus.Completed
            };
            _context.Payments.Add(bundlePayment);

            var req = await _context.PurchaseRequests
                .FirstOrDefaultAsync(r => r.BundleId == bundleId
                                       && r.BuyerId  == buyerId
                                       && r.Status   == PurchaseRequestStatus.Accepted);
            if (req != null) req.Status = PurchaseRequestStatus.Completed;

            var bundleItems = await _context.BundleItems
                .Include(bi => bi.Item)
                .Where(bi => bi.BundleId == bundleId)
                .ToListAsync();

            foreach (var bi in bundleItems)
            {
                bi.Item.IsActive = false;
                bi.Item.IsReserved = false;
                bi.Item.IsSold = true;
            }

            var bundle = await _context.Bundles.FirstOrDefaultAsync(b => b.Id == bundleId);
            if (bundle != null) { bundle.IsSold = true; bundle.IsActive = false; }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            paymentId = bundlePayment.Id;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            _logger.LogError(ex, "Bundle webhook transaction rolled back for bundle {BundleId} — Stripe will retry.", bundleId);
            throw;
        }

        // Shipment creation and event publish are intentionally outside the transaction:
        // they are safe side-effects that do not need to be rolled back if they fail.
        if (session.Metadata.ContainsKey("delivery_name") && !string.IsNullOrEmpty(session.Metadata["delivery_name"]))
        {
            try { await CreateShipmentFromMetadataAsync(paymentId, session.Metadata); }
            catch (Exception ex) { _logger.LogError(ex, "Auto-shipment creation failed for bundle session {SessionId}.", session.Id); }
        }

        await _publisher.Publish(new PaymentCompletedEvent(paymentId));
    }

    private async Task HandleBulkWebhookAsync(Session session)
    {
        var itemIds   = session.Metadata["itemIds"].Split(',').Select(Guid.Parse).ToList();
        var sellerIds = session.Metadata["sellerIds"].Split(',');
        var buyerId   = session.Metadata["buyerId"];
        var payments  = new List<PaymentEntity>();

        // Single serializable transaction: payment rows + item sold-marking commit together.
        // A failure mid-loop rolls everything back; Stripe's retry will land here again
        // and the idempotency guard at the caller short-circuits on the second pass.
        await using var tx = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        try
        {
            var items = await _context.Items.Where(i => itemIds.Contains(i.Id)).ToListAsync();

            for (var idx = 0; idx < itemIds.Count; idx++)
            {
                var currentItem = items.FirstOrDefault(i => i.Id == itemIds[idx]);
                if (currentItem == null) continue;
                if (currentItem.IsSold)
                {
                    _logger.LogWarning("Bulk webhook for already-sold item {ItemId} in session {SessionId} — refund required.", currentItem.Id, session.Id);
                    continue;
                }

                var payment = new PaymentEntity
                {
                    ItemId          = itemIds[idx],
                    BuyerId         = buyerId,
                    SellerId        = idx < sellerIds.Length ? sellerIds[idx] : currentItem.UserId,
                    Amount          = currentItem.Price ?? 0,
                    PaymentMethod   = Domain.Enums.PaymentMethod.Card,
                    StripeSessionId = session.Id,
                    Status          = PaymentStatus.Completed
                };
                _context.Payments.Add(payment);
                payments.Add(payment);

                currentItem.IsActive   = false;
                currentItem.IsReserved = false;
                currentItem.IsSold     = true;
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            _logger.LogError(ex, "Bulk webhook transaction rolled back for session {SessionId} — Stripe will retry.", session.Id);
            throw;
        }

        foreach (var payment in payments)
            await _publisher.Publish(new PaymentCompletedEvent(payment.Id));
    }

    private async Task HandleSingleWebhookAsync(Session session)
    {
        var itemId = Guid.Parse(session.Metadata["itemId"]);

        Guid paymentId;
        await using var tx = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        try
        {
            // Double-sell guard inside the serializable tx: re-check IsSold to defeat the race
            // where two checkout flows both reached Stripe for the same item.
            var item = await _context.Items
                .Where(i => i.Id == itemId && !i.IsSold)
                .FirstOrDefaultAsync();
            if (item is null)
            {
                _logger.LogWarning("Single webhook resolved already-sold item {ItemId} for session {SessionId} — refund required.", itemId, session.Id);
                await tx.RollbackAsync();
                return;
            }

            var payment = new PaymentEntity
            {
                ItemId          = itemId,
                BuyerId         = session.Metadata["buyerId"],
                SellerId        = session.Metadata["sellerId"],
                Amount          = session.AmountTotal!.Value / 100m,
                PaymentMethod   = Domain.Enums.PaymentMethod.Card,
                StripeSessionId = session.Id,
                Status          = PaymentStatus.Completed
            };
            _context.Payments.Add(payment);

            item.IsActive   = false;
            item.IsReserved = false;
            item.IsSold     = true;

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            paymentId = payment.Id;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            _logger.LogError(ex, "Single webhook transaction rolled back for session {SessionId} — Stripe will retry.", session.Id);
            throw;
        }

        if (session.Metadata.ContainsKey("delivery_name") && !string.IsNullOrEmpty(session.Metadata["delivery_name"]))
        {
            try { await CreateShipmentFromMetadataAsync(paymentId, session.Metadata); }
            catch (Exception ex) { _logger.LogError(ex, "Auto-shipment creation failed for session {SessionId}.", session.Id); }
        }

        await _publisher.Publish(new PaymentCompletedEvent(paymentId));
    }

    private async Task CreateShipmentFromMetadataAsync(Guid paymentId, Dictionary<string, string> meta)
    {
        await _shippingService.CreateShipmentAsync(new CreateShipmentDto
        {
            PaymentId       = paymentId,
            CourierProvider = Enum.Parse<Domain.Enums.CourierProvider>(meta["delivery_courier"]),
            DeliveryType    = Enum.Parse<Domain.Enums.DeliveryType>(meta["delivery_type"]),
            RecipientName   = meta["delivery_name"],
            RecipientPhone  = meta["delivery_phone"],
            City            = string.IsNullOrEmpty(meta.GetValueOrDefault("delivery_city"))    ? null : meta["delivery_city"],
            DeliveryAddress = string.IsNullOrEmpty(meta.GetValueOrDefault("delivery_address")) ? null : meta["delivery_address"],
            OfficeId        = string.IsNullOrEmpty(meta.GetValueOrDefault("delivery_office_id"))   ? null : meta["delivery_office_id"],
            OfficeName      = string.IsNullOrEmpty(meta.GetValueOrDefault("delivery_office_name")) ? null : meta["delivery_office_name"],
            Weight          = decimal.TryParse(meta.GetValueOrDefault("delivery_weight"), out var w) ? w : 1m,
            IsCod = false, CodAmount = 0, IsInsured = false, InsuredAmount = 0
        });
    }

    public async Task<string> CreateBulkCheckoutSessionAsync(
        List<Guid> itemIds, string buyerId, string successUrl, string cancelUrl,
        string? idempotencyKey = null)
    {
        var items = await _context.Items.Where(i => itemIds.Contains(i.Id)).ToListAsync();
        if (items.Count != itemIds.Count) throw new KeyNotFoundException("One or more items not found.");

        var saleItems = items.Where(i => i.ListingType == ListingType.Sell).ToList();
        if (saleItems.Count == 0) throw new InvalidOperationException("No sale items in the cart.");

        if (!IsStripeConfigured())
        {
            var existingBatch = await FindBulkByIdempotencyKeyAsync(idempotencyKey);
            if (existingBatch.Count > 0) return successUrl + "?session_id=test_simulated";

            var testSessionId = $"test_{Guid.NewGuid()}";
            var created = new List<PaymentEntity>();
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
                    Status = PaymentStatus.Completed,
                    IdempotencyKey = BulkKeyFor(idempotencyKey, item.Id)
                };
                _context.Payments.Add(payment);
                created.Add(payment);
            }
            await _context.SaveChangesAsync();

            foreach (var p in created)
                await _publisher.Publish(new PaymentCompletedEvent(p.Id, IsTestMode: true));

            return successUrl + "?session_id=test_simulated";
        }

        var lineItems = saleItems.Select(item => new SessionLineItemOptions
        {
            PriceData = new SessionLineItemPriceDataOptions
            {
                UnitAmount = (long)(item.Price!.Value * 100),
                Currency = "eur",
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
                { "itemIds",   string.Join(",", saleItems.Select(i => i.Id)) },
                { "buyerId",   buyerId },
                { "sellerIds", string.Join(",", saleItems.Select(i => i.UserId)) },
                { "isBulk",    "true" }
            }
        };

        var sessionService = new SessionService();
        var session = await sessionService.CreateAsync(options, StripeOptionsForKey(idempotencyKey));
        return session.Url!;
    }

    public async Task<string> CreatePaymentIntentAsync(Guid itemId, string buyerId, string? idempotencyKey = null)
    {
        var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == itemId);
        if (item == null) throw new KeyNotFoundException("Item not found.");
        if (item.ListingType != ListingType.Sell)
            throw new InvalidOperationException("This item is not for sale.");
        if (item.UserId == buyerId)
            throw new InvalidOperationException("You cannot purchase your own item.");

        if (!IsStripeConfigured()) return "test_simulated_client_secret";

        var options = new PaymentIntentCreateOptions
        {
            Amount   = (long)(item.Price!.Value * 100),
            Currency = "eur",
            Metadata = new Dictionary<string, string>
            {
                { "itemId",   item.Id.ToString() },
                { "buyerId",  buyerId },
                { "sellerId", item.UserId }
            }
        };

        var service = new PaymentIntentService();
        var intent  = await service.CreateAsync(options, StripeOptionsForKey(idempotencyKey));
        return intent.ClientSecret;
    }

    public async Task<string> CreateDonationCheckoutAsync(
        decimal amount, string successUrl, string cancelUrl, string? idempotencyKey = null)
    {
        if (amount < 1.00m || amount > 10000.00m)
            throw new InvalidOperationException("Donation amount must be between 1.00 and 10,000.00 EUR.");

        if (!IsStripeConfigured()) return successUrl + "?session_id=test_donation_simulated";

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = ["card"],
            LineItems =
            [
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount  = (long)(amount * 100),
                        Currency    = "eur",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name        = "MamVibe Donation 💛",
                            Description = "Thank you for supporting MamVibe!"
                        }
                    },
                    Quantity = 1
                }
            ],
            Mode       = "payment",
            SuccessUrl = successUrl + "?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl  = cancelUrl,
            Metadata   = new Dictionary<string, string>
            {
                { "type",   "donation" },
                { "amount", amount.ToString("F2") }
            }
        };

        var sessionService = new SessionService();
        var session = await sessionService.CreateAsync(options, StripeOptionsForKey(idempotencyKey));
        return session.Url!;
    }

    public async Task<string> CreateDonationIntentAsync(decimal amount, string? idempotencyKey = null)
    {
        if (amount < 1.00m || amount > 10000.00m)
            throw new InvalidOperationException("Donation amount must be between 1.00 and 10,000.00 EUR.");

        if (!IsStripeConfigured()) return "test_simulated_donation_client_secret";

        var options = new PaymentIntentCreateOptions
        {
            Amount   = (long)(amount * 100),
            Currency = "eur",
            Metadata = new Dictionary<string, string>
            {
                { "type",   "donation" },
                { "amount", amount.ToString("F2") }
            }
        };

        var service = new PaymentIntentService();
        var intent  = await service.CreateAsync(options, StripeOptionsForKey(idempotencyKey));
        return intent.ClientSecret;
    }
}
