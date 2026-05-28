namespace MomVibe.WebApi.Services;

using Microsoft.AspNetCore.SignalR;

using Application.Interfaces;
using Application.DTOs.SavedSearches;
using Hubs;

public class SignalRSavedSearchNotifier : ISavedSearchNotifier
{
    private readonly IHubContext<ChatHub, IChatClient> _hub;

    public SignalRSavedSearchNotifier(IHubContext<ChatHub, IChatClient> hub)
        => this._hub = hub;

    public Task NotifyAsync(string userId, SavedSearchMatchNotification notification)
        => this._hub.Clients.Group($"user_{userId}").SavedSearchMatch(notification);
}
