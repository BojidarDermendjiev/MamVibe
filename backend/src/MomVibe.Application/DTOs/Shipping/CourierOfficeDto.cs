namespace MomVibe.Application.DTOs.Shipping;

/// <summary>
/// DTO representing a courier office or locker location:
/// - Id: courier-specific office identifier.
/// - Name: human-readable office name.
/// - City: city where the office is located.
/// - Address: street address of the office.
/// - Lat/Lng: geographic coordinates for map display.
/// - IsLocker: whether this location is an automated parcel locker.
/// </summary>
public class CourierOfficeDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }
    public bool IsLocker { get; set; }
}
