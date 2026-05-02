using Serilog;
using System.Text;
using System.Threading.RateLimiting;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.JwtBearer;

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

// Serilog
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration)
          .MinimumLevel.Information()
          .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
          .MinimumLevel.Override("Microsoft.AspNetCore.ResponseCaching", Serilog.Events.LogEventLevel.Error)
          .MinimumLevel.Override("Microsoft.AspNetCore.Routing", Serilog.Events.LogEventLevel.Warning)
          .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", Serilog.Events.LogEventLevel.Warning)
          .Enrich.FromLogContext()
          .WriteTo.Console());

// Add layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// SignalR-backed notifiers (registered here because they depend on IHubContext from WebApi)
builder.Services.AddScoped<IPurchaseRequestNotifier, SignalRPurchaseRequestNotifier>();
builder.Services.AddScoped<IShipmentNotifier, SignalRShipmentNotifier>();

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
    options.AddPolicy(CorsPolicy.MamVibe, policy =>
    {
        policy.WithOrigins(
                builder.Configuration["FrontendUrl"] ?? "https://localhost:5173")
              .WithHeaders("Content-Type", "Authorization", "X-Language", "Cache-Control")
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
});

// Request body size limit (10MB max)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024;
});

// Memory cache (used by BlockedUserMiddleware for per-user block-status caching)
builder.Services.AddMemoryCache();

// Controllers
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

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

var app = builder.Build();

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
    await DataSeeder.SeedDemoDataAsync(userManager, dbContext, app.Environment);
}

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
        await next();
    });
}

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

// If behind a reverse proxy (Kestrel behind Nginx/Apache/Azure App Service, etc.)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRequestLocalization();
app.UseResponseCaching();
app.UseRateLimiter();
app.UseCors(CorsPolicy.MamVibe);

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<BlockedUserMiddleware>();

app.MapGet("/health", () => Results.Ok("healthy"));
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat").RequireAuthorization(AuthorizationPolicies.ActiveUser);
    
app.Run();

// Make StartUp accessible for integration tests
public partial class StartUp { }