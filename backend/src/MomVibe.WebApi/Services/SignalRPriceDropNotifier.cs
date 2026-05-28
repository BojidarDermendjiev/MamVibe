namespace MomVibe.WebApi.Services;

using Microsoft.AspNetCore.SignalR;

using Application.Interfaces;
using Application.DTOs.Items;
using Hubs;

public class SignalRPriceDropNotifier : IPriceDropNotifier
{
    private readonly IHubContext<ChatHub, IChatClient> _hub;

    public SignalRPriceDropNotifier(IHubContext<ChatHub, IChatClient> hub)
        => this._hub = hub;

    public Task NotifyAsync(string userId, PriceDropNotification notification)
        => this._hub.Clients.Group($"user_{userId}").PriceDropped(notification);
}
