namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;
using Domain.Enums;

/// <summary>
/// EF Core configuration for <see cref="BusinessProfile"/>.
/// Enforces one-profile-per-user (unique UserId), persists enum statuses as int for
/// index-friendly filtering, and indexes the admin-queue lookups (Status + City + Kind).
/// </summary>
public class BusinessProfileConfiguration : IEntityTypeConfiguration<BusinessProfile>
{
    public void Configure(EntityTypeBuilder<BusinessProfile> builder)
    {
        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.LegalName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Bio).HasMaxLength(2000);
        builder.Property(x => x.ContactEmail).HasMaxLength(254).IsRequired();
        builder.Property(x => x.ContactPhone).HasMaxLength(32);
        builder.Property(x => x.Website).HasMaxLength(2048);
        builder.Property(x => x.City).HasMaxLength(100).IsRequired();
        builder.Property(x => x.StripeCustomerId).HasMaxLength(64);
        builder.Property(x => x.DeviceFingerprintHash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.IpAtRegistration).HasMaxLength(45);
        builder.Property(x => x.UserAgentAtRegistration).HasMaxLength(512);
        builder.Property(x => x.DeviceCheckBypassedByAdminId).HasMaxLength(450);
        builder.Property(x => x.DeviceCheckBypassReason).HasMaxLength(500);

        builder.Property(x => x.ProfileKind).HasConversion<int>();
        builder.Property(x => x.Category).HasConversion<int>().HasDefaultValue(BusinessCategory.Coach);
        builder.Property(x => x.Status).HasConversion<int>().HasDefaultValue(BusinessProfileStatus.PendingPolicy);

        // 1 profile per user.
        builder.HasIndex(x => x.UserId).IsUnique().HasDatabaseName("UX_BusinessProfiles_UserId");
        builder.HasIndex(x => x.Category).HasDatabaseName("IX_BusinessProfiles_Category");
        // Admin queue filters.
        builder.HasIndex(x => x.Status).HasDatabaseName("IX_BusinessProfiles_Status");
        builder.HasIndex(x => x.City).HasDatabaseName("IX_BusinessProfiles_City");
        builder.HasIndex(x => x.DeviceFingerprintHash).HasDatabaseName("IX_BusinessProfiles_DeviceFingerprintHash");
        builder.HasIndex(x => x.StripeCustomerId).HasDatabaseName("IX_BusinessProfiles_StripeCustomerId");

        builder.HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<BusinessProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Listing)
            .WithOne(l => l.BusinessProfile)
            .HasForeignKey<BusinessListing>(l => l.BusinessProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Subscription)
            .WithOne(s => s.BusinessProfile)
            .HasForeignKey<BusinessSubscription>(s => s.BusinessProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
