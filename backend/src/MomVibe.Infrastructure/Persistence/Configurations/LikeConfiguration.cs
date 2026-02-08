namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>
/// EF Core configuration for <see cref="Like"/> that enforces uniqueness per (UserId, ItemId)
/// and defines relationships to <see cref="ApplicationUser"/> and <see cref="Item"/>.
/// </summary>
/// <remarks>
/// - Composite unique index prevents a user from liking the same item more than once.
/// - Uses <c>DeleteBehavior.NoAction</c> for the user relationship to avoid removing likes when
///   a user is deleted (or to prevent multiple cascade paths). Adjust to <c>Cascade</c> if your
///   domain requires likes to be removed with the user.
/// - Uses <c>DeleteBehavior.Cascade</c> for the item relationship so removing an item removes
///   associated likes.
/// </remarks>
public class LikeConfiguration : IEntityTypeConfiguration<Like>
{
    /// <summary>
    /// Configures the <see cref="Like"/> entity's indexes and relationships.
    /// </summary>
    /// <param name="builder">The entity type builder for <see cref="Like"/>.</param>
    public void Configure(EntityTypeBuilder<Like> builder)
    {
        builder.HasIndex(l => new { l.UserId, l.ItemId }).IsUnique();

        builder.HasOne(l => l.User)
            .WithMany(u => u.Likes)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(l => l.Item)
            .WithMany(i => i.Likes)
            .HasForeignKey(l => l.ItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
