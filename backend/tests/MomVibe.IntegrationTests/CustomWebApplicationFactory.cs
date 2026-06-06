using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using MomVibe.Application.Interfaces;
using MomVibe.IntegrationTests.Infrastructure;
using MomVibe.Infrastructure.Persistence;

namespace MomVibe.IntegrationTests;

/// <summary>
/// Integration-test factory backed by a real PostgreSQL container (via Testcontainers).
/// Each factory instance owns its own database within the shared container so test classes
/// don't see each other's data. Migrations are applied by <c>StartUp.cs</c>'s non-production
/// auto-migrate path on first request.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<StartUp>
{
    private readonly string _dbName = $"momvibe_test_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Resolve the per-factory connection string before host services are built so the
        // DbContext registration below can capture it. Sync-over-async is acceptable here —
        // this runs once per factory at xUnit fixture setup, never on a request hot path.
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
                ["AdminSeed:Enabled"] = "false",
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replace the production DbContext registration so it targets the test database.
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ApplicationDbContext));
            if (dbContextDescriptor != null) services.Remove(dbContextDescriptor);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    connectionString,
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
        });

        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            var existing = services.SingleOrDefault(d => d.ServiceType == typeof(ITurnstileService));
            if (existing != null) services.Remove(existing);
            services.AddSingleton<ITurnstileService, AlwaysValidTurnstileService>();
        });
    }

    private sealed class AlwaysValidTurnstileService : ITurnstileService
    {
        public Task<bool> VerifyAsync(string token, string ip) => Task.FromResult(true);
        public Task<bool> VerifyTokenAsync(string token) => Task.FromResult(true);
    }
}
