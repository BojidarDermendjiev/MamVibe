namespace MomVibe.Application.Interfaces;

using DTOs.Business;

/// <summary>
/// Manages the <c>PromoterProfile</c> owned by the calling user — generates the unique
/// referral code, assigns the <c>Promoter</c> role, and aggregates dashboard counters.
/// </summary>
public interface IPromoterService
{
    /// <summary>Returns the calling user's profile, or null when they have not registered yet.</summary>
    Task<PromoterProfileDto?> GetMineAsync(string userId);

    /// <summary>
    /// Creates the calling user's promoter profile, generating an 8-char Base32 referral code
    /// (e.g. <c>MAMA-AB12CD34</c>) and granting the <c>Promoter</c> role. Idempotent — returns
    /// the existing profile when one already exists.
    /// </summary>
    Task<PromoterProfileDto> CreateAsync(string userId);

    /// <summary>Returns the promoter dashboard payload — profile + recent referrals + counters.</summary>
    Task<PromoterDashboardDto> GetDashboardAsync(string userId);
}
