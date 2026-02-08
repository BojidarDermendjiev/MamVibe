namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>
/// EF Core configuration for <see cref="Shipment"/> defining monetary precision, weight precision,
/// string lengths, and the relationship to <see cref="Payment"/> with non-cascading delete.
/// </summary>
/// <remarks>
/// - Sets currency columns to <c>decimal(18,2)</c> for precision.
/// - Sets weight to <c>decimal(10,3)</c> for sub-gram accuracy.
/// - Uses <c>DeleteBehavior.NoAction</c> on Payment FK to preserve shipment history.
/// </remarks>
public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    /// <summary>
    /// Configures the <see cref="Shipment"/> entity's column mappings and relationships.
    /// </summary>
    /// <param name="builder">The entity type builder for <see cref="Shipment"/>.</param>
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.Property(s => s.ShippingPrice).HasColumnType("decimal(18,2)");
        builder.Property(s => s.CodAmount).HasColumnType("decimal(18,2)");
        builder.Property(s => s.InsuredAmount).HasColumnType("decimal(18,2)");
        builder.Property(s => s.Weight).HasColumnType("decimal(10,3)");

        builder.HasOne(s => s.Payment)
            .WithMany()
            .HasForeignKey(s => s.PaymentId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
