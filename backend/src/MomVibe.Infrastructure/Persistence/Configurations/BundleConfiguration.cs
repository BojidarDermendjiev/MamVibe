namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>
/// Configures the EF Core schema for the <see cref="Bundle"/> entity.
/// </summary>
public class BundleConfiguration : IEntityTypeConfiguration<Bundle>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Bundle> builder)
    {
        builder.Property(b => b.Price).HasColumnType("numeric(18,2)");
        builder.HasIndex(b => b.SellerId);

        builder.HasOne(b => b.Seller)
            .WithMany(u => u.Bundles)
            .HasForeignKey(b => b.SellerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.BundleItems)
            .WithOne(bi => bi.Bundle)
            .HasForeignKey(bi => bi.BundleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
