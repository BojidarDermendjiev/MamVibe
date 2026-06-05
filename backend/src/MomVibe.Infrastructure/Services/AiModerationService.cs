namespace MomVibe.Infrastructure.Services;

using System.Text.Json;

using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Application.DTOs.Items;
using Application.Interfaces;
using Infrastructure.Configuration;
using Domain.Enums;

public class AiModerationService : IAiModerationService
{
    private readonly AnthropicSettings _settings;
    private readonly IApplicationDbContext _context;
    private readonly IChatClient _chatClient;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public AiModerationService(
        IOptions<AnthropicSettings> settings,
        IApplicationDbContext context,
        [FromKeyedServices("anthropic-cached")] IChatClient chatClient,
        IWebHostEnvironment env,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _settings          = settings.Value;
        _context           = context;
        _chatClient        = chatClient;
        _env               = env;
        _configuration     = configuration;
        _httpClientFactory = httpClientFactory;
    }

    private async Task<string> GetModelAsync()
    {
        var setting = await _context.AppSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == "AI:Model");
        return setting?.Value ?? _settings.Model;
    }

    private static string ExtractJson(string text)
    {
        var start = text.IndexOf('{');
        var end   = text.LastIndexOf('}');
        return start >= 0 && end > start ? text[start..(end + 1)] : text;
    }

    public async Task<AiModerationResultDto> ModerateItemAsync(
        string title, string description, string categoryName,
        ListingType listingType, decimal? price, string? firstPhotoUrl = null)
    {
        var priceText = price.HasValue ? $"{price} EUR" : "Free / Donation";
        var typeText  = listingType == ListingType.Sell ? "Sell" : "Donate";

        var prompt = $$"""
            You are a content moderator for MamVibe, a Bulgarian family marketplace for second-hand baby and children's items.
            Review this new listing and decide if it is appropriate for the platform.

            Title: {{title}}
            Category: {{categoryName}}
            Listing type: {{typeText}}
            Price: {{priceText}}
            Description: {{description}}

            Return ONLY valid JSON with no markdown or extra text:
            {
              "recommendation": "approve",
              "confidence": 0.95,
              "reason": "brief explanation under 100 words",
              "flags": []
            }

            recommendation must be one of:
            - "approve": clearly appropriate baby/children item, sensible description and price
            - "review": needs human check — vague description, unusual item, price seems off
            - "reject": spam, adult content, dangerous item, completely unrelated to babies/children

            confidence: 0.0 (totally unsure) to 1.0 (completely certain)
            flags: optional array of short concern tags from this list (use exact strings):
            - "contact-info" — description contains phone number, email, social handle, or any attempt to move communication off-platform
            - "price-anomaly" — price is suspiciously low or high for the item type
            - "unsafe-item" — item may be recalled, banned, or unsafe for children (e.g. drop-side crib, certain car seats)
            - "counterfeit-risk" — brand name used but item looks non-genuine
            - "category-mismatch" — item does not belong in the selected category
            - "spam" — duplicate listing or keyword stuffing
            """;

        var model   = await GetModelAsync();
        var options = new ChatOptions { ModelId = model, MaxOutputTokens = 300 };

        List<AIContent> parts = [];
        if (!string.IsNullOrEmpty(firstPhotoUrl))
        {
            var base64 = await FetchPhotoAsBase64Async(firstPhotoUrl);
            if (base64 != null)
                parts.Add(new DataContent(Convert.FromBase64String(base64), "image/jpeg"));
        }
        parts.Add(new TextContent(prompt));

        var result = await _chatClient.GetResponseAsync(
            [new ChatMessage(ChatRole.User, parts)],
            options);

        return ParseModerationResult(ExtractJson(result.Text ?? "{}"));
    }

    private async Task<string?> FetchPhotoAsBase64Async(string url)
    {
        try
        {
            if (url.StartsWith("/uploads/items/", StringComparison.OrdinalIgnoreCase))
            {
                var fileName   = Path.GetFileName(url);
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "items");
                var filePath   = Path.GetFullPath(Path.Combine(uploadsDir, fileName));

                if (!filePath.StartsWith(Path.GetFullPath(uploadsDir), StringComparison.OrdinalIgnoreCase))
                    return null;

                if (!File.Exists(filePath)) return null;

                var bytes = await File.ReadAllBytesAsync(filePath);
                if (bytes.Length > 5 * 1024 * 1024) return null;

                return Convert.ToBase64String(bytes);
            }

            if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                var r2PublicUrl = _configuration["R2:PublicUrl"];
                if (string.IsNullOrWhiteSpace(r2PublicUrl)) return null;

                // SSRF guard: only allow URLs from our own R2 bucket
                if (!url.StartsWith(r2PublicUrl.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                    return null;

                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);
                var resp = await httpClient.GetAsync(url);
                if (!resp.IsSuccessStatusCode) return null;

                var bytes = await resp.Content.ReadAsByteArrayAsync();
                if (bytes.Length > 5 * 1024 * 1024) return null;

                return Convert.ToBase64String(bytes);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static AiModerationResultDto ParseModerationResult(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var r = doc.RootElement;

            return new AiModerationResultDto
            {
                Recommendation = r.TryGetProperty("recommendation", out var rec)
                    ? rec.GetString() ?? "review" : "review",
                Confidence = r.TryGetProperty("confidence", out var conf) && conf.ValueKind == JsonValueKind.Number
                    ? conf.GetDouble() : 0.5,
                Reason = r.TryGetProperty("reason", out var reason)
                    ? reason.GetString() ?? string.Empty : string.Empty,
                Flags = r.TryGetProperty("flags", out var flags) && flags.ValueKind == JsonValueKind.Array
                    ? [.. flags.EnumerateArray().Select(f => f.GetString() ?? "").Where(f => f.Length > 0)]
                    : []
            };
        }
        catch
        {
            return new AiModerationResultDto { Recommendation = "review", Confidence = 0.5 };
        }
    }
}
