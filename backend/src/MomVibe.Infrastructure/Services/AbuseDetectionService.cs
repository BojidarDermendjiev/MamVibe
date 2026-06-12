namespace MomVibe.Infrastructure.Services;

using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Application.Interfaces;
using Application.DTOs.Moderation;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Configuration;

/// <summary>
/// Heuristics that watch authentication, listing, messaging, and registration activity and
/// flag user accounts for admin review via <see cref="AbuseSignal"/> rows. Never auto-enforces.
/// </summary>
public class AbuseDetectionService : IAbuseDetectionService
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AbuseDetectionSettings _settings;
    private readonly ILogger<AbuseDetectionService> _logger;

    private readonly Lazy<List<Regex>> _spamRegexes;

    public AbuseDetectionService(
        IApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IOptions<AbuseDetectionSettings> settings,
        ILogger<AbuseDetectionService> logger)
    {
        this._context = context;
        this._userManager = userManager;
        this._settings = settings.Value;
        this._logger = logger;
        this._spamRegexes = new Lazy<List<Regex>>(() => this._settings.SpamKeywords
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Select(k => new Regex(k, RegexOptions.IgnoreCase | RegexOptions.Compiled))
            .ToList());
    }

    public async Task RecordFailedLoginAsync(string? userIdOrEmail, string? ipAddress)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userIdOrEmail) && string.IsNullOrWhiteSpace(ipAddress))
                return;

            var since = DateTime.UtcNow.AddMinutes(-this._settings.FailedLoginWindowMinutes);

            // Count recent failed-login audit rows. Match on either userId (audit Action="Auth.Login")
            // or IP address — whichever is available.
            var failureQuery = this._context.AuditLogs
                .AsNoTracking()
                .Where(a => a.Action == "Auth.Login" && !a.Success && a.CreatedAt >= since);

            string? subjectUserId = null;
            int count;
            if (!string.IsNullOrEmpty(userIdOrEmail) && !userIdOrEmail.Contains('@'))
            {
                subjectUserId = userIdOrEmail;
                count = await failureQuery.CountAsync(a => a.UserId == userIdOrEmail);
            }
            else if (!string.IsNullOrEmpty(ipAddress))
            {
                count = await failureQuery.CountAsync(a => a.IpAddress == ipAddress);
            }
            else
            {
                return;
            }

            if (count < this._settings.FailedLoginThreshold) return;

            this._context.AbuseSignals.Add(new AbuseSignal
            {
                Type = AbuseSignalType.FailedLoginBurst,
                SubjectUserId = subjectUserId ?? "anonymous",
                Score = this._settings.FailedLoginScore,
                Details = System.Text.Json.JsonSerializer.Serialize(new
                {
                    failedAttempts = count,
                    windowMinutes = this._settings.FailedLoginWindowMinutes,
                    ip = ipAddress
                })
            });
            await this._context.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            this._logger.LogWarning(ex, "RecordFailedLoginAsync failed");
        }
    }

    public async Task EvaluateListingBurstAsync(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId)) return;
            var since = DateTime.UtcNow.AddMinutes(-this._settings.MassListingWindowMinutes);
            var count = await this._context.Items
                .AsNoTracking()
                .CountAsync(i => i.UserId == userId && i.CreatedAt >= since);
            if (count < this._settings.MassListingThreshold) return;

            this._context.AbuseSignals.Add(new AbuseSignal
            {
                Type = AbuseSignalType.MassListingCreation,
                SubjectUserId = userId,
                Score = this._settings.MassListingScore,
                Details = System.Text.Json.JsonSerializer.Serialize(new
                {
                    listingsCreated = count,
                    windowMinutes = this._settings.MassListingWindowMinutes
                })
            });
            await this._context.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            this._logger.LogWarning(ex, "EvaluateListingBurstAsync failed for {UserId}", userId);
        }
    }

    public async Task EvaluateMessageAsync(string senderId, string content)
    {
        try
        {
            if (string.IsNullOrEmpty(senderId) || string.IsNullOrWhiteSpace(content)) return;
            if (this._spamRegexes.Value.Count == 0) return;

            var firstHit = this._spamRegexes.Value.FirstOrDefault(r => r.IsMatch(content));
            if (firstHit is null) return;

            this._context.AbuseSignals.Add(new AbuseSignal
            {
                Type = AbuseSignalType.SpamKeywordMessage,
                SubjectUserId = senderId,
                Score = this._settings.SpamMessageScore,
                Details = System.Text.Json.JsonSerializer.Serialize(new
                {
                    matchedPattern = firstHit.ToString(),
                    contentExcerpt = content.Length > 200 ? content[..200] : content
                })
            });
            await this._context.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            this._logger.LogWarning(ex, "EvaluateMessageAsync failed for {SenderId}", senderId);
        }
    }

    public async Task EvaluateMultiAccountAsync(string newUserId, string? ipAddress)
    {
        try
        {
            if (string.IsNullOrEmpty(ipAddress)) return;
            var since = DateTime.UtcNow.AddHours(-this._settings.MultiAccountWindowHours);
            var distinctUserCount = await this._context.AuditLogs
                .AsNoTracking()
                .Where(a => a.Action == "Auth.Register" && a.Success
                         && a.IpAddress == ipAddress
                         && a.CreatedAt >= since)
                .Select(a => a.UserId)
                .Distinct()
                .CountAsync();

            if (distinctUserCount < this._settings.MultiAccountThreshold) return;

            this._context.AbuseSignals.Add(new AbuseSignal
            {
                Type = AbuseSignalType.MultiAccountSameIp,
                SubjectUserId = newUserId,
                Score = this._settings.MultiAccountScore,
                Details = System.Text.Json.JsonSerializer.Serialize(new
                {
                    distinctAccountsFromIp = distinctUserCount,
                    windowHours = this._settings.MultiAccountWindowHours,
                    ip = ipAddress
                })
            });
            await this._context.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            this._logger.LogWarning(ex, "EvaluateMultiAccountAsync failed for {UserId}", newUserId);
        }
    }

    public async Task<PagedModerationResult<AbuseSignalDto>> GetAdminQueueAsync(AbuseSignalFilter filter)
    {
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var query = this._context.AbuseSignals.AsNoTracking();
        if (!filter.IncludeAcknowledged) query = query.Where(s => !s.Acknowledged);
        if (filter.Type.HasValue) query = query.Where(s => s.Type == filter.Type.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new AbuseSignalDto(
                s.Id, s.Type, s.SubjectUserId, s.Score, s.Details, s.EvidenceTargetId,
                s.Acknowledged, s.AcknowledgedByAdminId, s.AcknowledgedAt, s.CreatedAt))
            .ToListAsync();

        return new PagedModerationResult<AbuseSignalDto>(items, total, page, pageSize);
    }

    public async Task AcknowledgeAsync(Guid signalId, string adminId)
    {
        var signal = await this._context.AbuseSignals.FirstOrDefaultAsync(s => s.Id == signalId)
            ?? throw new KeyNotFoundException($"Signal '{signalId}' not found.");
        if (signal.Acknowledged) return;
        signal.Acknowledged = true;
        signal.AcknowledgedByAdminId = adminId;
        signal.AcknowledgedAt = DateTime.UtcNow;
        await this._context.SaveChangesAsync(CancellationToken.None);
    }
}
