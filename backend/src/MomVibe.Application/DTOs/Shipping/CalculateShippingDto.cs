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
    /// <summary>Gets or sets the courier provider to calculate the price for (Econt, Speedy, BoxNow, etc.).</summary>
    public CourierProvider CourierProvider { get; set; }
    /// <summary>Gets or sets the delivery method (address, office, or locker).</summary>
    public DeliveryType DeliveryType { get; set; }
    /// <summary>Gets or sets the origin city for distance-based pricing.</summary>
    public string? FromCity { get; set; }
    /// <summary>Gets or sets the destination city for distance-based pricing.</summary>
    public string? ToCity { get; set; }
    /// <summary>Gets or sets the target office or locker identifier for office/locker delivery.</summary>
    public string? OfficeId { get; set; }
    /// <summary>Gets or sets the package weight in kilograms.</summary>
    public decimal Weight { get; set; }
    /// <summary>Gets or sets a value indicating whether cash-on-delivery is requested.</summary>
    public bool IsCod { get; set; }
    /// <summary>Gets or sets the cash-on-delivery amount in BGN; used to calculate the COD surcharge.</summary>
    public decimal CodAmount { get; set; }
    /// <summary>Gets or sets a value indicating whether shipment insurance is requested.</summary>
    public bool IsInsured { get; set; }
    /// <summary>Gets or sets the declared value in BGN for insurance premium calculation.</summary>
    public decimal InsuredAmount { get; set; }
}
