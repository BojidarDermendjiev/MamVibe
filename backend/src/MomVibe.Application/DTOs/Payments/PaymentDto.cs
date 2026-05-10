namespace MomVibe.Application.DTOs.Payments;

using Domain.Enums;

/// <summary>
/// DTO representing a payment transaction:
/// - Identity & linkage: Id, ItemId, optional ItemTitle.
/// - Parties: BuyerId, SellerId.
/// - Financials: Amount, PaymentMethod.
/// - Status: PaymentStatus lifecycle state.
/// - CreatedAt: timestamp when the payment record was created.
/// </summary>
public class PaymentDto
{
    /// <summary>Gets or sets the unique payment identifier.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the identifier of the item involved in the transaction.</summary>
    public Guid ItemId { get; set; }
    /// <summary>Gets or sets the title of the item at the time of purchase, for display purposes.</summary>
    public string? ItemTitle { get; set; }
    /// <summary>Gets or sets the identifier of the user who made the purchase.</summary>
    public required string BuyerId { get; set; }
    /// <summary>Gets or sets the identifier of the user who listed the item.</summary>
    public required string SellerId { get; set; }
    /// <summary>Gets or sets the total amount charged for this payment in BGN.</summary>
    public decimal Amount { get; set; }
    /// <summary>Gets or sets the payment method used (Stripe, on-spot, booking, etc.).</summary>
    public PaymentMethod PaymentMethod { get; set; }
    /// <summary>Gets or sets the current lifecycle status of the payment.</summary>
    public PaymentStatus Status { get; set; }
    /// <summary>Gets or sets the timestamp when this payment record was created.</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>Gets or sets the URL to the Stripe-hosted payment receipt, if available.</summary>
    public string? ReceiptUrl { get; set; }
}
