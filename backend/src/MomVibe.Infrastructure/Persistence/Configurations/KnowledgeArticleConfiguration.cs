namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

public class KnowledgeArticleConfiguration : IEntityTypeConfiguration<KnowledgeArticle>
{
    public void Configure(EntityTypeBuilder<KnowledgeArticle> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).UseIdentityByDefaultColumn();
        builder.Property(a => a.Title).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Content).IsRequired().HasMaxLength(4000);
        builder.Property(a => a.Language).IsRequired().HasMaxLength(5).HasDefaultValue("en");
        builder.Property(a => a.Tags).HasColumnType("text[]");

        builder.HasIndex(a => a.Language);
        // SearchVector (NpgsqlTsVector) and its GIN index are Postgres-only;
        // they are applied conditionally in ApplicationDbContext.OnModelCreating.
    }
}
