namespace MomVibe.Application.DTOs.Offers;

using System.ComponentModel.DataAnnotations;

public class CounterOfferDto
{
    [Range(0.01, 999999)]
    public decimal CounterPrice { get; set; }
}
