namespace MomVibe.Application.Interfaces;

using DTOs.SavedSearches;
using DTOs.Items;

public interface ISavedSearchService
{
    Task<SavedSearchDto> CreateAsync(string userId, CreateSavedSearchDto dto);
    Task DeleteAsync(Guid id, string userId);
    Task<List<SavedSearchDto>> GetMyAsync(string userId);
    Task NotifyMatchingSearchesAsync(ItemDto item);
}
