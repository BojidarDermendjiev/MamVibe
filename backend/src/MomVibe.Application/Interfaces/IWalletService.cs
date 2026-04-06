namespace MomVibe.Application.Interfaces;

using Domain.Enums;
using DTOs.Common;
using DTOs.Wallet;

/// <summary>
/// Wallet service contract covering the full lifecycle of user wallet operations
/// and admin monitoring/control capabilities.
///
/// Key design rules enforced by all implementations:
/// - Balance is never stored on the Wallet entity; it is derived from WalletTransaction.BalanceAfter.
/// - All debit/credit mutations run inside a serializable DB transaction to prevent race conditions.
/// - TakeANap fiscal receipts are generated automatically (fire-and-forget) for every credit event.
/// - WalletFrozenException or InsufficientFundsException are thrown for user-facing rule violations.
/// </summary>
public interface IWalletService
{
    // -------------------------------------------------------------------------
    // User operations
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the wallet for the given user, creating one automatically on first call.
    /// </summary>
    Task<WalletDto> GetOrCreateWalletAsync(string userId);

    /// <summary>
    /// Creates a Stripe PaymentIntent for a wallet top-up and returns the client secret
    /// so the frontend can render the Stripe Elements card form inline.
    /// A pending WalletTransaction is created immediately; it is completed by
    /// <see cref="HandleTopUpWebhookAsync"/> when Stripe confirms payment.
    /// </summary>
    Task<WalletTopUpResultDto> CreateTopUpIntentAsync(string userId, decimal amount);

    /// <summary>
    /// Processes an incoming Stripe webhook payload for top-up events.
    /// On payment_intent.succeeded: credits the wallet, updates the pending transaction to
    /// Completed, and fires TakeANap receipt creation in the background.
    /// </summary>
    Task HandleTopUpWebhookAsync(string json, string stripeSignature);

    /// <summary>
    /// Transfers funds from the sender's wallet to the receiver identified by email.
    /// Runs as a serializable DB transaction (double-entry: debit sender, credit receiver).
    /// TakeANap receipt is fired for the receiver's credit transaction.
    /// Throws <see cref="Domain.Exceptions.WalletFrozenException"/> if either wallet is not Active.
    /// Throws <see cref="Domain.Exceptions.InsufficientFundsException"/> if balance is too low.
    /// </summary>
    Task<WalletTransferDto> TransferAsync(
        string senderUserId,
        string receiverEmail,
        decimal amount,
        string? note,
        string? initiatedByIp);

    /// <summary>
    /// Pays for a marketplace item directly from the buyer's wallet balance.
    /// Creates a Payment record (Method = Wallet, Status = Pending) and debits the buyer's
    /// wallet atomically — funds are held in escrow until delivery is confirmed.
    /// Throws KeyNotFoundException if item not found.
    /// Throws InvalidOperationException if item is not for sale.
    /// Throws WalletFrozenException / InsufficientFundsException on wallet guards.
    /// </summary>
    Task<WalletTransactionDto> PayForItemFromWalletAsync(string buyerUserId, Guid itemId);

    /// <summary>
    /// Confirms delivery of a wallet-paid item, releasing escrowed funds to the seller.
    /// Only the buyer of the payment may call this. Creates a Credit transaction on the
    /// seller's wallet and marks the Payment as Completed.
    /// Throws KeyNotFoundException if payment not found.
    /// Throws DomainException if the caller is not the buyer, the payment is not a pending
    /// wallet payment, or the seller's wallet is unavailable.
    /// </summary>
    Task<WalletTransactionDto> ConfirmDeliveryAsync(Guid paymentId, string buyerUserId);

    /// <summary>
    /// Queues a withdrawal request (debit) to the user's registered IBAN.
    /// The balance is reserved immediately (debit with Pending status).
    /// An admin must approve or reject via <see cref="ApproveWithdrawalAsync"/> /
    /// <see cref="RejectWithdrawalAsync"/>.
    /// Throws if the user has no IBAN registered or balance is insufficient.
    /// </summary>
    Task<WalletTransactionDto> WithdrawAsync(string userId, decimal amount);

    /// <summary>
    /// Returns paginated transaction history for the authenticated user's wallet.
    /// </summary>
    Task<PagedResult<WalletTransactionDto>> GetTransactionsAsync(string userId, int page, int pageSize);

    // -------------------------------------------------------------------------
    // Admin operations
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns all wallets with optional status filter, for admin monitoring.
    /// </summary>
    Task<PagedResult<AdminWalletDto>> GetAllWalletsAsync(int page, int pageSize, WalletStatus? status = null);

    /// <summary>
    /// Returns a single wallet by its ID, for admin detail view.
    /// </summary>
    Task<AdminWalletDto> GetWalletByIdAsync(Guid walletId);

    /// <summary>
    /// Sets the wallet status to Frozen. All operations on the wallet will be blocked.
    /// </summary>
    Task FreezeWalletAsync(Guid walletId, string reason);

    /// <summary>
    /// Restores a Frozen or Suspended wallet to Active status.
    /// </summary>
    Task UnfreezeWalletAsync(Guid walletId);

    /// <summary>
    /// Returns paginated transactions across all wallets with optional filters,
    /// for admin transaction monitoring.
    /// </summary>
    Task<PagedResult<WalletTransactionDto>> GetAllTransactionsAsync(TransactionFilterDto filter);

    /// <summary>
    /// Reverses a completed transaction by crediting the affected wallet.
    /// Creates a new Reversed WalletTransaction and fires TakeANap receipt.
    /// </summary>
    Task<WalletTransactionDto> RefundTransactionAsync(Guid transactionId, string adminUserId, string reason);

    /// <summary>
    /// Returns all pending withdrawal transactions awaiting admin processing.
    /// </summary>
    Task<PagedResult<WalletTransactionDto>> GetPendingWithdrawalsAsync(int page, int pageSize);

    /// <summary>
    /// Marks a withdrawal transaction as Completed (funds sent to IBAN by admin).
    /// </summary>
    Task ApproveWithdrawalAsync(Guid transactionId, string adminUserId);

    /// <summary>
    /// Rejects a withdrawal request: cancels the pending debit and returns the funds to the wallet.
    /// </summary>
    Task RejectWithdrawalAsync(Guid transactionId, string adminUserId, string reason);
}
