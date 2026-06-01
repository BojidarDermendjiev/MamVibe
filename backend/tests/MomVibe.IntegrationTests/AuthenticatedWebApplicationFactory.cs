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
using MomVibe.IntegrationTests.Infrastructure;
using MomVibe.Infrastructure.Persistence;

namespace MomVibe.IntegrationTests;

/// <summary>
/// Factory that replaces JWT authentication with a fixed-identity test scheme.
/// Any request to a protected endpoint is automatically treated as authenticated
/// with <see cref="TestUserId"/> — no real user registration or token needed.
/// This avoids flakiness caused by missing JWT secrets in the CI environment.
/// </summary>
public class AuthenticatedWebApplicationFactory : WebApplicationFactory<StartUp>
{
    public const string TestUserId = "test-ebill-user-001";

    private readonly string _dbName = $"momvibe_auth_{Guid.NewGuid():N}";

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
            // Replace the production DbContext registration to point at the test database.
            var dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbDescriptor != null) services.Remove(dbDescriptor);

            var ctxDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ApplicationDbContext));
            if (ctxDescriptor != null) services.Remove(ctxDescriptor);

            services.AddDbContext<ApplicationDbContext>(o => o
                .UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            // Register our test auth scheme (handler must be registered first).
            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            // PostConfigure runs after ALL Configure<AuthenticationOptions> actions,
            // so it beats the JWT DefaultAuthenticateScheme set in StartUp.cs.
            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultScheme = TestAuthHandler.SchemeName;
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                options.DefaultForbidScheme = TestAuthHandler.SchemeName;
            });

            // Seed the test user so Payment.BuyerId → AspNetUsers FK resolves in Postgres.
            services.AddHostedService<AuthenticatedTestDataSeeder>();
        });

        builder.UseEnvironment("Development");
    }
}

/// <summary>
/// Seeds the <see cref="AuthenticatedWebApplicationFactory.TestUserId"/> ApplicationUser
/// on host start so endpoints that insert a row referencing the authenticated user
/// (Payment.BuyerId, Item.UserId, …) don't trip the AspNetUsers FK on real Postgres.
/// </summary>
file sealed class AuthenticatedTestDataSeeder(IServiceProvider sp) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (!await db.Users.AnyAsync(u => u.Id == AuthenticatedWebApplicationFactory.TestUserId, ct))
        {
            db.Users.Add(new ApplicationUser
            {
                Id = AuthenticatedWebApplicationFactory.TestUserId,
                UserName = "testebill@momvibe.test",
                NormalizedUserName = "TESTEBILL@MOMVIBE.TEST",
                Email = "testebill@momvibe.test",
                NormalizedEmail = "TESTEBILL@MOMVIBE.TEST",
                DisplayName = "Test E-Bill User",
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            });
            await db.SaveChangesAsync(ct);
        }
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}

/// <summary>
/// Always authenticates requests as <see cref="AuthenticatedWebApplicationFactory.TestUserId"/>.
/// Tests that want to test the 401 path should use a plain <see cref="CustomWebApplicationFactory"/>
/// client without this scheme registered.
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestScheme";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, AuthenticatedWebApplicationFactory.TestUserId),
            new Claim(ClaimTypes.Name, "Test E-Bill User")
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var ticket  = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
