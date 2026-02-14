namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.Property(p => p.Amount).HasColumnType("numeric(18,2)");
        builder.Property(p => p.StripeSessionId).HasMaxLength(500);

        builder.HasIndex(p => p.ItemId);
        builder.HasIndex(p => p.BuyerId);
        builder.HasIndex(p => p.SellerId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.CreatedAt);
        builder.HasIndex(p => new { p.BuyerId, p.Status });
        builder.HasIndex(p => new { p.SellerId, p.Status });

        builder.HasOne(p => p.Item)
            .WithMany()
            .HasForeignKey(p => p.ItemId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(p => p.Buyer)
            .WithMany()
            .HasForeignKey(p => p.BuyerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(p => p.Seller)
            .WithMany()
            .HasForeignKey(p => p.SellerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
