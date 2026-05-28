namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;

using Common;
using Enums;

/// <summary>
/// Represents a buyer's request to purchase or reserve a specific item,
/// pending the seller's acceptance or decline.
/// While a request is Pending, the related Item is locked (IsActive = false)
/// so no other buyer can simultaneously request the same item.
/// </summary>
public class PurchaseRequest : BaseEntity
{
    /// <summary>Foreign key to the requested item. Null when the request is for a bundle.</summary>
    public Guid? ItemId { get; set; }

    /// <summary>Foreign key to the requested bundle. Null when the request is for a single item.</summary>
    public Guid? BundleId { get; set; }

    /// <summary>Identity of the buyer who sent the request.</summary>
    [Required]
    public required string BuyerId { get; set; }

    /// <summary>Identity of the seller who owns the item.</summary>
    [Required]
    public required string SellerId { get; set; }

    /// <summary>Current lifecycle status of the request.</summary>
    public PurchaseRequestStatus Status { get; set; } = PurchaseRequestStatus.Pending;

    /// <summary>Navigation to the requested item. Null for bundle requests.</summary>
    public Item? Item { get; set; }

    /// <summary>Navigation to the requested bundle. Null for single-item requests.</summary>
    public Bundle? Bundle { get; set; }

    /// <summary>Navigation to the buying user.</summary>
    public ApplicationUser Buyer { get; set; } = null!;

    /// <summary>Navigation to the selling user.</summary>
    public ApplicationUser Seller { get; set; } = null!;
}
