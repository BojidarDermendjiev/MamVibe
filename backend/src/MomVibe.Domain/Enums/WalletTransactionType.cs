namespace MomVibe.Domain.Enums;

/// <summary>
/// Indicates the direction of money movement in a wallet transaction.
/// </summary>
public enum WalletTransactionType
{
    /// <summary>
    /// Money enters the wallet (top-up, transfer received, refund).
    /// </summary>
    Credit = 0,

    /// <summary>
    /// Money leaves the wallet (transfer sent, item payment, withdrawal).
    /// </summary>
    Debit = 1
}
