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
    /// <summary>Gets or sets the courier-specific identifier for this office or locker.</summary>
    public required string Id { get; set; }
    /// <summary>Gets or sets the human-readable name of the office or locker.</summary>
    public required string Name { get; set; }
    /// <summary>Gets or sets the city where the office is located.</summary>
    public string? City { get; set; }
    /// <summary>Gets or sets the street address of the office.</summary>
    public string? Address { get; set; }
    /// <summary>Gets or sets the geographic latitude of the office for map display.</summary>
    public double? Lat { get; set; }
    /// <summary>Gets or sets the geographic longitude of the office for map display.</summary>
    public double? Lng { get; set; }
    /// <summary>Gets or sets a value indicating whether this location is an automated parcel locker.</summary>
    public bool IsLocker { get; set; }
}
