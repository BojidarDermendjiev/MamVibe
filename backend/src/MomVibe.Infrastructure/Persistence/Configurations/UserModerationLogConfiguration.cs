namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>
/// EF Core configuration for <see cref="UserModerationLog"/> — append-only history of
/// graded moderation actions applied to user accounts.
/// </summary>
public class UserModerationLogConfiguration : IEntityTypeConfiguration<UserModerationLog>
{
    public void Configure(EntityTypeBuilder<UserModerationLog> builder)
    {
        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.AdminId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.AdminDisplayName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PublicReason).HasMaxLength(500).IsRequired();
        builder.Property(x => x.InternalNote).HasMaxLength(2000);

        builder.Property(x => x.PreviousLevel).HasConversion<int>();
        builder.Property(x => x.NewLevel).HasConversion<int>();
        builder.Property(x => x.Reason).HasConversion<int>();

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

        // Primary query: per-user history sorted newest-first.
        builder.HasIndex(x => new { x.UserId, x.CreatedAt })
            .IsDescending(false, true)
            .HasDatabaseName("IX_UserModerationLogs_User_CreatedAt");

        // Analytics: how many users are at each level over time.
        builder.HasIndex(x => x.NewLevel)
            .HasDatabaseName("IX_UserModerationLogs_NewLevel");

        // Look-ups when resolving a report or appeal to find the resulting log.
        builder.HasIndex(x => x.RelatedReportId)
            .HasDatabaseName("IX_UserModerationLogs_RelatedReportId");
    }
}
