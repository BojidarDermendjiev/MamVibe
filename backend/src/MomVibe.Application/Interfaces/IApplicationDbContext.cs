namespace MomVibe.Application.Interfaces;

using Microsoft.EntityFrameworkCore;

using Domain.Entities;

/// <summary>
/// Application data context abstraction over EF Core.
/// Exposes DbSets for core entities (Items, ItemPhotos, Categories, Likes, Messages, Payments,
/// RefreshTokens, Feedbacks, Shipments) and SaveChangesAsync for committing changes.
/// Facilitates testability and layering by decoupling services from the concrete DbContext.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Item> Items { get; }
    DbSet<ItemPhoto> ItemPhotos { get; }
    DbSet<Category> Categories { get; }
    DbSet<Like> Likes { get; }
    DbSet<Message> Messages { get; }
    DbSet<Payment> Payments { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Feedback> Feedbacks { get; }
    DbSet<Shipment> Shipments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
