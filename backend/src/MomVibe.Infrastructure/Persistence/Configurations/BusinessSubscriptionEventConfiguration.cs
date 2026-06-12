namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>EF Core configuration for <see cref="BusinessSubscriptionEvent"/> (Stripe webhook ledger).</summary>
public class BusinessSubscriptionEventConfiguration : IEntityTypeConfiguration<BusinessSubscriptionEvent>
{
    public void Configure(EntityTypeBuilder<BusinessSubscriptionEvent> builder)
    {
        builder.Property(e => e.StripeEventId).HasMaxLength(64).IsRequired();
        builder.Property(e => e.RawType).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Type).HasConversion<int>();
        // PostgreSQL text — payload size is variable; the [MaxLength(32000)] is an upper bound only.
        builder.Property(e => e.PayloadJson).HasColumnType("text").IsRequired();

        // Stripe replays the same event on transient failures — dedup by event id.
        builder.HasIndex(e => e.StripeEventId)
            .IsUnique()
            .HasDatabaseName("UX_BusinessSubscriptionEvents_StripeEventId");
        builder.HasIndex(e => new { e.SubscriptionId, e.OccurredAt })
            .IsDescending(false, true)
            .HasDatabaseName("IX_BusinessSubscriptionEvents_Subscription_OccurredAt");

        builder.HasOne(e => e.Subscription)
            .WithMany(s => s.Events)
            .HasForeignKey(e => e.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
