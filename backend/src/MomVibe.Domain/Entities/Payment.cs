namespace MomVibe.Domain.Entities;

using Microsoft.EntityFrameworkCore;
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
/// - EF Core <see cref="CommentAttribute"/> provides descriptive database column comments.
/// - Indexes support common query patterns (by item, participants, status, creation time).
/// </remarks>
[Index(nameof(ItemId))]
[Index(nameof(BuyerId))]
[Index(nameof(SellerId))]
[Index(nameof(Status))]
[Index(nameof(CreatedAt))]
public class Payment : BaseEntity
{
    /// <summary>
    /// Foreign key referencing the purchased item.
    /// </summary>
    [Comment(PaymentConstants.Comments.ItemId)]
    public Guid ItemId { get; set; }

    /// <summary>
    /// Identifier of the buying user (FK to ApplicationUser.Id).
    /// </summary>
    [Required]
    [Comment(PaymentConstants.Comments.BuyerId)]
    public required string BuyerId { get; set; }

    /// <summary>
    /// Identifier of the selling user (FK to ApplicationUser.Id).
    /// </summary>
    [Required]
    [Comment(PaymentConstants.Comments.SellerId)]
    public required string SellerId { get; set; }

    /// <summary>
    /// Monetary amount for the payment.
    /// </summary>
    [Precision(18, 2)]
    [Range(typeof(decimal), "0", "79228162514264337593543950335")] // non-negative; upper bound is decimal.MaxValue
    [Comment(PaymentConstants.Comments.Amount)]
    public decimal Amount { get; set; }

    /// <summary>
    /// Payment method (domain-specific enumeration).
    /// </summary>
    [Comment(PaymentConstants.Comments.PaymentMethod)]
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// Stripe checkout session identifier, if applicable.
    /// </summary>
    [MaxLength(PaymentConstants.Lengths.StripeSessionIdMax)]
    [Comment(PaymentConstants.Comments.StripeSessionId)]
    public string? StripeSessionId { get; set; }

    /// <summary>
    /// Current payment status (e.g., Pending, Succeeded, Failed).
    /// </summary>
    [Comment(PaymentConstants.Comments.Status)]
    public PaymentStatus Status { get; set; } = PaymentConstants.Defaults.Status;

    /// <summary>
    /// URL to the digital receipt from Take a NAP.
    /// </summary>
    [MaxLength(PaymentConstants.Lengths.ReceiptUrlMax)]
    [Comment(PaymentConstants.Comments.ReceiptUrl)]
    public string? ReceiptUrl { get; set; }

    /// <summary>
    /// Human-readable e-bill number assigned once at payment completion (e.g. "MV-2026-A1B2C3D4").
    /// Null until the payment transitions to <see cref="Enums.PaymentStatus.Completed"/>.
    /// </summary>
    [MaxLength(PaymentConstants.Lengths.EBillNumberMax)]
    [Comment(PaymentConstants.Comments.EBillNumber)]
    public string? EBillNumber { get; set; }

    /// <summary>
    /// Navigation to the purchased item.
    /// </summary>
    public Item Item { get; set; } = null!;

    /// <summary>
    /// Navigation to the buying user.
    /// </summary>
    public ApplicationUser Buyer { get; set; } = null!;

    /// <summary>
    /// Navigation to the selling user.
    /// </summary>
    public ApplicationUser Seller { get; set; } = null!;
}