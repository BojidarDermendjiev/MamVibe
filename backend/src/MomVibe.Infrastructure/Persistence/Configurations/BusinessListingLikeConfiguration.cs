namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>
/// EF Core configuration for <see cref="BusinessListingLike"/>. Mirrors the existing
/// <c>Like</c> shape: composite unique (UserId, ListingId), Cascade on listing delete,
/// NoAction on user delete to preserve historic likes on cleanup.
/// </summary>
public class BusinessListingLikeConfiguration : IEntityTypeConfiguration<BusinessListingLike>
{
    public void Configure(EntityTypeBuilder<BusinessListingLike> builder)
    {
        builder.Property(l => l.UserId).HasMaxLength(450).IsRequired();

        builder.HasIndex(l => l.UserId).HasDatabaseName("IX_BusinessListingLikes_UserId");
        builder.HasIndex(l => l.ListingId).HasDatabaseName("IX_BusinessListingLikes_ListingId");
        builder.HasIndex(l => new { l.UserId, l.ListingId })
            .IsUnique()
            .HasDatabaseName("UX_BusinessListingLikes_User_Listing");

        builder.HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(l => l.Listing)
            .WithMany(x => x.Likes)
            .HasForeignKey(l => l.ListingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
