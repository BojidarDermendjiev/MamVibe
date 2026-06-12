namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>EF Core configuration for <see cref="BusinessListingComment"/>.</summary>
public class BusinessListingCommentConfiguration : IEntityTypeConfiguration<BusinessListingComment>
{
    public void Configure(EntityTypeBuilder<BusinessListingComment> builder)
    {
        builder.Property(c => c.UserId).HasMaxLength(450).IsRequired();
        builder.Property(c => c.Body).HasMaxLength(1000).IsRequired();
        builder.Property(c => c.HiddenReason).HasMaxLength(500);

        builder.HasIndex(c => c.ListingId).HasDatabaseName("IX_BusinessListingComments_ListingId");
        builder.HasIndex(c => c.UserId).HasDatabaseName("IX_BusinessListingComments_UserId");
        builder.HasIndex(c => new { c.ListingId, c.CreatedAt })
            .IsDescending(false, true)
            .HasDatabaseName("IX_BusinessListingComments_Listing_CreatedAt");
        builder.HasIndex(c => c.ParentCommentId).HasDatabaseName("IX_BusinessListingComments_ParentCommentId");

        builder.HasOne(c => c.Listing)
            .WithMany(l => l.Comments)
            .HasForeignKey(c => c.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.ParentComment)
            .WithMany()
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
