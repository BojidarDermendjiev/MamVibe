namespace MomVibe.Infrastructure.Services.Chat;

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MomVibe.Application.Interfaces;
using MomVibe.Infrastructure.Configuration;

/// <summary>
/// Chat provider backed by Groq's OpenAI-compatible inference API.
/// Free tier: 30 RPM / 14 400 RPD for llama-3.3-70b-versatile.
/// Docs: https://console.groq.com/docs/openai
/// </summary>
public class GroqChatProvider : ILlmChatProvider
{
    private readonly GroqSettings _settings;
    private readonly HttpClient _httpClient;

    public GroqChatProvider(
        IOptions<GroqSettings> settings,
        IHttpClientFactory httpClientFactory)
    {
        _settings = settings.Value;
        _httpClient = httpClientFactory.CreateClient("Groq");
    }

    public async Task<string> ChatAsync(
        string systemPrompt,
        IReadOnlyList<(string role, string content)> history,
        string model)
    {
        // Groq uses the OpenAI chat-completions format: system message as the first array element.
        var messages = new List<object>(history.Count + 1)
        {
            new { role = "system", content = systemPrompt }
        };
        foreach (var (role, content) in history)
            messages.Add(new { role, content });

        var requestBody = new
        {
            model,
            max_tokens = 500,
            messages
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
        request.Headers.Add("Authorization", $"Bearer {_settings.ApiKey}");
        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;
    }
}
