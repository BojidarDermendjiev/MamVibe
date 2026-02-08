namespace MomVibe.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain.Entities;

/// <summary>
/// EF Core configuration for <see cref="ApplicationUser"/> defining validation-aligned column sizes,
/// default values, and creation timestamp behavior.
/// </summary>
/// <remarks>
/// - Applies explicit <c>HasMaxLength</c> and <c>IsRequired</c> to ensure DB-level enforcement.
/// - Sets sensible defaults (e.g., language preference, blocked flag) and uses UTC for <c>CreatedAt</c>.
/// - Consider centralizing these values in a constants class to avoid magic numbers and keep constraints consistent.
/// </remarks>
public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{

    /// <summary>
    /// Configures the <see cref="ApplicationUser"/> entity's column mappings, constraints, and defaults.
    /// </summary>
    /// <param name="builder">The entity type builder for <see cref="ApplicationUser"/>.</param>
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Bio).HasMaxLength(500);
        builder.Property(u => u.AvatarUrl).HasMaxLength(500);
        builder.Property(u => u.LanguagePreference).HasMaxLength(10).HasDefaultValue("en");
        builder.Property(u => u.IsBlocked).HasDefaultValue(false);
        builder.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
    }
}
