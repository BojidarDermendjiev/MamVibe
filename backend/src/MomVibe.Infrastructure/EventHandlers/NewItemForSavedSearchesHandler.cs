namespace MomVibe.Infrastructure.EventHandlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Application.Events;
using Application.Interfaces;

/// <summary>
/// On <see cref="ItemCreatedEvent"/>, asks <see cref="ISavedSearchService"/> to match
/// the new item against every active saved search and push SignalR notifications to
/// matching subscribers. Skipped while the item is still inactive (pending moderation).
/// </summary>
public sealed class NewItemForSavedSearchesHandler : INotificationHandler<ItemCreatedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly ISavedSearchService _savedSearches;
    private readonly IItemService _items;
    private readonly ILogger<NewItemForSavedSearchesHandler> _logger;

    public NewItemForSavedSearchesHandler(
        IApplicationDbContext context,
        ISavedSearchService savedSearches,
        IItemService items,
        ILogger<NewItemForSavedSearchesHandler> logger)
    {
        this._context = context;
        this._savedSearches = savedSearches;
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

            var dto = await this._items.GetByIdAsync(notification.ItemId);
            if (dto is null) return;

            await this._savedSearches.NotifyMatchingSearchesAsync(dto);
        }
        catch (Exception ex)
        {
            this._logger.LogWarning(ex, "Saved-search notification fan-out failed for item {ItemId}", notification.ItemId);
        }
    }
}
