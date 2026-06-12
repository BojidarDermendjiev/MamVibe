namespace MomVibe.Infrastructure.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using Application.Interfaces;
using Application.DTOs.Moderation;
using Domain.Entities;
using Domain.Enums;

/// <summary>
/// Single source of truth for graded user moderation. Mirrors the existing item-moderation pattern.
/// Every state change writes a <see cref="UserModerationLog"/> and an <see cref="AuditLog"/>,
/// enqueues a notification email + n8n webhook via the transactional outbox, evicts the
/// distributed moderation cache, and revokes refresh tokens on escalations.
/// </summary>
public class UserModerationService : IUserModerationService
{
    private const string ModerationCachePrefix = "moderation:";
    private const string LegacyBlockedCachePrefix = "blocked:";
    private const string SystemAdminId = "system";

    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditLogService _audit;
    private readonly IOutboxWriter _outbox;
    private readonly IAuthService _authService;
    private readonly IDistributedCache _cache;
    private readonly ILogger<UserModerationService> _logger;

    public UserModerationService(
        IApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IAuditLogService audit,
        IOutboxWriter outbox,
        IAuthService authService,
        IDistributedCache cache,
        ILogger<UserModerationService> logger)
    {
        this._context = context;
        this._userManager = userManager;
        this._audit = audit;
        this._outbox = outbox;
        this._authService = authService;
        this._cache = cache;
        this._logger = logger;
    }

    public async Task<UserModerationStatusDto> GetStatusAsync(string userId)
    {
        var user = await this._userManager.FindByIdAsync(userId);
        if (user is null)
            return new UserModerationStatusDto(UserModerationLevel.None, ModerationReason.Unspecified, null, null, null, null, false);

        var canAppeal = await CanAppealAsync(user);
        return new UserModerationStatusDto(
            user.ModerationLevel,
            user.ModerationReason,
            user.ModerationPublicReason,
            user.ModerationStartedAt,
            user.ModerationExpiresAt,
            user.ActiveModerationLogId,
            canAppeal);
    }

    public async Task<UserModerationDetailDto?> GetUserModerationAsync(string userId)
    {
        var user = await this._userManager.FindByIdAsync(userId);
        if (user is null) return null;

        var history = await this._context.UserModerationLogs
            .AsNoTracking()
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .Take(50)
            .Select(l => new UserModerationLogDto(
                l.Id, l.AdminId, l.AdminDisplayName, l.PreviousLevel, l.NewLevel,
                l.Reason, l.PublicReason, l.InternalNote, l.ExpiresAt,
                l.RelatedReportId, l.RelatedAppealId, l.CreatedAt))
            .ToListAsync();

        var openReportCount = await this._context.AbuseReports
            .CountAsync(r => r.TargetUserId == userId
                          && (r.Status == ReportStatus.Pending || r.Status == ReportStatus.UnderReview));

        var unackSignals = await this._context.AbuseSignals
            .Where(s => s.SubjectUserId == userId && !s.Acknowledged)
            .ToListAsync();
        var unackCount = unackSignals.Count;
        var totalScore = unackSignals.Sum(s => s.Score);

        var canAppeal = await CanAppealAsync(user);
        var status = new UserModerationStatusDto(
            user.ModerationLevel, user.ModerationReason, user.ModerationPublicReason,
            user.ModerationStartedAt, user.ModerationExpiresAt, user.ActiveModerationLogId, canAppeal);

        return new UserModerationDetailDto(
            user.Id, user.Email ?? string.Empty, user.DisplayName,
            status, history, openReportCount, unackCount, totalScore);
    }

    public async Task<Guid> ApplyActionAsync(string userId, ModerationActionRequest request, string adminId, string adminDisplayName)
    {
        if (string.IsNullOrWhiteSpace(request.PublicReason))
            throw new ArgumentException("PublicReason is required.", nameof(request));

        var user = await this._userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User '{userId}' not found.");

        var previousLevel = user.ModerationLevel;
        var nowUtc = DateTime.UtcNow;
        var expiresAt = request.DurationMinutes is int minutes && minutes > 0
            ? nowUtc.AddMinutes(minutes)
            : (DateTime?)null;

        var log = new UserModerationLog
        {
            UserId = userId,
            AdminId = adminId,
            AdminDisplayName = adminDisplayName,
            PreviousLevel = previousLevel,
            NewLevel = request.NewLevel,
            Reason = request.Reason,
            PublicReason = request.PublicReason,
            InternalNote = request.InternalNote,
            ExpiresAt = expiresAt,
            RelatedReportId = request.RelatedReportId,
            RelatedAppealId = request.RelatedAppealId
        };
        this._context.UserModerationLogs.Add(log);

        user.ModerationLevel = request.NewLevel;
        user.ModerationReason = request.Reason;
        user.ModerationStartedAt = nowUtc;
        user.ModerationExpiresAt = expiresAt;
        user.ModerationPublicReason = request.PublicReason;
        user.ActiveModerationLogId = log.Id;

        // Outbox: notification email + n8n webhook. Both share the EF unit of work, so commit
        // failures roll the side-effects back automatically.
        var locale = string.IsNullOrEmpty(user.LanguagePreference) ? "bg" : user.LanguagePreference;
        this._outbox.Enqueue(OutboxMessageTypes.UserModerationEmail, new UserModerationEmailOutboxPayload(
            ToEmail: user.Email ?? string.Empty,
            DisplayName: user.DisplayName,
            Locale: locale,
            TemplateKey: ResolveTemplateKey(request.NewLevel),
            Level: request.NewLevel.ToString(),
            Reason: request.Reason.ToString(),
            PublicReason: request.PublicReason,
            ExpiresAtUtc: expiresAt));

        this._outbox.Enqueue(OutboxMessageTypes.N8nWebhook, new N8nWebhookOutboxPayload(
            "user-moderated",
            System.Text.Json.JsonSerializer.Serialize(new
            {
                @event = "user.moderated",
                timestamp = nowUtc,
                userId = user.Id,
                email = user.Email,
                user.DisplayName,
                previousLevel = previousLevel.ToString(),
                newLevel = request.NewLevel.ToString(),
                reason = request.Reason.ToString(),
                expiresAt
            }, OutboxJson)));

        await this._context.SaveChangesAsync(CancellationToken.None);
        await this._userManager.UpdateAsync(user);
        await EvictCacheAsync(userId);
        await this._audit.LogAsync(adminId, "Admin.ModerateUser", success: true, targetId: userId,
            details: $"{previousLevel}→{request.NewLevel} ({request.Reason})");

        // Refresh-token revocation on escalations only. Warned does not revoke.
        if (request.NewLevel > previousLevel
            && (request.NewLevel == UserModerationLevel.Restricted
                || request.NewLevel == UserModerationLevel.Suspended
                || request.NewLevel == UserModerationLevel.Banned))
        {
            await this._authService.RevokeTokenAsync(userId);
        }

        return log.Id;
    }

    public async Task ManualClearAsync(string userId, string adminId, string adminDisplayName, string reason)
    {
        var user = await this._userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User '{userId}' not found.");

        if (user.ModerationLevel == UserModerationLevel.None) return;

        var previousLevel = user.ModerationLevel;
        var log = new UserModerationLog
        {
            UserId = userId,
            AdminId = adminId,
            AdminDisplayName = adminDisplayName,
            PreviousLevel = previousLevel,
            NewLevel = UserModerationLevel.None,
            Reason = ModerationReason.ManualReview,
            PublicReason = string.IsNullOrWhiteSpace(reason) ? "Cleared by administrator" : reason,
            ExpiresAt = null
        };
        this._context.UserModerationLogs.Add(log);

        user.ModerationLevel = UserModerationLevel.None;
        user.ModerationReason = ModerationReason.Unspecified;
        user.ModerationStartedAt = null;
        user.ModerationExpiresAt = null;
        user.ModerationPublicReason = null;
        user.ActiveModerationLogId = null;

        await this._context.SaveChangesAsync(CancellationToken.None);
        await this._userManager.UpdateAsync(user);
        await EvictCacheAsync(userId);
        await this._audit.LogAsync(adminId, "Admin.ModerateUser.Clear", success: true, targetId: userId,
            details: $"Cleared from {previousLevel}");
    }

    public async Task<int> ClearExpiredAsync(CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;
        var candidates = await this._context.Users
            .Where(u => (u.ModerationLevel == UserModerationLevel.Restricted
                      || u.ModerationLevel == UserModerationLevel.Suspended)
                     && u.ModerationExpiresAt != null
                     && u.ModerationExpiresAt <= nowUtc)
            .ToListAsync(cancellationToken);

        if (candidates.Count == 0) return 0;

        foreach (var user in candidates)
        {
            var previousLevel = user.ModerationLevel;
            this._context.UserModerationLogs.Add(new UserModerationLog
            {
                UserId = user.Id,
                AdminId = SystemAdminId,
                AdminDisplayName = "System (auto-expiry)",
                PreviousLevel = previousLevel,
                NewLevel = UserModerationLevel.None,
                Reason = ModerationReason.ManualReview,
                PublicReason = "Moderation expired",
                ExpiresAt = null
            });

            user.ModerationLevel = UserModerationLevel.None;
            user.ModerationReason = ModerationReason.Unspecified;
            user.ModerationStartedAt = null;
            user.ModerationExpiresAt = null;
            user.ModerationPublicReason = null;
            user.ActiveModerationLogId = null;
        }

        try
        {
            await this._context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            this._logger.LogWarning(ex, "Optimistic concurrency hit during moderation expiry sweep — skipping conflicting rows.");
            return 0;
        }

        foreach (var user in candidates)
        {
            await EvictCacheAsync(user.Id);
            await this._audit.LogAsync(SystemAdminId, "System.ModerateUser.Clear", success: true,
                targetId: user.Id, details: "Auto-expiry");
        }

        return candidates.Count;
    }

    private async Task<bool> CanAppealAsync(ApplicationUser user)
    {
        if (user.ModerationLevel == UserModerationLevel.None || user.ModerationLevel == UserModerationLevel.Warned)
            return false;
        if (user.ActiveModerationLogId is not Guid activeLogId) return false;

        // Block if there's already an open appeal against this moderation log.
        return !await this._context.ModerationAppeals
            .AnyAsync(a => a.ModerationLogId == activeLogId
                        && (a.Status == AppealStatus.Pending || a.Status == AppealStatus.UnderReview));
    }

    private async Task EvictCacheAsync(string userId)
    {
        await this._cache.RemoveAsync(ModerationCachePrefix + userId);
        await this._cache.RemoveAsync(LegacyBlockedCachePrefix + userId);
    }

    private static string ResolveTemplateKey(UserModerationLevel level) => level switch
    {
        UserModerationLevel.Warned     => "moderation.warned",
        UserModerationLevel.Restricted => "moderation.restricted",
        UserModerationLevel.Suspended  => "moderation.suspended",
        UserModerationLevel.Banned     => "moderation.banned",
        _                              => "moderation.cleared",
    };

    private static readonly System.Text.Json.JsonSerializerOptions OutboxJson = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
    };
}
