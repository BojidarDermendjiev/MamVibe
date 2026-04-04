namespace MomVibe.Domain.Constants;

using Enums;

/// <summary>
/// Centralized constants for <see cref="MomVibe.Domain.Entities.WalletTransfer"/>.
/// </summary>
public static class WalletTransferConstants
{
    public static class Lengths
    {
        public const int NoteMax = 200;
        public const int InitiatedByIpMax = 45;
        public const int CurrencyMax = 3;
    }

    public static class Range
    {
        public const decimal AmountMin = 0.01m;
        public const decimal AmountMax = 10000m;
    }

    public static class Defaults
    {
        public const WalletTransferStatus Status = WalletTransferStatus.Pending;
        public const string Currency = "EUR";
    }

    public static class Comments
    {
        public const string SenderWalletId = "FK to the wallet that initiates the transfer (debit side).";
        public const string ReceiverWalletId = "FK to the wallet that receives the funds (credit side).";
        public const string Amount = "Amount transferred between wallets.";
        public const string Currency = "ISO 4217 currency code of the transferred amount.";
        public const string Status = "Overall status of the transfer operation.";
        public const string Note = "Optional message from the sender shown in the receiver's transaction history.";
        public const string InitiatedByIp = "IP address of the client that initiated the transfer, stored for fraud monitoring.";
        public const string SenderTransactionId = "FK to the debit WalletTransaction created for the sender.";
        public const string ReceiverTransactionId = "FK to the credit WalletTransaction created for the receiver.";
    }
}
