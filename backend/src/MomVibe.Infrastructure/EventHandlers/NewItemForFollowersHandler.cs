namespace MomVibe.Infrastructure.EventHandlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Application.Events;
using Application.Interfaces;

/// <summary>
/// On <see cref="ItemCreatedEvent"/>, fans out a SignalR <c>NewItemFromFollowedSeller</c>
/// push to every follower of the seller — but only when the item is already active
/// (auto-approved by AI moderation). Items still pending review stay invisible to
/// followers until an admin approves them.
/// </summary>
public sealed class NewItemForFollowersHandler : INotificationHandler<ItemCreatedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IFollowNotifier _notifier;
    private readonly IItemService _items;
    private readonly ILogger<NewItemForFollowersHandler> _logger;

    public NewItemForFollowersHandler(
        IApplicationDbContext context,
        IFollowNotifier notifier,
        IItemService items,
        ILogger<NewItemForFollowersHandler> logger)
    {
        this._context = context;
        this._notifier = notifier;
        this._items = items;
        this._logger = logger;
    }

    public async Task Handle(ItemCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var isActive = await this._context.Items
                .AsNoTracking()
                .Where(i => i.Id == notification.ItemId)
                .Select(i => (bool?)i.IsActive)
                .FirstOrDefaultAsync(cancellationToken);
            if (isActive != true) return;

            var followerIds = await this._context.Follows
                .AsNoTracking()
                .Where(f => f.FolloweeId == notification.SellerId)
                .Select(f => f.FollowerId)
                .ToListAsync(cancellationToken);
            if (followerIds.Count == 0) return;

            var dto = await this._items.GetByIdAsync(notification.ItemId);
            if (dto is null) return;

            await this._notifier.NotifyFollowersOfNewItemAsync(followerIds, dto);
        }
        catch (Exception ex)
        {
            this._logger.LogWarning(ex, "Follower-of-new-item notification fan-out failed for item {ItemId} by seller {SellerId}", notification.ItemId, notification.SellerId);
        }
    }
}
