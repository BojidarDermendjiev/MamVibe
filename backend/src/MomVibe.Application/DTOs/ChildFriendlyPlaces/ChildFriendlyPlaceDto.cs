namespace MomVibe.Application.DTOs.ChildFriendlyPlaces;

using Domain.Enums;

/// <summary>
/// Read-only projection of a <see cref="Domain.Entities.ChildFriendlyPlace"/> returned by the API.
/// </summary>
public class ChildFriendlyPlaceDto
{
    /// <summary>Gets or sets the unique identifier of the place.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the identifier of the user who submitted the place.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name of the submitting user, or <c>null</c> if not available.</summary>
    public string? AuthorDisplayName { get; set; }

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

    /// <summary>Gets or sets a value indicating whether the place has been approved by an administrator.</summary>
    public bool IsApproved { get; set; }

    /// <summary>Gets or sets the UTC date and time when the place was submitted.</summary>
    public DateTime CreatedAt { get; set; }
}
