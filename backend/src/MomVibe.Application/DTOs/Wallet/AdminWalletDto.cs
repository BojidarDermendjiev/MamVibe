namespace MomVibe.Application.DTOs.Wallet;

using Domain.Enums;

/// <summary>
/// Extended wallet view for admin monitoring.
/// Includes user identity fields and transaction count for the admin dashboard.
/// </summary>
public class AdminWalletDto
{
    public Guid Id { get; set; }
    public required string UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserDisplayName { get; set; }
    public string Currency { get; set; } = "EUR";
    public WalletStatus Status { get; set; }
    public decimal Balance { get; set; }
    public int TransactionCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
