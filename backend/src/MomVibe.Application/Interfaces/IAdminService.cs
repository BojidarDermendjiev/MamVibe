namespace MomVibe.Application.Interfaces;

using DTOs.Admin;
using DTOs.Common;
using DTOs.Items;

/// <summary>
/// Administrative service contract for user management and platform metrics.
/// Exposes paginated, searchable user listing; block/unblock operations; and
/// aggregate dashboard statistics for admin dashboards.
/// </summary>
public interface IAdminService
{
    /// <summary>Returns a paginated, optionally searched list of all registered users.</summary>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Number of results per page.</param>
    /// <param name="search">Optional search term matched against email or display name.</param>
    Task<PagedResult<AdminUserDto>> GetAllUsersAsync(int page = 1, int pageSize = 20, string? search = null);

    /// <summary>Blocks the specified user, preventing them from accessing the platform.</summary>
    /// <param name="userId">The identifier of the user to block.</param>
    Task BlockUserAsync(string userId);

    /// <summary>Unblocks the specified user, restoring their access to the platform.</summary>
    /// <param name="userId">The identifier of the user to unblock.</param>
    Task UnblockUserAsync(string userId);

    /// <summary>Returns aggregated platform statistics for the admin dashboard.</summary>
    Task<DashboardStatsDto> GetDashboardStatsAsync();

    /// <summary>Approves the specified item, making it publicly visible on the marketplace.</summary>
    /// <param name="itemId">The unique identifier of the item to approve.</param>
    Task ApproveItemAsync(Guid itemId);

    /// <summary>Returns a paginated list of items that are awaiting admin approval.</summary>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Number of results per page.</param>
    Task<List<ItemDto>> GetPendingItemsAsync(int page = 1, int pageSize = 50);
}
