namespace MomVibe.Infrastructure.Services;

using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
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
    private readonly IWebHostEnvironment _env;

    private static readonly string[] AllowedContentTypes =
        ["image/jpeg", "image/jpg", "image/png", "image/webp"];

    public AiService(
        IOptions<AnthropicSettings> settings,
        IApplicationDbContext context,
        IHttpClientFactory httpClientFactory,
        IWebHostEnvironment env)
    {
        _settings = settings.Value;
        _context = context;
        _httpClient = httpClientFactory.CreateClient("Anthropic");
        _env = env;
    }

    private async Task<string> GetModelAsync()
    {
        var setting = await _context.AppSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == "AI:Model");
        return setting?.Value ?? _settings.Model;
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
            model = await GetModelAsync(),
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
        decimal? price,
        string? firstPhotoUrl = null)
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
            flags: optional array of short concern tags from this list (use exact strings):
            - "contact-info" — description contains phone number, email, social handle, or any attempt to move communication off-platform
            - "price-anomaly" — price is suspiciously low or high for the item type
            - "unsafe-item" — item may be recalled, banned, or unsafe for children (e.g. drop-side crib, certain car seats)
            - "counterfeit-risk" — brand name used but item looks non-genuine
            - "category-mismatch" — item does not belong in the selected category
            - "spam" — duplicate listing or keyword stuffing
            """;

        object messageContent;
        if (!string.IsNullOrEmpty(firstPhotoUrl))
        {
            var photoBase64 = await FetchPhotoAsBase64Async(firstPhotoUrl);
            if (photoBase64 != null)
            {
                messageContent = new object[]
                {
                    new { type = "image", source = new { type = "base64", media_type = "image/jpeg", data = photoBase64 } },
                    new { type = "text", text = prompt }
                };
            }
            else
            {
                messageContent = prompt;
            }
        }
        else
        {
            messageContent = prompt;
        }

        var requestBody = new
        {
            model = await GetModelAsync(),
            max_tokens = 300,
            messages = new[] { new { role = "user", content = messageContent } }
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

    private async Task<string?> FetchPhotoAsBase64Async(string url)
    {
        try
        {
            // Only allow photos stored in our own uploads directory.
            // Reject absolute URLs and anything outside /uploads/items/ to prevent SSRF.
            if (!url.StartsWith("/uploads/items/", StringComparison.OrdinalIgnoreCase))
                return null;

            var fileName = Path.GetFileName(url);
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "items");
            var filePath   = Path.GetFullPath(Path.Combine(uploadsDir, fileName));

            // Path traversal guard: resolved path must stay inside uploads dir.
            if (!filePath.StartsWith(Path.GetFullPath(uploadsDir), StringComparison.OrdinalIgnoreCase))
                return null;

            if (!File.Exists(filePath)) return null;

            var bytes = await File.ReadAllBytesAsync(filePath);
            if (bytes.Length > 5 * 1024 * 1024) return null;

            return Convert.ToBase64String(bytes);
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

    public async Task<PriceSuggestionResultDto> SuggestPriceAsync(
        string title,
        string description,
        string categoryName,
        AgeGroup? ageGroup,
        int? clothingSize,
        int? shoeSize,
        IReadOnlyList<decimal> comparablePrices)
    {
        var comparablesText = comparablePrices.Count > 0
            ? $"Comparable listings currently on this platform ({comparablePrices.Count} items): {string.Join(", ", comparablePrices.Select(p => $"{p} BGN"))}"
            : "No comparable listings found on this platform yet — use general knowledge of the Bulgarian second-hand baby market.";

        var sizeContext = new List<string>();
        if (ageGroup.HasValue) sizeContext.Add($"Age group: {ageGroup.Value}");
        if (clothingSize.HasValue) sizeContext.Add($"Clothing size: EU {clothingSize.Value}");
        if (shoeSize.HasValue) sizeContext.Add($"Shoe size: EU {shoeSize.Value}");
        var sizeText = sizeContext.Count > 0 ? string.Join(", ", sizeContext) : "Size: not specified";

        var prompt = $$"""
            You are a pricing assistant for MamVibe, a Bulgarian marketplace for second-hand baby and children's items.
            Suggest a fair selling price in BGN for this listing.

            Title: {{title}}
            Category: {{categoryName}}
            Description: {{description}}
            {{sizeText}}
            {{comparablesText}}

            Return ONLY valid JSON with no markdown or extra text:
            {
              "suggestedPrice": 45,
              "low": 30,
              "high": 60,
              "confidence": 0.85,
              "reason": "brief explanation under 80 words"
            }

            Guidelines:
            - suggestedPrice, low, high are numbers in BGN (Bulgarian Lev), rounded to the nearest whole number
            - If comparable prices exist, base your suggestion on them; otherwise use general knowledge of the Bulgarian second-hand baby market
            - low = reasonable minimum a seller could accept, high = reasonable maximum
            - confidence: 0.0 (very uncertain) to 1.0 (very confident)
            - reason: 1-2 sentences explaining the price, referencing comparable listings if available
            """;

        var requestBody = new
        {
            model = await GetModelAsync(),
            max_tokens = 200,
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

        return ParsePriceSuggestion(ExtractJson(text), comparablePrices.Count);
    }

    private static PriceSuggestionResultDto ParsePriceSuggestion(string json, int comparableCount)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var r = doc.RootElement;

            return new PriceSuggestionResultDto
            {
                SuggestedPrice = r.TryGetProperty("suggestedPrice", out var sp) && sp.ValueKind == JsonValueKind.Number
                    ? sp.GetDecimal() : null,
                Low = r.TryGetProperty("low", out var lo) && lo.ValueKind == JsonValueKind.Number
                    ? lo.GetDecimal() : null,
                High = r.TryGetProperty("high", out var hi) && hi.ValueKind == JsonValueKind.Number
                    ? hi.GetDecimal() : null,
                Confidence = r.TryGetProperty("confidence", out var conf) && conf.ValueKind == JsonValueKind.Number
                    ? conf.GetDouble() : 0.5,
                Reason = r.TryGetProperty("reason", out var reason)
                    ? reason.GetString() ?? string.Empty : string.Empty,
                ComparableCount = comparableCount
            };
        }
        catch
        {
            return new PriceSuggestionResultDto { Confidence = 0.5, ComparableCount = comparableCount };
        }
    }

    public async Task<string> ChatAsync(
        string systemPrompt,
        IReadOnlyList<(string role, string content)> history)
    {
        var messages = history.Select(h => new { role = h.role, content = h.content }).ToArray();

        var requestBody = new
        {
            model = await GetModelAsync(),
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
            .GetString() ?? "I'm sorry, I couldn't generate a response right now.";
    }
}
