namespace MomVibe.Application.Interfaces;

using DTOs.Items;
using Domain.Enums;

/// <summary>
/// AI-powered content moderation for marketplace listings.
/// </summary>
public interface IAiModerationService
{
    Task<AiModerationResultDto> ModerateItemAsync(
        string title,
        string description,
        string categoryName,
        ListingType listingType,
        decimal? price,
        string? firstPhotoUrl = null);
}
