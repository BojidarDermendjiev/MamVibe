namespace MomVibe.Application.DTOs.Shipping;

using Domain.Enums;

/// <summary>
/// DTO for calculating shipping price:
/// - CourierProvider and DeliveryType: determine pricing rules.
/// - FromCity and ToCity: origin and destination for distance-based pricing.
/// - OfficeId: target office for office/locker delivery.
/// - Weight: package weight in kilograms.
/// - IsCod and CodAmount: cash on delivery surcharge calculation.
/// - IsInsured and InsuredAmount: insurance surcharge calculation.
/// </summary>
public class CalculateShippingDto
{
    public CourierProvider CourierProvider { get; set; }
    public DeliveryType DeliveryType { get; set; }
    public string? FromCity { get; set; }
    public string? ToCity { get; set; }
    public string? OfficeId { get; set; }
    public decimal Weight { get; set; }
    public bool IsCod { get; set; }
    public decimal CodAmount { get; set; }
    public bool IsInsured { get; set; }
    public decimal InsuredAmount { get; set; }
}
