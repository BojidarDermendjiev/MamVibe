namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

public class ItemModerationLogConfiguration : IEntityTypeConfiguration<ItemModerationLog>
{
    public void Configure(EntityTypeBuilder<ItemModerationLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AdminId).IsRequired().HasMaxLength(450);
        builder.Property(x => x.AdminDisplayName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ItemTitle).IsRequired().HasMaxLength(200);
        builder.Property(x => x.AiNotesAtTime).HasMaxLength(500);

        builder.HasIndex(x => new { x.ItemId, x.CreatedAt });
        builder.HasIndex(x => x.AdminId);
        builder.HasIndex(x => x.Action);
    }
}
