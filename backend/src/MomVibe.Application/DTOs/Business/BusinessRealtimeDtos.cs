namespace MomVibe.Application.DTOs.Business;

using Domain.Enums;

/// <summary>Delta payload broadcast when a listing's view counter changes.</summary>
public sealed record ListingViewDelta(Guid ListingId, long NewViewCount);

/// <summary>Delta payload broadcast when a listing's like counter changes.</summary>
public sealed record ListingLikeDelta(Guid ListingId, long NewLikeCount, bool ActorLiked);

/// <summary>Broadcast when a comment is posted or hidden on a listing.</summary>
public sealed record ListingCommentBroadcast(Guid ListingId, BusinessListingCommentDto Comment, bool Deleted);

/// <summary>Broadcast when the subscription status changes via webhook.</summary>
public sealed record SubscriptionStatusBroadcast(
    Guid BusinessProfileId,
    BusinessSubscriptionStatus Status,
    string PlanCode);
