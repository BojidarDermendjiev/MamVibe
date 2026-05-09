namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>
/// Configures the EF Core schema for the <see cref="UserRating"/> entity.
/// Enforces a unique index on <c>PurchaseRequestId</c> so each completed transaction can be rated at most once.
/// </summary>
public class UserRatingConfiguration : IEntityTypeConfiguration<UserRating>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<UserRating> builder)
    {
        builder.HasIndex(r => r.RatedUserId);
        builder.HasIndex(r => r.RaterId);
        builder.HasIndex(r => r.PurchaseRequestId).IsUnique();

        builder.HasOne(r => r.Rater)
            .WithMany(u => u.RatingsGiven)
            .HasForeignKey(r => r.RaterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.RatedUser)
            .WithMany(u => u.RatingsReceived)
            .HasForeignKey(r => r.RatedUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.PurchaseRequest)
            .WithMany()
            .HasForeignKey(r => r.PurchaseRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
