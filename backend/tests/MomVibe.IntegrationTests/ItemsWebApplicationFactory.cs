using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MomVibe.Domain.Enums;
using MomVibe.Domain.Entities;
using MomVibe.Application.DTOs.Items;
using MomVibe.Application.Interfaces;
using MomVibe.Infrastructure.Persistence;

namespace MomVibe.IntegrationTests;

/// <summary>
/// Factory for item endpoint tests. Replaces the real AI service with a no-op stub
/// so item creation succeeds without live Anthropic API calls.
/// </summary>
public class ItemsWebApplicationFactory : WebApplicationFactory<StartUp>
{
    public const string TestUserId = AuthenticatedWebApplicationFactory.TestUserId;

    private readonly string _dbName = $"MomVibeItemsTestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:Secret"] = "integration-test-secret-key-must-be-at-least-32-chars",
                ["JwtSettings:Issuer"] = "MomVibeTest",
                ["JwtSettings:Audience"] = "MomVibeTest",
                ["JwtSettings:ExpiryMinutes"] = "60",
            });
        });

        builder.ConfigureServices(services =>
        {
            var dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbDescriptor != null) services.Remove(dbDescriptor);
            var ctxDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ApplicationDbContext));
            if (ctxDescriptor != null) services.Remove(ctxDescriptor);
            services.AddDbContext<ApplicationDbContext>(o => o
                .UseInMemoryDatabase(_dbName)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning)));

            // Swap out the real AI service so CreateAsync doesn't make live Anthropic HTTP calls
            var aiDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IAiService));
            if (aiDescriptor != null) services.Remove(aiDescriptor);
            services.AddScoped<IAiService, TestNoOpAiService>();

            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, ItemsTestAuthHandler>(
                    ItemsTestAuthHandler.SchemeName, _ => { });

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultScheme = ItemsTestAuthHandler.SchemeName;
                options.DefaultAuthenticateScheme = ItemsTestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = ItemsTestAuthHandler.SchemeName;
                options.DefaultForbidScheme = ItemsTestAuthHandler.SchemeName;
            });

            // Seed InMemory DB with test user + categories so Include(User)/Include(Category) resolves
            services.AddHostedService<ItemsTestDataSeeder>();
        });

        builder.UseEnvironment("Development");
    }
}

/// <summary>
/// Seeds the InMemory test database with the test user and HasData categories on startup.
/// Required because EF Core InMemory returns null from AsNoTracking+Include queries when the
/// navigation FK target entity doesn't exist in the in-memory store.
/// </summary>
file sealed class ItemsTestDataSeeder(IServiceProvider sp) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // EnsureCreated applies HasData seeds (Categories) to the InMemory store
        await db.Database.EnsureCreatedAsync(ct);

        // Seed the test user so Item.User navigation resolves in AsNoTracking queries
        if (!await db.Users.AnyAsync(u => u.Id == ItemsWebApplicationFactory.TestUserId, ct))
        {
            db.Users.Add(new ApplicationUser
            {
                Id = ItemsWebApplicationFactory.TestUserId,
                UserName = "testitems@momvibe.test",
                NormalizedUserName = "TESTITEMS@MOMVIBE.TEST",
                Email = "testitems@momvibe.test",
                NormalizedEmail = "TESTITEMS@MOMVIBE.TEST",
                DisplayName = "Test Items User",
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            });
            await db.SaveChangesAsync(ct);
        }
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}

public class ItemsTestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "ItemsTestScheme";

    public ItemsTestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, ItemsWebApplicationFactory.TestUserId),
            new Claim(ClaimTypes.Name, "Test Items User")
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

/// <summary>
/// Auto-approves every item so CreateAsync never blocks on a live Anthropic call.
/// </summary>
public class TestNoOpAiService : IAiService
{
    public Task<AiListingSuggestionDto> SuggestListingAsync(IFormFile photo) =>
        Task.FromResult(new AiListingSuggestionDto
        {
            Title = "Test Item",
            Description = "Test description",
            CategorySlug = "clothing",
            ListingType = ListingType.Donate
        });

    public Task<AiModerationResultDto> ModerateItemAsync(
        string title, string description, string categoryName,
        ListingType listingType, decimal? price, string? firstPhotoUrl = null) =>
        Task.FromResult(new AiModerationResultDto
        {
            Recommendation = "approve",
            Confidence = 0.99,
            Reason = "Test auto-approve"
        });

    public Task<PriceSuggestionResultDto> SuggestPriceAsync(
        string title, string description, string categoryName,
        AgeGroup? ageGroup, int? clothingSize, int? shoeSize,
        IReadOnlyList<decimal> comparablePrices) =>
        Task.FromResult(new PriceSuggestionResultDto
        {
            SuggestedPrice = 25m,
            Low = 20m,
            High = 30m,
            Confidence = 0.8,
            Reason = "Test suggestion",
            ComparableCount = comparablePrices.Count
        });

    public Task<string> ChatAsync(
        string systemPrompt, IReadOnlyList<(string role, string content)> history) =>
        Task.FromResult("Test AI response");
}
