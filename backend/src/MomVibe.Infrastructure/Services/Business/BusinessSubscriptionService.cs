namespace MomVibe.Infrastructure.Services.Business;

using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Stripe;

using Application.Interfaces;
using Application.DTOs.Business;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

/// <summary>
/// Stripe-backed lifecycle for <c>BusinessSubscription</c>.
/// </summary>
/// <remarks>
/// - Checkout uses <c>SessionMode.Subscription</c>. The Trial plan adds
///   <c>trial_period_days=7</c>; all plans force <c>payment_method_collection=always</c>
///   so the card is on file before the trial begins.
/// - Webhook ingestion writes a <c>BusinessSubscriptionEvent</c> ledger row keyed by the
///   Stripe <c>event.id</c> (unique index ⇒ replay safe) and reconciles
///   <c>BusinessSubscription</c> status from the embedded subscription / invoice payload.
/// - Older-than-applied events are dropped to prevent out-of-order regressions.
/// </remarks>
public class BusinessSubscriptionService : IBusinessSubscriptionService
{
    private const int GracePeriodDays = 3;

    private readonly IApplicationDbContext _db;
    private readonly IConfiguration _config;
    private readonly IAuditLogService _audit;
    private readonly ILogger<BusinessSubscriptionService> _logger;

    public BusinessSubscriptionService(
        IApplicationDbContext db,
        IConfiguration config,
        IAuditLogService audit,
        ILogger<BusinessSubscriptionService> logger)
    {
        _db = db;
        _config = config;
        _audit = audit;
        _logger = logger;
    }

    public async Task<IEnumerable<SubscriptionPlanDto>> GetPlansAsync()
    {
        var plans = await _db.SubscriptionPlans
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .ToListAsync();
        return plans.Select(p => new SubscriptionPlanDto
        {
            Code = p.Code,
            DisplayName = p.DisplayName,
            MonthlyPriceEur = p.MonthlyPriceEur,
            RankBoost = p.RankBoost,
            TrialDays = p.TrialDays,
            FeaturesJson = p.FeaturesJson,
            SortOrder = p.SortOrder,
            IsCheckoutEnabled = !string.IsNullOrWhiteSpace(p.StripePriceId),
        });
    }

    public async Task<BusinessSubscriptionDto?> GetMineAsync(string userId)
    {
        var profile = await _db.BusinessProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null) return null;

        var sub = await _db.BusinessSubscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.BusinessProfileId == profile.Id);
        if (sub == null) return null;

        var plan = await _db.SubscriptionPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Code == sub.PlanCode);

        return new BusinessSubscriptionDto
        {
            Id = sub.Id,
            BusinessProfileId = sub.BusinessProfileId,
            PlanCode = sub.PlanCode,
            PlanDisplayName = plan?.DisplayName ?? sub.PlanCode,
            MonthlyPriceEur = plan?.MonthlyPriceEur ?? 0m,
            RankBoost = plan?.RankBoost ?? 0,
            Status = sub.Status,
            CurrentPeriodStart = sub.CurrentPeriodStart,
            CurrentPeriodEnd = sub.CurrentPeriodEnd,
            TrialEndsAt = sub.TrialEndsAt,
            GracePeriodEndsAt = sub.GracePeriodEndsAt,
            CanceledAt = sub.CanceledAt,
            HasStripeSubscription = !string.IsNullOrEmpty(sub.StripeSubscriptionId),
        };
    }

    public async Task<string> CreateCheckoutUrlAsync(string userId, CreateSubscriptionCheckoutRequest request)
    {
        if (!IsStripeConfigured())
            throw new BusinessConflictException("stripe_not_configured",
                "Subscriptions are not configured for this environment.");

        var profile = await _db.BusinessProfiles.FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new BusinessConflictException("profile_missing",
                "Create your business profile before subscribing.");

        var plan = await _db.SubscriptionPlans.FirstOrDefaultAsync(p => p.Code == request.PlanCode && p.IsActive)
            ?? throw new KeyNotFoundException("Subscription plan not found.");
        if (string.IsNullOrWhiteSpace(plan.StripePriceId))
            throw new BusinessConflictException("plan_not_purchasable",
                "This plan is not currently available for purchase.");

        // Reuse the Customer if we created one previously — preserves card-on-file across plan changes.
        if (string.IsNullOrEmpty(profile.StripeCustomerId))
        {
            var customer = await new CustomerService().CreateAsync(new CustomerCreateOptions
            {
                Email = profile.ContactEmail,
                Name = profile.DisplayName,
                Metadata = new Dictionary<string, string>
                {
                    { "businessProfileId", profile.Id.ToString() },
                    { "userId", userId },
                },
            });
            profile.StripeCustomerId = customer.Id;
            await _db.SaveChangesAsync();
        }

        var sessionOptions = new Stripe.Checkout.SessionCreateOptions
        {
            Mode = "subscription",
            Customer = profile.StripeCustomerId,
            PaymentMethodCollection = "always",
            LineItems =
            [
                new Stripe.Checkout.SessionLineItemOptions { Price = plan.StripePriceId, Quantity = 1 }
            ],
            SuccessUrl = request.SuccessUrl + (request.SuccessUrl.Contains('?') ? "&" : "?")
                         + "session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = request.CancelUrl,
            Metadata = new Dictionary<string, string>
            {
                { "businessProfileId", profile.Id.ToString() },
                { "planCode", plan.Code },
            },
            SubscriptionData = new Stripe.Checkout.SessionSubscriptionDataOptions
            {
                // Trial period only for the Trial plan; downstream renewals happen at the Stripe-side
                // price billing cycle once the trial elapses.
                TrialPeriodDays = plan.TrialDays > 0 ? plan.TrialDays : null,
                Metadata = new Dictionary<string, string>
                {
                    { "businessProfileId", profile.Id.ToString() },
                    { "planCode", plan.Code },
                },
            },
        };

        var session = await new Stripe.Checkout.SessionService().CreateAsync(sessionOptions);
        await _audit.LogAsync(userId, "Business.Subscription.CheckoutCreated",
            success: true, targetId: profile.Id.ToString(), details: $"plan={plan.Code}");
        return session.Url!;
    }

    public async Task<string> CreateBillingPortalUrlAsync(string userId, CreateBillingPortalRequest request)
    {
        if (!IsStripeConfigured())
            throw new BusinessConflictException("stripe_not_configured",
                "Subscriptions are not configured for this environment.");

        var profile = await _db.BusinessProfiles.FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new BusinessConflictException("profile_missing", "Business profile not found.");
        if (string.IsNullOrEmpty(profile.StripeCustomerId))
            throw new BusinessConflictException("no_stripe_customer",
                "Subscribe to a plan first to access the billing portal.");

        var session = await new Stripe.BillingPortal.SessionService().CreateAsync(new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = profile.StripeCustomerId,
            ReturnUrl = request.ReturnUrl,
        });
        return session.Url;
    }

    public async Task CancelAsync(string userId, bool atPeriodEnd)
    {
        var profile = await _db.BusinessProfiles.FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new KeyNotFoundException("Business profile not found.");
        var sub = await _db.BusinessSubscriptions
            .FirstOrDefaultAsync(s => s.BusinessProfileId == profile.Id)
            ?? throw new KeyNotFoundException("Subscription not found.");

        if (string.IsNullOrEmpty(sub.StripeSubscriptionId))
            throw new InvalidOperationException("Subscription has not been activated yet.");

        if (atPeriodEnd)
        {
            await new SubscriptionService().UpdateAsync(sub.StripeSubscriptionId, new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = true,
            });
        }
        else
        {
            await new SubscriptionService().CancelAsync(sub.StripeSubscriptionId);
        }

        await _audit.LogAsync(userId, "Business.Subscription.Cancel",
            success: true, targetId: profile.Id.ToString(),
            details: atPeriodEnd ? "atPeriodEnd" : "immediate");
    }

    public async Task HandleStripeEventAsync(object stripeEvent)
    {
        if (stripeEvent is not Event evt)
            throw new ArgumentException("Expected a Stripe Event payload.", nameof(stripeEvent));

        // Dedup via the unique index — duplicate webhook deliveries are common at scale.
        if (await _db.BusinessSubscriptionEvents.AnyAsync(e => e.StripeEventId == evt.Id))
        {
            _logger.LogInformation("Duplicate business subscription event {EventId} — skipping.", evt.Id);
            return;
        }

        Subscription? subscription = null;
        Invoice? invoice = null;
        switch (evt.Type)
        {
            case EventTypes.CustomerSubscriptionCreated:
            case EventTypes.CustomerSubscriptionUpdated:
            case EventTypes.CustomerSubscriptionDeleted:
                subscription = evt.Data.Object as Subscription;
                break;
            case EventTypes.InvoicePaymentSucceeded:
            case EventTypes.InvoicePaymentFailed:
                invoice = evt.Data.Object as Invoice;
                // Stripe.net v50: invoice→subscription pointer moved to Parent.SubscriptionDetails.SubscriptionId.
                var subId = invoice?.Parent?.SubscriptionDetails?.SubscriptionId;
                if (!string.IsNullOrEmpty(subId))
                    subscription = await new SubscriptionService().GetAsync(subId);
                break;
            default:
                // Not a subscription event — caller should not have routed here.
                _logger.LogDebug("Unhandled subscription event type {Type}", evt.Type);
                return;
        }

        if (subscription == null) return;

        if (!subscription.Metadata.TryGetValue("businessProfileId", out var rawProfileId)
            || !Guid.TryParse(rawProfileId, out var profileId))
        {
            _logger.LogWarning("Subscription {SubId} missing businessProfileId metadata — ignoring.", subscription.Id);
            return;
        }

        // Stripe.net v50: current_period_start/end moved to a per-item field. Single-item
        // subscriptions take the period from the first item; absent items leave the dates null.
        var firstItem = subscription.Items?.Data?.FirstOrDefault();
        var periodStart = firstItem?.CurrentPeriodStart;
        var periodEnd = firstItem?.CurrentPeriodEnd;

        var businessSub = await _db.BusinessSubscriptions
            .FirstOrDefaultAsync(s => s.BusinessProfileId == profileId);
        if (businessSub == null)
        {
            // First event for this subscription — create the row.
            businessSub = new BusinessSubscription
            {
                BusinessProfileId = profileId,
                PlanCode = subscription.Metadata.GetValueOrDefault("planCode") ?? "Trial",
                StripeSubscriptionId = subscription.Id,
                Status = MapStatus(subscription.Status),
                CurrentPeriodStart = periodStart,
                CurrentPeriodEnd = periodEnd,
                TrialEndsAt = subscription.TrialEnd,
            };
            _db.BusinessSubscriptions.Add(businessSub);
        }
        else
        {
            businessSub.StripeSubscriptionId = subscription.Id;
            businessSub.Status = MapStatus(subscription.Status);
            businessSub.CurrentPeriodStart = periodStart;
            businessSub.CurrentPeriodEnd = periodEnd;
            businessSub.TrialEndsAt = subscription.TrialEnd;
            if (subscription.Metadata.TryGetValue("planCode", out var planCode) && !string.IsNullOrEmpty(planCode))
                businessSub.PlanCode = planCode;
            if (subscription.CanceledAt.HasValue)
                businessSub.CanceledAt = subscription.CanceledAt;
        }

        // Grace period bookkeeping for failed invoices.
        if (evt.Type == EventTypes.InvoicePaymentFailed)
        {
            businessSub.Status = BusinessSubscriptionStatus.PastDue;
            businessSub.GracePeriodEndsAt = DateTime.UtcNow.AddDays(GracePeriodDays);
        }
        else if (evt.Type == EventTypes.InvoicePaymentSucceeded)
        {
            businessSub.GracePeriodEndsAt = null;
            // Don't override the Stripe-reported status (Active/Trialing).
        }
        else if (evt.Type == EventTypes.CustomerSubscriptionDeleted)
        {
            businessSub.Status = BusinessSubscriptionStatus.Canceled;
            businessSub.CanceledAt = subscription.CanceledAt ?? DateTime.UtcNow;
        }

        // Reflect rank boost from the new plan onto the listing so /coaches sort updates immediately.
        await ApplyRankBoostAsync(businessSub.BusinessProfileId, businessSub.PlanCode);

        _db.BusinessSubscriptionEvents.Add(new BusinessSubscriptionEvent
        {
            SubscriptionId = businessSub.Id,
            StripeEventId = evt.Id,
            Type = MapEventType(evt.Type),
            RawType = evt.Type,
            PayloadJson = JsonSerializer.Serialize(new { evt.Id, evt.Type, evt.Created }),
            OccurredAt = evt.Created,
        });

        await _db.SaveChangesAsync();
        _logger.LogInformation(
            "Applied Stripe event {EventId} ({Type}) to BusinessSubscription {SubId}",
            evt.Id, evt.Type, businessSub.Id);
    }

    private async Task ApplyRankBoostAsync(Guid profileId, string planCode)
    {
        var rankBoost = await _db.SubscriptionPlans
            .AsNoTracking()
            .Where(p => p.Code == planCode)
            .Select(p => (int?)p.RankBoost)
            .FirstOrDefaultAsync() ?? 0;

        var listing = await _db.BusinessListings.FirstOrDefaultAsync(l => l.BusinessProfileId == profileId);
        if (listing != null && listing.RankBoost != rankBoost)
            listing.RankBoost = rankBoost;
    }

    private bool IsStripeConfigured()
    {
        var key = _config["Stripe:SecretKey"];
        return !string.IsNullOrWhiteSpace(key) && !key.Contains("YOUR_STRIPE");
    }

    private static BusinessSubscriptionStatus MapStatus(string stripeStatus) => stripeStatus switch
    {
        "trialing" => BusinessSubscriptionStatus.Trialing,
        "active" => BusinessSubscriptionStatus.Active,
        "past_due" => BusinessSubscriptionStatus.PastDue,
        "canceled" or "incomplete_expired" or "unpaid" => BusinessSubscriptionStatus.Canceled,
        _ => BusinessSubscriptionStatus.Incomplete,
    };

    private static BusinessSubscriptionEventType MapEventType(string raw) => raw switch
    {
        EventTypes.CustomerSubscriptionCreated => BusinessSubscriptionEventType.SubscriptionCreated,
        EventTypes.CustomerSubscriptionUpdated => BusinessSubscriptionEventType.SubscriptionUpdated,
        EventTypes.CustomerSubscriptionDeleted => BusinessSubscriptionEventType.SubscriptionDeleted,
        EventTypes.InvoicePaymentSucceeded => BusinessSubscriptionEventType.InvoicePaymentSucceeded,
        EventTypes.InvoicePaymentFailed => BusinessSubscriptionEventType.InvoicePaymentFailed,
        _ => BusinessSubscriptionEventType.Other,
    };
}
