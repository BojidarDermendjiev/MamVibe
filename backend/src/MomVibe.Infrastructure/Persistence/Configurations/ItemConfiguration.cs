namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>
/// Configures the EF Core schema for the <see cref="Item"/> entity.
/// </summary>
public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.Property(i => i.Title).HasMaxLength(200).IsRequired();
        builder.Property(i => i.Description).HasMaxLength(5000).IsRequired();
        builder.Property(i => i.Price).HasColumnType("numeric(18,2)");
        builder.Property(i => i.IsActive).HasDefaultValue(true);
        builder.Property(i => i.ViewCount).HasDefaultValue(0);
        builder.Property(i => i.LikeCount).HasDefaultValue(0);

        builder.HasIndex(i => i.UserId);
        builder.HasIndex(i => i.CategoryId);
        builder.HasIndex(i => i.IsActive);
        builder.HasIndex(i => i.CreatedAt);
        builder.HasIndex(i => new { i.IsActive, i.CreatedAt });

        // Composite indices covering common filter + sort combinations in GetAllAsync
        builder.HasIndex(i => new { i.IsActive, i.CategoryId, i.CreatedAt });
        builder.HasIndex(i => new { i.IsActive, i.ListingType, i.CreatedAt });
        builder.HasIndex(i => new { i.IsActive, i.LikeCount });
        builder.HasIndex(i => new { i.IsActive, i.ViewCount });

        builder.HasOne(i => i.User)
            .WithMany(u => u.Items)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Category)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
