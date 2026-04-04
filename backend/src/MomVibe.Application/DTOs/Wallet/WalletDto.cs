namespace MomVibe.Application.DTOs.Wallet;

using Domain.Enums;

/// <summary>
/// Wallet summary returned to the owning user.
/// Balance is always computed from the latest WalletTransaction.BalanceAfter snapshot —
/// never stored as a mutable field on the wallet itself.
/// </summary>
public class WalletDto
{
    public Guid Id { get; set; }
    public required string UserId { get; set; }
    public string Currency { get; set; } = "EUR";
    public WalletStatus Status { get; set; }
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
}
