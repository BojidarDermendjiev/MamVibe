namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.HasIndex(w => w.UserId).IsUnique();
        builder.HasIndex(w => w.Status);

        builder.HasOne(w => w.User)
            .WithOne(u => u.Wallet)
            .HasForeignKey<Wallet>(w => w.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(w => w.Transactions)
            .WithOne(t => t.Wallet)
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(w => w.SentTransfers)
            .WithOne(t => t.SenderWallet)
            .HasForeignKey(t => t.SenderWalletId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(w => w.ReceivedTransfers)
            .WithOne(t => t.ReceiverWallet)
            .HasForeignKey(t => t.ReceiverWalletId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
