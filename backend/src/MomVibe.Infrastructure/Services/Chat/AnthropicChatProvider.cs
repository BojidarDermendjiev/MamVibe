namespace MomVibe.Infrastructure.Services.Chat;

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MomVibe.Application.Interfaces;
using MomVibe.Infrastructure.Configuration;

/// <summary>
/// Chat provider backed by Anthropic Claude.
/// Uses the Anthropic Messages API with a top-level system prompt parameter.
/// </summary>
public class AnthropicChatProvider : ILlmChatProvider
{
    private readonly AnthropicSettings _settings;
    private readonly HttpClient _httpClient;

    public AnthropicChatProvider(
        IOptions<AnthropicSettings> settings,
        IHttpClientFactory httpClientFactory)
    {
        _settings = settings.Value;
        _httpClient = httpClientFactory.CreateClient("Anthropic");
    }

    public async Task<string> ChatAsync(
        string systemPrompt,
        IReadOnlyList<(string role, string content)> history,
        string model)
    {
        var messages = history.Select(h => new { role = h.role, content = h.content }).ToArray();

        var requestBody = new
        {
            model,
            max_tokens = 500,
            system = systemPrompt,
            messages
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        request.Headers.Add("x-api-key", _settings.ApiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? string.Empty;
    }
}
