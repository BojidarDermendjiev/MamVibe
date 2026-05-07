namespace MomVibe.Application.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

using Domain.Entities;

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
    DbSet<PurchaseRequest> PurchaseRequests { get; }
    DbSet<DoctorReview> DoctorReviews { get; }
    DbSet<ChildFriendlyPlace> ChildFriendlyPlaces { get; }
    DbSet<UserRating> UserRatings { get; }
    DbSet<AppSetting> AppSettings { get; }
    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
