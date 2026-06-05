namespace MomVibe.Infrastructure;

using Anthropic.SDK;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using System.ClientModel;

using Application.Interfaces;
using Infrastructure.Services;
using Infrastructure.Persistence;
using Infrastructure.Configuration;
using Infrastructure.Services.Shipping;

/// <summary>
/// Configures infrastructure dependencies:
/// - Registers ApplicationDbContext with PostgreSQL and migrations assembly.
/// - Binds IApplicationDbContext to ApplicationDbContext.
/// - Adds scoped services for tokens, auth, current user, items, messages, photos, payments,
///   admin operations, feedback, doctor reviews, child-friendly places,
///   Cloudflare Turnstile verification, and shipping (Econt, Speedy, BoxNow, PigeonExpress).
/// Provides the AddInfrastructureServices extension to wire these into the DI container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all infrastructure services: EF Core DbContext, courier providers, payment, shipping,
    /// email, AI, n8n webhook integration, and all scoped application service implementations.
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    /// <param name="configuration">The application configuration used to bind settings sections.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // Strongly-typed JwtSettings — validated at app startup so a misconfigured
        // deployment fails immediately rather than crashing on the first auth request.
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection("JwtSettings"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IItemService, ItemService>();
        services.AddScoped<IMessageService, MessageService>();
        // Photo storage — Cloudflare R2 when configured, local disk otherwise
        services.Configure<R2Settings>(configuration.GetSection("R2"));
        var r2 = configuration.GetSection("R2").Get<R2Settings>();
        if (r2 is { IsConfigured: true })
            services.AddScoped<IPhotoService, R2PhotoService>();
        else
            services.AddScoped<IPhotoService, PhotoService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IStripePaymentService, StripePaymentService>();
        services.AddScoped<IEBillService, EBillService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddScoped<IDoctorReviewService, DoctorReviewService>();
        services.AddScoped<IChildFriendlyPlaceService, ChildFriendlyPlaceService>();
        services.AddScoped<IUserRatingService, UserRatingService>();
        services.AddScoped<ITurnstileService, TurnstileService>();
        services.AddScoped<IPurchaseRequestService, PurchaseRequestService>();
        services.AddScoped<IOfferService, OfferService>();
        services.AddScoped<IFollowService, FollowService>();
        services.AddScoped<ISavedSearchService, SavedSearchService>();
        services.AddScoped<IBundleService, BundleService>();
        services.AddScoped<IGdprService, GdprService>();

        // Email
        services.Configure<SmtpSettings>(configuration.GetSection("Smtp"));
        services.AddScoped<IEmailService, EmailService>();

        // Take a NAP: Digital receipts
        services.Configure<TakeANapSettings>(configuration.GetSection("TakeANap"));
        services.AddHttpClient("TakeANap").AddStandardResilienceHandler();
        services.AddScoped<ITakeANapService, TakeANapService>();

        // Shipping: Econt + Speedy + BoxNow + PigeonExpress courier integrations
        services.Configure<ShippingSettings>(configuration.GetSection("Shipping"));
        services.Configure<EcontSettings>(configuration.GetSection("Econt"));
        services.Configure<SpeedySettings>(configuration.GetSection("Speedy"));
        services.Configure<BoxNowSettings>(configuration.GetSection("BoxNow"));
        services.Configure<PigeonExpressSettings>(configuration.GetSection("PigeonExpress"));

        services.AddHttpClient("Econt").AddStandardResilienceHandler();
        services.AddHttpClient("Speedy").AddStandardResilienceHandler();
        services.AddHttpClient("BoxNow").AddStandardResilienceHandler();
        services.AddHttpClient("PigeonExpress").AddStandardResilienceHandler();

        services.AddScoped<ICourierProvider, EcontCourierProvider>();
        services.AddScoped<ICourierProvider, SpeedyCourierProvider>();
        services.AddScoped<ICourierProvider, BoxNowCourierProvider>();
        services.AddScoped<ICourierProvider, PigeonExpressCourierProvider>();
        services.AddScoped<CourierProviderFactory>();
        services.AddScoped<IShippingService, ShippingService>();

        // Nekorekten.com buyer reputation checks
        services.Configure<NekorektenSettings>(configuration.GetSection("Nekorekten"));
        services.AddHttpClient("Nekorekten", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        }).AddStandardResilienceHandler();
        services.AddScoped<INekorektenService, NekorektenService>();

        // n8n Webhook integration
        services.Configure<N8nSettings>(configuration.GetSection("N8n"));
        var n8nSecret = configuration["N8n:WebhookSecret"] ?? string.Empty;
        services.AddTransient(_ => new N8nHmacHandler(n8nSecret));
        services.AddHttpClient("N8n", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        })
        .AddHttpMessageHandler<N8nHmacHandler>();
        // (The legacy in-memory Channel-based N8nWebhookService was retired once every caller
        //  switched to the transactional outbox below. Webhook delivery is now durable.)

        // Transactional outbox: writer stages messages in the caller's EF unit of work,
        // dispatchers route by MessageType, OutboxProcessor drains pending rows with retry.
        services.AddScoped<IOutboxWriter, Outbox.OutboxWriter>();
        services.AddScoped<IOutboxMessageDispatcher, Outbox.N8nOutboxDispatcher>();
        services.AddHostedService<Outbox.OutboxProcessor>();

        // Distributed cache — Redis when available, in-memory fallback for local dev
        var redisUrl = configuration["Redis:Url"];
        if (!string.IsNullOrWhiteSpace(redisUrl))
            services.AddStackExchangeRedisCache(o => o.Configuration = redisUrl);
        else
            services.AddDistributedMemoryCache();

        // User presence tracking (singleton shared between ChatHub and MessageService)
        services.AddSingleton<UserPresenceTracker>();

        // n8n scheduled daily checks
        services.AddHostedService<N8nScheduledService>();

        // AI — listing assistant, moderation, price suggestion, chat widget
        services.Configure<AnthropicSettings>(configuration.GetSection("Anthropic"));
        services.Configure<GroqSettings>(configuration.GetSection("Groq"));

        // Anthropic IChatClient — used by AiService + AiListingService
        services.AddKeyedSingleton<IChatClient>("anthropic", (sp, _) =>
        {
            var s = sp.GetRequiredService<IOptions<AnthropicSettings>>().Value;
            return new AnthropicClient(apiKeys: new APIAuthentication(s.ApiKey))
                .Messages
                .AsBuilder()
                .UseOpenTelemetry(configure: b => b.EnableSensitiveData = false)
                .Build();
        });

        // Anthropic + Redis cache — used by AiModerationService
        services.AddKeyedSingleton<IChatClient>("anthropic-cached", (sp, _) =>
        {
            var s     = sp.GetRequiredService<IOptions<AnthropicSettings>>().Value;
            var cache = sp.GetRequiredService<IDistributedCache>();
            return new AnthropicClient(apiKeys: new APIAuthentication(s.ApiKey))
                .Messages
                .AsBuilder()
                .UseDistributedCache(cache)
                .UseOpenTelemetry(configure: b => b.EnableSensitiveData = false)
                .Build();
        });

        // Groq IChatClient — OpenAI-compatible endpoint
        services.AddKeyedSingleton<IChatClient>("groq", (sp, _) =>
        {
            var s = sp.GetRequiredService<IOptions<GroqSettings>>().Value;
            return new OpenAI.Chat.ChatClient(
                    s.Model,
                    new ApiKeyCredential(s.ApiKey),
                    new OpenAI.OpenAIClientOptions { Endpoint = new Uri("https://api.groq.com/openai/v1") })
                .AsIChatClient()
                .AsBuilder()
                .UseOpenTelemetry(configure: b => b.EnableSensitiveData = false)
                .Build();
        });

        services.AddScoped<IAiService, AiService>();
        services.AddScoped<IAiListingService, AiListingService>();
        services.AddScoped<IAiModerationService, AiModerationService>();
        services.AddScoped<IKnowledgeService, KnowledgeService>();

        return services;
    }
}
