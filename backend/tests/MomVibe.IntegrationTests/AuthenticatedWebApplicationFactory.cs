using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

    private readonly string _dbName = $"MomVibeAuthTestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace real DB with InMemory
            var dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbDescriptor != null) services.Remove(dbDescriptor);

            var ctxDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ApplicationDbContext));
            if (ctxDescriptor != null) services.Remove(ctxDescriptor);

            services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(_dbName));

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
        });

        builder.UseEnvironment("Development");
    }
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
