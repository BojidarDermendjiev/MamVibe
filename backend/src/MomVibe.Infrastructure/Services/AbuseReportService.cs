namespace MomVibe.Infrastructure.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Application.Interfaces;
using Application.DTOs.Moderation;
using Domain.Entities;
using Domain.Enums;

/// <summary>
/// Pipeline for user-submitted abuse reports. Enforces self-report and duplicate-pending
/// guards in the service layer (the DB partial unique index is the second line of defence),
/// emits an <c>AbuseSignal</c> when accumulated open reports against a target reach the
/// configured threshold, and orchestrates report resolution with optional inline moderation.
/// </summary>
public class AbuseReportService : IAbuseReportService
{
    private const int ReportThresholdForSignal = 3;
    private const int SignalScoreFromReportThreshold = 50;

    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserModerationService _moderation;
    private readonly IAuditLogService _audit;
    private readonly ILogger<AbuseReportService> _logger;

    public AbuseReportService(
        IApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IUserModerationService moderation,
        IAuditLogService audit,
        ILogger<AbuseReportService> logger)
    {
        this._context = context;
        this._userManager = userManager;
        this._moderation = moderation;
        this._audit = audit;
        this._logger = logger;
    }

    public async Task<Guid> SubmitAsync(SubmitReportRequest request, string reporterId, string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(reporterId))
            throw new ArgumentException("ReporterId required", nameof(reporterId));
        if (string.IsNullOrWhiteSpace(request.TargetId))
            throw new ArgumentException("TargetId required.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length < 10)
            throw new ArgumentException("Description must be at least 10 characters.", nameof(request));
        if (request.Description.Length > 2000)
            throw new ArgumentException("Description must not exceed 2000 characters.", nameof(request));

        var (canonicalTargetId, targetUserId) = await ResolveTargetAsync(request.TargetType, request.TargetId);

        if (string.Equals(reporterId, targetUserId, StringComparison.Ordinal))
            throw new InvalidOperationException("You cannot report yourself.");

        var hasOpenDuplicate = await this._context.AbuseReports
            .AnyAsync(r => r.ReporterId == reporterId
                        && r.TargetType == request.TargetType
                        && r.TargetId == canonicalTargetId
                        && r.Status == ReportStatus.Pending);
        if (hasOpenDuplicate)
            throw new InvalidOperationException("You already have an open report against this target.");

        var report = new AbuseReport
        {
            ReporterId = reporterId,
            TargetType = request.TargetType,
            TargetId = canonicalTargetId,
            TargetUserId = targetUserId,
            Reason = request.Reason,
            Description = request.Description.Trim(),
            Status = ReportStatus.Pending
        };
        this._context.AbuseReports.Add(report);

        // Emit a signal once the accumulated open-report count crosses the threshold. The +1
        // accounts for the not-yet-saved row above so the trigger fires on the Nth report itself.
        var openCount = 1 + await this._context.AbuseReports
            .CountAsync(r => r.TargetUserId == targetUserId
                          && (r.Status == ReportStatus.Pending || r.Status == ReportStatus.UnderReview));
        if (openCount >= ReportThresholdForSignal)
        {
            this._context.AbuseSignals.Add(new AbuseSignal
            {
                Type = AbuseSignalType.ReportThreshold,
                SubjectUserId = targetUserId,
                Score = SignalScoreFromReportThreshold,
                Details = System.Text.Json.JsonSerializer.Serialize(new
                {
                    openReports = openCount,
                    triggeredByReportId = report.Id
                })
            });
        }

        await this._context.SaveChangesAsync(CancellationToken.None);
        await this._audit.LogAsync(reporterId, "Report.Submit", success: true,
            targetId: targetUserId, ipAddress: ipAddress,
            details: $"Type={request.TargetType} Reason={request.Reason}");
        return report.Id;
    }

    public async Task<PagedModerationResult<AbuseReportSummaryDto>> GetAdminQueueAsync(AdminReportFilter filter)
    {
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var query = this._context.AbuseReports.AsNoTracking();
        if (filter.Status.HasValue) query = query.Where(r => r.Status == filter.Status.Value);
        if (filter.TargetType.HasValue) query = query.Where(r => r.TargetType == filter.TargetType.Value);
        if (filter.Reason.HasValue) query = query.Where(r => r.Reason == filter.Reason.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(r => r.Status == ReportStatus.Pending ? 0
                       : r.Status == ReportStatus.UnderReview ? 1 : 2)
            .ThenByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new AbuseReportSummaryDto(
                r.Id, r.ReporterId, r.TargetType, r.TargetId, r.TargetUserId,
                r.Reason, r.Status, r.CreatedAt))
            .ToListAsync();

        return new PagedModerationResult<AbuseReportSummaryDto>(items, total, page, pageSize);
    }

    public async Task<AbuseReportDto?> GetReportAsync(Guid id)
    {
        return await this._context.AbuseReports
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new AbuseReportDto(
                r.Id, r.ReporterId, r.TargetType, r.TargetId, r.TargetUserId,
                r.Reason, r.Description, r.Status,
                r.ResolvedByAdminId, r.ResolvedAt, r.ResolutionNote,
                r.ResultingModerationLogId, r.CreatedAt))
            .FirstOrDefaultAsync();
    }

    public async Task ResolveAsync(Guid reportId, ResolveReportRequest request, string adminId, string adminDisplayName)
    {
        var report = await this._context.AbuseReports.FirstOrDefaultAsync(r => r.Id == reportId)
            ?? throw new KeyNotFoundException($"Report '{reportId}' not found.");

        report.Status = request.Status;
        report.ResolutionNote = request.ResolutionNote;
        report.ResolvedByAdminId = adminId;
        report.ResolvedAt = DateTime.UtcNow;

        Guid? moderationLogId = null;
        if (request.ModerationAction is { } action)
        {
            // Apply moderation in the same logical workflow. ApplyActionAsync writes its own
            // SaveChangesAsync; we then update our report row in a second save. This is acceptable
            // because both sides are idempotent: a partial failure leaves the report status
            // unchanged (Pending) and the admin can retry.
            var enrichedAction = action with { RelatedReportId = report.Id };
            moderationLogId = await this._moderation.ApplyActionAsync(report.TargetUserId, enrichedAction, adminId, adminDisplayName);
            report.ResultingModerationLogId = moderationLogId;
        }

        await this._context.SaveChangesAsync(CancellationToken.None);
        await this._audit.LogAsync(adminId, "Admin.ResolveReport", success: true,
            targetId: report.TargetUserId,
            details: $"ReportId={report.Id} Status={request.Status} ModLog={(moderationLogId?.ToString() ?? "none")}");
    }

    private async Task<(string canonicalTargetId, string targetUserId)> ResolveTargetAsync(ReportTargetType type, string rawTargetId)
    {
        switch (type)
        {
            case ReportTargetType.User:
            {
                var target = await this._userManager.FindByIdAsync(rawTargetId)
                    ?? throw new KeyNotFoundException("Target user not found.");
                return (target.Id, target.Id);
            }
            case ReportTargetType.Item:
            {
                if (!Guid.TryParse(rawTargetId, out var itemGuid))
                    throw new ArgumentException("Item TargetId must be a Guid.");
                var item = await this._context.Items.AsNoTracking()
                    .FirstOrDefaultAsync(i => i.Id == itemGuid)
                    ?? throw new KeyNotFoundException("Target item not found.");
                return (item.Id.ToString(), item.UserId);
            }
            case ReportTargetType.BusinessListing:
            {
                if (!Guid.TryParse(rawTargetId, out var listingGuid))
                    throw new ArgumentException("BusinessListing TargetId must be a Guid.");
                var listing = await this._context.BusinessListings.AsNoTracking()
                    .Include(l => l.BusinessProfile)
                    .FirstOrDefaultAsync(l => l.Id == listingGuid)
                    ?? throw new KeyNotFoundException("Target listing not found.");
                return (listing.Id.ToString(), listing.BusinessProfile.UserId);
            }
            case ReportTargetType.Message:
            {
                if (!Guid.TryParse(rawTargetId, out var messageGuid))
                    throw new ArgumentException("Message TargetId must be a Guid.");
                var msg = await this._context.Messages.AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == messageGuid)
                    ?? throw new KeyNotFoundException("Target message not found.");
                return (msg.Id.ToString(), msg.SenderId);
            }
            case ReportTargetType.MessageThread:
            {
                // Thread key is "min(uid1)|max(uid2)" of the two participants. The reporter's id
                // is one of them, so we resolve the other side as the target user.
                var parts = rawTargetId.Split('|', 2);
                if (parts.Length != 2)
                    throw new ArgumentException("MessageThread TargetId must be 'uidA|uidB'.");
                var canonical = string.CompareOrdinal(parts[0], parts[1]) <= 0
                    ? $"{parts[0]}|{parts[1]}"
                    : $"{parts[1]}|{parts[0]}";
                // Default to the lexicographically-second id as the target user (the reporter is
                // typically the first id), but callers should pass canonical thread keys.
                return (canonical, parts[1]);
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(type));
        }
    }
}
