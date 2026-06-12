namespace MomVibe.Application.DTOs.Moderation;

using MomVibe.Domain.Enums;

/// <summary>
/// User-facing snapshot of their own moderation state, returned by
/// <c>GET /api/v1/me/moderation-status</c> and used to render the suspension banner.
/// </summary>
public sealed record UserModerationStatusDto(
    UserModerationLevel Level,
    ModerationReason Reason,
    string? PublicReason,
    DateTime? StartedAt,
    DateTime? ExpiresAt,
    Guid? ActiveModerationLogId,
    bool CanAppeal);

/// <summary>
/// Admin-facing detail view of a single user's moderation state, history, and recent reports/signals.
/// </summary>
public sealed record UserModerationDetailDto(
    string UserId,
    string Email,
    string DisplayName,
    UserModerationStatusDto Current,
    IReadOnlyList<UserModerationLogDto> History,
    int OpenReportCount,
    int UnacknowledgedSignalCount,
    int TotalScore);

/// <summary>
/// Single row in the per-user moderation history.
/// </summary>
public sealed record UserModerationLogDto(
    Guid Id,
    string AdminId,
    string AdminDisplayName,
    UserModerationLevel PreviousLevel,
    UserModerationLevel NewLevel,
    ModerationReason Reason,
    string PublicReason,
    string? InternalNote,
    DateTime? ExpiresAt,
    Guid? RelatedReportId,
    Guid? RelatedAppealId,
    DateTime CreatedAt);

/// <summary>
/// Admin input describing a new moderation action to apply. Used by
/// <c>POST /api/v1/admin/users/{id}/moderate</c>.
/// </summary>
public sealed record ModerationActionRequest(
    UserModerationLevel NewLevel,
    ModerationReason Reason,
    string PublicReason,
    string? InternalNote,
    int? DurationMinutes,
    Guid? RelatedReportId,
    Guid? RelatedAppealId);

/// <summary>
/// Admin input describing a manual clear (revert to <see cref="UserModerationLevel.None"/>).
/// </summary>
public sealed record ModerationClearRequest(string Reason);
