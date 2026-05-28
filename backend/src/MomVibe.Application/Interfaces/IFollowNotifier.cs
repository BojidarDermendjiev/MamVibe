namespace MomVibe.Application.Interfaces;

using DTOs.Follows;
using DTOs.Items;

public interface IFollowNotifier
{
    Task NotifyNewFollowerAsync(string followeeId, NewFollowerNotification notification);
    Task NotifyFollowersOfNewItemAsync(IEnumerable<string> followerIds, ItemDto item);
}
