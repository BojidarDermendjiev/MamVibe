namespace MomVibe.Application.Interfaces;

using DTOs.Follows;
using DTOs.Items;
using DTOs.Common;

public interface IFollowService
{
    Task<FollowToggleResult> ToggleFollowAsync(string followerId, string followeeId);
    Task<bool> IsFollowingAsync(string followerId, string followeeId);
    Task<List<FollowUserDto>> GetFollowingAsync(string userId);
    Task<List<FollowUserDto>> GetFollowersAsync(string userId);
    Task<PagedResult<ItemDto>> GetFollowingFeedAsync(string userId, int page, int pageSize);
    Task<int> GetFollowerCountAsync(string userId);
}
