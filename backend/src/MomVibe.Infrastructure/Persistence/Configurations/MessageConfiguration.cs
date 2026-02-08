namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>
/// EF Core configuration for <see cref="Message"/> defining content constraints, default read state,
/// supporting indexes, and relationships to sender and receiver users.
/// </summary>
/// <remarks>
/// - Enforces required <c>Content</c> with a maximum length for DB-level validation.
/// - Sets <c>IsRead</c> default to <c>false</c> for newly created messages.
/// - Adds indexes to optimize queries by participants and creation time.
/// - Uses <c>DeleteBehavior.NoAction</c> on both user relationships to avoid multiple cascade path
///   issues (notably on SQL Server) and to preserve messages when users are removed, if desired.
/// </remarks>
public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    /// <summary>
    /// Configures the <see cref="Message"/> entity's column mappings, defaults, indexes, and relationships.
    /// </summary>
    /// <param name="builder">The entity type builder for <see cref="Message"/>.</param>
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.Property(m => m.Content).HasMaxLength(2000).IsRequired();
        builder.Property(m => m.IsRead).HasDefaultValue(false);

        builder.HasIndex(m => m.SenderId);
        builder.HasIndex(m => m.ReceiverId);
        builder.HasIndex(m => m.CreatedAt);

        builder.HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(m => m.Receiver)
            .WithMany(u => u.ReceivedMessages)
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
