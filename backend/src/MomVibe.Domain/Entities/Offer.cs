namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;

using Common;
using Enums;

public class Offer : BaseEntity
{
    public Guid ItemId { get; set; }

    [Required]
    public required string BuyerId { get; set; }

    [Required]
    public required string SellerId { get; set; }

    public decimal OfferedPrice { get; set; }

    public decimal? CounterPrice { get; set; }

    public OfferStatus Status { get; set; } = OfferStatus.Pending;

    public Item Item { get; set; } = null!;
    public ApplicationUser Buyer { get; set; } = null!;
    public ApplicationUser Seller { get; set; } = null!;
}
