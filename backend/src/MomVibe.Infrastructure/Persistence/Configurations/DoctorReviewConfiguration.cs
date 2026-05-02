namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

public class DoctorReviewConfiguration : IEntityTypeConfiguration<DoctorReview>
{
    public void Configure(EntityTypeBuilder<DoctorReview> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DoctorName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Specialization).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ClinicName).HasMaxLength(150);
        builder.Property(x => x.City).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Content).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.SuperdocUrl).HasMaxLength(2048);
        builder.HasIndex(x => x.City);
        builder.HasIndex(x => x.Specialization);
        builder.HasOne(x => x.User)
            .WithMany(u => u.DoctorReviews)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
