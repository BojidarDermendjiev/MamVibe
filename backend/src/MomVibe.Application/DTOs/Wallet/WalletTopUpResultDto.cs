namespace MomVibe.Application.DTOs.Wallet;

/// <summary>
/// Returned after creating a Stripe PaymentIntent for a wallet top-up.
/// The frontend uses ClientSecret to render the Stripe Elements card form inline
/// (no redirect — user stays on the page).
/// </summary>
public class WalletTopUpResultDto
{
    /// <summary>Stripe PaymentIntent client secret passed to Stripe.js on the frontend.</summary>
    public required string ClientSecret { get; set; }

    /// <summary>Amount that will be charged in EUR.</summary>
    public decimal Amount { get; set; }

    public string Currency { get; set; } = "EUR";
}
