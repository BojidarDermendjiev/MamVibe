using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MomVibe.Domain.Entities;
using MomVibe.Application.Interfaces;
using MomVibe.IntegrationTests.Infrastructure;
using MomVibe.Infrastructure.Persistence;

namespace MomVibe.IntegrationTests;

/// <summary>
/// General-purpose authenticated factory that:
/// - Seeds the test user so FK navigations resolve in InMemory queries
/// - Stubs AI service (avoids live Anthropic calls)
/// - Stubs Turnstile service (avoids live Cloudflare calls)
/// - Installs a test auth scheme so every request is auto-authenticated
/// </summary>
public class GeneralAuthWebApplicationFactory : WebApplicationFactory<StartUp>
{
    public const string TestUserId = AuthenticatedWebApplicationFactory.TestUserId;

    private readonly string _dbName = $"momvibe_generalauth_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionString = PostgresContainerFixture
            .GetConnectionStringAsync(_dbName)
            .GetAwaiter()
            .GetResult();

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:Secret"] = "integration-test-secret-key-must-be-at-least-32-chars",
                ["JwtSettings:Issuer"] = "MomVibeTest",
                ["JwtSettings:Audience"] = "MomVibeTest",
                ["JwtSettings:ExpiryMinutes"] = "60",
                ["ConnectionStrings:DefaultConnection"] = connectionString,
            });
        });

        builder.ConfigureServices(services =>
        {
            var dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbDescriptor != null) services.Remove(dbDescriptor);
            var ctxDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ApplicationDbContext));
            if (ctxDescriptor != null) services.Remove(ctxDescriptor);

            services.AddDbContext<ApplicationDbContext>(o => o
                .UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            // Stub AI service — avoids live Anthropic API calls
            var aiDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IAiService));
            if (aiDescriptor != null) services.Remove(aiDescriptor);
            services.AddScoped<IAiService, TestNoOpAiService>();

            // Stub Turnstile service — avoids live Cloudflare calls
            var turnstileDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ITurnstileService));
            if (turnstileDescriptor != null) services.Remove(turnstileDescriptor);
            services.AddScoped<ITurnstileService, TestTurnstileService>();

            // Test authentication scheme
            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, GeneralTestAuthHandler>(
                    GeneralTestAuthHandler.SchemeName, _ => { });

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultScheme = GeneralTestAuthHandler.SchemeName;
                options.DefaultAuthenticateScheme = GeneralTestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = GeneralTestAuthHandler.SchemeName;
                options.DefaultForbidScheme = GeneralTestAuthHandler.SchemeName;
            });

            // Seed test user so Include(User) navigations resolve
            services.AddHostedService<GeneralTestDataSeeder>();
        });

        builder.UseEnvironment("Development");
    }
}

file sealed class GeneralTestDataSeeder(IServiceProvider sp) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync(ct);

        if (!await db.Users.AnyAsync(u => u.Id == GeneralAuthWebApplicationFactory.TestUserId, ct))
        {
            db.Users.Add(new ApplicationUser
            {
                Id = GeneralAuthWebApplicationFactory.TestUserId,
                UserName = "testgeneral@momvibe.test",
                NormalizedUserName = "TESTGENERAL@MOMVIBE.TEST",
                Email = "testgeneral@momvibe.test",
                NormalizedEmail = "TESTGENERAL@MOMVIBE.TEST",
                DisplayName = "Test General User",
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            });
            await db.SaveChangesAsync(ct);
        }
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}

public class GeneralTestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "GeneralTestScheme";

    public GeneralTestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, GeneralAuthWebApplicationFactory.TestUserId),
            new Claim(ClaimTypes.Name, "Test General User"),
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

/// <summary>Turnstile stub that always returns verified=true for integration tests.</summary>
public class TestTurnstileService : ITurnstileService
{
    public Task<bool> VerifyAsync(string token, string ip) => Task.FromResult(true);
    public Task<bool> VerifyTokenAsync(string token) => Task.FromResult(token != "invalid-token");
}
