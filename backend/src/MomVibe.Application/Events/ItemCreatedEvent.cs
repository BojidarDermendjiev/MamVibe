namespace MomVibe.Application.Events;

/// <summary>
/// Raised after a new <c>Item</c> row has been persisted (before AI moderation runs).
/// Subscribers replace the inline fan-out previously hard-coded in
/// <c>ItemService.CreateAsync</c>: AI moderation, the n8n <c>item.pending_approval</c>
/// admin alert, follower notifications, and saved-search match notifications.
/// </summary>
/// <param name="ItemId">The newly created item's identifier.</param>
/// <param name="SellerId">The owning seller's user id, so follower-notification handlers can avoid an extra fetch.</param>
public sealed record ItemCreatedEvent(Guid ItemId, string SellerId) : IDomainEvent;
