using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MomVibe.Infrastructure.Persistence;

namespace MomVibe.IntegrationTests;

/// <summary>
/// Factory that injects a test auth scheme whose principal carries the "Admin" role claim.
/// Use this for endpoints protected by the AdminOnly policy.
/// </summary>
public class AdminWebApplicationFactory : WebApplicationFactory<StartUp>
{
    public const string TestAdminId = "test-admin-user-001";

    private readonly string _dbName = $"MomVibeAdminTestDb_{Guid.NewGuid()}";

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

            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, AdminTestAuthHandler>(AdminTestAuthHandler.SchemeName, _ => { });

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultScheme = AdminTestAuthHandler.SchemeName;
                options.DefaultAuthenticateScheme = AdminTestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = AdminTestAuthHandler.SchemeName;
                options.DefaultForbidScheme = AdminTestAuthHandler.SchemeName;
            });
        });

        builder.UseEnvironment("Development");
    }
}

public class AdminTestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "AdminTestScheme";

    public AdminTestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, AdminWebApplicationFactory.TestAdminId),
            new Claim(ClaimTypes.Name, "Test Admin User"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
