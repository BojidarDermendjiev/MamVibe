namespace MomVibe.Application.DTOs.Moderation;

using MomVibe.Domain.Enums;

/// <summary>User-submitted report payload — bound from the request body on <c>POST /api/v1/reports</c>.</summary>
public sealed record SubmitReportRequest(
    ReportTargetType TargetType,
    string TargetId,
    ModerationReason Reason,
    string Description);

/// <summary>Detailed view of a single report (admin).</summary>
public sealed record AbuseReportDto(
    Guid Id,
    string ReporterId,
    ReportTargetType TargetType,
    string TargetId,
    string TargetUserId,
    ModerationReason Reason,
    string Description,
    ReportStatus Status,
    string? ResolvedByAdminId,
    DateTime? ResolvedAt,
    string? ResolutionNote,
    Guid? ResultingModerationLogId,
    DateTime CreatedAt);

/// <summary>Compact view for admin queue lists.</summary>
public sealed record AbuseReportSummaryDto(
    Guid Id,
    string ReporterId,
    ReportTargetType TargetType,
    string TargetId,
    string TargetUserId,
    ModerationReason Reason,
    ReportStatus Status,
    DateTime CreatedAt);

/// <summary>Filter parameters for <c>GET /api/v1/admin/reports</c>.</summary>
public sealed record AdminReportFilter(
    ReportStatus? Status,
    ReportTargetType? TargetType,
    ModerationReason? Reason,
    int Page,
    int PageSize);

/// <summary>Resolve action body for <c>POST /api/v1/admin/reports/{id}/resolve</c>.</summary>
public sealed record ResolveReportRequest(
    ReportStatus Status,
    string? ResolutionNote,
    ModerationActionRequest? ModerationAction);

/// <summary>Generic paged result wrapper for moderation admin queues.</summary>
public sealed record PagedModerationResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize);
