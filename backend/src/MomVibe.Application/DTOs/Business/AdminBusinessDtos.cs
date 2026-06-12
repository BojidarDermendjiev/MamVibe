namespace MomVibe.Application.DTOs.Business;

using Domain.Enums;

/// <summary>Compact admin row for a <c>BusinessProfile</c>.</summary>
public class BusinessProfileAdminDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public BusinessCategory Category { get; set; }
    public ProfileKind ProfileKind { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public BusinessProfileStatus Status { get; set; }
    public string? SubscriptionPlanCode { get; set; }
    public BusinessSubscriptionStatus? SubscriptionStatus { get; set; }
    public bool HasListing { get; set; }
    public bool IsListingApproved { get; set; }
    public bool HasDeviceConflict { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Compact admin row for a <c>BusinessListing</c> (moderation queue).</summary>
public class BusinessListingAdminDto
{
    public Guid Id { get; set; }
    public Guid BusinessProfileId { get; set; }
    public string BusinessDisplayName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public ActivityType ActivityType { get; set; }
    public BusinessCategory Category { get; set; }
    public string City { get; set; } = string.Empty;
    public string? CoverPhotoUrl { get; set; }
    public bool IsActive { get; set; }
    public bool IsApproved { get; set; }
    public int RankBoost { get; set; }
    public long ViewCount { get; set; }
    public long LikeCount { get; set; }
    public long CommentCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Revenue KPI snapshot for the admin dashboard.</summary>
public class BusinessRevenueDto
{
    public decimal MonthlyRecurringRevenueEur { get; set; }
    public int ActiveSubscriptionCount { get; set; }
    public int TrialingSubscriptionCount { get; set; }
    public int PastDueSubscriptionCount { get; set; }
    public int CanceledLast30Days { get; set; }
    public List<TierBreakdownDto> ByTier { get; set; } = [];
    public decimal TrialToPaidConversionRate { get; set; }
    public int TotalListings { get; set; }
    public int ApprovedListings { get; set; }
    public int PendingApprovalListings { get; set; }
}

public class TierBreakdownDto
{
    public string PlanCode { get; set; } = string.Empty;
    public int ActiveCount { get; set; }
    public decimal MonthlyContributionEur { get; set; }
}

public class AdminProfileFilter
{
    public BusinessCategory? Category { get; set; }
    public BusinessProfileStatus? Status { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public class AdminListingFilter
{
    public BusinessCategory? Category { get; set; }
    public bool? IsApproved { get; set; }
    public bool? IsActive { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public class PagedAdminProfilesResult
{
    public IEnumerable<BusinessProfileAdminDto> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}

public class PagedAdminListingsResult
{
    public IEnumerable<BusinessListingAdminDto> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}

public class AdminSuspendProfileRequest
{
    public string? Reason { get; set; }
}
