namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>
/// EF Core configuration for <see cref="AbuseSignal"/> — auto-detected events queued for admin
/// review. Indexes are tuned for the admin "unacknowledged signals, newest first, grouped by
/// subject user" query that drives the abuse-signals admin page.
/// </summary>
public class AbuseSignalConfiguration : IEntityTypeConfiguration<AbuseSignal>
{
    public void Configure(EntityTypeBuilder<AbuseSignal> builder)
    {
        builder.Property(x => x.SubjectUserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Details).HasMaxLength(2000);
        builder.Property(x => x.EvidenceTargetId).HasMaxLength(450);
        builder.Property(x => x.AcknowledgedByAdminId).HasMaxLength(450);

        builder.Property(x => x.Type).HasConversion<int>();

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

        builder.HasIndex(x => new { x.SubjectUserId, x.CreatedAt })
            .IsDescending(false, true)
            .HasDatabaseName("IX_AbuseSignals_Subject_CreatedAt");

        builder.HasIndex(x => new { x.Acknowledged, x.CreatedAt })
            .HasDatabaseName("IX_AbuseSignals_Acknowledged_CreatedAt");

        builder.HasIndex(x => new { x.Type, x.CreatedAt })
            .HasDatabaseName("IX_AbuseSignals_Type_CreatedAt");
    }
}
