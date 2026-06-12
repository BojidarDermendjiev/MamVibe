namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>EF Core configuration for <see cref="SubscriptionPlan"/> (config table).</summary>
public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.Property(p => p.Code).HasMaxLength(50).IsRequired();
        builder.Property(p => p.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.MonthlyPriceEur).HasColumnType("numeric(10,2)");
        builder.Property(p => p.StripePriceId).HasMaxLength(120);
        builder.Property(p => p.FeaturesJson).HasMaxLength(2000);

        builder.HasIndex(p => p.Code).IsUnique().HasDatabaseName("UX_SubscriptionPlans_Code");
        builder.HasIndex(p => p.SortOrder).HasDatabaseName("IX_SubscriptionPlans_SortOrder");
    }
}
