namespace MomVibe.Application.DTOs.Payments;

using Domain.Enums;

/// <summary>
/// Snapshot of a user's Stripe Connect Express account state, returned by the status
/// endpoint and used to render the "Bank payouts" card on the dashboard.
/// </summary>
public class StripeConnectStatusDto
{
    /// <summary>Current Connect lifecycle state.</summary>
    public StripeConnectStatus Status { get; set; }

    /// <summary>True when payouts are enabled — equivalent to <c>Status == Verified</c>.</summary>
    public bool CanReceivePayouts { get; set; }

    /// <summary>
    /// True when the user has a Stripe Connect account but onboarding is incomplete —
    /// the frontend renders a "Continue onboarding" CTA instead of "Start onboarding".
    /// </summary>
    public bool HasAccount { get; set; }

    /// <summary>UTC timestamp when the status last changed (for staleness display).</summary>
    public DateTime? StatusUpdatedAt { get; set; }
}

/// <summary>
/// Response from the onboarding endpoint — the frontend redirects (or opens in a new
/// tab) the user to <see cref="OnboardingUrl"/> to complete Stripe's hosted KYC flow.
/// </summary>
public class StripeConnectOnboardingLinkDto
{
    /// <summary>Stripe-hosted onboarding URL — short-lived (~5 min TTL from Stripe).</summary>
    public string OnboardingUrl { get; set; } = string.Empty;
}

/// <summary>
/// Response from the dashboard-link endpoint — only available for fully verified accounts.
/// Opens the seller's personal Stripe Express dashboard (balance, payouts, tax forms).
/// </summary>
public class StripeConnectDashboardLinkDto
{
    /// <summary>Stripe-hosted Express dashboard URL.</summary>
    public string DashboardUrl { get; set; } = string.Empty;
}
