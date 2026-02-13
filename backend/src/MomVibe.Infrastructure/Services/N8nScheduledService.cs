namespace MomVibe.Infrastructure.Services;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Domain.Enums;
using Application.Interfaces;
using Infrastructure.Configuration;

/// <summary>
/// Daily background service that runs at 8:00 AM UTC.
/// Checks for stale items, stuck shipments, deliveries needing feedback, and sends a daily summary.
/// </summary>
public class N8nScheduledService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IN8nWebhookService _webhook;
    private readonly N8nSettings _settings;
    private readonly ILogger<N8nScheduledService> _logger;

    public N8nScheduledService(
        IServiceScopeFactory scopeFactory,
        IN8nWebhookService webhook,
        IOptions<N8nSettings> settings,
        ILogger<N8nScheduledService> logger)
    {
        this._scopeFactory = scopeFactory;
        this._webhook = webhook;
        this._settings = settings.Value;
        this._logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRun = now.Date.AddHours(8);
            if (now >= nextRun) nextRun = nextRun.AddDays(1);

            var delay = nextRun - now;
            this._logger.LogInformation("N8nScheduledService next run at {NextRun} (in {Delay})", nextRun, delay);

            await Task.Delay(delay, stoppingToken);

            try
            {
                await RunDailyChecksAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                this._logger.LogWarning(ex, "N8nScheduledService daily checks failed");
            }
        }
    }

    private async Task RunDailyChecksAsync(CancellationToken ct)
    {
        if (!this._settings.Enabled) return;

        using var scope = this._scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var now = DateTime.UtcNow;

        // Stale items: active sell items listed 30+ days
        var staleThreshold = now.AddDays(-30);
        var staleItems = await context.Items
            .Include(i => i.User)
            .Where(i => i.IsActive && i.ListingType == ListingType.Sell && i.CreatedAt <= staleThreshold)
            .Select(i => new
            {
                i.Title,
                Price = i.Price ?? 0,
                DaysListed = (int)(now - i.CreatedAt).TotalDays,
                i.ViewCount,
                SellerEmail = i.User.Email,
                SellerName = i.User.DisplayName
            })
            .ToListAsync(ct);

        if (staleItems.Count > 0)
        {
            this._webhook.Send(this._settings.StaleItems, new
            {
                Event = "stale_items",
                Timestamp = now,
                Items = staleItems
            });
        }

        // Stuck shipments: InTransit 7+ days
        var stuckThreshold = now.AddDays(-7);
        var stuckShipments = await context.Shipments
            .Include(s => s.Payment).ThenInclude(p => p.Item)
            .Include(s => s.Payment).ThenInclude(p => p.Buyer)
            .Include(s => s.Payment).ThenInclude(p => p.Seller)
            .Where(s => s.Status == ShipmentStatus.InTransit && s.CreatedAt <= stuckThreshold)
            .Select(s => new
            {
                s.Id,
                s.TrackingNumber,
                s.CourierProvider,
                ItemTitle = s.Payment.Item.Title,
                BuyerEmail = s.Payment.Buyer.Email,
                SellerEmail = s.Payment.Seller.Email,
                DaysInTransit = (int)(now - s.CreatedAt).TotalDays
            })
            .ToListAsync(ct);

        if (stuckShipments.Count > 0)
        {
            this._webhook.Send(this._settings.ShipmentStuck, new
            {
                Event = "shipment.stuck",
                Timestamp = now,
                Shipments = stuckShipments
            });
        }

        // Delivered needing feedback: delivered 2+ days ago
        var feedbackThreshold = now.AddDays(-2);
        var deliveredNeedingFeedback = await context.Shipments
            .Include(s => s.Payment).ThenInclude(p => p.Item)
            .Include(s => s.Payment).ThenInclude(p => p.Buyer)
            .Where(s => s.Status == ShipmentStatus.Delivered
                     && s.UpdatedAt != null
                     && s.UpdatedAt <= feedbackThreshold)
            .Select(s => new
            {
                ItemTitle = s.Payment.Item.Title,
                BuyerEmail = s.Payment.Buyer.Email,
                BuyerName = s.Payment.Buyer.DisplayName
            })
            .ToListAsync(ct);

        if (deliveredNeedingFeedback.Count > 0)
        {
            this._webhook.Send(this._settings.FeedbackPrompt, new
            {
                Event = "feedback_prompt",
                Timestamp = now,
                Deliveries = deliveredNeedingFeedback
            });
        }

        // Daily summary
        var yesterday = now.AddDays(-1);
        var newItems = await context.Items.CountAsync(i => i.CreatedAt >= yesterday, ct);
        var newPayments = await context.Payments.CountAsync(p => p.CreatedAt >= yesterday, ct);
        var newShipments = await context.Shipments.CountAsync(s => s.CreatedAt >= yesterday, ct);
        var activeShipments = await context.Shipments
            .CountAsync(s => s.Status != ShipmentStatus.Delivered
                          && s.Status != ShipmentStatus.Cancelled
                          && s.Status != ShipmentStatus.Returned, ct);

        this._webhook.Send(this._settings.DailySummary, new
        {
            Event = "daily_summary",
            Timestamp = now,
            NewItems = newItems,
            NewPayments = newPayments,
            NewShipments = newShipments,
            ActiveShipments = activeShipments
        });
    }
}
