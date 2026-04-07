namespace MomVibe.Application.DTOs.Payments;

using Domain.Enums;

/// <summary>
/// Read-only projection of a completed purchase, surfaced to the buyer as an e-bill.
/// Aggregates payment identity, item info, parties, financial data, and the TakeANap receipt URL.
/// </summary>
public class EBillDto
{
    public Guid Id { get; set; }

    /// <summary>Human-readable receipt number (e.g. "MV-2026-A1B2C3D4"). Null if not yet assigned.</summary>
    public string? EBillNumber { get; set; }

    public Guid ItemId { get; set; }
    public string? ItemTitle { get; set; }

    public required string BuyerId { get; set; }
    public required string SellerId { get; set; }
    public string? SellerDisplayName { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "BGN";

    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>Timestamp when the payment was completed and the e-bill was issued.</summary>
    public DateTime IssuedAt { get; set; }

    /// <summary>Direct download URL from Take a NAP. Null when TakeANap is unconfigured (dev/test mode).</summary>
    public string? ReceiptUrl { get; set; }
}
