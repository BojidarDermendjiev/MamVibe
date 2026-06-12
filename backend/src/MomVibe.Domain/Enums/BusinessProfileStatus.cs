namespace MomVibe.Domain.Enums;

/// <summary>
/// Lifecycle state of a <c>BusinessProfile</c> from registration through optional
/// admin suspension or removal. Distinct from <c>BusinessSubscriptionStatus</c>
/// (which tracks Stripe billing) so the platform can show, hide, or moderate a
/// profile independently of payment state.
/// </summary>
public enum BusinessProfileStatus
{
    /// <summary>Profile created but the current platform policy has not been accepted yet.</summary>
    PendingPolicy = 0,

    /// <summary>Policy accepted; awaiting first successful Stripe subscription checkout.</summary>
    PendingPayment = 1,

    /// <summary>Profile is fully onboarded and may operate a listing.</summary>
    Active = 2,

    /// <summary>Subscription has lapsed; profile remains visible for grace period.</summary>
    PastDue = 3,

    /// <summary>Admin has suspended the profile; listing hidden, dashboard read-only.</summary>
    Suspended = 4,

    /// <summary>Admin has removed the profile permanently; treated as soft-delete for audit.</summary>
    Removed = 5
}
