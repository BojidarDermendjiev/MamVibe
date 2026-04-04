namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.Property(t => t.Amount).HasColumnType("numeric(18,2)");
        builder.Property(t => t.BalanceAfter).HasColumnType("numeric(18,2)");

        builder.HasIndex(t => t.WalletId);
        builder.HasIndex(t => t.Kind);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.CreatedAt);
        builder.HasIndex(t => new { t.WalletId, t.CreatedAt });

        // Self-referencing link between the two legs of a transfer
        builder.HasOne<WalletTransaction>()
            .WithMany()
            .HasForeignKey(t => t.RelatedTransactionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(t => t.Payment)
            .WithMany()
            .HasForeignKey(t => t.PaymentId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
