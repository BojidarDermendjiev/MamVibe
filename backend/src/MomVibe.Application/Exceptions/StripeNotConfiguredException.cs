namespace MomVibe.Application.Exceptions;

/// <summary>
/// Thrown by Stripe-backed services when <c>Stripe:SecretKey</c> is missing or
/// still set to a placeholder value. Controllers map this to HTTP 503 so the
/// frontend can surface a clear "payments are unavailable" message instead of
/// silently failing inside Stripe.js with an invalid client-secret error.
/// </summary>
public sealed class StripeNotConfiguredException : Exception
{
    public StripeNotConfiguredException()
        : base("Stripe is not configured on the server.") { }
}
