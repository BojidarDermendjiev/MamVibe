namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.Property(s => s.ShippingPrice).HasColumnType("numeric(18,2)");
        builder.Property(s => s.CodAmount).HasColumnType("numeric(18,2)");
        builder.Property(s => s.InsuredAmount).HasColumnType("numeric(18,2)");
        builder.Property(s => s.Weight).HasColumnType("numeric(10,3)");

        builder.HasIndex(s => s.PaymentId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.CreatedAt);

        builder.HasOne(s => s.Payment)
            .WithMany()
            .HasForeignKey(s => s.PaymentId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
