namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

using Common;

/// <summary>
/// Transactional outbox row. Holds a serialized payload destined for an external system
/// (n8n webhook, push notification, third-party email, …) so delivery survives process
/// crashes between the moment the business state was committed and the moment the
/// external call succeeds.
/// </summary>
/// <remarks>
/// Persisted in the same EF Core <c>SaveChangesAsync</c> as the originating state change,
/// then drained by <c>OutboxProcessor</c> with retry + exponential backoff.
/// The composite index on <c>(ProcessedAt, NextAttemptAt)</c> keeps the queue query cheap.
/// </remarks>
[Index(nameof(ProcessedAt), nameof(NextAttemptAt))]
public class OutboxMessage : BaseEntity
{
    /// <summary>
    /// Discriminator that the <c>IOutboxMessageDispatcher</c> uses to route the payload
    /// to the correct external-system adapter (e.g. <c>"N8nWebhook"</c>).
    /// </summary>
    [Required]
    [MaxLength(100)]
    public required string MessageType { get; set; }

    /// <summary>
    /// JSON-serialized payload. Schema is owned by each dispatcher implementation.
    /// </summary>
    [Required]
    public required string Payload { get; set; }

    /// <summary>
    /// UTC timestamp when this message was successfully dispatched. <c>null</c> while still pending.
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Number of delivery attempts so far. Bounded by <c>OutboxProcessor.MaxAttempts</c>;
    /// once exhausted the message is left with <c>ProcessedAt = null</c> and inspected manually.
    /// </summary>
    public int AttemptCount { get; set; }

    /// <summary>
    /// UTC timestamp at which the next delivery attempt may begin. Set to <see cref="BaseEntity.CreatedAt"/>
    /// on insert; bumped forward by an exponential backoff on each failure.
    /// </summary>
    public DateTime NextAttemptAt { get; set; }

    /// <summary>
    /// Short truncated description of the last failure, for operational triage. <c>null</c> on success.
    /// </summary>
    [MaxLength(1000)]
    public string? LastError { get; set; }
}
