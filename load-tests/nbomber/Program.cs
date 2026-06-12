/**
 * MomVibe — NBomber Load Test (.NET 8)
 *
 * Alternative to the k6 script for teams that prefer staying in the C# ecosystem.
 * Simulates 500–1000 concurrent users in three profiles.
 *
 * Usage (from this directory):
 *   dotnet run
 *   BASE_URL=http://localhost:5038 PEAK_USERS=1000 dotnet run
 *   (Windows PS) $env:BASE_URL="http://localhost:5038"; dotnet run
 *
 * Reports are written to ./reports/ after each run.
 */

using System.Net.Http.Json;
using System.Text.Json;
using NBomber.CSharp;
using NBomber.Http.CSharp;

// ── Config ─────────────────────────────────────────────────────────────────────
var baseUrl   = Environment.GetEnvironmentVariable("BASE_URL")   ?? "http://localhost:5038";
var peakUsers = int.Parse(Environment.GetEnvironmentVariable("PEAK_USERS") ?? "1000");

// Scale per-profile VU counts from peakUsers
var peakAnon   = (int)(peakUsers * 0.60);
var peakAuth   = (int)(peakUsers * 0.28);
var peakTrader = (int)(peakUsers * 0.12);

using var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };

// ── Test users (create these accounts in your dev DB) ─────────────────────────
var testUsers = new[]
{
    ("loadtest1@test.com", "LoadTest@123"),
    ("loadtest2@test.com", "LoadTest@123"),
    ("loadtest3@test.com", "LoadTest@123"),
    ("loadtest4@test.com", "LoadTest@123"),
    ("loadtest5@test.com", "LoadTest@123"),
};

// ── Helper: login and return JWT token ────────────────────────────────────────
static async Task<string?> LoginAsync(HttpClient client, string email, string password)
{
    try
    {
        var res = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        if (!res.IsSuccessStatusCode) return null;

        using var doc = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync());
        var root = doc.RootElement;
        if (root.TryGetProperty("accessToken", out var t)) return t.GetString();
        if (root.TryGetProperty("token",       out var t2)) return t2.GetString();
    }
    catch { /* ignore */ }
    return null;
}

// ── Scenario 1: Anonymous Browsing ────────────────────────────────────────────
var anonymousBrowse = Scenario.Create("anonymous_browse", async ctx =>
{
    var page = Random.Shared.Next(1, 11);

    // Categories (cached)
    var cats = await http.CreateRequest("GET", $"/api/v1/categories")
                         .SendAsync(httpClient);
    if (!cats.IsSuccessStatusCode && cats.StatusCode != System.Net.HttpStatusCode.TooManyRequests)
        return Response.Fail(statusCode: (int)cats.StatusCode, message: "categories failed");

    await Task.Delay(TimeSpan.FromMilliseconds(Random.Shared.Next(500, 1500)));

    // Item list (30-second output cache, keyed by query params + anonymous)
    var list = await http.CreateRequest("GET", $"/api/v1/items?page={page}&pageSize=20")
                         .SendAsync(httpClient);

    await Task.Delay(TimeSpan.FromMilliseconds(Random.Shared.Next(1000, 2500)));

    // Item detail (if we got results)
    if (list.IsSuccessStatusCode)
    {
        try
        {
            using var doc = await JsonDocument.ParseAsync(await list.Content.ReadAsStreamAsync());
            var items = doc.RootElement.TryGetProperty("data", out var d) ? d
                      : doc.RootElement.TryGetProperty("items", out var i) ? i
                      : doc.RootElement;

            if (items.ValueKind == JsonValueKind.Array && items.GetArrayLength() > 0)
            {
                var idx    = Random.Shared.Next(0, items.GetArrayLength());
                var itemId = items[idx].GetProperty("id").GetString();

                if (!string.IsNullOrEmpty(itemId))
                {
                    var detail = await http.CreateRequest("GET", $"/api/v1/items/{itemId}")
                                           .SendAsync(httpClient);

                    await Task.Delay(TimeSpan.FromMilliseconds(Random.Shared.Next(500, 1500)));

                    // View increment (rate limited: 30/min/IP)
                    await http.CreateRequest("POST", $"/api/v1/items/{itemId}/view")
                              .SendAsync(httpClient);
                }
            }
        }
        catch { /* ignore parse errors */ }
    }

    await Task.Delay(TimeSpan.FromMilliseconds(Random.Shared.Next(1000, 3000)));
    return Response.Ok();
})
.WithoutWarmUp()
.WithLoadSimulations(
    Simulation.RampingConstant(copies: (int)(peakAnon * 0.05), during: TimeSpan.FromSeconds(30)),
    Simulation.RampingConstant(copies: (int)(peakAnon * 0.30), during: TimeSpan.FromSeconds(60)),
    Simulation.RampingConstant(copies: (int)(peakAnon * 0.70), during: TimeSpan.FromSeconds(60)),
    Simulation.KeepConstant(copies: peakAnon, during: TimeSpan.FromSeconds(120)),   // peak 600
    Simulation.RampingConstant(copies: (int)(peakAnon * 0.40), during: TimeSpan.FromSeconds(60)),
    Simulation.RampingConstant(copies: 0, during: TimeSpan.FromSeconds(30))
);

// ── Scenario 2: Authenticated Browse ─────────────────────────────────────────
var authenticatedBrowse = Scenario.Create("auth_browse", async ctx =>
{
    var (email, password) = testUsers[ctx.ScenarioInfo.ThreadNumber % testUsers.Length];
    var token = await LoginAsync(httpClient, email, password);
    if (token is null) { await Task.Delay(5000); return Response.Fail(message: "login failed"); }

    var authHeader = $"Bearer {token}";

    var me = await http.CreateRequest("GET", "/api/v1/auth/me")
                       .WithHeader("Authorization", authHeader)
                       .SendAsync(httpClient);

    await Task.Delay(TimeSpan.FromMilliseconds(Random.Shared.Next(500, 1500)));

    var page = Random.Shared.Next(1, 9);
    var list = await http.CreateRequest("GET", $"/api/v1/items?page={page}&pageSize=20")
                         .WithHeader("Authorization", authHeader)
                         .SendAsync(httpClient);

    await Task.Delay(TimeSpan.FromMilliseconds(Random.Shared.Next(1000, 2000)));

    if (list.IsSuccessStatusCode)
    {
        try
        {
            using var doc = await JsonDocument.ParseAsync(await list.Content.ReadAsStreamAsync());
            var items = doc.RootElement.TryGetProperty("data", out var d) ? d
                      : doc.RootElement.TryGetProperty("items", out var i) ? i
                      : doc.RootElement;

            if (items.ValueKind == JsonValueKind.Array && items.GetArrayLength() > 0)
            {
                var itemId = items[Random.Shared.Next(0, items.GetArrayLength())]
                                   .GetProperty("id").GetString();
                if (!string.IsNullOrEmpty(itemId))
                {
                    await http.CreateRequest("GET", $"/api/v1/items/{itemId}")
                              .WithHeader("Authorization", authHeader)
                              .SendAsync(httpClient);

                    // 1 in 3 users likes the item
                    if (Random.Shared.Next(1, 4) == 1)
                    {
                        await Task.Delay(500);
                        await http.CreateRequest("POST", $"/api/v1/items/{itemId}/like")
                                  .WithHeader("Authorization", authHeader)
                                  .SendAsync(httpClient);
                    }
                }
            }
        }
        catch { /* ignore */ }
    }

    await Task.Delay(TimeSpan.FromMilliseconds(Random.Shared.Next(1000, 2000)));

    await http.CreateRequest("GET", "/api/v1/users/my-items?page=1&pageSize=10")
              .WithHeader("Authorization", authHeader)
              .SendAsync(httpClient);

    await Task.Delay(TimeSpan.FromMilliseconds(Random.Shared.Next(1000, 2000)));

    await http.CreateRequest("GET", "/api/v1/messages")
              .WithHeader("Authorization", authHeader)
              .SendAsync(httpClient);

    await Task.Delay(TimeSpan.FromMilliseconds(Random.Shared.Next(2000, 4000)));
    return Response.Ok();
})
.WithoutWarmUp()
.WithLoadSimulations(
    Simulation.RampingConstant(copies: (int)(peakAuth * 0.05), during: TimeSpan.FromSeconds(30)),
    Simulation.RampingConstant(copies: (int)(peakAuth * 0.50), during: TimeSpan.FromSeconds(90)),
    Simulation.KeepConstant(copies: peakAuth, during: TimeSpan.FromSeconds(120)),   // peak 280
    Simulation.RampingConstant(copies: 0, during: TimeSpan.FromSeconds(60))
);

// ── Scenario 3: Active Trader ─────────────────────────────────────────────────
var activeTrader = Scenario.Create("active_trader", async ctx =>
{
    var (email, password) = testUsers[ctx.ScenarioInfo.ThreadNumber % testUsers.Length];
    var token = await LoginAsync(httpClient, email, password);
    if (token is null) { await Task.Delay(5000); return Response.Fail(message: "login failed"); }

    var auth = $"Bearer {token}";

    await http.CreateRequest("GET", "/api/v1/offers/received")
              .WithHeader("Authorization", auth).SendAsync(httpClient);

    await Task.Delay(TimeSpan.FromMilliseconds(Random.Shared.Next(500, 1500)));

    await http.CreateRequest("GET", "/api/v1/offers/sent")
              .WithHeader("Authorization", auth).SendAsync(httpClient);

    await Task.Delay(TimeSpan.FromMilliseconds(Random.Shared.Next(500, 1000)));

    await http.CreateRequest("GET", "/api/v1/purchase-requests/buyer")
              .WithHeader("Authorization", auth).SendAsync(httpClient);

    await Task.Delay(300);

    await http.CreateRequest("GET", "/api/v1/purchase-requests/seller")
              .WithHeader("Authorization", auth).SendAsync(httpClient);

    await Task.Delay(TimeSpan.FromMilliseconds(Random.Shared.Next(1000, 2000)));

    await http.CreateRequest("GET", $"/api/v1/items?page={Random.Shared.Next(1, 6)}&pageSize=20")
              .WithHeader("Authorization", auth).SendAsync(httpClient);

    await Task.Delay(TimeSpan.FromMilliseconds(Random.Shared.Next(1000, 2000)));

    await http.CreateRequest("GET", "/api/v1/ebills")
              .WithHeader("Authorization", auth).SendAsync(httpClient);

    await Task.Delay(TimeSpan.FromMilliseconds(Random.Shared.Next(2000, 5000)));
    return Response.Ok();
})
.WithoutWarmUp()
.WithLoadSimulations(
    Simulation.RampingConstant(copies: (int)(peakTrader * 0.10), during: TimeSpan.FromSeconds(30)),
    Simulation.RampingConstant(copies: (int)(peakTrader * 0.50), during: TimeSpan.FromSeconds(60)),
    Simulation.KeepConstant(copies: peakTrader, during: TimeSpan.FromSeconds(150)),  // peak 120
    Simulation.RampingConstant(copies: 0, during: TimeSpan.FromSeconds(60))
);

// ── Run ────────────────────────────────────────────────────────────────────────
NBomberRunner
    .RegisterScenarios(anonymousBrowse, authenticatedBrowse, activeTrader)
    .WithReportFormats(ReportFormat.Html, ReportFormat.Csv)
    .WithReportFolder("reports")
    .Run();
