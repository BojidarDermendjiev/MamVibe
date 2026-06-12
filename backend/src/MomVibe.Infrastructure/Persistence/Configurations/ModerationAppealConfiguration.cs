namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;
using Domain.Enums;

/// <summary>
/// EF Core configuration for <see cref="ModerationAppeal"/>.
/// Partial unique index enforces "one open appeal per moderation event" at the database layer.
/// </summary>
public class ModerationAppealConfiguration : IEntityTypeConfiguration<ModerationAppeal>
{
    public void Configure(EntityTypeBuilder<ModerationAppeal> builder)
    {
        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.UserStatement).HasMaxLength(3000).IsRequired();
        builder.Property(x => x.AdminId).HasMaxLength(450);
        builder.Property(x => x.AdminDecisionNote).HasMaxLength(2000);

        builder.Property(x => x.Status).HasConversion<int>().HasDefaultValue(AppealStatus.Pending);

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

        // Admin queue ordered by status (Pending first) then oldest first within a status.
        builder.HasIndex(x => new { x.Status, x.CreatedAt })
            .HasDatabaseName("IX_ModerationAppeals_Status_CreatedAt");

        // User's own appeals history.
        builder.HasIndex(x => new { x.UserId, x.CreatedAt })
            .IsDescending(false, true)
            .HasDatabaseName("IX_ModerationAppeals_User_CreatedAt");

        // One open appeal per moderation event. Status 0 = Pending, 1 = UnderReview.
        builder.HasIndex(x => x.ModerationLogId)
            .HasFilter("\"Status\" IN (0, 1)")
            .IsUnique()
            .HasDatabaseName("UX_ModerationAppeals_OpenPerEvent");
    }
}
