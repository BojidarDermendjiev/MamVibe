namespace MomVibe.Infrastructure.Services;

using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
    private readonly IN8nWebhookService _webhook;
    private readonly N8nSettings _n8nSettings;
    private readonly IMemoryCache _cache;

    public AdminService(
        UserManager<ApplicationUser> userManager,
        IApplicationDbContext context,
        ApplicationDbContext dbContext,
        IMapper mapper,
        IN8nWebhookService webhook,
        IOptions<N8nSettings> n8nSettings,
        IMemoryCache cache)
    {
        this._userManager = userManager;
        this._context = context;
        this._dbContext = dbContext;
        this._mapper = mapper;
        this._webhook = webhook;
        this._n8nSettings = n8nSettings.Value;
        this._cache = cache;
    }

    public async Task<PagedResult<AdminUserDto>> GetAllUsersAsync(int page = 1, int pageSize = 20, string? search = null)
    {
        var query = this._userManager.Users.AsNoTracking().Include(u => u.Items).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(u => u.DisplayName.ToLower().Contains(term) || u.Email!.ToLower().Contains(term));
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
        this._cache.Remove($"blocked:{userId}"); // Invalidate immediately — don't wait for TTL

        try
        {
            this._webhook.Send(this._n8nSettings.UserBlocked, new
            {
                Event = "user.blocked",
                Timestamp = DateTime.UtcNow,
                UserId = user.Id,
                Email = user.Email,
                user.DisplayName
            });
        }
        catch { /* Webhook failure must not break admin flow */ }
    }

    public async Task UnblockUserAsync(string userId)
    {
        var user = await this._userManager.FindByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found.");
        user.IsBlocked = false;
        await this._userManager.UpdateAsync(user);
        this._cache.Remove($"blocked:{userId}"); // Invalidate immediately — don't wait for TTL
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        return new DashboardStatsDto
        {
            TotalUsers = await this._userManager.Users.CountAsync(),
            TotalItems = await this._context.Items.CountAsync(),
            ActiveItems = await this._context.Items.CountAsync(i => i.IsActive),
            TotalDonations = await this._context.Items.CountAsync(i => i.ListingType == ListingType.Donate),
            TotalSales = await this._context.Payments.CountAsync(p => p.Status == PaymentStatus.Completed),
            TotalRevenue = await this._context.Payments
                .Where(p => p.Status == PaymentStatus.Completed)
                .SumAsync(p => p.Amount),
            TotalMessages = await this._context.Messages.CountAsync(),
            BlockedUsers = await this._userManager.Users.CountAsync(u => u.IsBlocked)
        };
    }

    public async Task ApproveItemAsync(Guid itemId)
    {
        var item = await this._context.Items.FindAsync(itemId);
        if (item == null) throw new KeyNotFoundException("Item not found.");
        item.IsActive = true;
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
}
