namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>
/// EF Core configuration for <see cref="ItemPhoto"/> defining URL constraints and the relationship
/// to <see cref="Item"/> with cascade delete behavior.
/// </summary>
/// <remarks>
/// - Enforces <c>Url</c> to be required with a maximum length for database-level validation.
/// - Configures a many-to-one relationship: each photo belongs to a single item; deleting the item
///   cascades to its photos.
/// - Consider adding indexes (e.g., on <c>ItemId</c>) or a composite unique index on
///   <c>(ItemId, DisplayOrder)</c> if you need deterministic ordering and uniqueness per item.
/// </remarks>
public class ItemPhotoConfiguration : IEntityTypeConfiguration<ItemPhoto>
{
    /// <summary>
    /// Configures the <see cref="ItemPhoto"/> entity's column mappings and relationships.
    /// </summary>
    /// <param name="builder">The entity type builder for <see cref="ItemPhoto"/>.</param>
    public void Configure(EntityTypeBuilder<ItemPhoto> builder)
    {
        builder.Property(p => p.Url).HasMaxLength(500).IsRequired();
        builder.HasOne(p => p.Item)
            .WithMany(i => i.Photos)
            .HasForeignKey(p => p.ItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
