namespace MomVibe.Application.DTOs.Bundles;

/// <summary>
/// Full bundle representation returned to clients, including seller details and item slots.
/// </summary>
public class BundleDto
{
    /// <summary>Gets or sets the unique identifier of the bundle.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the human-readable bundle title.</summary>
    public string Title { get; set; } = "";

    /// <summary>Gets or sets the optional description providing context about the bundle.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the discounted bundle price in the platform currency.</summary>
    public decimal Price { get; set; }

    /// <summary>Gets or sets the identifier of the seller who created the bundle.</summary>
    public string SellerId { get; set; } = "";

    /// <summary>Gets or sets whether the current authenticated user owns this bundle. Set server-side; do not use SellerId for ownership checks on the client.</summary>
    public bool IsOwnedByCurrentUser { get; set; }

    /// <summary>Gets or sets the display name of the seller, for UI rendering.</summary>
    public string? SellerDisplayName { get; set; }

    /// <summary>Gets or sets the avatar URL of the seller, for UI rendering.</summary>
    public string? SellerAvatarUrl { get; set; }

    /// <summary>Gets or sets whether the bundle is visible and available for purchase requests.</summary>
    public bool IsActive { get; set; }

    /// <summary>Gets or sets whether the bundle has been fully purchased.</summary>
    public bool IsSold { get; set; }

    /// <summary>Gets or sets the items that belong to this bundle.</summary>
    public List<BundleItemDto> Items { get; set; } = [];

    /// <summary>Gets or sets the UTC timestamp when the bundle was created.</summary>
    public DateTime CreatedAt { get; set; }
}
