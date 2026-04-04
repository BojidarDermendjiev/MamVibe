namespace MomVibe.Application.DTOs.Wallet;

using Domain.Enums;

/// <summary>
/// Represents a completed or pending user-to-user wallet transfer.
/// </summary>
public class WalletTransferDto
{
    public Guid Id { get; set; }
    public Guid SenderWalletId { get; set; }
    public Guid ReceiverWalletId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EUR";
    public WalletTransferStatus Status { get; set; }
    public string? Note { get; set; }
    public string? SenderDisplayName { get; set; }
    public string? ReceiverDisplayName { get; set; }
    public DateTime CreatedAt { get; set; }
}
