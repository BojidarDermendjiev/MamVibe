namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>EF Core configuration for <see cref="DeviceFingerprintUser"/> (hash↔user join).</summary>
public class DeviceFingerprintUserConfiguration : IEntityTypeConfiguration<DeviceFingerprintUser>
{
    public void Configure(EntityTypeBuilder<DeviceFingerprintUser> builder)
    {
        builder.Property(u => u.FingerprintHash).HasMaxLength(128).IsRequired();
        builder.Property(u => u.UserId).HasMaxLength(450).IsRequired();

        builder.HasIndex(u => new { u.FingerprintHash, u.UserId })
            .IsUnique()
            .HasDatabaseName("UX_DeviceFingerprintUsers_Hash_User");
        builder.HasIndex(u => u.UserId).HasDatabaseName("IX_DeviceFingerprintUsers_UserId");

        builder.HasOne(u => u.Fingerprint)
            .WithMany(f => f.LinkedUsers)
            .HasForeignKey(u => u.FingerprintHash)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.User)
            .WithMany()
            .HasForeignKey(u => u.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
