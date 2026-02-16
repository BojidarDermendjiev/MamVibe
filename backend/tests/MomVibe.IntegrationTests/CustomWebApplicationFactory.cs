namespace MomVibe.IntegrationTests;

using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Infrastructure.Persistence;

public class CustomWebApplicationFactory : WebApplicationFactory<StartUp>
{
    private readonly string _dbName = $"MomVibeTestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
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
