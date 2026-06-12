namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;
using Domain.Enums;

/// <summary>
/// EF Core configuration for <see cref="CoachReferral"/>.
/// Dedup of repeated submissions for the same contact within 30 days is enforced
/// at the service layer (window-based, not naturally expressible as a unique index).
/// </summary>
public class CoachReferralConfiguration : IEntityTypeConfiguration<CoachReferral>
{
    public void Configure(EntityTypeBuilder<CoachReferral> builder)
    {
        builder.Property(r => r.BusinessName).HasMaxLength(200).IsRequired();
        builder.Property(r => r.ContactEmail).HasMaxLength(254);
        builder.Property(r => r.ContactPhone).HasMaxLength(32);
        builder.Property(r => r.City).HasMaxLength(100).IsRequired();
        builder.Property(r => r.Notes).HasMaxLength(2000);
        builder.Property(r => r.ReferrerUserId).HasMaxLength(450);
        builder.Property(r => r.ReferralCode).HasMaxLength(16);
        builder.Property(r => r.IpHash).HasMaxLength(64);
        builder.Property(r => r.AdminNote).HasMaxLength(1000);
        builder.Property(r => r.ActionedByAdminId).HasMaxLength(450);

        builder.Property(r => r.ActivityType).HasConversion<int>();
        builder.Property(r => r.Status).HasConversion<int>().HasDefaultValue(CoachReferralStatus.Submitted);

        builder.HasIndex(r => new { r.Status, r.CreatedAt })
            .HasDatabaseName("IX_CoachReferrals_Status_CreatedAt");
        builder.HasIndex(r => r.ContactEmail).HasDatabaseName("IX_CoachReferrals_ContactEmail");
        builder.HasIndex(r => r.ContactPhone).HasDatabaseName("IX_CoachReferrals_ContactPhone");
        builder.HasIndex(r => r.ReferralCode).HasDatabaseName("IX_CoachReferrals_ReferralCode");
        builder.HasIndex(r => r.ReferrerUserId).HasDatabaseName("IX_CoachReferrals_ReferrerUserId");

        builder.HasOne(r => r.Referrer)
            .WithMany()
            .HasForeignKey(r => r.ReferrerUserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
