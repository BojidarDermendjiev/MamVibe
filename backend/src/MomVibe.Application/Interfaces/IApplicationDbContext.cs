namespace MomVibe.Application.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

using Domain.Entities;

/// <summary>
/// Abstracts the EF Core database context, exposing the application's entity sets
/// and persistence operations. Implement this interface for testability and
/// to enforce dependency inversion between Application and Infrastructure layers.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>Gets the set of registered application users.</summary>
    DbSet<ApplicationUser> Users { get; }

    /// <summary>Gets the set of marketplace items.</summary>
    DbSet<Item> Items { get; }

    /// <summary>Gets the set of seller-curated item bundles.</summary>
    DbSet<Bundle> Bundles { get; }

    /// <summary>Gets the set of item-to-bundle membership records.</summary>
    DbSet<BundleItem> BundleItems { get; }

    /// <summary>Gets the set of photos attached to marketplace items.</summary>
    DbSet<ItemPhoto> ItemPhotos { get; }

    /// <summary>Gets the set of item categories.</summary>
    DbSet<Category> Categories { get; }

    /// <summary>Gets the set of item likes recorded by users.</summary>
    DbSet<Like> Likes { get; }

    /// <summary>Gets the set of direct messages exchanged between users.</summary>
    DbSet<Message> Messages { get; }

    /// <summary>Gets the set of payment records.</summary>
    DbSet<Payment> Payments { get; }

    /// <summary>Gets the set of refresh tokens issued to authenticated users.</summary>
    DbSet<RefreshToken> RefreshTokens { get; }

    /// <summary>Gets the set of user-submitted feedback entries.</summary>
    DbSet<Feedback> Feedbacks { get; }

    /// <summary>Gets the set of shipment records associated with completed payments.</summary>
    DbSet<Shipment> Shipments { get; }

    /// <summary>Gets the set of purchase requests initiated by buyers.</summary>
    DbSet<PurchaseRequest> PurchaseRequests { get; }

    /// <summary>Gets the set of doctor reviews submitted by users.</summary>
    DbSet<DoctorReview> DoctorReviews { get; }

    /// <summary>Gets the set of child-friendly place submissions.</summary>
    DbSet<ChildFriendlyPlace> ChildFriendlyPlaces { get; }

    /// <summary>Gets the set of user ratings given after completed transactions.</summary>
    DbSet<UserRating> UserRatings { get; }

    /// <summary>Gets the set of application-wide key/value settings.</summary>
    DbSet<AppSetting> AppSettings { get; }

    /// <summary>Gets the set of admin moderation action logs for items.</summary>
    DbSet<ItemModerationLog> ItemModerationLogs { get; }

    /// <summary>Gets the append-only graded moderation action log for user accounts.</summary>
    DbSet<UserModerationLog> UserModerationLogs { get; }

    /// <summary>Gets the user-submitted abuse reports queue.</summary>
    DbSet<AbuseReport> AbuseReports { get; }

    /// <summary>Gets the auto-detected abuse signals queue (heuristics flagging accounts for admin review).</summary>
    DbSet<AbuseSignal> AbuseSignals { get; }

    /// <summary>Gets user-submitted appeals against prior moderation actions.</summary>
    DbSet<ModerationAppeal> ModerationAppeals { get; }

    /// <summary>Gets the append-only security audit log.</summary>
    DbSet<AuditLog> AuditLogs { get; }

    /// <summary>Gets the set of price offers made by buyers on sell listings.</summary>
    DbSet<Offer> Offers { get; }

    /// <summary>Gets the set of seller follow relationships.</summary>
    DbSet<Follow> Follows { get; }

    /// <summary>Gets the set of saved search alert subscriptions.</summary>
    DbSet<SavedSearch> SavedSearches { get; }

    /// <summary>Gets the set of transactional outbox messages pending external delivery.</summary>
    DbSet<OutboxMessage> OutboxMessages { get; }

    /// <summary>Gets the set of RAG knowledge base articles for the AI assistant.</summary>
    DbSet<KnowledgeArticle> KnowledgeArticles { get; }

    /// <summary>Gets the set of coach / agency business profiles registered for the business vertical.</summary>
    DbSet<BusinessProfile> BusinessProfiles { get; }

    /// <summary>Gets the set of single-per-profile business listings advertising children's activities.</summary>
    DbSet<BusinessListing> BusinessListings { get; }

    /// <summary>Gets the set of photos attached to business listings.</summary>
    DbSet<BusinessListingPhoto> BusinessListingPhotos { get; }

    /// <summary>Gets the set of parent likes on business listings.</summary>
    DbSet<BusinessListingLike> BusinessListingLikes { get; }

    /// <summary>Gets the set of parent comments on business listings.</summary>
    DbSet<BusinessListingComment> BusinessListingComments { get; }

    /// <summary>Gets the append-only view-event fact table for business listings.</summary>
    DbSet<BusinessListingViewEvent> BusinessListingViewEvents { get; }

    /// <summary>Gets the daily rollup of business-listing engagement counters.</summary>
    DbSet<BusinessListingDailyStat> BusinessListingDailyStats { get; }

    /// <summary>Gets the config table of available business subscription tiers.</summary>
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }

    /// <summary>Gets the set of business subscriptions mirrored from Stripe.</summary>
    DbSet<BusinessSubscription> BusinessSubscriptions { get; }

    /// <summary>Gets the append-only ledger of Stripe webhook events applied to business subscriptions.</summary>
    DbSet<BusinessSubscriptionEvent> BusinessSubscriptionEvents { get; }

    /// <summary>Gets the set of published business-vertical policy versions.</summary>
    DbSet<BusinessPolicyVersion> BusinessPolicyVersions { get; }

    /// <summary>Gets the immutable evidence rows of business-profile policy acceptances.</summary>
    DbSet<BusinessPolicyAcceptance> BusinessPolicyAcceptances { get; }

    /// <summary>Gets the device-fingerprint hash registry used for anti-abuse duplicate detection.</summary>
    DbSet<DeviceFingerprint> DeviceFingerprints { get; }

    /// <summary>Gets the join rows linking device fingerprints to users.</summary>
    DbSet<DeviceFingerprintUser> DeviceFingerprintUsers { get; }

    /// <summary>Gets the set of promoter profiles with their unique referral codes.</summary>
    DbSet<PromoterProfile> PromoterProfiles { get; }

    /// <summary>Gets the set of coach referrals submitted via the public recommend form or a promoter link.</summary>
    DbSet<CoachReferral> CoachReferrals { get; }

    /// <summary>Gets the underlying <see cref="DatabaseFacade"/> for executing raw SQL and managing transactions.</summary>
    DatabaseFacade Database { get; }

    /// <summary>
    /// Persists all pending changes to the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
