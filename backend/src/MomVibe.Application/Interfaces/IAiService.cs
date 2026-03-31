namespace MomVibe.Application.Interfaces;

using Microsoft.AspNetCore.Http;
using DTOs.Items;
using Domain.Enums;

/// <summary>
/// AI-powered service for listing suggestions and content moderation.
/// </summary>
public interface IAiService
{
    /// <summary>
    /// Analyzes the provided photo using Claude vision and returns prefilled listing suggestions.
    /// </summary>
    Task<AiListingSuggestionDto> SuggestListingAsync(IFormFile photo);

    /// <summary>
    /// Runs text-based content moderation on a new listing.
    /// Returns a recommendation (approve / review / reject) with confidence and reason.
    /// </summary>
    Task<AiModerationResultDto> ModerateItemAsync(
        string title,
        string description,
        string categoryName,
        ListingType listingType,
        decimal? price);
}
