namespace MomVibe.Application.DTOs.Offers;

using System.ComponentModel.DataAnnotations;

public class CreateOfferDto
{
    [Required]
    public Guid ItemId { get; set; }

    [Range(0.01, 999999)]
    public decimal OfferedPrice { get; set; }
}
