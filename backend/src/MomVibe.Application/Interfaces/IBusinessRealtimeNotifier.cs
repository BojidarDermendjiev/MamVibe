namespace MomVibe.Application.Interfaces;

using DTOs.Business;

/// <summary>
/// SignalR-backed notifier for the business owner's live dashboard. All methods are
/// fire-and-forget from the caller's perspective; the implementation handles
/// per-profile group routing.
/// </summary>
public interface IBusinessRealtimeNotifier
{
    /// <summary>Push a view-count delta to the listing owner.</summary>
    Task NotifyViewDeltaAsync(string ownerUserId, ListingViewDelta delta);

    /// <summary>Push a like-count delta to the listing owner.</summary>
    Task NotifyLikeDeltaAsync(string ownerUserId, ListingLikeDelta delta);

    /// <summary>Push a new or deleted comment to the listing owner.</summary>
    Task NotifyCommentAsync(string ownerUserId, ListingCommentBroadcast broadcast);

    /// <summary>Push a subscription status transition to the listing owner.</summary>
    Task NotifySubscriptionStatusAsync(string ownerUserId, SubscriptionStatusBroadcast broadcast);
}
