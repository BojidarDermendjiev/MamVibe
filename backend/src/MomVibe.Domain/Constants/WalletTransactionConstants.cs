namespace MomVibe.Domain.Constants;

using Enums;

/// <summary>
/// Centralized constants for <see cref="MomVibe.Domain.Entities.WalletTransaction"/>.
/// </summary>
public static class WalletTransactionConstants
{
    public static class Lengths
    {
        public const int ReferenceMax = 500;
        public const int DescriptionMax = 500;
        public const int ReceiptUrlMax = 2048;
    }

    public static class Range
    {
        public const decimal AmountMin = 0.01m;
        public const decimal AmountMax = 50000m;
    }

    public static class Defaults
    {
        public const WalletTransactionStatus Status = WalletTransactionStatus.Pending;
    }

    public static class Comments
    {
        public const string WalletId = "Foreign key referencing the wallet this transaction belongs to.";
        public const string Type = "Direction of money movement: Credit (money in) or Debit (money out).";
        public const string Kind = "Business reason for the transaction (TopUp, Transfer, ItemPayment, Withdrawal, Refund, Fee).";
        public const string Amount = "Absolute monetary amount of this transaction in the wallet currency.";
        public const string BalanceAfter = "Wallet balance snapshot immediately after this transaction was applied.";
        public const string Status = "Settlement state of this transaction.";
        public const string Reference = "External reference identifier (e.g. Stripe PaymentIntent ID, transfer ID).";
        public const string Description = "Human-readable description shown in transaction history.";
        public const string RelatedTransactionId = "ID of the counterpart transaction in a double-entry transfer (the other leg).";
        public const string PaymentId = "FK to the marketplace Payment record when kind is ItemPayment.";
        public const string ReceiptUrl = "URL to the TakeANap fiscal receipt generated for this transaction.";
    }
}
