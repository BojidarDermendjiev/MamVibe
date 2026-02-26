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
    Task<PagedResult<AdminUserDto>> GetAllUsersAsync(int page = 1, int pageSize = 20, string? search = null);
    Task BlockUserAsync(string userId);
    Task UnblockUserAsync(string userId);
    Task<DashboardStatsDto> GetDashboardStatsAsync();
    Task ApproveItemAsync(Guid itemId);
    Task<List<ItemDto>> GetPendingItemsAsync(int page = 1, int pageSize = 50);
}
