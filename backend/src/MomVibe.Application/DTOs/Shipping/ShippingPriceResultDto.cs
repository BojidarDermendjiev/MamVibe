namespace MomVibe.Application.DTOs.Shipping;

/// <summary>
/// DTO representing a calculated shipping price result:
/// - Price: total shipping cost.
/// - Currency: ISO currency code (e.g., BGN).
/// - EstimatedDelivery: estimated delivery date/time description.
/// </summary>
public class ShippingPriceResultDto
{
    public decimal Price { get; set; }
    public string Currency { get; set; } = "BGN";
    public string? EstimatedDelivery { get; set; }
}
