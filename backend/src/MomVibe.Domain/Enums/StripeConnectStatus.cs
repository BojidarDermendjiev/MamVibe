namespace MomVibe.Domain.Enums;

/// <summary>
/// Lifecycle state of a user's Stripe Connect Express account — the destination
/// for peer-to-peer item sale payouts. Reflects Stripe's <c>account.updated</c>
/// signals (charges_enabled / payouts_enabled / requirements.disabled_reason).
/// </summary>
public enum StripeConnectStatus
{
    /// <summary>No Connect account has been created for this user yet.</summary>
    None = 0,

    /// <summary>Connect account exists but onboarding is incomplete — Stripe still requires KYC / bank details.</summary>
    Pending = 1,

    /// <summary>Connect account is fully verified — both charges_enabled and payouts_enabled are true.</summary>
    Verified = 2,

    /// <summary>Account exists but has been restricted by Stripe (requirements past_due, disabled_reason set, etc.).</summary>
    Restricted = 3,
}
