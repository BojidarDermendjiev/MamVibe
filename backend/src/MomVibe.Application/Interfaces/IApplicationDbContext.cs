namespace MomVibe.Application.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

using Domain.Entities;

/// <summary>
/// Abstracts the EF Core database context, exposing the application's entity sets
/// and persistence operations. Implement this interface for testability and
/// to enforce dependency inversion between Application and Infrastructure layers.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>Gets the set of marketplace items.</summary>
    DbSet<Item> Items { get; }

    /// <summary>Gets the set of photos attached to marketplace items.</summary>
    DbSet<ItemPhoto> ItemPhotos { get; }

    /// <summary>Gets the set of item categories.</summary>
    DbSet<Category> Categories { get; }

    /// <summary>Gets the set of item likes recorded by users.</summary>
    DbSet<Like> Likes { get; }

    /// <summary>Gets the set of direct messages exchanged between users.</summary>
    DbSet<Message> Messages { get; }

    /// <summary>Gets the set of payment records.</summary>
    DbSet<Payment> Payments { get; }

    /// <summary>Gets the set of refresh tokens issued to authenticated users.</summary>
    DbSet<RefreshToken> RefreshTokens { get; }

    /// <summary>Gets the set of user-submitted feedback entries.</summary>
    DbSet<Feedback> Feedbacks { get; }

    /// <summary>Gets the set of shipment records associated with completed payments.</summary>
    DbSet<Shipment> Shipments { get; }

    /// <summary>Gets the set of purchase requests initiated by buyers.</summary>
    DbSet<PurchaseRequest> PurchaseRequests { get; }

    /// <summary>Gets the set of doctor reviews submitted by users.</summary>
    DbSet<DoctorReview> DoctorReviews { get; }

    /// <summary>Gets the set of child-friendly place submissions.</summary>
    DbSet<ChildFriendlyPlace> ChildFriendlyPlaces { get; }

    /// <summary>Gets the set of user ratings given after completed transactions.</summary>
    DbSet<UserRating> UserRatings { get; }

    /// <summary>Gets the set of application-wide key/value settings.</summary>
    DbSet<AppSetting> AppSettings { get; }

    /// <summary>Gets the set of admin moderation action logs for items.</summary>
    DbSet<ItemModerationLog> ItemModerationLogs { get; }

    /// <summary>Gets the underlying <see cref="DatabaseFacade"/> for executing raw SQL and managing transactions.</summary>
    DatabaseFacade Database { get; }

    /// <summary>
    /// Persists all pending changes to the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
