namespace MomVibe.Infrastructure.Services;

using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

using Domain.Enums;
using Domain.Entities;
using Application.Interfaces;
using Application.DTOs.Admin;
using Application.DTOs.Items;
using Application.DTOs.Common;
using Infrastructure.Configuration;
using MomVibe.Infrastructure.Persistence;

/// <summary>
/// Administrative service for user management, moderation, and dashboard statistics.
/// Optimized for case-insensitive search, paging, and reduced tracking overhead.
/// </summary>
public class AdminService : IAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _context;
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IOutboxWriter _outbox;
    private readonly N8nSettings _n8nSettings;
    private readonly IMemoryCache _cache;
    private readonly IDistributedCache _distributedCache;
    private readonly IAuditLogService _audit;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        UserManager<ApplicationUser> userManager,
        IApplicationDbContext context,
        ApplicationDbContext dbContext,
        IMapper mapper,
        IOutboxWriter outbox,
        IOptions<N8nSettings> n8nSettings,
        IMemoryCache cache,
        IDistributedCache distributedCache,
        IAuditLogService audit,
        ILogger<AdminService> logger)
    {
        this._userManager = userManager;
        this._context = context;
        this._dbContext = dbContext;
        this._mapper = mapper;
        this._outbox = outbox;
        this._n8nSettings = n8nSettings.Value;
        this._cache = cache;
        this._distributedCache = distributedCache;
        this._audit = audit;
        this._logger = logger;
    }

    public async Task<PagedResult<AdminUserDto>> GetAllUsersAsync(int page = 1, int pageSize = 20, string? search = null)
    {
        var query = this._userManager.Users.AsNoTracking().Include(u => u.Items).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search}%";
            query = query.Where(u => EF.Functions.ILike(u.DisplayName, pattern) || EF.Functions.ILike(u.Email!, pattern));
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Batch load roles to avoid N+1 queries
        var userIds = users.Select(u => u.Id).ToList();
        var userRoles = await this._dbContext.UserRoles
            .Where(ur => userIds.Contains(ur.UserId))
            .Join(this._dbContext.Roles,
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => new { ur.UserId, RoleName = r.Name! })
            .ToListAsync();
        var rolesByUser = userRoles.GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.RoleName).ToList());

        var adminUsers = new List<AdminUserDto>();
        foreach (var user in users)
        {
            var dto = this._mapper.Map<AdminUserDto>(user);
            dto.Roles = rolesByUser.GetValueOrDefault(user.Id, []);
            adminUsers.Add(dto);
        }

        return new PagedResult<AdminUserDto>
        {
            Items = adminUsers,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task BlockUserAsync(string userId)
    {
        var user = await this._userManager.FindByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found.");
        user.IsBlocked = true;
        await this._userManager.UpdateAsync(user);
        await this._distributedCache.RemoveAsync($"blocked:{userId}");
        await this._audit.LogAsync("admin", "Admin.BlockUser", success: true, targetId: userId);

        try
        {
            var body = new
            {
                Event = "user.blocked",
                Timestamp = DateTime.UtcNow,
                UserId = user.Id,
                Email = user.Email,
                user.DisplayName
            };
            this._outbox.Enqueue(OutboxMessageTypes.N8nWebhook, new N8nWebhookOutboxPayload(
                this._n8nSettings.UserBlocked,
                System.Text.Json.JsonSerializer.Serialize(body, OutboxJson)));
            await this._context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            this._logger.LogWarning(ex, "Failed to enqueue n8n user.blocked for user {UserId}", user.Id);
        }
    }

    private static readonly System.Text.Json.JsonSerializerOptions OutboxJson = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
    };

    public async Task UnblockUserAsync(string userId)
    {
        var user = await this._userManager.FindByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found.");
        user.IsBlocked = false;
        await this._userManager.UpdateAsync(user);
        await this._distributedCache.RemoveAsync($"blocked:{userId}");
        await this._audit.LogAsync("admin", "Admin.UnblockUser", success: true, targetId: userId);
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        const string cacheKey = "admin:dashboard:stats";
        if (this._cache.TryGetValue(cacheKey, out DashboardStatsDto? cached))
            return cached!;

        // Single EF Core query: all item/payment/message aggregates in one round-trip.
        // EF Core translates each correlated Count/Sum into a scalar subquery so the
        // database executes a single SELECT statement.
        var aggregatesTask = this._context.Items
            .GroupBy(_ => 1)
            .Select(_ => new
            {
                TotalItems    = this._context.Items.Count(),
                ActiveItems   = this._context.Items.Count(i => i.IsActive),
                TotalDonations = this._context.Items.Count(i => i.ListingType == ListingType.Donate),
                TotalSales    = this._context.Payments.Count(p => p.Status == PaymentStatus.Completed),
                TotalRevenue  = (decimal?)this._context.Payments
                                    .Where(p => p.Status == PaymentStatus.Completed)
                                    .Sum(p => p.Amount),
                TotalMessages = this._context.Messages.Count(),
            })
            .FirstOrDefaultAsync();

        // UserManager counts run in parallel against the same underlying store.
        var totalUsersTask  = this._userManager.Users.CountAsync();
        var blockedUsersTask = this._userManager.Users.CountAsync(u => u.IsBlocked);

        await Task.WhenAll(aggregatesTask, totalUsersTask, blockedUsersTask);

        var agg = aggregatesTask.Result;
        var stats = new DashboardStatsDto
        {
            TotalUsers    = totalUsersTask.Result,
            BlockedUsers  = blockedUsersTask.Result,
            TotalItems    = agg?.TotalItems    ?? 0,
            ActiveItems   = agg?.ActiveItems   ?? 0,
            TotalDonations = agg?.TotalDonations ?? 0,
            TotalSales    = agg?.TotalSales    ?? 0,
            TotalRevenue  = agg?.TotalRevenue  ?? 0m,
            TotalMessages = agg?.TotalMessages ?? 0,
        };

        this._cache.Set(cacheKey, stats, TimeSpan.FromMinutes(5));
        return stats;
    }

    public async Task ApproveItemAsync(Guid itemId, string adminId, string adminDisplayName)
    {
        var item = await this._context.Items.FindAsync(itemId)
            ?? throw new KeyNotFoundException("Item not found.");

        item.IsActive = true;

        this._context.ItemModerationLogs.Add(new ItemModerationLog
        {
            ItemId = itemId,
            AdminId = adminId,
            AdminDisplayName = adminDisplayName,
            Action = ModerationAction.Approved,
            AiStatusAtTime = item.AiModerationStatus,
            AiNotesAtTime = item.AiModerationNotes,
            ItemTitle = item.Title
        });

        await this._context.SaveChangesAsync();
    }

    public async Task AdminDeleteItemAsync(Guid itemId, string adminId, string adminDisplayName)
    {
        var item = await this._context.Items.FindAsync(itemId)
            ?? throw new KeyNotFoundException("Item not found.");

        this._context.ItemModerationLogs.Add(new ItemModerationLog
        {
            ItemId = itemId,
            AdminId = adminId,
            AdminDisplayName = adminDisplayName,
            Action = ModerationAction.Deleted,
            AiStatusAtTime = item.AiModerationStatus,
            AiNotesAtTime = item.AiModerationNotes,
            ItemTitle = item.Title
        });

        this._context.Items.Remove(item);
        await this._context.SaveChangesAsync();
    }

    public async Task<List<ItemDto>> GetPendingItemsAsync(int page = 1, int pageSize = 50)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);

        var items = await this._context.Items
            .AsNoTracking()
            .Where(i => !i.IsActive)
            .Include(i => i.Photos)
            .Include(i => i.User)
            .Include(i => i.Category)
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return this._mapper.Map<List<ItemDto>>(items);
    }

    public async Task<List<ModerationLogEntryDto>> GetModerationHistoryAsync(Guid itemId)
    {
        return await this._context.ItemModerationLogs
            .AsNoTracking()
            .Where(l => l.ItemId == itemId)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new ModerationLogEntryDto
            {
                AdminDisplayName = l.AdminDisplayName,
                Action = l.Action.ToString(),
                AiStatusAtTime = l.AiStatusAtTime.ToString(),
                AiNotesAtTime = l.AiNotesAtTime,
                Timestamp = l.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(
        int page = 1, int pageSize = 50,
        string? action = null, string? userId = null, bool? success = null)
    {
        pageSize = Math.Min(pageSize, 100);

        var query = this._context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(l => l.Action.StartsWith(action));

        if (!string.IsNullOrWhiteSpace(userId))
            query = query.Where(l => l.UserId == userId);

        if (success.HasValue)
            query = query.Where(l => l.Success == success.Value);

        var totalCount = await query.CountAsync();
        var logs = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new AuditLogDto
            {
                Id = l.Id,
                UserId = l.UserId,
                Action = l.Action,
                Success = l.Success,
                TargetId = l.TargetId,
                IpAddress = l.IpAddress,
                Details = l.Details,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<AuditLogDto>
        {
            Items = logs,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
