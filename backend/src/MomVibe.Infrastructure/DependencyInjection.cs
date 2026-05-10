namespace MomVibe.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IItemService, ItemService>();
        services.AddScoped<IMessageService, MessageService>();
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

        // Email
        services.Configure<SmtpSettings>(configuration.GetSection("Smtp"));
        services.AddScoped<IEmailService, EmailService>();

        // Take a NAP: Digital receipts
        services.Configure<TakeANapSettings>(configuration.GetSection("TakeANap"));
        services.AddHttpClient("TakeANap");
        services.AddScoped<ITakeANapService, TakeANapService>();

        // Shipping: Econt + Speedy + BoxNow + PigeonExpress courier integrations
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
        services.AddHttpClient("N8n", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        });
        services.AddSingleton<N8nWebhookService>();
        services.AddSingleton<IN8nWebhookService>(sp => sp.GetRequiredService<N8nWebhookService>());
        services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<N8nWebhookService>());

        // User presence tracking (singleton shared between ChatHub and MessageService)
        services.AddSingleton<UserPresenceTracker>();

        // n8n scheduled daily checks
        services.AddHostedService<N8nScheduledService>();

        // Anthropic Claude AI — listing assistant
        services.Configure<AnthropicSettings>(configuration.GetSection("Anthropic"));
        services.AddHttpClient("Anthropic");
        services.AddScoped<IAiService, AiService>();

        return services;
    }
}
