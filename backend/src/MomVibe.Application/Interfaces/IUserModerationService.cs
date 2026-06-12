namespace MomVibe.Application.Interfaces;

using MomVibe.Application.DTOs.Moderation;

/// <summary>
/// Authoritative service for applying and querying graded user moderation actions.
/// Single source of truth for transitions; mirrors the existing item-moderation pattern.
/// </summary>
/// <remarks>
/// Every state-changing call writes both a <c>UserModerationLog</c> row (entity history)
/// and an <c>AuditLog</c> row (cross-cutting audit). Cache eviction and refresh-token
/// revocation happen inside <c>ApplyActionAsync</c> so callers do not have to coordinate.
/// </remarks>
public interface IUserModerationService
{
    /// <summary>Returns the current moderation state for the specified user, intended for the user's own /me view.</summary>
    Task<UserModerationStatusDto> GetStatusAsync(string userId);

    /// <summary>Returns an admin view of a user including status, history, and report/signal counts.</summary>
    Task<UserModerationDetailDto?> GetUserModerationAsync(string userId);

    /// <summary>
    /// Applies a moderation action to the user. Writes <c>UserModerationLog</c> + <c>AuditLog</c>,
    /// enqueues the notification email + n8n webhook via the transactional outbox, evicts the
    /// distributed moderation cache, and revokes refresh tokens on escalations.
    /// </summary>
    /// <returns>The new <c>UserModerationLog</c> row id.</returns>
    Task<Guid> ApplyActionAsync(string userId, ModerationActionRequest request, string adminId, string adminDisplayName);

    /// <summary>
    /// Reverts a user to <see cref="MomVibe.Domain.Enums.UserModerationLevel.None"/>. Used both by manual
    /// admin clears and by the background <c>ModerationExpiryService</c> (which passes <c>"system"</c> as the admin id).
    /// </summary>
    Task ManualClearAsync(string userId, string adminId, string adminDisplayName, string reason);

    /// <summary>
    /// Hosted-service entry point: finds all users whose timed Restricted/Suspended has elapsed
    /// and reverts them to None. Optimistically concurrent on <c>ApplicationUser.UpdatedAt</c>.
    /// </summary>
    Task<int> ClearExpiredAsync(CancellationToken cancellationToken);
}
