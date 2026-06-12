namespace MomVibe.Application.Interfaces;

using MomVibe.Application.DTOs.Moderation;

/// <summary>
/// User-to-user reporting pipeline. Submits validation-aware reports, returns the admin
/// review queue, and resolves reports (optionally chaining a moderation action against
/// the target user in the same transaction).
/// </summary>
public interface IAbuseReportService
{
    /// <summary>Validates and persists a new abuse report. Returns the new id.</summary>
    Task<Guid> SubmitAsync(SubmitReportRequest request, string reporterId, string? ipAddress);

    /// <summary>Returns a paged slice of the admin review queue.</summary>
    Task<PagedModerationResult<AbuseReportSummaryDto>> GetAdminQueueAsync(AdminReportFilter filter);

    /// <summary>Returns the detail view for a single report.</summary>
    Task<AbuseReportDto?> GetReportAsync(Guid id);

    /// <summary>Resolves a report. When <c>ModerationAction</c> is provided, applies it to the target user in the same transaction.</summary>
    Task ResolveAsync(Guid reportId, ResolveReportRequest request, string adminId, string adminDisplayName);
}
