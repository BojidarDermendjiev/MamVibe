namespace MomVibe.Application.Interfaces;

using Microsoft.AspNetCore.Http;
using DTOs.Items;
using Domain.Enums;

/// <summary>
/// AI-powered listing assistance: photo-based listing suggestions and price recommendations.
/// </summary>
public interface IAiListingService
{
    Task<AiListingSuggestionDto> SuggestListingAsync(IFormFile photo);

    Task<PriceSuggestionResultDto> SuggestPriceAsync(
        string title,
        string description,
        string categoryName,
        AgeGroup? ageGroup,
        int? clothingSize,
        int? shoeSize,
        IReadOnlyList<decimal> comparablePrices);
}
