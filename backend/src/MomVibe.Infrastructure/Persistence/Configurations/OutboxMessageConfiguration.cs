namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>
/// Configures the EF Core schema for the <see cref="OutboxMessage"/> entity.
/// </summary>
public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.Property(m => m.MessageType).HasMaxLength(100).IsRequired();
        builder.Property(m => m.LastError).HasMaxLength(1000);

        // Composite index keeps the pending-queue query cheap: filters on ProcessedAt IS NULL
        // and orders/filters by NextAttemptAt.
        builder.HasIndex(m => new { m.ProcessedAt, m.NextAttemptAt });
    }
}
