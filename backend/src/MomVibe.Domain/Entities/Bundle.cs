namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Common;

/// <summary>
/// Represents a seller-curated group of 2–10 active items offered together at a discounted bundle price.
/// Buyers can send a purchase request for the bundle; when payment completes all member items are marked sold.
/// </summary>
public class Bundle : BaseEntity
{
    /// <summary>Human-readable bundle title shown to buyers.</summary>
    [Required, MaxLength(150)]
    public required string Title { get; set; }

    /// <summary>Optional description providing context about the bundle.</summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>Discounted price for the entire bundle in the platform currency.</summary>
    public decimal Price { get; set; }

    /// <summary>Identity of the seller who created this bundle (FK to ApplicationUser).</summary>
    [Required]
    public required string SellerId { get; set; }

    /// <summary>Whether the bundle is currently visible and available for purchase requests.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Whether the bundle has been fully purchased and items have been transferred.</summary>
    public bool IsSold { get; set; }

    /// <summary>Navigation to the seller's user account.</summary>
    public ApplicationUser Seller { get; set; } = null!;

    /// <summary>Navigation to the individual item memberships of this bundle.</summary>
    public ICollection<BundleItem> BundleItems { get; set; } = [];
}
