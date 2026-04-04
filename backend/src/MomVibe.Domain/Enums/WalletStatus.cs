namespace MomVibe.Domain.Enums;

/// <summary>
/// Represents the operational state of a user wallet.
/// </summary>
public enum WalletStatus
{
    /// <summary>
    /// Wallet is fully operational — deposits, transfers, and withdrawals are allowed.
    /// </summary>
    Active = 0,

    /// <summary>
    /// Wallet is temporarily frozen by an administrator. All operations are blocked.
    /// </summary>
    Frozen = 1,

    /// <summary>
    /// Wallet is suspended due to policy violation. Requires admin review to restore.
    /// </summary>
    Suspended = 2,

    /// <summary>
    /// Wallet is permanently closed. No operations are permitted.
    /// </summary>
    Closed = 3
}
