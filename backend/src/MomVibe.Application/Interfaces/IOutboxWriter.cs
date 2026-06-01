namespace MomVibe.Application.Interfaces;

/// <summary>
/// Writes a row to the transactional outbox so a deferred side-effect (n8n webhook,
/// push notification, e-bill email, …) is durable from the moment the originating
/// business state is committed.
/// </summary>
/// <remarks>
/// Implementations must add the entity to the EF Core change tracker WITHOUT calling
/// <c>SaveChangesAsync</c> — the caller commits the outbox row in the same transaction
/// as the business state change. Calling SaveChanges here would split the unit of work
/// and re-introduce the lost-message risk this pattern exists to eliminate.
///
/// On the read side, <c>OutboxProcessor</c> polls pending rows and dispatches each one
/// through an <c>IOutboxMessageDispatcher</c> keyed by <see cref="OutboxMessageTypes"/>.
/// </remarks>
public interface IOutboxWriter
{
    /// <summary>
    /// Stages a new outbox message for the current EF Core unit of work.
    /// </summary>
    /// <typeparam name="T">Concrete payload type — serialized to JSON for storage.</typeparam>
    /// <param name="messageType">Routing discriminator (see <see cref="OutboxMessageTypes"/>).</param>
    /// <param name="payload">Payload object; serialized with camelCase JSON naming.</param>
    void Enqueue<T>(string messageType, T payload) where T : notnull;
}

/// <summary>
/// Stable string constants identifying outbox payload schemas. The processor looks up
/// an <c>IOutboxMessageDispatcher</c> implementation by this discriminator at dispatch time.
/// </summary>
public static class OutboxMessageTypes
{
    /// <summary>Payload <see cref="N8nWebhookOutboxPayload"/> — POSTed to an n8n webhook path.</summary>
    public const string N8nWebhook = "N8nWebhook";
}

/// <summary>
/// Wire format for an n8n webhook delivered through the transactional outbox.
/// </summary>
/// <param name="Path">Relative n8n webhook path (e.g. <c>"payment-completed"</c>).</param>
/// <param name="PayloadJson">
/// Already-serialized JSON body. Pre-serializing at enqueue time captures the original
/// payload state and removes any dependency on the originating service's types at dispatch.
/// </param>
public sealed record N8nWebhookOutboxPayload(string Path, string PayloadJson);
