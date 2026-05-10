namespace MomVibe.Application.DTOs.PurchaseRequests;

using Domain.Enums;

/// <summary>
/// Data transfer object returned to clients for purchase/reservation requests.
/// </summary>
public class PurchaseRequestDto
{
    /// <summary>Gets or sets the unique identifier of this purchase request.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the identifier of the item being requested.</summary>
    public Guid ItemId { get; set; }
    /// <summary>Gets or sets the title of the requested item, for display purposes.</summary>
    public string? ItemTitle { get; set; }
    /// <summary>Gets or sets the URL of the item's primary photo, for display purposes.</summary>
    public string? ItemPhotoUrl { get; set; }
    /// <summary>Gets or sets the listing type of the item (Sell, Donate, etc.).</summary>
    public ListingType ListingType { get; set; }
    /// <summary>Gets or sets the item price in BGN; null for donated items.</summary>
    public decimal? Price { get; set; }
    /// <summary>Gets or sets the identifier of the user who submitted the purchase request.</summary>
    public string BuyerId { get; set; } = "";
    /// <summary>Gets or sets the display name of the buyer.</summary>
    public string? BuyerDisplayName { get; set; }
    /// <summary>Gets or sets the avatar URL of the buyer.</summary>
    public string? BuyerAvatarUrl { get; set; }
    /// <summary>Gets or sets the identifier of the user who listed the item.</summary>
    public string SellerId { get; set; } = "";
    /// <summary>Gets or sets the current status of this purchase request.</summary>
    public PurchaseRequestStatus Status { get; set; }
    /// <summary>Gets or sets the timestamp when this purchase request was created.</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>Populated for Completed requests when a shipment was auto-created, so the seller can navigate directly to the waybill.</summary>
    public Guid? ShipmentId { get; set; }
}
