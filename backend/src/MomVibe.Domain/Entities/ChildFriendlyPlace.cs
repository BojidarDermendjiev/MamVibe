namespace MomVibe.Domain.Entities;

using Common;
using Enums;

public class ChildFriendlyPlace : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

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
}
