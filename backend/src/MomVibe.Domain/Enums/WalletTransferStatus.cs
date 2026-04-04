namespace MomVibe.Domain.Enums;

/// <summary>
/// Represents the lifecycle state of a wallet-to-wallet transfer.
/// </summary>
public enum WalletTransferStatus
{
    /// <summary>
    /// Transfer is being processed.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Both the debit and credit legs have been applied successfully.
    /// </summary>
    Completed = 1,

    /// <summary>
    /// Transfer could not be completed (insufficient funds, frozen wallet, etc.).
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Completed transfer was reversed by an administrator.
    /// </summary>
    Reversed = 3
}
