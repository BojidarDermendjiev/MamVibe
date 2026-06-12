namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>EF Core configuration for <see cref="DeviceFingerprint"/> (hash-keyed registry).</summary>
public class DeviceFingerprintConfiguration : IEntityTypeConfiguration<DeviceFingerprint>
{
    public void Configure(EntityTypeBuilder<DeviceFingerprint> builder)
    {
        builder.HasKey(f => f.Hash);
        builder.Property(f => f.Hash).HasMaxLength(128);
    }
}
