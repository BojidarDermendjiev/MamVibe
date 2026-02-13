namespace MomVibe.Application.Interfaces;

/// <summary>
/// Fire-and-forget webhook dispatcher for n8n integration.
/// Queues payloads onto a background channel — never blocks the caller.
/// </summary>
public interface IN8nWebhookService
{
    void Send(string webhookPath, object payload);
}
