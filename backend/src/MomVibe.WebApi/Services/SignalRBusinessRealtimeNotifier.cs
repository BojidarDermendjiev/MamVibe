namespace MomVibe.WebApi.Services;

using Microsoft.AspNetCore.SignalR;

using MomVibe.Application.DTOs.Business;
using MomVibe.Application.Interfaces;
using MomVibe.WebApi.Hubs;

/// <summary>SignalR-backed implementation of <see cref="IBusinessRealtimeNotifier"/>.</summary>
public class SignalRBusinessRealtimeNotifier : IBusinessRealtimeNotifier
{
    private readonly IHubContext<BusinessHub, IBusinessClient> _hub;

    public SignalRBusinessRealtimeNotifier(IHubContext<BusinessHub, IBusinessClient> hub)
    {
        _hub = hub;
    }

    public Task NotifyViewDeltaAsync(string ownerUserId, ListingViewDelta delta)
        => _hub.Clients.Group(BusinessHub.GroupFor(ownerUserId)).ListingViewed(delta);

    public Task NotifyLikeDeltaAsync(string ownerUserId, ListingLikeDelta delta)
        => _hub.Clients.Group(BusinessHub.GroupFor(ownerUserId)).ListingLiked(delta);

    public Task NotifyCommentAsync(string ownerUserId, ListingCommentBroadcast broadcast)
        => _hub.Clients.Group(BusinessHub.GroupFor(ownerUserId)).ListingCommented(broadcast);

    public Task NotifySubscriptionStatusAsync(string ownerUserId, SubscriptionStatusBroadcast broadcast)
        => _hub.Clients.Group(BusinessHub.GroupFor(ownerUserId)).SubscriptionStatusChanged(broadcast);
}
