namespace MomVibe.Application.Interfaces;

using DTOs.Business;

/// <summary>
/// Stripe-backed subscription lifecycle for <c>BusinessProfile</c>. Encapsulates plan
/// listing, checkout session creation (with the card-on-file trial), Billing Portal links,
/// cancellation, and webhook event ingestion. Webhook entry-point is reachable via
/// <see cref="HandleStripeEventAsync"/> from the central Stripe webhook controller.
/// </summary>
public interface IBusinessSubscriptionService
{
    /// <summary>Returns the available plans for the plan-selector UI.</summary>
    Task<IEnumerable<SubscriptionPlanDto>> GetPlansAsync();

    /// <summary>Returns the calling user's subscription, or null when not yet subscribed.</summary>
    Task<BusinessSubscriptionDto?> GetMineAsync(string userId);

    /// <summary>
    /// Creates a Stripe Checkout session in <c>subscription</c> mode and returns the redirect URL.
    /// The Trial plan adds a 7-day trial window and forces card collection. Throws 409 if the
    /// business profile is missing, or if a Stripe Price id is not configured for the chosen plan.
    /// </summary>
    Task<string> CreateCheckoutUrlAsync(string userId, CreateSubscriptionCheckoutRequest request);

    /// <summary>Returns a one-time Stripe Customer Portal URL for self-serve plan changes / cancellation.</summary>
    Task<string> CreateBillingPortalUrlAsync(string userId, CreateBillingPortalRequest request);

    /// <summary>
    /// Cancels the calling user's subscription. <paramref name="atPeriodEnd"/>=true defers the
    /// cancellation until the current period ends (keeps the listing visible until then).
    /// </summary>
    Task CancelAsync(string userId, bool atPeriodEnd);

    /// <summary>
    /// Applies a Stripe webhook event to the local <c>BusinessSubscription</c> ledger.
    /// Idempotent — duplicate event ids are ignored via the unique index on
    /// <c>BusinessSubscriptionEvent.StripeEventId</c>. Events are accepted as object so this
    /// service does not impose a Stripe SDK reference on the Application layer.
    /// </summary>
    Task HandleStripeEventAsync(object stripeEvent);
}
