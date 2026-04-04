namespace MomVibe.Application.DTOs.Wallet;

/// <summary>
/// Request body for starting a wallet top-up via Stripe PaymentIntent.
/// </summary>
public class TopUpRequestDto
{
    /// <summary>Amount to deposit in EUR. Min 1, max 5000.</summary>
    public decimal Amount { get; set; }
}
