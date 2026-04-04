namespace MomVibe.Application.DTOs.Wallet;

/// <summary>
/// Request body for sending money from the authenticated user's wallet to another user.
/// The receiver is identified by email (user-friendly, like Revolut).
/// </summary>
public class TransferRequestDto
{
    /// <summary>Email of the user to send money to.</summary>
    public required string ReceiverEmail { get; set; }

    /// <summary>Amount to transfer in EUR. Min 0.01, max 10 000.</summary>
    public decimal Amount { get; set; }

    /// <summary>Optional note visible to the receiver in their transaction history.</summary>
    public string? Note { get; set; }
}
