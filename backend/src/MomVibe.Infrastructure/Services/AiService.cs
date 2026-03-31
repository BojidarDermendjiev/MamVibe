namespace MomVibe.Infrastructure.Services;

using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Application.DTOs.Items;
using Application.Interfaces;
using Infrastructure.Configuration;
using Domain.Enums;

/// <summary>
/// Calls the Anthropic Claude API with a product photo to generate item listing suggestions.
/// </summary>
public class AiService : IAiService
{
    private readonly AnthropicSettings _settings;
    private readonly IApplicationDbContext _context;
    private readonly HttpClient _httpClient;

    private static readonly string[] AllowedContentTypes =
        ["image/jpeg", "image/jpg", "image/png", "image/webp"];

    public AiService(
        IOptions<AnthropicSettings> settings,
        IApplicationDbContext context,
        IHttpClientFactory httpClientFactory)
    {
        _settings = settings.Value;
        _context = context;
        _httpClient = httpClientFactory.CreateClient("Anthropic");
    }

    public async Task<AiListingSuggestionDto> SuggestListingAsync(IFormFile photo)
    {
        if (photo.Length > 5 * 1024 * 1024)
            throw new InvalidOperationException("Photo must be under 5 MB.");

        if (!AllowedContentTypes.Contains(photo.ContentType.ToLowerInvariant()))
            throw new InvalidOperationException("Unsupported image format. Use JPEG, PNG, or WebP.");

        var categorySlugs = await _context.Categories
            .AsNoTracking()
            .Select(c => c.Slug)
            .ToListAsync();

        using var ms = new MemoryStream();
        await photo.CopyToAsync(ms);
        var base64 = Convert.ToBase64String(ms.ToArray());
        var mediaType = ResolveMediaType(photo.ContentType);

        var requestBody = new
        {
            model = _settings.Model,
            max_tokens = 500,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "image",
                            source = new { type = "base64", media_type = mediaType, data = base64 }
                        },
                        new
                        {
                            type = "text",
                            text = BuildPrompt(categorySlugs)
                        }
                    }
                }
            }
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
        var text = doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? "{}";

        return ParseSuggestion(ExtractJson(text));
    }

    private static string BuildPrompt(IEnumerable<string> categorySlugs)
    {
        var slugList = string.Join(", ", categorySlugs);
        return $$"""
            You are a listing assistant for MamVibe, a Bulgarian marketplace for second-hand baby and children's items.
            Analyze this product image and suggest listing details.

            Available category slugs (use one exactly as written):
            {{slugList}}

            Age group values: Newborn=0, Infant=1, Toddler=2, Preschool=3, SchoolAge=4, Teen=5

            Return ONLY valid JSON with this exact structure — no markdown, no extra text:
            {
              "title": "concise English product title (max 80 characters)",
              "description": "2-3 sentence English description covering visible condition and key features",
              "categorySlug": "one slug from the list above",
              "listingType": 1 for Sell or 0 for Donate,
              "suggestedPrice": price in BGN as a number if selling, null if donating,
              "ageGroup": integer 0-5 or null,
              "clothingSize": EU clothing size as integer (e.g. 68, 74, 80, 86, 92) or null,
              "shoeSize": EU shoe size as integer or null
            }

            Rules:
            - Use Sell (1) for good/excellent condition items, Donate (0) for heavily worn ones
            - If a field cannot be determined from the image, use null
            """;
    }

    private static string ResolveMediaType(string contentType) =>
        contentType.ToLowerInvariant() switch
        {
            "image/png" => "image/png",
            "image/webp" => "image/webp",
            _ => "image/jpeg"
        };

    private static string ExtractJson(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        return start >= 0 && end > start ? text[start..(end + 1)] : text;
    }

    private static AiListingSuggestionDto ParseSuggestion(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var r = doc.RootElement;

            return new AiListingSuggestionDto
            {
                Title = r.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "",
                Description = r.TryGetProperty("description", out var d) ? d.GetString() ?? "" : "",
                CategorySlug = r.TryGetProperty("categorySlug", out var s) ? s.GetString() ?? "" : "",
                ListingType = r.TryGetProperty("listingType", out var lt) && lt.GetInt32() == 1
                    ? ListingType.Sell
                    : ListingType.Donate,
                SuggestedPrice = r.TryGetProperty("suggestedPrice", out var p) && p.ValueKind == JsonValueKind.Number
                    ? p.GetDecimal()
                    : null,
                AgeGroup = r.TryGetProperty("ageGroup", out var ag) && ag.ValueKind == JsonValueKind.Number
                    ? (AgeGroup)ag.GetInt32()
                    : null,
                ClothingSize = r.TryGetProperty("clothingSize", out var cs) && cs.ValueKind == JsonValueKind.Number
                    ? cs.GetInt32()
                    : null,
                ShoeSize = r.TryGetProperty("shoeSize", out var ss) && ss.ValueKind == JsonValueKind.Number
                    ? ss.GetInt32()
                    : null,
            };
        }
        catch
        {
            return new AiListingSuggestionDto();
        }
    }

    public async Task<AiModerationResultDto> ModerateItemAsync(
        string title,
        string description,
        string categoryName,
        ListingType listingType,
        decimal? price)
    {
        var priceText = price.HasValue ? $"{price} BGN" : "Free / Donation";
        var typeText = listingType == ListingType.Sell ? "Sell" : "Donate";

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
            flags: optional array of short concern tags, empty if none
            """;

        var requestBody = new
        {
            model = _settings.Model,
            max_tokens = 300,
            messages = new[] { new { role = "user", content = prompt } }
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
        var text = doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? "{}";

        return ParseModerationResult(ExtractJson(text));
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
                    ? rec.GetString() ?? "review"
                    : "review",
                Confidence = r.TryGetProperty("confidence", out var conf) && conf.ValueKind == JsonValueKind.Number
                    ? conf.GetDouble()
                    : 0.5,
                Reason = r.TryGetProperty("reason", out var reason)
                    ? reason.GetString() ?? string.Empty
                    : string.Empty,
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
