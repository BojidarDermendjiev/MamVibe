namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>EF Core configuration for <see cref="PromoterProfile"/>.</summary>
public class PromoterProfileConfiguration : IEntityTypeConfiguration<PromoterProfile>
{
    public void Configure(EntityTypeBuilder<PromoterProfile> builder)
    {
        builder.Property(p => p.UserId).HasMaxLength(450).IsRequired();
        builder.Property(p => p.ReferralCode).HasMaxLength(16).IsRequired();

        builder.HasIndex(p => p.UserId).IsUnique().HasDatabaseName("UX_PromoterProfiles_UserId");
        builder.HasIndex(p => p.ReferralCode).IsUnique().HasDatabaseName("UX_PromoterProfiles_ReferralCode");

        builder.HasOne(p => p.User)
            .WithOne()
            .HasForeignKey<PromoterProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
