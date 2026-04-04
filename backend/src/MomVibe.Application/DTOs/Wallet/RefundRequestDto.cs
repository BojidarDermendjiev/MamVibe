namespace MomVibe.Application.DTOs.Wallet;

/// <summary>
/// Request body for an admin-initiated refund of a completed wallet transaction.
/// </summary>
public class RefundRequestDto
{
    /// <summary>Reason for the refund, stored in the audit log.</summary>
    public required string Reason { get; set; }
}
