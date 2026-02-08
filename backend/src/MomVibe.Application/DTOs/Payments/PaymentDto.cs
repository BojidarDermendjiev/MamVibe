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
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public string? ItemTitle { get; set; }
    public required string BuyerId { get; set; }
    public required string SellerId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ReceiptUrl { get; set; }
}
