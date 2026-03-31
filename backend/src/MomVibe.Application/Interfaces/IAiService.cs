namespace MomVibe.Application.Interfaces;

using Microsoft.AspNetCore.Http;
using DTOs.Items;

/// <summary>
/// AI-powered service for generating item listing suggestions from a photo.
/// </summary>
public interface IAiService
{
    /// <summary>
    /// Analyzes the provided photo using Claude vision and returns prefilled listing suggestions.
    /// </summary>
    Task<AiListingSuggestionDto> SuggestListingAsync(IFormFile photo);
}
