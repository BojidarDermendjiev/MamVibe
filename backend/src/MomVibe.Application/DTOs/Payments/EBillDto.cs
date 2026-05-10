namespace MomVibe.Application.DTOs.Payments;

using Domain.Enums;

/// <summary>
/// Read-only projection of a completed purchase, surfaced to the buyer as an e-bill.
/// Aggregates payment identity, item info, parties, financial data, and the TakeANap receipt URL.
/// </summary>
public class EBillDto
{
    /// <summary>Gets or sets the unique identifier of the underlying payment record.</summary>
    public Guid Id { get; set; }

    /// <summary>Human-readable receipt number (e.g. "MV-2026-A1B2C3D4"). Null if not yet assigned.</summary>
    public string? EBillNumber { get; set; }

    /// <summary>Gets or sets the identifier of the item that was purchased.</summary>
    public Guid ItemId { get; set; }
    /// <summary>Gets or sets the title of the item at the time of purchase.</summary>
    public string? ItemTitle { get; set; }

    /// <summary>Gets or sets the identifier of the buyer.</summary>
    public required string BuyerId { get; set; }
    /// <summary>Gets or sets the identifier of the seller.</summary>
    public required string SellerId { get; set; }
    /// <summary>Gets or sets the display name of the seller, for rendering on the e-bill.</summary>
    public string? SellerDisplayName { get; set; }

    /// <summary>Gets or sets the total amount charged for this transaction in BGN.</summary>
    public decimal Amount { get; set; }
    /// <summary>Gets or sets the ISO 4217 currency code for the transaction (default: "BGN").</summary>
    public string Currency { get; set; } = "BGN";

    /// <summary>Gets or sets the payment method used for this purchase.</summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>Timestamp when the payment was completed and the e-bill was issued.</summary>
    public DateTime IssuedAt { get; set; }

    /// <summary>Direct download URL from Take a NAP. Null when TakeANap is unconfigured (dev/test mode).</summary>
    public string? ReceiptUrl { get; set; }
}
