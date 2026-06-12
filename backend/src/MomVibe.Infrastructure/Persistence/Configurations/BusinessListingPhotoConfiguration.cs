namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>EF Core configuration for <see cref="BusinessListingPhoto"/>.</summary>
public class BusinessListingPhotoConfiguration : IEntityTypeConfiguration<BusinessListingPhoto>
{
    public void Configure(EntityTypeBuilder<BusinessListingPhoto> builder)
    {
        builder.Property(p => p.Url).HasMaxLength(500).IsRequired();

        builder.HasIndex(p => p.ListingId).HasDatabaseName("IX_BusinessListingPhotos_ListingId");
        builder.HasIndex(p => new { p.ListingId, p.DisplayOrder })
            .IsUnique()
            .HasDatabaseName("UX_BusinessListingPhotos_Listing_Order");

        builder.HasOne(p => p.Listing)
            .WithMany(l => l.Photos)
            .HasForeignKey(p => p.ListingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
