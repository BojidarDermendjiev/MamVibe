namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>
/// EF Core configuration for <see cref="Payment"/> defining monetary precision, Stripe session ID length,
/// and relationships to <see cref="Item"/>, buyer, and seller users with non-cascading deletes.
/// </summary>
/// <remarks>
/// - Sets <c>Amount</c> to <c>decimal(18,2)</c> for currency precision.
/// - Applies a maximum length to <c>StripeSessionId</c> to constrain stored identifiers.
/// - Uses <c>DeleteBehavior.NoAction</c> on all foreign keys to avoid multiple cascade paths
///   (due to two user relationships) and to preserve payment history records.
/// - Consider adding indexes (e.g., on <c>ItemId</c>, <c>BuyerId</c>, <c>SellerId</c>, <c>Status</c>, <c>CreatedAt</c>)
///   and a non-negative check constraint on <c>Amount</c> for performance and integrity.
/// </remarks>
public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    /// <summary>
    /// Configures the <see cref="Payment"/> entity's column mappings and relationships.
    /// </summary>
    /// <param name="builder">The entity type builder for <see cref="Payment"/>.</param>
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.Property(p => p.Amount).HasColumnType("numeric(18,2)");
        builder.Property(p => p.StripeSessionId).HasMaxLength(500);

        builder.HasOne(p => p.Item)
            .WithMany()
            .HasForeignKey(p => p.ItemId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(p => p.Buyer)
            .WithMany()
            .HasForeignKey(p => p.BuyerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(p => p.Seller)
            .WithMany()
            .HasForeignKey(p => p.SellerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
