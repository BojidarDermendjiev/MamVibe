namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

public class ChildFriendlyPlaceConfiguration : IEntityTypeConfiguration<ChildFriendlyPlace>
{
    public void Configure(EntityTypeBuilder<ChildFriendlyPlace> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.Address).HasMaxLength(300);
        builder.Property(x => x.City).IsRequired().HasMaxLength(100);
        builder.Property(x => x.PhotoUrl).HasMaxLength(2048);
        builder.Property(x => x.Website).HasMaxLength(2048);
        builder.HasIndex(x => x.City);
        builder.HasIndex(x => x.PlaceType);
        builder.HasIndex(x => x.IsApproved);
        builder.HasOne(x => x.User)
            .WithMany(u => u.ChildFriendlyPlaces)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
