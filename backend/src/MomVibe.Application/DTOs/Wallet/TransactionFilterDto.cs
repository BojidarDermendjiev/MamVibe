namespace MomVibe.Application.DTOs.Wallet;

using Domain.Enums;

/// <summary>
/// Admin filter parameters for querying wallet transactions across all wallets.
/// All fields are optional — omitting a field means no filter on that dimension.
/// </summary>
public class TransactionFilterDto
{
    public string? UserId { get; set; }
    public Guid? WalletId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public WalletTransactionKind? Kind { get; set; }
    public WalletTransactionStatus? Status { get; set; }
    public WalletTransactionType? Type { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
