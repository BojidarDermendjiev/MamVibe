namespace MomVibe.Application.Interfaces;

using DTOs.Items;
using DTOs.Common;

/// <summary>
/// Item service contract for marketplace operations:
/// - Paginated, filterable listing of active items; optionally marks items liked by the current user.
/// - Retrieve an item by ID; optionally includes whether it's liked by the current user.
/// - Create, update, and delete items with ownership checks and optional admin override.
/// - Increment an item's view count.
/// - Toggle likes for a given item and user (returns true if now liked).
/// - Fetch items owned by a user and items the user has liked.
/// </summary>
public interface IItemService
{
    Task<PagedResult<ItemDto>> GetAllAsync(ItemFilterDto filter, string? currentUserId = null);
    Task<ItemDto?> GetByIdAsync(Guid id, string? currentUserId = null);
    Task<ItemDto> CreateAsync(CreateItemDto dto, string userId);
    Task<ItemDto> UpdateAsync(Guid id, UpdateItemDto dto, string userId);
    Task DeleteAsync(Guid id, string userId, bool isAdmin = false);
    Task IncrementViewCountAsync(Guid id);
    Task<bool> ToggleLikeAsync(Guid itemId, string userId);
    Task<List<ItemDto>> GetUserItemsAsync(string userId);
    Task<List<ItemDto>> GetLikedItemsAsync(string userId);
    Task<PriceSuggestionResultDto> SuggestPriceAsync(PriceSuggestionRequestDto dto);
}
