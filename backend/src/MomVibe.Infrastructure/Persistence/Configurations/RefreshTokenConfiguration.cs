namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>
/// EF Core configuration for <see cref="RefreshToken"/> defining token length constraints,
/// indexing, and the relationship to <see cref="ApplicationUser"/> with cascading deletes.
/// </summary>
/// <remarks>
/// - Enforces required <c>Token</c> with a maximum length and optional <c>ReplacedByToken</c>.
/// - Adds an index on <c>Token</c> to speed lookup; consider making it unique if tokens are guaranteed unique.
/// - Configures a many-to-one relationship to <see cref="ApplicationUser"/> with cascade delete,
///   removing refresh tokens when the owning user is deleted.
/// - You may also consider additional indexes (e.g., <c>ExpiresAt</c>, <c>RevokedAt</c>) and
///   check constraints (e.g., expiry after creation) for integrity and performance.
/// </remarks>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    /// <summary>
    /// Configures the <see cref="RefreshToken"/> entity's column mappings, indexes, and relationships.
    /// </summary>
    /// <param name="builder">The entity type builder for <see cref="RefreshToken"/>.</param>
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.Property(r => r.Token).HasMaxLength(500).IsRequired();
        builder.Property(r => r.ReplacedByToken).HasMaxLength(500);

        builder.HasIndex(r => r.Token);

        builder.HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
