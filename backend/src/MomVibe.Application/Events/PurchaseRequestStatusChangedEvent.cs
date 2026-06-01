namespace MomVibe.Application.Events;

using Domain.Enums;

/// <summary>
/// Raised when a purchase request transitions to a new <see cref="PurchaseRequestStatus"/>
/// (Accepted, Declined, Completed). Subscribers push real-time updates back to the buyer.
/// </summary>
/// <param name="RequestId">The purchase request identifier.</param>
/// <param name="NewStatus">The status the request transitioned into.</param>
public sealed record PurchaseRequestStatusChangedEvent(Guid RequestId, PurchaseRequestStatus NewStatus) : IDomainEvent;
