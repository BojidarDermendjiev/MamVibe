namespace MomVibe.Application.Events;

/// <summary>
/// Raised when an item's price is reduced via <c>ItemService.UpdateAsync</c>.
/// The dedicated handler fans the notification out to every user who has liked
/// the item, capped at <c>50</c> concurrent SignalR dispatches to protect the
/// thread pool on popular items.
/// </summary>
/// <param name="ItemId">The item whose price decreased.</param>
/// <param name="OldPrice">Previous price in EUR.</param>
/// <param name="NewPrice">New (lower) price in EUR.</param>
public sealed record ItemPriceDroppedEvent(Guid ItemId, decimal OldPrice, decimal NewPrice) : IDomainEvent;
