using Serilog;
using Prometheus;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Net;
using System.Text;
using System.Threading.RateLimiting;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

using MomVibe.WebApi;
using MomVibe.Application;
using MomVibe.WebApi.Hubs;
using MomVibe.Infrastructure;
using MomVibe.Domain.Entities;
using MomVibe.WebApi.Services;
using MomVibe.WebApi.Middleware;
using MomVibe.Application.Interfaces;
using MomVibe.Infrastructure.Persistence;
using MomVibe.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);

// Data Protection — persist keys to a Docker volume so they survive container restarts.
// Without this, every restart invalidates all auth cookies and forces all users to log in again.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/dataprotection-keys"))
    .SetApplicationName("MomVibe");

// Serilog — enriched with OpenTelemetry's Activity context so log lines and traces
// share the same TraceId/SpanId for cross-system correlation.
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration)
          .MinimumLevel.Information()
          .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
          .MinimumLevel.Override("Microsoft.AspNetCore.ResponseCaching", Serilog.Events.LogEventLevel.Error)
          .MinimumLevel.Override("Microsoft.AspNetCore.Routing", Serilog.Events.LogEventLevel.Warning)
          .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", Serilog.Events.LogEventLevel.Warning)
          .Enrich.FromLogContext()
          .Enrich.WithProperty("Application", "MomVibe")
          .Enrich.With(new MomVibe.WebApi.Logging.ActivityEnricher())
          .WriteTo.Console());

// OpenTelemetry — traces for ASP.NET Core requests, outbound HttpClient calls, and EF Core
// commands. Exported via OTLP when OpenTelemetry:Otlp:Endpoint is configured; otherwise the
// instrumentation runs (so logs/Activity still get TraceId enrichment) but nothing exits.
var otlpEndpoint = builder.Configuration["OpenTelemetry:Otlp:Endpoint"];
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(serviceName: "momvibe-api"))
    .WithTracing(t =>
    {
        t.AddAspNetCoreInstrumentation(opts =>
        {
            // Suppress noisy traces for liveness/readiness probes and Prometheus scrapes.
            opts.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/health")
                              && !ctx.Request.Path.StartsWithSegments("/metrics");
        });
        t.AddHttpClientInstrumentation();
        t.AddEntityFrameworkCoreInstrumentation();
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
            t.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
    });

// Add layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// SignalR-backed notifiers (registered here because they depend on IHubContext from WebApi)
builder.Services.AddScoped<IPurchaseRequestNotifier, SignalRPurchaseRequestNotifier>();
builder.Services.AddScoped<IShipmentNotifier, SignalRShipmentNotifier>();
builder.Services.AddScoped<IOfferNotifier, SignalROfferNotifier>();
builder.Services.AddScoped<IFollowNotifier, SignalRFollowNotifier>();
builder.Services.AddScoped<ISavedSearchNotifier, SignalRSavedSearchNotifier>();
builder.Services.AddScoped<IPriceDropNotifier, SignalRPriceDropNotifier>();

// Register IHttpClientFactory
builder.Services.AddHttpClient();

builder.Services.AddHttpClient("CloudflareTurnstile", client =>
{
    client.BaseAddress = new Uri("https://challenges.cloudflare.com/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.User.RequireUniqueEmail = true;

    // Lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Read secret inside the lambda so WebApplicationFactory config injection (applied at Build())
    // is visible here. Eager reads before Build() race against test factory setup.
    var secret = builder.Configuration["JwtSettings:Secret"];
    if (string.IsNullOrWhiteSpace(secret))
        throw new InvalidOperationException("JwtSettings:Secret is not configured.");
    if (secret.Length < 32)
        throw new InvalidOperationException("JwtSettings:Secret must be at least 32 characters for adequate security.");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
        ClockSkew = TimeSpan.FromSeconds(30)
    };
    // SignalR token from query string
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicies.AdminOnly, policy => policy.RequireRole("Admin"));
    options.AddPolicy(AuthorizationPolicies.ActiveUser, policy =>
        policy.RequireAuthenticatedUser()
              .RequireAssertion(context =>
                  !context.User.HasClaim("IsBlocked", "true")));
});

// CORS
builder.Services.AddCors(options =>
{
    var frontendUrl = builder.Configuration["FrontendUrl"]
        ?? throw new InvalidOperationException("FrontendUrl must be configured.");

    if (!builder.Environment.IsDevelopment() && !frontendUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("FrontendUrl must use HTTPS in non-development environments.");

    options.AddPolicy(CorsPolicy.MamVibe, policy =>
    {
        policy.WithOrigins(frontendUrl)
              .WithHeaders("Content-Type", "Authorization", "X-Language", "Cache-Control", "Idempotency-Key", "X-Client")
              .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS")
              .AllowCredentials()
              .WithExposedHeaders("X-Pagination");
    });
});

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Global limiter: user-aware (by userId for authenticated, by IP for anonymous)
    // This applies to ALL requests before any named policy.
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var key = userId ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(key,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 200,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });

    // Named policy retained for AuthController's stricter limit: 10 requests per minute per IP
    options.AddPolicy(RateLimitPolicies.Global, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    // Auth endpoint rate limit: 30 req/min per IP.
    // Sufficient brute-force protection; /auth/refresh is called on every page load
    // so 10/min was too tight when multiple tabs or hot-reloads are in use.
    options.AddPolicy(RateLimitPolicies.Auth, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,   
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    // Upload rate limit: 20 uploads per minute per IP
    options.AddPolicy(RateLimitPolicies.Upload, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    // Assistant chat: 20 messages per minute per IP — enough for real users, blocks abuse.
    options.AddPolicy(RateLimitPolicies.Assistant, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    // E-bill resend: 3 re-sends per minute per authenticated user.
    // Prevents email abuse while allowing reasonable retries.
    options.AddPolicy(RateLimitPolicies.EBillResend, context =>
    {
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                     ?? context.Connection.RemoteIpAddress?.ToString()
                     ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(userId,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });

    // IncrementView: 30 view increments per minute per IP.
    // Prevents a single IP from artificially inflating view counts in bulk.
    // (Per-item keying would require the item ID in the partition key, which is not available
    // at the rate-limiter middleware level; the per-IP cap is sufficient to block automated abuse.)
    options.AddPolicy(RateLimitPolicies.IncrementView, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

// Request body size limit (10MB max)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024;
});

// Memory cache (used by BlockedUserMiddleware for per-user block-status caching)
builder.Services.AddMemoryCache();

// Output cache — used for public, non-user-specific endpoints (categories, items list, etc.)
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(policy => policy.Expire(TimeSpan.FromSeconds(30)));
    options.AddPolicy("Categories", policy => policy.Expire(TimeSpan.FromHours(1)).Tag("categories"));

    // Items list: 30-second cache keyed by all query params, anonymous requests only.
    // Authenticated requests are personalised (isLikedByCurrentUser) so they bypass the cache.
    // Lock prevents thundering herd: on cache miss only one request rebuilds; others wait for it.
    options.AddPolicy("ItemsList", policy => policy
        .Expire(TimeSpan.FromSeconds(30))
        .SetVaryByQuery("*")
        .Tag("items")
        .SetLocking(true)
        .With(ctx => !ctx.HttpContext.Request.Headers.ContainsKey("Authorization")));
});

// Controllers + API versioning
// Versioning uses the URL-segment scheme: /api/v1/... is the only path that resolves a
// controller. DefaultApiVersion = 1.0 + AssumeDefaultVersionWhenUnspecified means the
// version constraint in route templates resolves to "v1" if the client omits an explicit
// version (defensive — every controller declares [ApiVersion("1.0")] explicitly so this
// only matters for routes that forget to).
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// SignalR — Redis backplane when available for multi-instance support
var redisUrl = builder.Configuration["Redis:Url"];
var signalR = builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});
if (!string.IsNullOrWhiteSpace(redisUrl))
    signalR.AddStackExchangeRedis(redisUrl);

// Response Compression (GZip + Brotli) — reduces bandwidth by ~60-80% for JSON/text responses
builder.Services.AddResponseCompression(opts =>
{
    opts.EnableForHttps = true;
    opts.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    opts.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});
builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(opts =>
    opts.Level = System.IO.Compression.CompressionLevel.Fastest);
builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(opts =>
    opts.Level = System.IO.Compression.CompressionLevel.Fastest);

// Response Caching
builder.Services.AddResponseCaching();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "MamVibe API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer {token}'",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en", "bg" };
    options.SetDefaultCulture("en");
    options.AddSupportedCultures(supportedCultures);
    options.AddSupportedUICultures(supportedCultures);
});

// Health checks — tagged so /health/live (process up) and /health/ready (downstream deps)
// can be probed independently. The "live" tag is the cheap liveness probe; "ready" includes
// everything a request actually depends on so Kubernetes-style readiness gating works.
var healthChecks = builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database", tags: ["ready"]);

// Redis: only registered when configured. Tagged "ready" because losing Redis degrades
// SignalR backplane + distributed cache, both of which the API depends on at request time.
var healthRedisUrl = builder.Configuration["Redis:Url"];
if (!string.IsNullOrWhiteSpace(healthRedisUrl))
    healthChecks.AddRedis(healthRedisUrl, name: "redis", tags: ["ready"]);

// Stripe reachability: a 2-second HEAD against the Stripe API root. Only registered when
// a real key is present so dev environments without Stripe stay healthy by default.
var stripeKey = builder.Configuration["Stripe:SecretKey"];
if (!string.IsNullOrWhiteSpace(stripeKey) && !stripeKey.Contains("YOUR_STRIPE"))
{
    healthChecks.AddUrlGroup(
        new Uri("https://api.stripe.com/healthcheck"),
        name: "stripe",
        tags: ["ready"],
        timeout: TimeSpan.FromSeconds(2));
}

var app = builder.Build();

// Stripe SDK uses a process-global static for its API key. Set it once at app startup.
// Previously this was assigned in PaymentService's constructor (scoped lifetime),
// which rewrote the global on every request.
Stripe.StripeConfiguration.ApiKey = app.Configuration["Stripe:SecretKey"];

// Seed roles and admin
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (dbContext.Database.IsRelational())
    {
        // Only auto-migrate outside production — in production apply migrations via CI/CD pipeline
        if (!app.Environment.IsProduction())
            await dbContext.Database.MigrateAsync();    
    }   
    else
        await dbContext.Database.EnsureCreatedAsync();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await DataSeeder.SeedRolesAsync(roleManager);
    await DataSeeder.SeedAdminAsync(userManager, app.Configuration);
    await DataSeeder.SeedAiBotAsync(userManager);
    await DataSeeder.SeedCategoriesAsync(dbContext);
}

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// HSTS is now emitted by SecurityHeadersMiddleware (registered below) in non-development
// environments — consolidated so all security headers live in one place.

// ForwardedHeaders MUST be first so that RemoteIpAddress is resolved from X-Forwarded-For
// before rate limiting, authentication, and all other middleware inspect it.
// Without this, rate limits and IP-based checks apply to the reverse-proxy IP, not the real client.
//
// KnownNetworks restricts which upstream proxies are trusted to set X-Forwarded-For.
// The 172.16.0.0/12 block covers the default Docker bridge network; adjust to match your
// actual infrastructure. This prevents external clients from spoofing X-Forwarded-For.
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
// Trust the Docker internal network (172.16.0.0/12) and standard loopback.
// In production, replace/extend with the actual IP range of your load balancer / Cloudflare egress.
forwardedHeadersOptions.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("172.16.0.0"), 12));
forwardedHeadersOptions.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("10.0.0.0"), 8));
app.UseForwardedHeaders(forwardedHeadersOptions);

app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        // Suppress noisy 404s for paths that don't exist (e.g. /weatherforecast)
        if (httpContext.Response.StatusCode == 404
            && !httpContext.Request.Path.StartsWithSegments("/api"))
            return Serilog.Events.LogEventLevel.Debug;

        return Serilog.Events.LogEventLevel.Information;
    };
});
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<MetricsProtectionMiddleware>();
app.UseHttpMetrics();

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}
app.UseResponseCompression();
app.UseStaticFiles();
app.UseRequestLocalization();
app.UseResponseCaching();
app.UseOutputCache();
app.UseRateLimiter();
app.UseCors(CorsPolicy.MamVibe);

app.UseAuthentication();

// BlockedUserMiddleware MUST run after UseAuthentication (needs User identity) but BEFORE
// UseAuthorization so that blocked users are denied before policy evaluation grants access
// to admin or other protected endpoints.
app.UseMiddleware<BlockedUserMiddleware>();

app.UseAuthorization();

// /health/live — process is up; matches a Kubernetes liveness probe (cheap, no deps touched).
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});
// /health/ready — full readiness; DB + Redis (if configured) + Stripe (if configured).
// Matches a Kubernetes readiness probe — failing means "don't send me traffic yet."
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
// Backwards-compat alias for existing monitors that hit /health.
app.MapHealthChecks("/health");
app.MapMetrics("/metrics");
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat").RequireAuthorization(AuthorizationPolicies.ActiveUser);
    
app.Run();

// Make StartUp accessible for integration tests
public partial class StartUp { }
