namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;

using Common;

/// <summary>
/// Append-only fact row recording a single view of a <see cref="BusinessListing"/>.
/// Aggregated daily into <see cref="BusinessListingDailyStat"/> by a small background
/// service so historic data is queryable without scanning the raw events table.
/// </summary>
public class BusinessListingViewEvent : BaseEntity
{
    /// <summary>FK to the viewed listing.</summary>
    public Guid ListingId { get; set; }

    /// <summary>
    /// SHA-256 hex of (UserId || IP /24 || UserAgent) for unique-viewer counting
    /// without persisting raw viewer identity. Empty string for fully anonymous views.
    /// </summary>
    [Required]
    [MaxLength(64)]
    public required string ViewerHash { get; set; }

    /// <summary>UTC timestamp of the view.</summary>
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    /// <summary>Navigation to the viewed listing.</summary>
    public BusinessListing Listing { get; set; } = null!;
}
