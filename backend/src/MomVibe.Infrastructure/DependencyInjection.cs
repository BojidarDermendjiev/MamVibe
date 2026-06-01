namespace MomVibe.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Application.Interfaces;
using Infrastructure.Services;
using Infrastructure.Services.Chat;
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
        services.AddHttpClient("TakeANap");
        services.AddScoped<ITakeANapService, TakeANapService>();

        // Shipping: Econt + Speedy + BoxNow + PigeonExpress courier integrations
        services.Configure<ShippingSettings>(configuration.GetSection("Shipping"));
        services.Configure<EcontSettings>(configuration.GetSection("Econt"));
        services.Configure<SpeedySettings>(configuration.GetSection("Speedy"));
        services.Configure<BoxNowSettings>(configuration.GetSection("BoxNow"));
        services.Configure<PigeonExpressSettings>(configuration.GetSection("PigeonExpress"));

        services.AddHttpClient("Econt");
        services.AddHttpClient("Speedy");
        services.AddHttpClient("BoxNow");
        services.AddHttpClient("PigeonExpress");

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
        });
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

        services.AddHttpClient("Anthropic");
        services.AddHttpClient("Groq");

        // Keyed chat providers — select active one via AI:ChatProvider config key (default: "anthropic")
        services.AddKeyedScoped<ILlmChatProvider, AnthropicChatProvider>("anthropic");
        services.AddKeyedScoped<ILlmChatProvider, GroqChatProvider>("groq");

        services.AddScoped<IAiService, AiService>();

        return services;
    }
}
