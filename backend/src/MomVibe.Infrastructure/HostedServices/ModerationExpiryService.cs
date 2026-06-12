namespace MomVibe.Infrastructure.HostedServices;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Application.Interfaces;

/// <summary>
/// Hosted service that periodically clears expired temporary moderation actions
/// (Restricted/Suspended whose <c>ModerationExpiresAt</c> has passed). Runs every 60 seconds.
/// </summary>
public sealed class ModerationExpiryService : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(60);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ModerationExpiryService> _logger;

    public ModerationExpiryService(IServiceScopeFactory scopeFactory, ILogger<ModerationExpiryService> logger)
    {
        this._scopeFactory = scopeFactory;
        this._logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // First tick after a short delay so we don't race the startup-time DB migration apply.
        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = this._scopeFactory.CreateScope();
                var moderation = scope.ServiceProvider.GetRequiredService<IUserModerationService>();
                var cleared = await moderation.ClearExpiredAsync(stoppingToken);
                if (cleared > 0)
                    this._logger.LogInformation("ModerationExpiryService cleared {Count} expired moderation actions.", cleared);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "ModerationExpiryService tick failed — will retry on next interval.");
            }

            try { await Task.Delay(PollInterval, stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }
}
