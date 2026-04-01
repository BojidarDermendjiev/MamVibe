namespace MomVibe.Application.Interfaces;

using Microsoft.AspNetCore.Http;
using DTOs.Items;
using Domain.Enums;

/// <summary>
/// AI-powered service for listing suggestions, content moderation, and price suggestions.
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

    /// <summary>
    /// Suggests a fair selling price based on comparable active listings and item context.
    /// Returns a suggested price with low/high range, confidence, and reasoning.
    /// </summary>
    Task<PriceSuggestionResultDto> SuggestPriceAsync(
        string title,
        string description,
        string categoryName,
        AgeGroup? ageGroup,
        int? clothingSize,
        int? shoeSize,
        IReadOnlyList<decimal> comparablePrices);

    /// <summary>
    /// Generates a conversational reply for the MamVibe AI assistant.
    /// </summary>
    Task<string> ChatAsync(
        string systemPrompt,
        IReadOnlyList<(string role, string content)> history);
}
