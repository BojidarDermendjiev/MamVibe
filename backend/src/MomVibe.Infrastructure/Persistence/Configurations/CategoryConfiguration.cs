namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>
/// EF Core configuration for <see cref="Category"/> defining column sizes, required fields,
/// a unique index on <c>Slug</c>, and initial seed data for common categories.
/// </summary>
/// <remarks>
/// - Enforces database-level constraints via <c>HasMaxLength</c> and <c>IsRequired</c>.
/// - Adds a unique index on <c>Slug</c> to prevent duplicate categories.
/// - Seeds a curated set of initial categories with deterministic IDs and UTC <c>CreatedAt</c>.
///   Seeded data will be inserted during migrations if not already present.
/// </remarks>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    /// <summary>
    /// Configures the <see cref="Category"/> entity's column mappings, constraints, indexes, and seed data.
    /// </summary>
    /// <param name="builder">The entity type builder for <see cref="Category"/>.</param>
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.Slug).HasMaxLength(100).IsRequired();
        builder.HasIndex(c => c.Slug).IsUnique();

        builder.HasData(
            new Category { Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), Name = "Clothes", Description = "Baby clothing and accessories", Slug = "clothes", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901"), Name = "Strollers", Description = "Baby strollers and carriers", Slug = "strollers", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012"), Name = "Other", Description = "Other baby supplies and essentials", Slug = "other", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = Guid.Parse("d4e5f6a7-b8c9-0123-def0-234567890123"), Name = "Car Seats", Description = "Baby and toddler car seats", Slug = "car-seats", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = Guid.Parse("e5f6a7b8-c9d0-1234-ef01-345678901234"), Name = "Toys", Description = "Baby and toddler toys", Slug = "toys", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = Guid.Parse("f6a7b8c9-d0e1-2345-f012-456789012345"), Name = "Furniture", Description = "Baby furniture and nursery items", Slug = "furniture", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = Guid.Parse("a7b8c9d0-e1f2-3456-0123-567890123456"), Name = "Shoes", Description = "Kids shoes and footwear", Slug = "shoes", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = Guid.Parse("b8c9d0e1-f2a3-4567-1234-678901234567"), Name = "Feeding", Description = "Bottles, breast pumps, and feeding accessories", Slug = "feeding", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
