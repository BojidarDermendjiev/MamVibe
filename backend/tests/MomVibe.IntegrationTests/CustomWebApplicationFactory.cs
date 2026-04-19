using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using MomVibe.Infrastructure.Persistence;

namespace MomVibe.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<StartUp>
{
    private readonly string _dbName = $"MomVibeTestDb_{Guid.NewGuid()}";

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
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Also remove the DbContext itself to avoid conflicts
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ApplicationDbContext));
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            // Add InMemory database with a fixed name per factory instance
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });
        });

        builder.UseEnvironment("Development");
    }
}
