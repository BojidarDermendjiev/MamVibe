namespace MomVibe.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

using Enums;
using Common;
using Constants;

/// <summary>
/// Immutable ledger entry that records a single money movement on a wallet.
/// </summary>
/// <remarks>
/// - Every financial event (top-up, transfer leg, item payment, withdrawal, refund) produces
///   exactly one <see cref="WalletTransaction"/> row.
/// - Transfers produce two rows (double-entry): a <see cref="WalletTransactionType.Debit"/> on
///   the sender and a <see cref="WalletTransactionType.Credit"/> on the receiver, linked via
///   <see cref="RelatedTransactionId"/>.
/// - <see cref="BalanceAfter"/> is a point-in-time snapshot that enables O(1) balance reads
///   without summing the entire ledger.
/// - <see cref="ReceiptUrl"/> is populated automatically (fire-and-forget) by
///   <c>ITakeANapService</c> for all credit-side taxable events.
/// </remarks>
[Index(nameof(WalletId))]
[Index(nameof(Kind))]
[Index(nameof(Status))]
[Index(nameof(CreatedAt))]
[Index(nameof(WalletId), nameof(CreatedAt))]
public class WalletTransaction : BaseEntity
{
    /// <summary>
    /// Foreign key to the wallet this transaction belongs to.
    /// </summary>
    [Comment(WalletTransactionConstants.Comments.WalletId)]
    public Guid WalletId { get; set; }

    /// <summary>
    /// Direction of money movement.
    /// </summary>
    [Comment(WalletTransactionConstants.Comments.Type)]
    public WalletTransactionType Type { get; set; }

    /// <summary>
    /// Business reason for this transaction.
    /// </summary>
    [Comment(WalletTransactionConstants.Comments.Kind)]
    public WalletTransactionKind Kind { get; set; }

    /// <summary>
    /// Absolute amount of this transaction (always positive).
    /// </summary>
    [Precision(18, 2)]
    [Comment(WalletTransactionConstants.Comments.Amount)]
    public decimal Amount { get; set; }

    /// <summary>
    /// Wallet balance snapshot immediately after this transaction was applied.
    /// </summary>
    [Precision(18, 2)]
    [Comment(WalletTransactionConstants.Comments.BalanceAfter)]
    public decimal BalanceAfter { get; set; }

    /// <summary>
    /// Settlement state of this transaction.
    /// </summary>
    [Comment(WalletTransactionConstants.Comments.Status)]
    public WalletTransactionStatus Status { get; set; } = WalletTransactionConstants.Defaults.Status;

    /// <summary>
    /// External reference (e.g. Stripe PaymentIntent ID, WalletTransfer ID).
    /// </summary>
    [MaxLength(WalletTransactionConstants.Lengths.ReferenceMax)]
    [Comment(WalletTransactionConstants.Comments.Reference)]
    public string? Reference { get; set; }

    /// <summary>
    /// Human-readable description shown in transaction history.
    /// </summary>
    [MaxLength(WalletTransactionConstants.Lengths.DescriptionMax)]
    [Comment(WalletTransactionConstants.Comments.Description)]
    public string? Description { get; set; }

    /// <summary>
    /// ID of the counterpart transaction in a double-entry transfer.
    /// Null for single-leg events (top-up, withdrawal, refund).
    /// </summary>
    [Comment(WalletTransactionConstants.Comments.RelatedTransactionId)]
    public Guid? RelatedTransactionId { get; set; }

    /// <summary>
    /// FK to the marketplace <see cref="Payment"/> when <see cref="Kind"/> is
    /// <see cref="WalletTransactionKind.ItemPayment"/>.
    /// </summary>
    [Comment(WalletTransactionConstants.Comments.PaymentId)]
    public Guid? PaymentId { get; set; }

    /// <summary>
    /// URL to the TakeANap fiscal receipt, populated automatically after creation.
    /// Null for debit-side transactions and events that do not require a receipt.
    /// </summary>
    [MaxLength(WalletTransactionConstants.Lengths.ReceiptUrlMax)]
    [Comment(WalletTransactionConstants.Comments.ReceiptUrl)]
    public string? ReceiptUrl { get; set; }

    /// <summary>
    /// Navigation to the parent wallet.
    /// </summary>
    public Wallet Wallet { get; set; } = null!;

    /// <summary>
    /// Navigation to the related marketplace payment (when Kind is ItemPayment).
    /// </summary>
    public Payment? Payment { get; set; }
}
