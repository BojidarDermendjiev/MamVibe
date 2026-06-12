namespace MomVibe.Application.DTOs.Business;

using Domain.Enums;

/// <summary>
/// Payload for editing the calling user's listing. <c>IsActive</c> is included so the
/// owner can soft-hide their listing without deleting it; admin-only flags (IsApproved,
/// RankBoost) are NOT editable here.
/// </summary>
public class UpdateBusinessListingRequest
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
    public bool IsActive { get; set; } = true;

    /// <summary>Replaces the existing photo set in full. First entry becomes the new cover.</summary>
    public List<string> PhotoUrls { get; set; } = [];
}
