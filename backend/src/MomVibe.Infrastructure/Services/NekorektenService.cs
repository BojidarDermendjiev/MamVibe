namespace MomVibe.Infrastructure.Services;

using System.Text.Json;
using Microsoft.Extensions.Options;

using Application.Interfaces;
using Configuration;

/// <summary>
/// Calls the nekorekten.com REST API to check whether a buyer has any fraud reports.
/// Uses searchMode=one-of so any matching identifier (name, email, phone) returns reports.
/// Fails silently — if the external service is down the caller gets ServiceUnavailable=true
/// and should allow the purchase flow to continue unblocked.
/// </summary>
public class NekorektenService : INekorektenService
{
    private readonly HttpClient _http;
    private readonly NekorektenSettings _settings;

    public NekorektenService(IHttpClientFactory factory, IOptions<NekorektenSettings> options)
    {
        this._http = factory.CreateClient("Nekorekten");
        this._settings = options.Value;
    }

    public async Task<BuyerCheckResult> CheckAsync(string? name, string? email, string? phone)
    {
        try
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(name))  parts.Add($"name={Uri.EscapeDataString(name)}");
            if (!string.IsNullOrWhiteSpace(email)) parts.Add($"email={Uri.EscapeDataString(email)}");
            if (!string.IsNullOrWhiteSpace(phone)) parts.Add($"phone={Uri.EscapeDataString(phone)}");

            // Nothing to search on — skip the call
            if (parts.Count == 0) return new BuyerCheckResult();

            parts.Add("searchMode=one-of");
            var url = $"{this._settings.BaseUrl}/api/v1/reports?{string.Join("&", parts)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Api-Key", this._settings.ApiKey);

            using var response = await this._http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return new BuyerCheckResult { ServiceUnavailable = true };

            var json = await response.Content.ReadAsStringAsync();
            var reports = ParseReports(json);

            return new BuyerCheckResult
            {
                HasReports = reports.Count > 0,
                ReportCount = reports.Count,
                Reports = reports,
            };
        }
        catch
        {
            return new BuyerCheckResult { ServiceUnavailable = true };
        }
    }

    // ── JSON parsing ──────────────────────────────────────────────────────────
    // Handles both a bare JSON array and a wrapped { "data": [...] } / { "reports": [...] }
    private static List<NekorektenReport> ParseReports(string json)
    {
        var result = new List<NekorektenReport>();
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            JsonElement array = root.ValueKind == JsonValueKind.Array
                ? root
                : root.TryGetProperty("data",    out var d) ? d
                : root.TryGetProperty("reports", out var r) ? r
                : default;

            if (array.ValueKind != JsonValueKind.Array) return result;

            foreach (var el in array.EnumerateArray())
            {
                result.Add(new NekorektenReport
                {
                    Text      = GetString(el, "text"),
                    Phone     = GetString(el, "phone"),
                    Email     = GetString(el, "email"),
                    FirstName = GetString(el, "firstName"),
                    LastName  = GetString(el, "lastName"),
                    Likes     = el.TryGetProperty("likes", out var l) && l.TryGetInt32(out var li) ? li : 0,
                    CreatedAt = el.TryGetProperty("createdAt", out var dt) && dt.ValueKind == JsonValueKind.String
                                && DateTime.TryParse(dt.GetString(), out var parsed) ? parsed : null,
                });
            }
        }
        catch { /* return whatever was parsed */ }

        return result;
    }

    private static string? GetString(JsonElement el, string prop)
        => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString()
            : null;
}
