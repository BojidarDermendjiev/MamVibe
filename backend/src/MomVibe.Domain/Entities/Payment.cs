namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;

using Enums;
using Common;
using Constants;

/// <summary>
/// Represents a payment transaction between a buyer and a seller for a specific item,
/// including method, amount, optional Stripe session, and status.
/// </summary>
/// <remarks>
/// - Inherits <see cref="BaseEntity"/> for identity and audit fields.
/// - Validation attributes use centralized constants for consistency.
/// - Indexes and column comments are defined in the Infrastructure configuration class.
/// </remarks>
public class Payment : BaseEntity
{
    /// <summary>
    /// Foreign key referencing the purchased item. Null when the payment is for a bundle.
    /// </summary>
    public Guid? ItemId { get; set; }

    /// <summary>
    /// Foreign key referencing the purchased bundle. Null when the payment is for a single item.
    /// </summary>
    public Guid? BundleId { get; set; }

    /// <summary>
    /// Identifier of the buying user (FK to ApplicationUser.Id).
    /// </summary>
    [Required]
    public required string BuyerId { get; set; }

    /// <summary>
    /// Identifier of the selling user (FK to ApplicationUser.Id).
    /// </summary>
    [Required]
    public required string SellerId { get; set; }

    /// <summary>
    /// Monetary amount for the payment.
    /// </summary>
    [Range(typeof(decimal), "0", "79228162514264337593543950335")] // non-negative; upper bound is decimal.MaxValue
    public decimal Amount { get; set; }

    /// <summary>
    /// Payment method (domain-specific enumeration).
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// Stripe checkout session identifier, if applicable.
    /// </summary>
    [MaxLength(PaymentConstants.Lengths.StripeSessionIdMax)]
    public string? StripeSessionId { get; set; }

    /// <summary>
    /// Current payment status (e.g., Pending, Succeeded, Failed).
    /// </summary>
    public PaymentStatus Status { get; set; } = PaymentConstants.Defaults.Status;

    /// <summary>
    /// URL to the digital receipt from Take a NAP.
    /// </summary>
    [MaxLength(PaymentConstants.Lengths.ReceiptUrlMax)]
    public string? ReceiptUrl { get; set; }

    /// <summary>
    /// Human-readable e-bill number assigned once at payment completion (e.g. "MV-2026-A1B2C3D4").
    /// Null until the payment transitions to <see cref="Enums.PaymentStatus.Completed"/>.
    /// </summary>
    [MaxLength(PaymentConstants.Lengths.EBillNumberMax)]
    public string? EBillNumber { get; set; }

    /// <summary>
    /// Client-supplied idempotency key (from the <c>Idempotency-Key</c> request header).
    /// Used to dedupe duplicate payment-creation requests caused by double-taps or retries.
    /// A unique index on this column lets a concurrent second request fail at the DB level
    /// rather than create a duplicate Payment row.
    /// </summary>
    [MaxLength(PaymentConstants.Lengths.IdempotencyKeyMax)]
    public string? IdempotencyKey { get; set; }

    /// <summary>
    /// Navigation to the purchased item. Null for bundle payments.
    /// </summary>
    public Item? Item { get; set; }

    /// <summary>
    /// Navigation to the purchased bundle. Null for single-item payments.
    /// </summary>
    public Bundle? Bundle { get; set; }

    /// <summary>
    /// Navigation to the buying user.
    /// </summary>
    public ApplicationUser Buyer { get; set; } = null!;

    /// <summary>
    /// Navigation to the selling user.
    /// </summary>
    public ApplicationUser Seller { get; set; } = null!;
}