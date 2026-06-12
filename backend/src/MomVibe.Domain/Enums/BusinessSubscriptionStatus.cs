namespace MomVibe.Domain.Enums;

/// <summary>
/// Local mirror of the relevant Stripe subscription statuses applied to a
/// <c>BusinessSubscription</c>. Stripe values not represented here
/// (<c>unpaid</c>, <c>paused</c>) are treated equivalently to <see cref="PastDue"/>
/// or <see cref="Canceled"/> on webhook ingestion.
/// </summary>
public enum BusinessSubscriptionStatus
{
    /// <summary>Stripe checkout completed but the first invoice has not been paid yet.</summary>
    Incomplete = 0,

    /// <summary>Inside the configured trial window; listing is fully active and not billed.</summary>
    Trialing = 1,

    /// <summary>Most recent invoice paid; recurring billing healthy.</summary>
    Active = 2,

    /// <summary>Last invoice failed; grace period applies before the listing is hidden.</summary>
    PastDue = 3,

    /// <summary>Subscription terminated by the user or by Stripe after retries exhausted.</summary>
    Canceled = 4
}
