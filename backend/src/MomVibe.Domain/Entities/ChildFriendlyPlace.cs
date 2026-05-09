namespace MomVibe.Domain.Entities;

using Common;
using Enums;

/// <summary>
/// Represents a child-friendly place submitted by a user, such as a playground, restaurant, or park.
/// Entries must be approved by an administrator before they become publicly visible.
/// </summary>
public class ChildFriendlyPlace : BaseEntity
{
    /// <summary>Gets or sets the identifier of the user who submitted this place.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Gets or sets the user who submitted this place.</summary>
    public ApplicationUser User { get; set; } = null!;

    /// <summary>Gets or sets the display name of the place.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets a detailed description of the place and its child-friendly features.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the street address of the place.</summary>
    public string? Address { get; set; }

    /// <summary>Gets or sets the city where the place is located.</summary>
    public string City { get; set; } = string.Empty;

    /// <summary>Gets or sets the category type of the place.</summary>
    public PlaceType PlaceType { get; set; }

    /// <summary>Gets or sets the minimum recommended child age in months.</summary>
    public int? AgeFromMonths { get; set; }

    /// <summary>Gets or sets the maximum recommended child age in months.</summary>
    public int? AgeToMonths { get; set; }

    /// <summary>Gets or sets the URL of a representative photo for the place.</summary>
    public string? PhotoUrl { get; set; }

    /// <summary>Gets or sets the official website URL of the place.</summary>
    public string? Website { get; set; }

    /// <summary>Gets or sets a value indicating whether an administrator has approved this submission.</summary>
    public bool IsApproved { get; set; }
}
