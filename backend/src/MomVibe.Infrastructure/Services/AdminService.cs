namespace MomVibe.Infrastructure.Services;

using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Domain.Enums;
using Domain.Entities;
using Application.Interfaces;
using Application.DTOs.Admin;
using Application.DTOs.Common;
using Application.DTOs.Items;

/// <summary>
/// Administrative service for user management, moderation, and dashboard statistics.
/// Optimized for case-insensitive search, paging, and reduced tracking overhead.
/// </summary>
public class AdminService : IAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public AdminService(UserManager<ApplicationUser> userManager, IApplicationDbContext context, IMapper mapper)
    {
        this._userManager = userManager;
        this._context = context;
        this._mapper = mapper;
    }

    public async Task<PagedResult<AdminUserDto>> GetAllUsersAsync(int page = 1, int pageSize = 20, string? search = null)
    {
        var query = this._userManager.Users.Include(u => u.Items).AsQueryable();

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

        var adminUsers = new List<AdminUserDto>();
        foreach (var user in users)
        {
            var dto = this._mapper.Map<AdminUserDto>(user);
            dto.Roles = (await this._userManager.GetRolesAsync(user)).ToList();
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
    }

    public async Task UnblockUserAsync(string userId)
    {
        var user = await this._userManager.FindByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found.");
        user.IsBlocked = false;
        await this._userManager.UpdateAsync(user);
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

    public async Task<List<ItemDto>> GetPendingItemsAsync()
    {
        var items = await this._context.Items
            .Where(i => !i.IsActive)
            .Include(i => i.Photos)
            .Include(i => i.User)
            .Include(i => i.Category)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return this._mapper.Map<List<ItemDto>>(items);
    }
}
