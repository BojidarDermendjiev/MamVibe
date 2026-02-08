namespace MomVibe.Application.DTOs.Shipping;

/// <summary>
/// DTO representing a single tracking event in a shipment's journey:
/// - Timestamp: when the event occurred.
/// - Description: human-readable event description.
/// - Location: where the event occurred (city, facility, etc.).
/// </summary>
public class TrackingEventDto
{
    public DateTime Timestamp { get; set; }
    public required string Description { get; set; }
    public string? Location { get; set; }
}
