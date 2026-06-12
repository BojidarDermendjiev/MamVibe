namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;
using Domain.Enums;

/// <summary>
/// EF Core configuration for <see cref="AbuseReport"/>.
/// Includes a partial unique index that prevents the same reporter from filing multiple
/// open (Pending) reports against the same target — service-layer fallback for InMemory tests.
/// </summary>
public class AbuseReportConfiguration : IEntityTypeConfiguration<AbuseReport>
{
    public void Configure(EntityTypeBuilder<AbuseReport> builder)
    {
        builder.Property(x => x.ReporterId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.TargetId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.TargetUserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.ResolvedByAdminId).HasMaxLength(450);
        builder.Property(x => x.ResolutionNote).HasMaxLength(1000);

        builder.Property(x => x.TargetType).HasConversion<int>();
        builder.Property(x => x.Reason).HasConversion<int>();
        builder.Property(x => x.Status).HasConversion<int>().HasDefaultValue(ReportStatus.Pending);

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

        // Admin queue grouping: reports against a user, newest first.
        builder.HasIndex(x => new { x.TargetUserId, x.CreatedAt })
            .IsDescending(false, true)
            .HasDatabaseName("IX_AbuseReports_TargetUser_CreatedAt");

        // Admin queue filtering by status (Pending/UnderReview), oldest first within a status.
        builder.HasIndex(x => new { x.Status, x.CreatedAt })
            .HasDatabaseName("IX_AbuseReports_Status_CreatedAt");

        // Per-reporter rate-limit / abuse-of-reporting analytics.
        builder.HasIndex(x => new { x.ReporterId, x.CreatedAt })
            .IsDescending(false, true)
            .HasDatabaseName("IX_AbuseReports_Reporter_CreatedAt");

        // Prevent duplicate open reports from the same reporter against the same target.
        // PostgreSQL partial unique index using HasFilter — InMemory provider ignores the filter
        // and reports duplicates as a unique constraint violation, so service layer keeps a
        // pre-check to surface a clean 409 either way.
        builder.HasIndex(x => new { x.ReporterId, x.TargetType, x.TargetId })
            .HasFilter("\"Status\" = 0")
            .IsUnique()
            .HasDatabaseName("UX_AbuseReports_Reporter_Target_Pending");
    }
}
