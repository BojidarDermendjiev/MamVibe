namespace MomVibe.Application.DTOs.ChildFriendlyPlaces;

using Domain.Enums;

public class CreateChildFriendlyPlaceDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string City { get; set; } = string.Empty;
    public PlaceType PlaceType { get; set; }
    public int? AgeFromMonths { get; set; }
    public int? AgeToMonths { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Website { get; set; }
}
