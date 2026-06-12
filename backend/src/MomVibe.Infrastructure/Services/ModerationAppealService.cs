namespace MomVibe.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;

using Application.Interfaces;
using Application.DTOs.Moderation;
using Domain.Entities;
using Domain.Enums;

/// <summary>
/// Lifecycle for user-submitted appeals against prior moderation actions. Enforces ownership,
/// one-open-appeal-per-event, and chains an automatic clear when an appeal is approved.
/// </summary>
public class ModerationAppealService : IModerationAppealService
{
    private readonly IApplicationDbContext _context;
    private readonly IUserModerationService _moderation;
    private readonly IAuditLogService _audit;

    public ModerationAppealService(
        IApplicationDbContext context,
        IUserModerationService moderation,
        IAuditLogService audit)
    {
        this._context = context;
        this._moderation = moderation;
        this._audit = audit;
    }

    public async Task<Guid> SubmitAsync(string userId, SubmitAppealRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Statement) || request.Statement.Length < 20)
            throw new ArgumentException("Statement must be at least 20 characters.");
        if (request.Statement.Length > 3000)
            throw new ArgumentException("Statement must not exceed 3000 characters.");

        var log = await this._context.UserModerationLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == request.ModerationLogId);
        if (log is null) throw new KeyNotFoundException("Moderation event not found.");
        if (log.UserId != userId)
            throw new InvalidOperationException("You can only appeal your own moderation actions.");

        var hasOpen = await this._context.ModerationAppeals
            .AnyAsync(a => a.ModerationLogId == request.ModerationLogId
                        && (a.Status == AppealStatus.Pending || a.Status == AppealStatus.UnderReview));
        if (hasOpen) throw new InvalidOperationException("An appeal for this moderation action is already under review.");

        var appeal = new ModerationAppeal
        {
            UserId = userId,
            ModerationLogId = request.ModerationLogId,
            UserStatement = request.Statement.Trim(),
            Status = AppealStatus.Pending
        };
        this._context.ModerationAppeals.Add(appeal);
        await this._context.SaveChangesAsync(CancellationToken.None);
        await this._audit.LogAsync(userId, "UserMod.Appeal.Submit", success: true, targetId: request.ModerationLogId.ToString());
        return appeal.Id;
    }

    public async Task<IReadOnlyList<AppealDto>> GetMyAppealsAsync(string userId)
    {
        return await this._context.ModerationAppeals
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AppealDto(
                a.Id, a.UserId, a.ModerationLogId, a.UserStatement, a.Status,
                a.AdminId, a.AdminDecisionNote, a.DecidedAt, a.CreatedAt))
            .ToListAsync();
    }

    public async Task<PagedModerationResult<AppealSummaryDto>> GetAdminQueueAsync(AdminAppealFilter filter)
    {
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var query = this._context.ModerationAppeals.AsNoTracking();
        if (filter.Status.HasValue) query = query.Where(a => a.Status == filter.Status.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(a => a.Status == AppealStatus.Pending ? 0 : a.Status == AppealStatus.UnderReview ? 1 : 2)
            .ThenByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AppealSummaryDto(a.Id, a.UserId, a.ModerationLogId, a.Status, a.CreatedAt))
            .ToListAsync();

        return new PagedModerationResult<AppealSummaryDto>(items, total, page, pageSize);
    }

    public async Task<AppealDto?> GetAppealAsync(Guid id)
    {
        return await this._context.ModerationAppeals
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AppealDto(
                a.Id, a.UserId, a.ModerationLogId, a.UserStatement, a.Status,
                a.AdminId, a.AdminDecisionNote, a.DecidedAt, a.CreatedAt))
            .FirstOrDefaultAsync();
    }

    public async Task DecideAsync(Guid appealId, DecideAppealRequest request, string adminId, string adminDisplayName)
    {
        if (request.Status != AppealStatus.Approved && request.Status != AppealStatus.Rejected)
            throw new ArgumentException("Decide accepts only Approved or Rejected.");

        var appeal = await this._context.ModerationAppeals.FirstOrDefaultAsync(a => a.Id == appealId)
            ?? throw new KeyNotFoundException("Appeal not found.");

        appeal.Status = request.Status;
        appeal.AdminId = adminId;
        appeal.AdminDecisionNote = request.DecisionNote;
        appeal.DecidedAt = DateTime.UtcNow;

        await this._context.SaveChangesAsync(CancellationToken.None);

        // If approved, clear the user's active moderation as a follow-up workflow.
        if (request.Status == AppealStatus.Approved)
        {
            await this._moderation.ManualClearAsync(
                appeal.UserId,
                adminId,
                adminDisplayName,
                $"Appeal approved: {request.DecisionNote ?? string.Empty}");
        }

        await this._audit.LogAsync(adminId, $"Admin.Appeal.{request.Status}", success: true,
            targetId: appeal.UserId, details: $"AppealId={appeal.Id}");
    }
}
