namespace MomVibe.Application.DTOs.Moderation;

using MomVibe.Domain.Enums;

public sealed record SubmitAppealRequest(Guid ModerationLogId, string Statement);

public sealed record AppealDto(
    Guid Id,
    string UserId,
    Guid ModerationLogId,
    string UserStatement,
    AppealStatus Status,
    string? AdminId,
    string? AdminDecisionNote,
    DateTime? DecidedAt,
    DateTime CreatedAt);

public sealed record AppealSummaryDto(
    Guid Id,
    string UserId,
    Guid ModerationLogId,
    AppealStatus Status,
    DateTime CreatedAt);

public sealed record AdminAppealFilter(AppealStatus? Status, int Page, int PageSize);

public sealed record DecideAppealRequest(AppealStatus Status, string? DecisionNote);
