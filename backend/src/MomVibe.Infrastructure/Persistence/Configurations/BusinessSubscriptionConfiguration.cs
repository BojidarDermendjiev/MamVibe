namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;
using Domain.Enums;

/// <summary>EF Core configuration for <see cref="BusinessSubscription"/>.</summary>
public class BusinessSubscriptionConfiguration : IEntityTypeConfiguration<BusinessSubscription>
{
    public void Configure(EntityTypeBuilder<BusinessSubscription> builder)
    {
        builder.Property(s => s.PlanCode).HasMaxLength(50).IsRequired();
        builder.Property(s => s.StripeSubscriptionId).HasMaxLength(64);
        builder.Property(s => s.Status).HasConversion<int>().HasDefaultValue(BusinessSubscriptionStatus.Incomplete);

        // 1 subscription per profile — note: the FK declaration lives on BusinessProfileConfiguration.
        builder.HasIndex(s => s.BusinessProfileId)
            .IsUnique()
            .HasDatabaseName("UX_BusinessSubscriptions_BusinessProfileId");
        builder.HasIndex(s => s.StripeSubscriptionId).HasDatabaseName("IX_BusinessSubscriptions_StripeSubscriptionId");
        // Trial-expiry sweeper query: Status=Trialing AND TrialEndsAt <= now.
        builder.HasIndex(s => new { s.Status, s.TrialEndsAt }).HasDatabaseName("IX_BusinessSubscriptions_Status_TrialEndsAt");
        // Grace-period sweeper: Status=PastDue AND GracePeriodEndsAt <= now.
        builder.HasIndex(s => new { s.Status, s.GracePeriodEndsAt }).HasDatabaseName("IX_BusinessSubscriptions_Status_GraceEndsAt");
    }
}
