namespace MomVibe.Application.Interfaces;

using MomVibe.Domain.Entities;

/// <summary>
/// Routes a single <see cref="OutboxMessage"/> to its concrete external-system adapter.
/// One implementation per <see cref="OutboxMessageTypes"/> value; <c>OutboxProcessor</c>
/// picks the right one by <c>MessageType</c>.
/// </summary>
public interface IOutboxMessageDispatcher
{
    /// <summary>The <see cref="OutboxMessage.MessageType"/> values this dispatcher handles.</summary>
    string MessageType { get; }

    /// <summary>
    /// Performs the external-system call. Returning normally signals success and the
    /// processor marks <see cref="OutboxMessage.ProcessedAt"/>. Throwing signals failure,
    /// the processor increments <see cref="OutboxMessage.AttemptCount"/>, records the
    /// exception in <see cref="OutboxMessage.LastError"/>, and schedules the next attempt.
    /// </summary>
    Task DispatchAsync(OutboxMessage message, CancellationToken cancellationToken);
}
