namespace MomVibe.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using NpgsqlTypes;

using Domain.Entities;
using Application.Interfaces;

/// <summary>
/// The primary EF Core database context for the MomVibe application.
/// Inherits ASP.NET Core Identity tables via <see cref="IdentityDbContext{TUser}"/> and
/// implements <see cref="IApplicationDbContext"/> for dependency-inversion across layers.
/// Automatically stamps <c>CreatedAt</c> and <c>UpdatedAt</c> on <see cref="Domain.Common.BaseEntity"/> entries.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="ApplicationDbContext"/> with the given options.
    /// </summary>
    /// <param name="options">EF Core context options, typically configured in <c>StartUp.cs</c>.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    /// <inheritdoc/>
    public DbSet<Item> Items => Set<Item>();

    /// <inheritdoc/>
    public DbSet<Bundle> Bundles => Set<Bundle>();

    /// <inheritdoc/>
    public DbSet<BundleItem> BundleItems => Set<BundleItem>();

    /// <inheritdoc/>
    public DbSet<ItemPhoto> ItemPhotos => Set<ItemPhoto>();

    /// <inheritdoc/>
    public DbSet<Category> Categories => Set<Category>();

    /// <inheritdoc/>
    public DbSet<Like> Likes => Set<Like>();

    /// <inheritdoc/>
    public DbSet<Message> Messages => Set<Message>();

    /// <inheritdoc/>
    public DbSet<Payment> Payments => Set<Payment>();

    /// <inheritdoc/>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <inheritdoc/>
    public DbSet<Feedback> Feedbacks => Set<Feedback>();

    /// <inheritdoc/>
    public DbSet<Shipment> Shipments => Set<Shipment>();

    /// <inheritdoc/>
    public DbSet<PurchaseRequest> PurchaseRequests => Set<PurchaseRequest>();

    /// <inheritdoc/>
    public DbSet<DoctorReview> DoctorReviews => Set<DoctorReview>();

    /// <inheritdoc/>
    public DbSet<ChildFriendlyPlace> ChildFriendlyPlaces => Set<ChildFriendlyPlace>();

    /// <inheritdoc/>
    public DbSet<UserRating> UserRatings => Set<UserRating>();

    /// <inheritdoc/>
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    /// <inheritdoc/>
    public DbSet<ItemModerationLog> ItemModerationLogs => Set<ItemModerationLog>();

    /// <inheritdoc/>
    public DbSet<UserModerationLog> UserModerationLogs => Set<UserModerationLog>();

    /// <inheritdoc/>
    public DbSet<AbuseReport> AbuseReports => Set<AbuseReport>();

    /// <inheritdoc/>
    public DbSet<AbuseSignal> AbuseSignals => Set<AbuseSignal>();

    /// <inheritdoc/>
    public DbSet<ModerationAppeal> ModerationAppeals => Set<ModerationAppeal>();

    /// <inheritdoc/>
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    /// <inheritdoc/>
    public DbSet<Offer> Offers => Set<Offer>();

    /// <inheritdoc/>
    public DbSet<Follow> Follows => Set<Follow>();

    /// <inheritdoc/>
    public DbSet<SavedSearch> SavedSearches => Set<SavedSearch>();

    /// <inheritdoc/>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <inheritdoc/>
    public DbSet<KnowledgeArticle> KnowledgeArticles => Set<KnowledgeArticle>();

    /// <inheritdoc/>
    public DbSet<BusinessProfile> BusinessProfiles => Set<BusinessProfile>();

    /// <inheritdoc/>
    public DbSet<BusinessListing> BusinessListings => Set<BusinessListing>();

    /// <inheritdoc/>
    public DbSet<BusinessListingPhoto> BusinessListingPhotos => Set<BusinessListingPhoto>();

    /// <inheritdoc/>
    public DbSet<BusinessListingLike> BusinessListingLikes => Set<BusinessListingLike>();

    /// <inheritdoc/>
    public DbSet<BusinessListingComment> BusinessListingComments => Set<BusinessListingComment>();

    /// <inheritdoc/>
    public DbSet<BusinessListingViewEvent> BusinessListingViewEvents => Set<BusinessListingViewEvent>();

    /// <inheritdoc/>
    public DbSet<BusinessListingDailyStat> BusinessListingDailyStats => Set<BusinessListingDailyStat>();

    /// <inheritdoc/>
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();

    /// <inheritdoc/>
    public DbSet<BusinessSubscription> BusinessSubscriptions => Set<BusinessSubscription>();

    /// <inheritdoc/>
    public DbSet<BusinessSubscriptionEvent> BusinessSubscriptionEvents => Set<BusinessSubscriptionEvent>();

    /// <inheritdoc/>
    public DbSet<BusinessPolicyVersion> BusinessPolicyVersions => Set<BusinessPolicyVersion>();

    /// <inheritdoc/>
    public DbSet<BusinessPolicyAcceptance> BusinessPolicyAcceptances => Set<BusinessPolicyAcceptance>();

    /// <inheritdoc/>
    public DbSet<DeviceFingerprint> DeviceFingerprints => Set<DeviceFingerprint>();

    /// <inheritdoc/>
    public DbSet<DeviceFingerprintUser> DeviceFingerprintUsers => Set<DeviceFingerprintUser>();

    /// <inheritdoc/>
    public DbSet<PromoterProfile> PromoterProfiles => Set<PromoterProfile>();

    /// <inheritdoc/>
    public DbSet<CoachReferral> CoachReferrals => Set<CoachReferral>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        Seed.KnowledgeArticleSeed.Seed(builder);

        // Postgres-only: stored generated tsvector column + GIN index for full-text search.
        // 'simple' config lowercases without stemming, which works for both EN and BG.
        // Skipped on InMemory/SQLite providers so unit tests build the model without errors.
        if (Database.IsNpgsql())
        {
            builder.Entity<KnowledgeArticle>()
                .Property<NpgsqlTsVector>("SearchVector")
                .HasComputedColumnSql(
                    "to_tsvector('simple', coalesce(\"Title\",'') || ' ' || coalesce(\"Content\",''))",
                    stored: true);

            builder.Entity<KnowledgeArticle>()
                .HasIndex("SearchVector")
                .HasMethod("GIN");
        }
    }

    /// <summary>
    /// Persists all pending changes to the database, automatically setting audit timestamps
    /// on <see cref="Domain.Common.BaseEntity"/> instances before saving.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The number of state entries written to the database.</returns>
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
