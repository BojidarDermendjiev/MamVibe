namespace MomVibe.Infrastructure.Outbox;

using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Options;

using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Configuration;

/// <summary>
/// Dispatcher for <see cref="OutboxMessageTypes.N8nWebhook"/> messages. POSTs the stored
/// JSON body to the configured n8n base URL + relative path. Failures throw so that
/// <c>OutboxProcessor</c> applies its retry + backoff policy.
/// </summary>
public sealed class N8nOutboxDispatcher : IOutboxMessageDispatcher
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly N8nSettings _settings;

    public N8nOutboxDispatcher(IHttpClientFactory httpClientFactory, IOptions<N8nSettings> settings)
    {
        this._httpClientFactory = httpClientFactory;
        this._settings = settings.Value;
    }

    public string MessageType => OutboxMessageTypes.N8nWebhook;

    public async Task DispatchAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Deserialize<N8nWebhookOutboxPayload>(message.Payload)
            ?? throw new InvalidOperationException("Outbox payload could not be deserialized as N8nWebhookOutboxPayload.");

        // Honour the global Enabled toggle: when n8n is disabled in config we still
        // accept the enqueue (so callers don't branch) but skip the network call here.
        if (!this._settings.Enabled) return;

        var client = this._httpClientFactory.CreateClient("N8n");
        var url = this._settings.BaseUrl.TrimEnd('/') + "/" + payload.Path.TrimStart('/');
        using var content = new StringContent(payload.PayloadJson, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(url, content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
