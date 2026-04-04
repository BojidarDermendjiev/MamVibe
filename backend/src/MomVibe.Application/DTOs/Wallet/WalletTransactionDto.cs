namespace MomVibe.Application.DTOs.Wallet;

using Domain.Enums;

/// <summary>
/// Represents a single immutable ledger entry on a wallet.
/// Returned in transaction history for both user and admin views.
/// </summary>
public class WalletTransactionDto
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public WalletTransactionType Type { get; set; }
    public WalletTransactionKind Kind { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public WalletTransactionStatus Status { get; set; }
    public string? Reference { get; set; }
    public string? Description { get; set; }
    public Guid? RelatedTransactionId { get; set; }
    public Guid? PaymentId { get; set; }
    public string? ReceiptUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
