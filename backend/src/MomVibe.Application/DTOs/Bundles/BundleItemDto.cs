namespace MomVibe.Application.DTOs.Bundles;

/// <summary>
/// Represents a single item slot within a bundle as returned to clients.
/// </summary>
public class BundleItemDto
{
    /// <summary>Gets or sets the unique identifier of the item.</summary>
    public Guid ItemId { get; set; }

    /// <summary>Gets or sets the title of the item.</summary>
    public string Title { get; set; } = "";

    /// <summary>Gets or sets the individual item price; null for donated items.</summary>
    public decimal? Price { get; set; }

    /// <summary>Gets or sets the URL of the item's primary photo, for display purposes.</summary>
    public string? PhotoUrl { get; set; }
}
