namespace MomVibe.Application.Events;

/// <summary>
/// Raised when a buyer submits a new purchase request. The handler pushes a SignalR
/// notification to the seller so their dashboard updates in real time.
/// </summary>
/// <param name="RequestId">The purchase request identifier.</param>
public sealed record PurchaseRequestCreatedEvent(Guid RequestId) : IDomainEvent;
