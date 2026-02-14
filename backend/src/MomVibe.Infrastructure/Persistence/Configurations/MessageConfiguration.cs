namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.Property(m => m.Content).HasMaxLength(2000).IsRequired();
        builder.Property(m => m.IsRead).HasDefaultValue(false);

        builder.HasIndex(m => m.SenderId);
        builder.HasIndex(m => m.ReceiverId);
        builder.HasIndex(m => m.CreatedAt);
        builder.HasIndex(m => new { m.SenderId, m.ReceiverId, m.CreatedAt });
        builder.HasIndex(m => new { m.ReceiverId, m.IsRead });

        builder.HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(m => m.Receiver)
            .WithMany(u => u.ReceivedMessages)
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
