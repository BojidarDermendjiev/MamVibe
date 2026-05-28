namespace MomVibe.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;

using Domain.Entities;
using Application.Interfaces;
using Application.DTOs.Follows;
using Application.DTOs.Items;
using Application.DTOs.Common;

public class FollowService : IFollowService
{
    private readonly IApplicationDbContext _context;
    private readonly IFollowNotifier _notifier;

    public FollowService(IApplicationDbContext context, IFollowNotifier notifier)
    {
        this._context = context;
        this._notifier = notifier;
    }

    public async Task<FollowToggleResult> ToggleFollowAsync(string followerId, string followeeId)
    {
        if (followerId == followeeId)
            throw new InvalidOperationException("You cannot follow yourself.");

        var existing = await this._context.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);

        bool isNowFollowing;
        if (existing != null)
        {
            this._context.Follows.Remove(existing);
            await this._context.SaveChangesAsync();
            isNowFollowing = false;
        }
        else
        {
            var follow = new Follow { FollowerId = followerId, FolloweeId = followeeId };
            this._context.Follows.Add(follow);
            try
            {
                await this._context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new KeyNotFoundException("User not found.");
            }
            isNowFollowing = true;

            // Best-effort SignalR push — failure must never block the toggle
            try
            {
                var follower = await this._context.Follows
                    .AsNoTracking()
                    .Where(f => f.FollowerId == followerId && f.FolloweeId == followeeId)
                    .Select(f => new { f.Follower.DisplayName, f.Follower.AvatarUrl, f.CreatedAt })
                    .FirstOrDefaultAsync();

                if (follower != null)
                {
                    await this._notifier.NotifyNewFollowerAsync(followeeId, new NewFollowerNotification
                    {
                        FollowerId = followerId,
                        FollowerDisplayName = follower.DisplayName,
                        FollowerAvatarUrl = follower.AvatarUrl,
                        FollowedAt = follower.CreatedAt
                    });
                }
            }
            catch { /* ignore */ }
        }

        var followerCount = await this._context.Follows
            .CountAsync(f => f.FolloweeId == followeeId);

        return new FollowToggleResult { IsFollowing = isNowFollowing, FollowerCount = followerCount };
    }

    public async Task<bool> IsFollowingAsync(string followerId, string followeeId)
        => await this._context.Follows
            .AnyAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);

    public async Task<List<FollowUserDto>> GetFollowingAsync(string userId)
    {
        return await this._context.Follows
            .AsNoTracking()
            .Where(f => f.FollowerId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FollowUserDto
            {
                Id = f.Followee.Id,
                DisplayName = f.Followee.DisplayName,
                AvatarUrl = f.Followee.AvatarUrl,
                IsOnHoliday = f.Followee.IsOnHoliday,
                FollowerCount = f.Followee.Followers.Count,
                ItemCount = f.Followee.Items.Count(i => i.IsActive),
                FollowedAt = f.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<FollowUserDto>> GetFollowersAsync(string userId)
    {
        return await this._context.Follows
            .AsNoTracking()
            .Where(f => f.FolloweeId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FollowUserDto
            {
                Id = f.Follower.Id,
                DisplayName = f.Follower.DisplayName,
                AvatarUrl = f.Follower.AvatarUrl,
                IsOnHoliday = f.Follower.IsOnHoliday,
                FollowerCount = f.Follower.Followers.Count,
                ItemCount = f.Follower.Items.Count(i => i.IsActive),
                FollowedAt = f.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<PagedResult<ItemDto>> GetFollowingFeedAsync(string userId, int page, int pageSize)
    {
        var followedIds = await this._context.Follows
            .AsNoTracking()
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FolloweeId)
            .ToListAsync();

        if (followedIds.Count == 0)
            return new PagedResult<ItemDto> { Items = [], TotalCount = 0, Page = page, PageSize = pageSize };

        var query = this._context.Items
            .AsNoTracking()
            .Include(i => i.Photos)
            .Include(i => i.User)
            .Include(i => i.Category)
            .Where(i => i.IsActive && followedIds.Contains(i.UserId))
            .OrderByDescending(i => i.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ItemDto>
        {
            Items = items.Select(MapItemToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<int> GetFollowerCountAsync(string userId)
        => await this._context.Follows.CountAsync(f => f.FolloweeId == userId);

    private static ItemDto MapItemToDto(Item i) => new()
    {
        Id = i.Id,
        Title = i.Title,
        Description = i.Description,
        CategoryId = i.CategoryId,
        CategoryName = i.Category?.Name ?? string.Empty,
        ListingType = i.ListingType,
        AgeGroup = i.AgeGroup,
        ShoeSize = i.ShoeSize,
        ClothingSize = i.ClothingSize,
        Price = i.Price,
        UserId = i.UserId,
        UserDisplayName = i.User?.DisplayName ?? string.Empty,
        UserAvatarUrl = i.User?.AvatarUrl,
        UserIsOnHoliday = i.User?.IsOnHoliday ?? false,
        Condition = i.Condition,
        IsActive = i.IsActive,
        IsReserved = i.IsReserved,
        IsSold = i.IsSold,
        ViewCount = i.ViewCount,
        LikeCount = i.LikeCount,
        Photos = i.Photos.OrderBy(p => p.DisplayOrder)
            .Select(p => new ItemPhotoDto { Id = p.Id, Url = p.Url, DisplayOrder = p.DisplayOrder })
            .ToList(),
        BumpedAt = i.BumpedAt,
        CreatedAt = i.CreatedAt,
        AiModerationStatus = i.AiModerationStatus,
        AiModerationNotes = i.AiModerationNotes,
        AiModerationScore = i.AiModerationScore
    };
}
