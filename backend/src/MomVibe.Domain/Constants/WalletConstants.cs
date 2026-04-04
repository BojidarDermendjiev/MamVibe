namespace MomVibe.Domain.Constants;

using Enums;

/// <summary>
/// Centralized constants for <see cref="MomVibe.Domain.Entities.Wallet"/>.
/// </summary>
public static class WalletConstants
{
    public static class Lengths
    {
        public const int CurrencyMax = 3;
        public const int FreezeReasonMax = 500;
    }

    public static class Defaults
    {
        public const string Currency = "EUR";
        public const WalletStatus Status = WalletStatus.Active;
    }

    public static class Comments
    {
        public const string UserId = "Foreign key referencing the wallet owner (FK to AspNetUsers.Id).";
        public const string Currency = "ISO 4217 currency code (e.g. EUR, BGN).";
        public const string Status = "Operational state of the wallet (Active, Frozen, Suspended, Closed).";
    }
}
