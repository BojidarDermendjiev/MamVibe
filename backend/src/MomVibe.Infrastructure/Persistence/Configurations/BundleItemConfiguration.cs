namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>
/// Configures the EF Core schema for the <see cref="BundleItem"/> entity.
/// </summary>
public class BundleItemConfiguration : IEntityTypeConfiguration<BundleItem>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<BundleItem> builder)
    {
        builder.HasIndex(bi => bi.BundleId);
        builder.HasIndex(bi => bi.ItemId);

        builder.HasOne(bi => bi.Item)
            .WithMany()
            .HasForeignKey(bi => bi.ItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
