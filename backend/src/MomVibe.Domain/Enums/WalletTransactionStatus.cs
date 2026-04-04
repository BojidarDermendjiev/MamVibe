namespace MomVibe.Domain.Enums;

/// <summary>
/// Represents the lifecycle state of an individual wallet transaction.
/// </summary>
public enum WalletTransactionStatus
{
    /// <summary>
    /// Transaction is initiated but not yet settled (e.g., waiting on Stripe webhook).
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Transaction is fully settled and balance has been updated.
    /// </summary>
    Completed = 1,

    /// <summary>
    /// Transaction failed and no balance change occurred.
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Transaction was reversed after completion (admin refund or chargeback).
    /// </summary>
    Reversed = 3
}
