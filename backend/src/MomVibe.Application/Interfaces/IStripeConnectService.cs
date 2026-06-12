namespace MomVibe.Application.Interfaces;

using DTOs.Payments;

/// <summary>
/// Stripe Connect Express integration for peer-to-peer item sellers. Wraps the
/// Stripe SDK calls that create an Express account, generate a hosted onboarding
/// link, reconcile lifecycle state from <c>account.updated</c> webhooks, and mint
/// a login link to the seller's Express dashboard.
/// </summary>
public interface IStripeConnectService
{
    /// <summary>
    /// Returns the user's current Connect status snapshot. Cheap read — does NOT
    /// round-trip to Stripe. Combine with <see cref="RefreshStatusFromStripeAsync"/>
    /// when a fresh authoritative read is required (e.g., immediately after the
    /// user returns from the hosted onboarding flow).
    /// </summary>
    Task<StripeConnectStatusDto> GetStatusAsync(string userId);

    /// <summary>
    /// Idempotently ensures the user has a Stripe Express account, then returns a
    /// short-lived hosted onboarding URL. Creates the account on first call;
    /// subsequent calls reuse the existing account and just mint a new link.
    /// </summary>
    Task<StripeConnectOnboardingLinkDto> CreateOnboardingLinkAsync(
        string userId,
        string returnUrl,
        string refreshUrl);

    /// <summary>
    /// Returns a one-time login link to the seller's Express dashboard. Only valid
    /// for verified accounts; throws if the user has not completed onboarding.
    /// </summary>
    Task<StripeConnectDashboardLinkDto> CreateDashboardLinkAsync(string userId);

    /// <summary>
    /// Fetches the latest account state from Stripe and reconciles
    /// <c>ApplicationUser.StripeConnectStatus</c>. Called after the user returns
    /// from the onboarding redirect and by the <c>account.updated</c> webhook
    /// handler (via <see cref="ApplyAccountUpdateAsync"/>).
    /// </summary>
    Task<StripeConnectStatusDto> RefreshStatusFromStripeAsync(string userId);

    /// <summary>
    /// Applies a Stripe-supplied account snapshot (charges_enabled, payouts_enabled,
    /// requirements) to the matching local <c>ApplicationUser</c>. Invoked by the
    /// webhook handler — keeps the Stripe SDK off the public service surface.
    /// </summary>
    Task ApplyAccountUpdateAsync(string stripeAccountId, bool chargesEnabled, bool payoutsEnabled, bool hasDisabledReason);
}
