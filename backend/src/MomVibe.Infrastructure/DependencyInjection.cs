namespace MomVibe.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Application.Interfaces;
using Infrastructure.Configuration;
using Infrastructure.Services;
using Infrastructure.Services.Shipping;
using Infrastructure.Persistence;

/// <summary>
/// Configures infrastructure dependencies:
/// - Registers ApplicationDbContext with SQL Server and migrations assembly.
/// - Binds IApplicationDbContext to ApplicationDbContext.
/// - Adds scoped services for tokens, auth, current user, items, messages, photos, payments,
///   admin operations, feedback, Cloudflare Turnstile verification, and shipping (Econt, Speedy).
/// Provides the AddInfrastructureServices extension to wire these into the DI container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
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
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddScoped<ITurnstileService, TurnstileService>();

        // Email
        services.Configure<SmtpSettings>(configuration.GetSection("Smtp"));
        services.AddScoped<IEmailService, EmailService>();

        // Take a NAP: Digital receipts
        services.Configure<TakeANapSettings>(configuration.GetSection("TakeANap"));
        services.AddHttpClient("TakeANap");
        services.AddScoped<ITakeANapService, TakeANapService>();

        // Shipping: Econt + Speedy + BoxNow courier integrations
        services.Configure<EcontSettings>(configuration.GetSection("Econt"));
        services.Configure<SpeedySettings>(configuration.GetSection("Speedy"));
        services.Configure<BoxNowSettings>(configuration.GetSection("BoxNow"));

        services.AddHttpClient("Econt");
        services.AddHttpClient("Speedy");
        services.AddHttpClient("BoxNow");

        services.AddScoped<ICourierProvider, EcontCourierProvider>();
        services.AddScoped<ICourierProvider, SpeedyCourierProvider>();
        services.AddScoped<ICourierProvider, BoxNowCourierProvider>();
        services.AddScoped<CourierProviderFactory>();
        services.AddScoped<IShippingService, ShippingService>();

        return services;
    }
}
