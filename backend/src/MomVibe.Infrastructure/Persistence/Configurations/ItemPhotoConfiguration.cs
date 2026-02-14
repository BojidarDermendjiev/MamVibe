namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

public class ItemPhotoConfiguration : IEntityTypeConfiguration<ItemPhoto>
{
    public void Configure(EntityTypeBuilder<ItemPhoto> builder)
    {
        builder.Property(p => p.Url).HasMaxLength(500).IsRequired();

        builder.HasIndex(p => p.ItemId);

        builder.HasOne(p => p.Item)
            .WithMany(i => i.Photos)
            .HasForeignKey(p => p.ItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
