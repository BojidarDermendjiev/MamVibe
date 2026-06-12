namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;

using Common;

/// <summary>
/// A parent's like on a <see cref="BusinessListing"/>. Enforces uniqueness per (UserId, ListingId)
/// via a composite unique index — mirrors the <see cref="Like"/> pattern used by the marketplace.
/// </summary>
public class BusinessListingLike : BaseEntity
{
    /// <summary>Identifier of the liking user (FK to ApplicationUser.Id).</summary>
    [Required]
    [MaxLength(450)]
    public required string UserId { get; set; }

    /// <summary>FK to the liked listing.</summary>
    public Guid ListingId { get; set; }

    /// <summary>Navigation to the liking user.</summary>
    public ApplicationUser User { get; set; } = null!;

    /// <summary>Navigation to the liked listing.</summary>
    public BusinessListing Listing { get; set; } = null!;
}
