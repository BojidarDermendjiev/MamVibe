namespace MomVibe.Domain.Constants;

/// <summary>
/// Centralized constants for <see cref="MomVibe.Domain.Entities.Payment"/> to keep validation,
/// defaults, and database comments consistent across the codebase.
/// </summary>
public static class PaymentConstants
{
    /// <summary>
    /// Length-related constraints for <see cref="MomVibe.Domain.Entities.Payment"/> properties.
    /// </summary>
    public static class Lengths
    {
        /// <summary>Maximum length for <c>StripeSessionId</c>.</summary>
        public const int StripeSessionIdMax = 255;

        /// <summary>Maximum length for <c>ReceiptUrl</c>.</summary>
        public const int ReceiptUrlMax = 2048;
    }

    /// <summary>
    /// Numeric range constraints.
    /// </summary>
    public static class Range
    {
        /// <summary>Minimum allowed amount (non-negative).</summary>
        public const decimal AmountMin = 0m;
    }

    /// <summary>
    /// Default values for <see cref="MomVibe.Domain.Entities.Payment"/>.
    /// </summary>
    public static class Defaults
    {
        /// <summary>Default payment status.</summary>
        public const MomVibe.Domain.Enums.PaymentStatus Status = MomVibe.Domain.Enums.PaymentStatus.Pending;
    }

    /// <summary>
    /// Database column comments for EF Core schema generation.
    /// </summary>
    /// <remarks>
    /// Use via attributes or fluent configuration to produce descriptive database metadata:
    /// - Attribute: <c>[Microsoft.EntityFrameworkCore.Comment(PaymentConstants.Comments.Amount)]</c>
    /// - Fluent API: <c>builder.Property(p =&gt; p.Amount).HasComment(PaymentConstants.Comments.Amount);</c>
    /// Centralizing these strings keeps database documentation consistent across the codebase.
    /// </remarks>
    public static class Comments
    {
        /// <summary>
        /// Column comment: foreign key referencing the purchased item.
        /// </summary>
        public const string ItemId = "Foreign key referencing the purchased item.";

        /// <summary>
        /// Column comment: identifier of the buying user (FK to ApplicationUser.Id).
        /// </summary>
        public const string BuyerId = "Identifier of the buying user (FK to ApplicationUser.Id).";

        /// <summary>
        /// Column comment: identifier of the selling user (FK to ApplicationUser.Id).
        /// </summary>
        public const string SellerId = "Identifier of the selling user (FK to ApplicationUser.Id).";

        /// <summary>
        /// Column comment: monetary amount for the payment.
        /// </summary>
        public const string Amount = "Monetary amount for the payment.";

        /// <summary>
        /// Column comment: payment method (e.g., Stripe, Cash).
        /// </summary>
        public const string PaymentMethod = "Payment method (domain-specific enumeration).";

        /// <summary>
        /// Column comment: Stripe checkout session identifier, if applicable.
        /// </summary>
        public const string StripeSessionId = "Stripe checkout session identifier, if applicable.";

        /// <summary>
        /// Column comment: current payment status (e.g., Pending, Succeeded, Failed).
        /// </summary>
        public const string Status = "Current payment status (e.g., Pending, Succeeded, Failed).";

        /// <summary>
        /// Column comment: URL to the digital receipt from Take a NAP.
        /// </summary>
        public const string ReceiptUrl = "URL to the digital receipt from Take a NAP.";
    }
}