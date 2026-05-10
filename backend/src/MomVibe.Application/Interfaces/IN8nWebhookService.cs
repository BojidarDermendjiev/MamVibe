namespace MomVibe.Application.Interfaces;

/// <summary>
/// Fire-and-forget webhook dispatcher for n8n integration.
/// Queues payloads onto a background channel — never blocks the caller.
/// </summary>
public interface IN8nWebhookService
{
    /// <summary>Enqueues a payload to be POSTed to the specified n8n webhook path without blocking the caller.</summary>
    /// <param name="webhookPath">The relative n8n webhook path (e.g. "payment-completed").</param>
    /// <param name="payload">The object to serialize and send as JSON.</param>
    void Send(string webhookPath, object payload);
}
