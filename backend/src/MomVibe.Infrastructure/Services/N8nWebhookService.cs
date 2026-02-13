namespace MomVibe.Infrastructure.Services;

using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Application.Interfaces;
using Infrastructure.Configuration;

/// <summary>
/// Background service that drains a bounded channel and POSTs JSON payloads to n8n webhooks.
/// Implements both <see cref="BackgroundService"/> and <see cref="IN8nWebhookService"/>.
/// </summary>
public class N8nWebhookService : BackgroundService, IN8nWebhookService
{
    private readonly Channel<(string Path, object Payload)> _channel =
        Channel.CreateBounded<(string, object)>(500);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly N8nSettings _settings;
    private readonly ILogger<N8nWebhookService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public N8nWebhookService(
        IHttpClientFactory httpClientFactory,
        IOptions<N8nSettings> settings,
        ILogger<N8nWebhookService> logger)
    {
        this._httpClientFactory = httpClientFactory;
        this._settings = settings.Value;
        this._logger = logger;
    }

    public void Send(string webhookPath, object payload)
    {
        if (!this._settings.Enabled) return;

        if (!this._channel.Writer.TryWrite((webhookPath, payload)))
        {
            this._logger.LogWarning("n8n webhook channel is full — dropping event for {Path}", webhookPath);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var (path, payload) in this._channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                var client = this._httpClientFactory.CreateClient("N8n");
                var url = this._settings.BaseUrl.TrimEnd('/') + "/" + path.TrimStart('/');
                var json = JsonSerializer.Serialize(payload, JsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content, stoppingToken);

                if (!response.IsSuccessStatusCode)
                {
                    this._logger.LogWarning(
                        "n8n webhook {Path} returned {StatusCode}",
                        path, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                this._logger.LogWarning(ex, "Failed to send n8n webhook for {Path}", path);
            }
        }
    }
}
