namespace MomVibe.WebApi.Hubs;

using MomVibe.Application.DTOs.Business;

/// <summary>
/// Strongly-typed contract of the client-side methods invoked by <see cref="BusinessHub"/>.
/// Mirrored on the frontend SignalR connection in <c>BusinessHubContext</c>.
/// </summary>
public interface IBusinessClient
{
    Task ListingViewed(ListingViewDelta delta);
    Task ListingLiked(ListingLikeDelta delta);
    Task ListingCommented(ListingCommentBroadcast broadcast);
    Task SubscriptionStatusChanged(SubscriptionStatusBroadcast broadcast);
}
