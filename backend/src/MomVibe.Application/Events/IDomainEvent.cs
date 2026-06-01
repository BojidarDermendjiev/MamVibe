namespace MomVibe.Application.Events;

using MediatR;

/// <summary>
/// Marker interface for in-process domain events dispatched through MediatR.
/// </summary>
/// <remarks>
/// Events are published by the service that owns a state change after persistence
/// succeeds, and consumed by <see cref="INotificationHandler{TNotification}"/>
/// implementations registered in the DI container.
///
/// Naming convention: past-tense (<c>PaymentCompletedEvent</c>, <c>ItemCreatedEvent</c>) —
/// events represent facts that have already happened, not commands.
///
/// Keep payloads lean: pass identifiers and minimal context, and let handlers
/// re-fetch from the database when they need additional state. This keeps
/// events trivially serialisable (forward-compatible with the transactional
/// outbox in <c>Infrastructure/Outbox/</c>).
/// </remarks>
public interface IDomainEvent : INotification
{
}
