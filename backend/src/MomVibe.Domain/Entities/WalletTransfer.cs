namespace MomVibe.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

using Enums;
using Common;
using Constants;

/// <summary>
/// Records a user-initiated transfer of funds from one wallet to another.
/// </summary>
/// <remarks>
/// - A completed transfer always has exactly two linked <see cref="WalletTransaction"/> rows:
///   a debit on <see cref="SenderWalletId"/> and a credit on <see cref="ReceiverWalletId"/>.
/// - <see cref="SenderTransactionId"/> and <see cref="ReceiverTransactionId"/> are set
///   atomically alongside the transaction rows inside a serializable DB transaction.
/// - <see cref="InitiatedByIp"/> is stored for fraud monitoring and admin review.
/// </remarks>
[Index(nameof(SenderWalletId))]
[Index(nameof(ReceiverWalletId))]
[Index(nameof(Status))]
[Index(nameof(CreatedAt))]
public class WalletTransfer : BaseEntity
{
    /// <summary>
    /// FK to the wallet that initiated the transfer (debit side).
    /// </summary>
    [Comment(WalletTransferConstants.Comments.SenderWalletId)]
    public Guid SenderWalletId { get; set; }

    /// <summary>
    /// FK to the wallet that receives the funds (credit side).
    /// </summary>
    [Comment(WalletTransferConstants.Comments.ReceiverWalletId)]
    public Guid ReceiverWalletId { get; set; }

    /// <summary>
    /// Amount transferred between wallets.
    /// </summary>
    [Precision(18, 2)]
    [Comment(WalletTransferConstants.Comments.Amount)]
    public decimal Amount { get; set; }

    /// <summary>
    /// ISO 4217 currency code of the transferred amount.
    /// </summary>
    [Required]
    [MaxLength(WalletTransferConstants.Lengths.CurrencyMax)]
    [Comment(WalletTransferConstants.Comments.Currency)]
    public string Currency { get; set; } = WalletTransferConstants.Defaults.Currency;

    /// <summary>
    /// Overall status of the transfer operation.
    /// </summary>
    [Comment(WalletTransferConstants.Comments.Status)]
    public WalletTransferStatus Status { get; set; } = WalletTransferConstants.Defaults.Status;

    /// <summary>
    /// Optional message from the sender shown in the receiver's transaction history.
    /// </summary>
    [MaxLength(WalletTransferConstants.Lengths.NoteMax)]
    [Comment(WalletTransferConstants.Comments.Note)]
    public string? Note { get; set; }

    /// <summary>
    /// IP address of the initiating client, stored for fraud monitoring.
    /// </summary>
    [MaxLength(WalletTransferConstants.Lengths.InitiatedByIpMax)]
    [Comment(WalletTransferConstants.Comments.InitiatedByIp)]
    public string? InitiatedByIp { get; set; }

    /// <summary>
    /// FK to the debit <see cref="WalletTransaction"/> created for the sender.
    /// </summary>
    [Comment(WalletTransferConstants.Comments.SenderTransactionId)]
    public Guid? SenderTransactionId { get; set; }

    /// <summary>
    /// FK to the credit <see cref="WalletTransaction"/> created for the receiver.
    /// </summary>
    [Comment(WalletTransferConstants.Comments.ReceiverTransactionId)]
    public Guid? ReceiverTransactionId { get; set; }

    /// <summary>
    /// Navigation to the sender wallet.
    /// </summary>
    public Wallet SenderWallet { get; set; } = null!;

    /// <summary>
    /// Navigation to the receiver wallet.
    /// </summary>
    public Wallet ReceiverWallet { get; set; } = null!;

    /// <summary>
    /// Navigation to the debit transaction on the sender's ledger.
    /// </summary>
    public WalletTransaction? SenderTransaction { get; set; }

    /// <summary>
    /// Navigation to the credit transaction on the receiver's ledger.
    /// </summary>
    public WalletTransaction? ReceiverTransaction { get; set; }
}
