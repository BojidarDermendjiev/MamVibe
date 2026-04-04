namespace MomVibe.Application.DTOs.Wallet;

/// <summary>
/// Request body for withdrawing funds from the wallet to the user's registered IBAN.
/// The IBAN is read from the user's profile — no need to supply it in the request.
/// Withdrawal is queued for admin review and does not immediately deduct the balance.
/// </summary>
public class WithdrawRequestDto
{
    /// <summary>Amount to withdraw in EUR. Min 1, max 50 000.</summary>
    public decimal Amount { get; set; }
}
