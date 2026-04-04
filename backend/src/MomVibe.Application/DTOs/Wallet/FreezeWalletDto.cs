namespace MomVibe.Application.DTOs.Wallet;

/// <summary>
/// Request body for freezing a wallet. The reason is stored on the wallet's
/// latest status-change transaction for audit purposes.
/// </summary>
public class FreezeWalletDto
{
    /// <summary>Human-readable reason for the freeze, stored in the audit log.</summary>
    public required string Reason { get; set; }
}
