namespace MomVibe.Application.DTOs.Shipping;

/// <summary>
/// DTO representing a calculated shipping price result:
/// - Price: total shipping cost.
/// - Currency: ISO currency code (e.g., BGN).
/// - EstimatedDelivery: estimated delivery date/time description.
/// </summary>
public class ShippingPriceResultDto
{
    /// <summary>Gets or sets the total calculated shipping cost.</summary>
    public decimal Price { get; set; }
    /// <summary>Gets or sets the ISO 4217 currency code for the price (default: "BGN").</summary>
    public string Currency { get; set; } = "BGN";
    /// <summary>Gets or sets a human-readable estimated delivery timeframe (e.g. "1-2 business days").</summary>
    public string? EstimatedDelivery { get; set; }
}
