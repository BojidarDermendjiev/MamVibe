namespace MomVibe.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

using Enums;
using Common;
using Constants;

/// <summary>
/// Represents a user's digital wallet that holds a balance in a single currency.
/// </summary>
/// <remarks>
/// - Each <see cref="ApplicationUser"/> has at most one wallet (created on first use).
/// - The live balance is never stored directly; it is derived from the running
///   <see cref="WalletTransaction.BalanceAfter"/> snapshot on the latest completed transaction.
/// - Use <see cref="WalletStatus"/> to freeze or suspend the wallet without deleting it.
/// </remarks>
[Index(nameof(UserId), IsUnique = true)]
[Index(nameof(Status))]
public class Wallet : BaseEntity
{
    /// <summary>
    /// Foreign key referencing the wallet owner.
    /// </summary>
    [Required]
    [Comment(WalletConstants.Comments.UserId)]
    public required string UserId { get; set; }

    /// <summary>
    /// ISO 4217 currency code (e.g. "EUR").
    /// </summary>
    [Required]
    [MaxLength(WalletConstants.Lengths.CurrencyMax)]
    [Comment(WalletConstants.Comments.Currency)]
    public string Currency { get; set; } = WalletConstants.Defaults.Currency;

    /// <summary>
    /// Operational state of the wallet.
    /// </summary>
    [Comment(WalletConstants.Comments.Status)]
    public WalletStatus Status { get; set; } = WalletConstants.Defaults.Status;

    /// <summary>
    /// Navigation to the wallet owner.
    /// </summary>
    public ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Ledger entries for this wallet (credits and debits).
    /// </summary>
    public ICollection<WalletTransaction> Transactions { get; set; } = [];

    /// <summary>
    /// Transfers initiated by this wallet (debit side).
    /// </summary>
    public ICollection<WalletTransfer> SentTransfers { get; set; } = [];

    /// <summary>
    /// Transfers received by this wallet (credit side).
    /// </summary>
    public ICollection<WalletTransfer> ReceivedTransfers { get; set; } = [];
}
