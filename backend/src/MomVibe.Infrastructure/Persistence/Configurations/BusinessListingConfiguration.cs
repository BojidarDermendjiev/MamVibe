namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>
/// EF Core configuration for <see cref="BusinessListing"/>.
/// The composite browse-sort filter (IsActive + IsApproved + RankBoost + CreatedAt) is the
/// dominant query path; the lat/lng columns are stored as numeric(10,7) for sub-cm precision.
/// </summary>
public class BusinessListingConfiguration : IEntityTypeConfiguration<BusinessListing>
{
    public void Configure(EntityTypeBuilder<BusinessListing> builder)
    {
        builder.Property(x => x.Title).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.City).HasMaxLength(100).IsRequired();
        builder.Property(x => x.AddressLine).HasMaxLength(300);
        builder.Property(x => x.Schedule).HasMaxLength(500);

        builder.Property(x => x.ActivityType).HasConversion<int>();

        builder.Property(x => x.Latitude).HasColumnType("numeric(10,7)");
        builder.Property(x => x.Longitude).HasColumnType("numeric(10,7)");
        builder.Property(x => x.PriceFromEur).HasColumnType("numeric(10,2)");

        // 1 listing per profile.
        builder.HasIndex(x => x.BusinessProfileId).IsUnique().HasDatabaseName("UX_BusinessListings_BusinessProfileId");

        builder.HasIndex(x => x.City).HasDatabaseName("IX_BusinessListings_City");
        builder.HasIndex(x => x.ActivityType).HasDatabaseName("IX_BusinessListings_ActivityType");
        // Browse default sort: only active + approved listings, ordered by RankBoost then CreatedAt.
        builder.HasIndex(x => new { x.IsActive, x.IsApproved, x.RankBoost, x.CreatedAt })
            .IsDescending(false, false, true, true)
            .HasDatabaseName("IX_BusinessListings_BrowseSort");
        builder.HasIndex(x => x.IsApproved).HasDatabaseName("IX_BusinessListings_IsApproved");

        // BusinessProfile↔Listing FK is configured on BusinessProfileConfiguration to keep
        // the 1:1 declaration in one place; do not redefine it here.
    }
}
