namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;

using Common;

/// <summary>
/// Photo attached to a <see cref="BusinessListing"/>. Storage and upload reuse the
/// existing <c>PhotosController</c> pipeline unchanged; only the foreign-key column differs
/// from <see cref="ItemPhoto"/>.
/// </summary>
public class BusinessListingPhoto : BaseEntity
{
    /// <summary>FK to the owning listing.</summary>
    public Guid ListingId { get; set; }

    /// <summary>Absolute or relative URL to the photo resource.</summary>
    [Required]
    [MaxLength(500)]
    public required string Url { get; set; }

    /// <summary>Zero-based display order in the gallery; cover photo is order 0.</summary>
    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; }

    /// <summary>Convenience flag — true when this photo is the cover/hero image.</summary>
    public bool IsCover { get; set; }

    /// <summary>Navigation to the owning listing.</summary>
    public BusinessListing Listing { get; set; } = null!;
}
