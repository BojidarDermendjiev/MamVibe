namespace MomVibe.Infrastructure.Services.Business;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Application.Interfaces;
using Application.DTOs.Business;
using Domain.Entities;
using Domain.Enums;

/// <summary>
/// EF Core-backed admin queries + mutations for the business vertical. All mutations
/// emit an audit log entry — keep these in sync with <c>AdminLayout</c> nav labels.
/// </summary>
public class BusinessAdminService : IBusinessAdminService
{
    private readonly IApplicationDbContext _db;
    private readonly IAuditLogService _audit;
    private readonly ILogger<BusinessAdminService> _logger;

    public BusinessAdminService(
        IApplicationDbContext db,
        IAuditLogService audit,
        ILogger<BusinessAdminService> logger)
    {
        _db = db;
        _audit = audit;
        _logger = logger;
    }

    public async Task<PagedAdminProfilesResult> ListProfilesAsync(AdminProfileFilter filter)
    {
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        // Multi-account-same-device flag: any fingerprint hash with more than one linked user.
        var conflictHashes = await _db.DeviceFingerprintUsers
            .AsNoTracking()
            .GroupBy(d => d.FingerprintHash)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToListAsync();
        var conflictSet = conflictHashes.ToHashSet();

        var query = _db.BusinessProfiles
            .AsNoTracking()
            .Where(p => p.Status != BusinessProfileStatus.Removed);
        if (filter.Category.HasValue) query = query.Where(p => p.Category == filter.Category.Value);
        if (filter.Status.HasValue) query = query.Where(p => p.Status == filter.Status.Value);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(p =>
                p.DisplayName.ToLower().Contains(s) ||
                p.LegalName.ToLower().Contains(s) ||
                p.City.ToLower().Contains(s) ||
                p.ContactEmail.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync();

        var rows = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Join(_db.Users.AsNoTracking(),
                  p => p.UserId,
                  u => u.Id,
                  (p, u) => new { p, OwnerEmail = u.Email ?? string.Empty })
            .GroupJoin(_db.BusinessListings.AsNoTracking(),
                       x => x.p.Id,
                       l => l.BusinessProfileId,
                       (x, ls) => new { x.p, x.OwnerEmail, listing = ls.FirstOrDefault() })
            .GroupJoin(_db.BusinessSubscriptions.AsNoTracking(),
                       x => x.p.Id,
                       s => s.BusinessProfileId,
                       (x, ss) => new { x.p, x.OwnerEmail, x.listing, sub = ss.FirstOrDefault() })
            .ToListAsync();

        var items = rows.Select(r => new BusinessProfileAdminDto
        {
            Id = r.p.Id,
            UserId = r.p.UserId,
            OwnerEmail = r.OwnerEmail,
            Category = r.p.Category,
            ProfileKind = r.p.ProfileKind,
            DisplayName = r.p.DisplayName,
            LegalName = r.p.LegalName,
            City = r.p.City,
            Status = r.p.Status,
            SubscriptionPlanCode = r.sub?.PlanCode,
            SubscriptionStatus = r.sub?.Status,
            HasListing = r.listing != null,
            IsListingApproved = r.listing?.IsApproved ?? false,
            HasDeviceConflict = conflictSet.Contains(r.p.DeviceFingerprintHash)
                                && r.p.DeviceCheckBypassedByAdminId == null,
            CreatedAt = r.p.CreatedAt,
        }).ToList();

        return new PagedAdminProfilesResult
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }

    public async Task<PagedAdminListingsResult> ListListingsAsync(AdminListingFilter filter)
    {
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var query = _db.BusinessListings.AsNoTracking().AsQueryable();
        if (filter.Category.HasValue)
            query = query.Where(l => l.BusinessProfile.Category == filter.Category.Value);
        if (filter.IsApproved.HasValue) query = query.Where(l => l.IsApproved == filter.IsApproved.Value);
        if (filter.IsActive.HasValue) query = query.Where(l => l.IsActive == filter.IsActive.Value);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(l =>
                l.Title.ToLower().Contains(s) ||
                l.City.ToLower().Contains(s) ||
                l.BusinessProfile.DisplayName.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            // Pending approval first, then by recency.
            .OrderBy(l => l.IsApproved ? 1 : 0)
            .ThenByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new BusinessListingAdminDto
            {
                Id = l.Id,
                BusinessProfileId = l.BusinessProfileId,
                BusinessDisplayName = l.BusinessProfile.DisplayName,
                OwnerEmail = _db.Users.Where(u => u.Id == l.BusinessProfile.UserId).Select(u => u.Email ?? "").FirstOrDefault() ?? "",
                Title = l.Title,
                ActivityType = l.ActivityType,
                Category = l.BusinessProfile.Category,
                City = l.City,
                CoverPhotoUrl = l.Photos.OrderBy(p => p.DisplayOrder).Select(p => p.Url).FirstOrDefault(),
                IsActive = l.IsActive,
                IsApproved = l.IsApproved,
                RankBoost = l.RankBoost,
                ViewCount = l.ViewCount,
                LikeCount = l.LikeCount,
                CommentCount = l.CommentCount,
                CreatedAt = l.CreatedAt,
            })
            .ToListAsync();

        return new PagedAdminListingsResult
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }

    public async Task SuspendProfileAsync(Guid profileId, string adminId, string? reason)
    {
        var profile = await _db.BusinessProfiles.FirstOrDefaultAsync(p => p.Id == profileId)
            ?? throw new KeyNotFoundException("Business profile not found.");
        profile.Status = BusinessProfileStatus.Suspended;
        var listing = await _db.BusinessListings.FirstOrDefaultAsync(l => l.BusinessProfileId == profileId);
        if (listing != null) listing.IsActive = false;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(adminId, "Admin.Business.Profile.Suspended", success: true,
            targetId: profileId.ToString(), details: reason);
    }

    public async Task RestoreProfileAsync(Guid profileId, string adminId)
    {
        var profile = await _db.BusinessProfiles.FirstOrDefaultAsync(p => p.Id == profileId)
            ?? throw new KeyNotFoundException("Business profile not found.");
        profile.Status = BusinessProfileStatus.Active;
        var listing = await _db.BusinessListings.FirstOrDefaultAsync(l => l.BusinessProfileId == profileId);
        if (listing != null) listing.IsActive = true;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(adminId, "Admin.Business.Profile.Restored", success: true,
            targetId: profileId.ToString());
    }

    public async Task RemoveProfileAsync(Guid profileId, string adminId, string? reason)
    {
        var profile = await _db.BusinessProfiles.FirstOrDefaultAsync(p => p.Id == profileId)
            ?? throw new KeyNotFoundException("Business profile not found.");
        profile.Status = BusinessProfileStatus.Removed;
        var listing = await _db.BusinessListings.FirstOrDefaultAsync(l => l.BusinessProfileId == profileId);
        if (listing != null) listing.IsActive = false;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(adminId, "Admin.Business.Profile.Removed", success: true,
            targetId: profileId.ToString(), details: reason);
    }

    public async Task ApproveListingAsync(Guid listingId, string adminId)
    {
        var listing = await _db.BusinessListings.FirstOrDefaultAsync(l => l.Id == listingId)
            ?? throw new KeyNotFoundException("Listing not found.");
        listing.IsApproved = true;
        listing.IsActive = true;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(adminId, "Admin.Business.Listing.Approved", success: true,
            targetId: listingId.ToString());
    }

    public async Task UnapproveListingAsync(Guid listingId, string adminId, string? reason)
    {
        var listing = await _db.BusinessListings.FirstOrDefaultAsync(l => l.Id == listingId)
            ?? throw new KeyNotFoundException("Listing not found.");
        listing.IsApproved = false;
        listing.IsActive = false;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(adminId, "Admin.Business.Listing.Unapproved", success: true,
            targetId: listingId.ToString(), details: reason);
    }

    public async Task<BusinessRevenueDto> GetRevenueAsync()
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        var allSubs = await _db.BusinessSubscriptions
            .AsNoTracking()
            .ToListAsync();
        var plans = await _db.SubscriptionPlans
            .AsNoTracking()
            .ToDictionaryAsync(p => p.Code, p => p);

        var activeSubs = allSubs.Where(s => s.Status == BusinessSubscriptionStatus.Active).ToList();
        var trialingSubs = allSubs.Where(s => s.Status == BusinessSubscriptionStatus.Trialing).ToList();
        var pastDueSubs = allSubs.Where(s => s.Status == BusinessSubscriptionStatus.PastDue).ToList();
        var canceledRecent = allSubs.Count(s =>
            s.Status == BusinessSubscriptionStatus.Canceled &&
            s.CanceledAt.HasValue && s.CanceledAt.Value >= thirtyDaysAgo);

        var byTier = activeSubs
            .GroupBy(s => s.PlanCode)
            .Select(g => new TierBreakdownDto
            {
                PlanCode = g.Key,
                ActiveCount = g.Count(),
                MonthlyContributionEur = plans.TryGetValue(g.Key, out var plan)
                    ? plan.MonthlyPriceEur * g.Count()
                    : 0m,
            })
            .OrderByDescending(t => t.MonthlyContributionEur)
            .ToList();

        var mrr = byTier.Sum(t => t.MonthlyContributionEur);

        // Trial-to-paid conversion: subscriptions whose first event ever was Trialing AND whose
        // current status is Active or PastDue. Approximation against the events ledger.
        var trialStarted = await _db.BusinessSubscriptionEvents
            .AsNoTracking()
            .Where(e => e.Type == BusinessSubscriptionEventType.SubscriptionCreated)
            .Select(e => e.SubscriptionId)
            .Distinct()
            .CountAsync();
        var trialConvertedToPaid = allSubs.Count(s =>
            s.Status == BusinessSubscriptionStatus.Active ||
            s.Status == BusinessSubscriptionStatus.PastDue);
        var conversionRate = trialStarted == 0 ? 0m : (decimal)trialConvertedToPaid / trialStarted;

        var totalListings = await _db.BusinessListings.CountAsync();
        var approvedListings = await _db.BusinessListings.CountAsync(l => l.IsApproved);

        return new BusinessRevenueDto
        {
            MonthlyRecurringRevenueEur = mrr,
            ActiveSubscriptionCount = activeSubs.Count,
            TrialingSubscriptionCount = trialingSubs.Count,
            PastDueSubscriptionCount = pastDueSubs.Count,
            CanceledLast30Days = canceledRecent,
            ByTier = byTier,
            TrialToPaidConversionRate = conversionRate,
            TotalListings = totalListings,
            ApprovedListings = approvedListings,
            PendingApprovalListings = totalListings - approvedListings,
        };
    }
}
