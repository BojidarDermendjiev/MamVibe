namespace MomVibe.WebApi.Services;

using Microsoft.AspNetCore.SignalR;

using Application.Interfaces;
using Application.DTOs.Follows;
using Application.DTOs.Items;
using Hubs;

public class SignalRFollowNotifier : IFollowNotifier
{
    private readonly IHubContext<ChatHub, IChatClient> _hub;

    public SignalRFollowNotifier(IHubContext<ChatHub, IChatClient> hub)
        => this._hub = hub;

    public Task NotifyNewFollowerAsync(string followeeId, NewFollowerNotification notification)
        => this._hub.Clients.Group($"user_{followeeId}").NewFollower(notification);

    public async Task NotifyFollowersOfNewItemAsync(IEnumerable<string> followerIds, ItemDto item)
    {
        foreach (var followerId in followerIds)
            await this._hub.Clients.Group($"user_{followerId}").NewItemFromFollowedSeller(item);
    }
}
