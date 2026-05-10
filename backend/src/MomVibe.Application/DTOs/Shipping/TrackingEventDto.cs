namespace MomVibe.Application.DTOs.Shipping;

/// <summary>
/// DTO representing a single tracking event in a shipment's journey:
/// - Timestamp: when the event occurred.
/// - Description: human-readable event description.
/// - Location: where the event occurred (city, facility, etc.).
/// </summary>
public class TrackingEventDto
{
    /// <summary>Gets or sets the date and time when this tracking event occurred.</summary>
    public DateTime Timestamp { get; set; }
    /// <summary>Gets or sets the human-readable description of the tracking event (e.g. "Parcel accepted at Econt office").</summary>
    public required string Description { get; set; }
    /// <summary>Gets or sets the location where the event occurred (city, facility name, etc.).</summary>
    public string? Location { get; set; }
}
