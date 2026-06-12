namespace MomVibe.Application.Interfaces;

using DTOs.Business;

/// <summary>
/// Admin-only operations for the business vertical: paged profile + listing queues,
/// approve / suspend / remove mutations, and the revenue KPI snapshot. All mutations
/// write an audit log entry keyed to the actioning admin id.
/// </summary>
public interface IBusinessAdminService
{
    Task<PagedAdminProfilesResult> ListProfilesAsync(AdminProfileFilter filter);
    Task<PagedAdminListingsResult> ListListingsAsync(AdminListingFilter filter);

    /// <summary>Suspends the profile (hides listing, blocks subscription actions); reversible via <see cref="RestoreProfileAsync"/>.</summary>
    Task SuspendProfileAsync(Guid profileId, string adminId, string? reason);

    /// <summary>Restores a Suspended profile back to Active.</summary>
    Task RestoreProfileAsync(Guid profileId, string adminId);

    /// <summary>Soft-removes the profile — <c>Status=Removed</c>. The row remains for audit; listing cascades.</summary>
    Task RemoveProfileAsync(Guid profileId, string adminId, string? reason);

    /// <summary>Marks a pending listing as approved (visible to parents).</summary>
    Task ApproveListingAsync(Guid listingId, string adminId);

    /// <summary>Reverts an approved listing back to pending review.</summary>
    Task UnapproveListingAsync(Guid listingId, string adminId, string? reason);

    /// <summary>Returns the revenue + subscription KPI snapshot for the admin overview.</summary>
    Task<BusinessRevenueDto> GetRevenueAsync();
}
