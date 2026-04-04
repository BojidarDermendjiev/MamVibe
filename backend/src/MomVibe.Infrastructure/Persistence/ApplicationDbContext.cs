namespace MomVibe.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Domain.Entities;
using Application.Interfaces;
using Infrastructure.Persistence.Converters;

/// <summary>
/// Primary EF Core DbContext integrating ASP.NET Core Identity with domain entities.
/// Applies entity type configurations and centralizes audit timestamps on save.
/// When "Security:IbanEncryptionKey" (32-byte Base64) is configured, the IBAN column
/// is transparently encrypted at rest using AES-256-CBC.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    private readonly byte[]? _ibanEncryptionKey;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
        : base(options)
    {
        var keyBase64 = configuration["Security:IbanEncryptionKey"];
        if (!string.IsNullOrWhiteSpace(keyBase64))
        {
            _ibanEncryptionKey = Convert.FromBase64String(keyBase64);
            if (_ibanEncryptionKey.Length != 32)
                throw new InvalidOperationException(
                    "Security:IbanEncryptionKey must be a 32-byte Base64 string (e.g. openssl rand -base64 32).");
        }
    }

    public DbSet<Item> Items => Set<Item>();
    public DbSet<ItemPhoto> ItemPhotos => Set<ItemPhoto>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<PurchaseRequest> PurchaseRequests => Set<PurchaseRequest>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
    public DbSet<WalletTransfer> WalletTransfers => Set<WalletTransfer>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        if (_ibanEncryptionKey != null)
        {
            builder.Entity<ApplicationUser>()
                .Property(u => u.Iban)
                .HasConversion(new AesEncryptionConverter(_ibanEncryptionKey));
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}
