namespace MomVibe.Application.DTOs.ChildFriendlyPlaces;

using Domain.Enums;

public class ChildFriendlyPlaceDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? AuthorDisplayName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string City { get; set; } = string.Empty;
    public PlaceType PlaceType { get; set; }
    public int? AgeFromMonths { get; set; }
    public int? AgeToMonths { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Website { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
}
