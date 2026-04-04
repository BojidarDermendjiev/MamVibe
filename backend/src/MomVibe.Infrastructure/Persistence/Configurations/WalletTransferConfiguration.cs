namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

public class WalletTransferConfiguration : IEntityTypeConfiguration<WalletTransfer>
{
    public void Configure(EntityTypeBuilder<WalletTransfer> builder)
    {
        builder.Property(t => t.Amount).HasColumnType("numeric(18,2)");

        builder.HasIndex(t => t.SenderWalletId);
        builder.HasIndex(t => t.ReceiverWalletId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.CreatedAt);

        builder.HasOne(t => t.SenderTransaction)
            .WithMany()
            .HasForeignKey(t => t.SenderTransactionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(t => t.ReceiverTransaction)
            .WithMany()
            .HasForeignKey(t => t.ReceiverTransactionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
