namespace MomVibe.Application.DTOs.Business;

using Domain.Enums;

/// <summary>Read projection of a <c>PromoterProfile</c>.</summary>
public class PromoterProfileDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string ReferralCode { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int TotalReferrals { get; set; }
    public int TotalActivations { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Recent referral row shown on the promoter dashboard.</summary>
public class RecentReferralDto
{
    public Guid Id { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public ActivityType ActivityType { get; set; }
    public CoachReferralStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Aggregated dashboard payload for the promoter home page.</summary>
public class PromoterDashboardDto
{
    public PromoterProfileDto Profile { get; set; } = new();
    public int TotalSubmitted { get; set; }
    public int TotalContacted { get; set; }
    public int TotalOnboarded { get; set; }
    public int TotalRejected { get; set; }
    public List<RecentReferralDto> Recent { get; set; } = [];
}

/// <summary>Public referral payload submitted via <c>/coaches/recommend</c>.</summary>
public class SubmitCoachReferralRequest
{
    public string BusinessName { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public ActivityType ActivityType { get; set; }
    public string City { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? ReferralCode { get; set; }
    public string? TurnstileToken { get; set; }
}

/// <summary>Admin-facing read DTO with full audit fields.</summary>
public class CoachReferralDto
{
    public Guid Id { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public ActivityType ActivityType { get; set; }
    public string City { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? ReferrerUserId { get; set; }
    public string? ReferrerDisplayName { get; set; }
    public string? ReferralCode { get; set; }
    public CoachReferralStatus Status { get; set; }
    public string? AdminNote { get; set; }
    public string? ActionedByAdminId { get; set; }
    public DateTime? ActionedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Admin payload to transition a referral's status.</summary>
public class UpdateCoachReferralStatusRequest
{
    public CoachReferralStatus Status { get; set; }
    public string? AdminNote { get; set; }
}

public class PagedReferralsResult
{
    public IEnumerable<CoachReferralDto> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}
