namespace MomVibe.Application.DTOs.Offers;

using Domain.Enums;

public class OfferDto
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public string? ItemTitle { get; set; }
    public string? ItemPhotoUrl { get; set; }
    public decimal? ItemPrice { get; set; }
    public string? BuyerDisplayName { get; set; }
    public string? BuyerAvatarUrl { get; set; }
    public string? SellerDisplayName { get; set; }
    public decimal OfferedPrice { get; set; }
    public decimal? CounterPrice { get; set; }
    public OfferStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
