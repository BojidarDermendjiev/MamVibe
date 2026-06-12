namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>
/// EF Core configuration for <see cref="BusinessPolicyVersion"/>.
/// A partial unique index ensures at most one row per language carries <c>IsCurrent=true</c> —
/// the service layer pre-checks duplicates because the InMemory provider ignores the filter.
/// </summary>
public class BusinessPolicyVersionConfiguration : IEntityTypeConfiguration<BusinessPolicyVersion>
{
    public void Configure(EntityTypeBuilder<BusinessPolicyVersion> builder)
    {
        builder.Property(v => v.Language).HasMaxLength(10).IsRequired();
        builder.Property(v => v.Title).HasMaxLength(200).IsRequired();
        builder.Property(v => v.BodyMarkdown).HasColumnType("text").IsRequired();

        builder.HasIndex(v => new { v.Language, v.Version })
            .IsUnique()
            .HasDatabaseName("UX_BusinessPolicyVersions_Language_Version");

        // Exactly one current version per language; HasFilter applies on PostgreSQL.
        builder.HasIndex(v => new { v.Language, v.IsCurrent })
            .HasFilter("\"IsCurrent\" = true")
            .IsUnique()
            .HasDatabaseName("UX_BusinessPolicyVersions_Language_Current");
    }
}
