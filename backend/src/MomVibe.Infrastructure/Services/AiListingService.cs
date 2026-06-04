namespace MomVibe.Infrastructure.Services;

using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Application.DTOs.Items;
using Application.Interfaces;
using Infrastructure.Configuration;
using Domain.Enums;

public class AiListingService : IAiListingService
{
    private readonly AnthropicSettings _settings;
    private readonly IApplicationDbContext _context;
    private readonly IChatClient _chatClient;

    private static readonly string[] AllowedContentTypes =
        ["image/jpeg", "image/jpg", "image/png", "image/webp"];

    public AiListingService(
        IOptions<AnthropicSettings> settings,
        IApplicationDbContext context,
        [FromKeyedServices("anthropic")] IChatClient chatClient)
    {
        _settings   = settings.Value;
        _context    = context;
        _chatClient = chatClient;
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

    private static string ResolveMediaType(string contentType) =>
        contentType.ToLowerInvariant() switch
        {
            "image/png"  => "image/png",
            "image/webp" => "image/webp",
            _            => "image/jpeg"
        };

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
        var imageBytes = ms.ToArray();
        var mediaType  = ResolveMediaType(photo.ContentType);

        var model   = await GetModelAsync();
        var options = new ChatOptions { ModelId = model, MaxOutputTokens = 500 };

        var result = await _chatClient.GetResponseAsync(
            [new ChatMessage(ChatRole.User,
                [new DataContent(imageBytes, mediaType), new TextContent(BuildSuggestPrompt(categorySlugs))])],
            options);

        return ParseSuggestion(ExtractJson(result.Text ?? "{}"));
    }

    private static string BuildSuggestPrompt(IEnumerable<string> categorySlugs)
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
              "suggestedPrice": price in EUR as a number if selling, null if donating,
              "ageGroup": integer 0-5 or null,
              "clothingSize": EU clothing size as integer (e.g. 68, 74, 80, 86, 92) or null,
              "shoeSize": EU shoe size as integer or null
            }

            Rules:
            - Use Sell (1) for good/excellent condition items, Donate (0) for heavily worn ones
            - If a field cannot be determined from the image, use null
            """;
    }

    private static AiListingSuggestionDto ParseSuggestion(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var r = doc.RootElement;

            return new AiListingSuggestionDto
            {
                Title         = r.TryGetProperty("title",         out var t)  ? t.GetString()  ?? "" : "",
                Description   = r.TryGetProperty("description",   out var d)  ? d.GetString()  ?? "" : "",
                CategorySlug  = r.TryGetProperty("categorySlug",  out var s)  ? s.GetString()  ?? "" : "",
                ListingType   = r.TryGetProperty("listingType",   out var lt) && lt.GetInt32() == 1
                    ? ListingType.Sell : ListingType.Donate,
                SuggestedPrice = r.TryGetProperty("suggestedPrice", out var p) && p.ValueKind == JsonValueKind.Number
                    ? p.GetDecimal() : null,
                AgeGroup       = r.TryGetProperty("ageGroup",  out var ag) && ag.ValueKind == JsonValueKind.Number
                    ? (AgeGroup)ag.GetInt32() : null,
                ClothingSize   = r.TryGetProperty("clothingSize", out var cs) && cs.ValueKind == JsonValueKind.Number
                    ? cs.GetInt32() : null,
                ShoeSize       = r.TryGetProperty("shoeSize", out var ss) && ss.ValueKind == JsonValueKind.Number
                    ? ss.GetInt32() : null,
            };
        }
        catch
        {
            return new AiListingSuggestionDto();
        }
    }

    public async Task<PriceSuggestionResultDto> SuggestPriceAsync(
        string title, string description, string categoryName,
        AgeGroup? ageGroup, int? clothingSize, int? shoeSize,
        IReadOnlyList<decimal> comparablePrices)
    {
        var comparablesText = comparablePrices.Count > 0
            ? $"Comparable listings currently on this platform ({comparablePrices.Count} items): {string.Join(", ", comparablePrices.Select(p => $"{p} EUR"))}"
            : "No comparable listings found on this platform yet — use general knowledge of the Bulgarian second-hand baby market.";

        var sizeContext = new List<string>();
        if (ageGroup.HasValue)     sizeContext.Add($"Age group: {ageGroup.Value}");
        if (clothingSize.HasValue) sizeContext.Add($"Clothing size: EU {clothingSize.Value}");
        if (shoeSize.HasValue)     sizeContext.Add($"Shoe size: EU {shoeSize.Value}");
        var sizeText = sizeContext.Count > 0 ? string.Join(", ", sizeContext) : "Size: not specified";

        var prompt = $$"""
            You are a pricing assistant for MamVibe, a Bulgarian marketplace for second-hand baby and children's items.
            Suggest a fair selling price in EUR for this listing.

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
            - suggestedPrice, low, high are numbers in EUR, rounded to the nearest whole number
            - If comparable prices exist, base your suggestion on them; otherwise use general knowledge of the Bulgarian second-hand baby market
            - low = reasonable minimum a seller could accept, high = reasonable maximum
            - confidence: 0.0 (very uncertain) to 1.0 (very confident)
            - reason: 1-2 sentences explaining the price, referencing comparable listings if available
            """;

        var model   = await GetModelAsync();
        var options = new ChatOptions { ModelId = model, MaxOutputTokens = 200 };

        var result = await _chatClient.GetResponseAsync(
            [new ChatMessage(ChatRole.User, prompt)],
            options);

        return ParsePriceSuggestion(ExtractJson(result.Text ?? "{}"), comparablePrices.Count);
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
                Low  = r.TryGetProperty("low",  out var lo) && lo.ValueKind == JsonValueKind.Number ? lo.GetDecimal() : null,
                High = r.TryGetProperty("high", out var hi) && hi.ValueKind == JsonValueKind.Number ? hi.GetDecimal() : null,
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
}
