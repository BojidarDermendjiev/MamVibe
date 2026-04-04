namespace MomVibe.Domain.Enums;

/// <summary>
/// Categorizes the business reason behind a wallet transaction.
/// </summary>
public enum WalletTransactionKind
{
    /// <summary>
    /// Funds deposited into the wallet via card (Stripe).
    /// Generates a TakeANap fiscal receipt automatically.
    /// </summary>
    TopUp = 0,

    /// <summary>
    /// Funds moved between two user wallets.
    /// TakeANap receipt generated on the credit (receiving) side only.
    /// </summary>
    Transfer = 1,

    /// <summary>
    /// Wallet balance used to pay for a marketplace item.
    /// TakeANap receipt generated with the item title as line item.
    /// </summary>
    ItemPayment = 2,

    /// <summary>
    /// Funds requested for withdrawal to the user's registered IBAN.
    /// No TakeANap receipt — outgoing money movement, not a taxable sale.
    /// </summary>
    Withdrawal = 3,

    /// <summary>
    /// Refund credited back to the wallet after a reversed payment or admin action.
    /// Generates a TakeANap fiscal receipt automatically.
    /// </summary>
    Refund = 4,

    /// <summary>
    /// Platform service fee deducted from the wallet.
    /// </summary>
    Fee = 5
}
