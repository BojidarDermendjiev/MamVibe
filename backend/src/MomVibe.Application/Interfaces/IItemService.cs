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
    /// <summary>Returns a paginated list of marketplace items matching the supplied filters.</summary>
    /// <param name="filter">Filter, sort, and pagination parameters.</param>
    /// <param name="currentUserId">When provided, marks items liked by this user.</param>
    Task<PagedResult<ItemDto>> GetAllAsync(ItemFilterDto filter, string? currentUserId = null);

    /// <summary>Returns a single item by its identifier, or <c>null</c> if not found.</summary>
    /// <param name="id">The unique identifier of the item.</param>
    /// <param name="currentUserId">When provided, indicates whether the item is liked by this user.</param>
    Task<ItemDto?> GetByIdAsync(Guid id, string? currentUserId = null);

    /// <summary>Creates a new item listing on behalf of the specified user.</summary>
    /// <param name="dto">The item data provided by the user.</param>
    /// <param name="userId">The identifier of the listing owner.</param>
    Task<ItemDto> CreateAsync(CreateItemDto dto, string userId);

    /// <summary>Updates an existing item listing, enforcing ownership unless the caller is an admin.</summary>
    /// <param name="id">The unique identifier of the item to update.</param>
    /// <param name="dto">The partial update data.</param>
    /// <param name="userId">The identifier of the requesting user.</param>
    Task<ItemDto> UpdateAsync(Guid id, UpdateItemDto dto, string userId);

    /// <summary>Deletes an item listing. Admins may delete any item; regular users may only delete their own.</summary>
    /// <param name="id">The unique identifier of the item to delete.</param>
    /// <param name="userId">The identifier of the requesting user.</param>
    /// <param name="isAdmin">When <c>true</c>, bypasses ownership checks.</param>
    Task DeleteAsync(Guid id, string userId, bool isAdmin = false);

    /// <summary>Atomically increments the view counter for the specified item.</summary>
    /// <param name="id">The unique identifier of the item.</param>
    Task IncrementViewCountAsync(Guid id);

    /// <summary>Toggles the like state for the given item and user.</summary>
    /// <param name="itemId">The unique identifier of the item.</param>
    /// <param name="userId">The identifier of the user toggling the like.</param>
    /// <returns><c>true</c> if the item is now liked; <c>false</c> if the like was removed.</returns>
    Task<bool> ToggleLikeAsync(Guid itemId, string userId);

    /// <summary>Returns all active items owned by the specified user.</summary>
    /// <param name="userId">The identifier of the user.</param>
    Task<List<ItemDto>> GetUserItemsAsync(string userId);

    /// <summary>Returns all items that the specified user has liked.</summary>
    /// <param name="userId">The identifier of the user.</param>
    Task<List<ItemDto>> GetLikedItemsAsync(string userId);

    /// <summary>Suggests a fair price for an item based on comparable active listings.</summary>
    /// <param name="dto">The item context used to find comparable listings.</param>
    Task<PriceSuggestionResultDto> SuggestPriceAsync(PriceSuggestionRequestDto dto);

    /// <summary>Checks the reputation of the seller who owns the specified item.</summary>
    /// <param name="itemId">The unique identifier of the item whose seller is to be checked.</param>
    Task<SellerCheckResultDto> CheckSellerAsync(Guid itemId);
}
