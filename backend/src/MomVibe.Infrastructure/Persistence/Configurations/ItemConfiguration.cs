namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>
/// EF Core configuration for <see cref="Item"/> defining column sizes, monetary precision,
/// sensible defaults, indexes, and relationships to <see cref="ApplicationUser"/> and <see cref="Category"/>.
/// </summary>
/// <remarks>
/// - Enforces DB-level constraints (required fields, max lengths, decimal precision).
/// - Sets defaults for activity and engagement counters.
/// - Adds indexes for common query patterns (by user, category, activity, creation time).
/// - Configures cascades/restrictions to protect data integrity.
/// </remarks>
public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    /// <summary>
    /// Configures the <see cref="Item"/> entity's column mappings, constraints, defaults,
    /// indexes, and relationships.
    /// </summary>
    /// <param name="builder">The entity type builder for <see cref="Item"/>.</param>
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
