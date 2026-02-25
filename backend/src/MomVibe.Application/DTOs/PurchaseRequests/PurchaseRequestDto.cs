namespace MomVibe.Application.DTOs.PurchaseRequests;

using Domain.Enums;

/// <summary>
/// Data transfer object returned to clients for purchase/reservation requests.
/// </summary>
public class PurchaseRequestDto
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public string? ItemTitle { get; set; }
    public string? ItemPhotoUrl { get; set; }
    public ListingType ListingType { get; set; }
    public decimal? Price { get; set; }
    public string BuyerId { get; set; } = "";
    public string? BuyerDisplayName { get; set; }
    public string? BuyerAvatarUrl { get; set; }
    public string SellerId { get; set; } = "";
    public PurchaseRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    /// <summary>Populated for Completed requests when a shipment was auto-created, so the seller can navigate directly to the waybill.</summary>
    public Guid? ShipmentId { get; set; }
}
