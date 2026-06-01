namespace MomVibe.Infrastructure.EventHandlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Application.DTOs.Items;
using Application.Events;
using Application.Interfaces;

/// <summary>
/// On <see cref="ItemPriceDroppedEvent"/>, fans out a SignalR price-drop notification to
/// every user who has liked the item. Concurrency is capped at <see cref="MaxConcurrency"/>
/// dispatches so a single popular item can't exhaust the thread pool.
/// </summary>
public sealed class ItemPriceDroppedHandler : INotificationHandler<ItemPriceDroppedEvent>
{
    private const int MaxConcurrency = 50;

    private readonly IApplicationDbContext _context;
    private readonly IPriceDropNotifier _notifier;
    private readonly ILogger<ItemPriceDroppedHandler> _logger;

    public ItemPriceDroppedHandler(
        IApplicationDbContext context,
        IPriceDropNotifier notifier,
        ILogger<ItemPriceDroppedHandler> logger)
    {
        this._context = context;
        this._notifier = notifier;
        this._logger = logger;
    }

    public async Task Handle(ItemPriceDroppedEvent notification, CancellationToken cancellationToken)
    {
        var itemTitle = await this._context.Items
            .AsNoTracking()
            .Where(i => i.Id == notification.ItemId)
            .Select(i => i.Title)
            .FirstOrDefaultAsync(cancellationToken);
        if (itemTitle is null) return;

        var photoUrl = await this._context.ItemPhotos
            .AsNoTracking()
            .Where(p => p.ItemId == notification.ItemId)
            .OrderBy(p => p.DisplayOrder)
            .Select(p => p.Url)
            .FirstOrDefaultAsync(cancellationToken);

        var payload = new PriceDropNotification
        {
            ItemId = notification.ItemId,
            ItemTitle = itemTitle,
            OldPrice = notification.OldPrice,
            NewPrice = notification.NewPrice,
            PhotoUrl = photoUrl
        };

        var likerIds = await this._context.Likes
            .AsNoTracking()
            .Where(l => l.ItemId == notification.ItemId)
            .Select(l => l.UserId)
            .ToListAsync(cancellationToken);
        if (likerIds.Count == 0) return;

        using var semaphore = new SemaphoreSlim(MaxConcurrency, MaxConcurrency);
        var tasks = likerIds.Select(async uid =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                await this._notifier.NotifyAsync(uid, payload);
            }
            catch (Exception ex)
            {
                this._logger.LogWarning(ex, "Price-drop SignalR notification failed for item {ItemId}, recipient {UserId}", notification.ItemId, uid);
            }
            finally
            {
                semaphore.Release();
            }
        });
        await Task.WhenAll(tasks);
    }
}
