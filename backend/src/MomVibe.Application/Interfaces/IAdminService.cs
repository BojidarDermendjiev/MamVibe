namespace MomVibe.Application.Interfaces;

using DTOs.Admin;
using DTOs.Common;
using DTOs.Items;
using Domain.Enums;

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

    /// <summary>Approves the specified item and writes a moderation audit log entry.</summary>
    Task ApproveItemAsync(Guid itemId, string adminId, string adminDisplayName);

    /// <summary>Deletes the specified item as an admin and writes a moderation audit log entry.</summary>
    Task AdminDeleteItemAsync(Guid itemId, string adminId, string adminDisplayName);

    /// <summary>Returns a paginated list of items that are awaiting admin approval.</summary>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Number of results per page.</param>
    Task<List<ItemDto>> GetPendingItemsAsync(int page = 1, int pageSize = 50);

    /// <summary>Returns the full moderation action history for a specific item.</summary>
    Task<List<ModerationLogEntryDto>> GetModerationHistoryAsync(Guid itemId);

    /// <summary>Returns a paginated, filterable view of the security audit log.</summary>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Number of results per page (max 100).</param>
    /// <param name="action">Optional prefix filter on the Action field (e.g. "Auth", "Admin").</param>
    /// <param name="userId">Optional filter to a specific user ID.</param>
    /// <param name="success">Optional filter on the success flag.</param>
    Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(
        int page = 1, int pageSize = 50,
        string? action = null, string? userId = null, bool? success = null);
}
