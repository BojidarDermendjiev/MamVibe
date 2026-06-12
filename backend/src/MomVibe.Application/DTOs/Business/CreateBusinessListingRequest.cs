namespace MomVibe.Application.DTOs.Business;

using Domain.Enums;

/// <summary>
/// Payload submitted when a <c>BusinessProfile</c> owner creates their (single) listing.
/// Photos are URLs of files already uploaded via the existing <c>PhotosController</c>
/// pipeline — we do not stream the file bytes through this endpoint.
/// </summary>
public class CreateBusinessListingRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ActivityType ActivityType { get; set; }
    public string City { get; set; } = string.Empty;
    public string? AddressLine { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public short? AgeFromMonths { get; set; }
    public short? AgeToMonths { get; set; }
    public decimal? PriceFromEur { get; set; }
    public string? Schedule { get; set; }

    /// <summary>Ordered list of already-uploaded photo URLs. The first entry becomes the cover.</summary>
    public List<string> PhotoUrls { get; set; } = [];
}
