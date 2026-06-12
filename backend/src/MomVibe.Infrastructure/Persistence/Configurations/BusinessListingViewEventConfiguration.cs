namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>EF Core configuration for <see cref="BusinessListingViewEvent"/>.</summary>
public class BusinessListingViewEventConfiguration : IEntityTypeConfiguration<BusinessListingViewEvent>
{
    public void Configure(EntityTypeBuilder<BusinessListingViewEvent> builder)
    {
        builder.Property(e => e.ViewerHash).HasMaxLength(64).IsRequired();

        // Daily aggregator scans by (ListingId, OccurredAt) — keep the composite tight.
        builder.HasIndex(e => new { e.ListingId, e.OccurredAt })
            .HasDatabaseName("IX_BusinessListingViewEvents_Listing_OccurredAt");

        builder.HasOne(e => e.Listing)
            .WithMany()
            .HasForeignKey(e => e.ListingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
