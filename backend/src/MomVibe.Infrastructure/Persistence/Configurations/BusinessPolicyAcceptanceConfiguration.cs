namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>EF Core configuration for <see cref="BusinessPolicyAcceptance"/>.</summary>
public class BusinessPolicyAcceptanceConfiguration : IEntityTypeConfiguration<BusinessPolicyAcceptance>
{
    public void Configure(EntityTypeBuilder<BusinessPolicyAcceptance> builder)
    {
        builder.Property(a => a.Ip).HasMaxLength(45);
        builder.Property(a => a.UserAgent).HasMaxLength(512);

        // One acceptance row per (profile, policy version).
        builder.HasIndex(a => new { a.BusinessProfileId, a.PolicyVersionId })
            .IsUnique()
            .HasDatabaseName("UX_BusinessPolicyAcceptances_Profile_Version");

        builder.HasOne(a => a.BusinessProfile)
            .WithMany(p => p.PolicyAcceptances)
            .HasForeignKey(a => a.BusinessProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.PolicyVersion)
            .WithMany()
            .HasForeignKey(a => a.PolicyVersionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
