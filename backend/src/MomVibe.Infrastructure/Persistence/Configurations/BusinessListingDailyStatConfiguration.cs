namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>EF Core configuration for <see cref="BusinessListingDailyStat"/>.</summary>
public class BusinessListingDailyStatConfiguration : IEntityTypeConfiguration<BusinessListingDailyStat>
{
    public void Configure(EntityTypeBuilder<BusinessListingDailyStat> builder)
    {
        // One row per (listing, day).
        builder.HasIndex(s => new { s.ListingId, s.Date })
            .IsUnique()
            .HasDatabaseName("UX_BusinessListingDailyStats_Listing_Date");

        builder.HasOne(s => s.Listing)
            .WithMany()
            .HasForeignKey(s => s.ListingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
