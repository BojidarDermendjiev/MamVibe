namespace MomVibe.Infrastructure.Services.Business;

using System.Security.Cryptography;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Application.Interfaces;
using Application.DTOs.Business;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

/// <summary>
/// Public submission + admin queue for <see cref="CoachReferral"/>. Submission enforces a
/// Turnstile gate (in non-dev environments), a 30-day per-contact dedup window, and writes
/// a SHA-256 hash of the truncated IP for downstream abuse correlation. The rate limit is
/// applied at the controller level via <c>EnableRateLimiting</c>.
/// </summary>
public class CoachReferralService : ICoachReferralService
{
    private static readonly TimeSpan DedupWindow = TimeSpan.FromDays(30);

    private readonly IApplicationDbContext _db;
    private readonly ITurnstileService _turnstile;
    private readonly IConfiguration _config;
    private readonly IAuditLogService _audit;
    private readonly IHostEnvironment _env;
    private readonly ILogger<CoachReferralService> _logger;

    public CoachReferralService(
        IApplicationDbContext db,
        ITurnstileService turnstile,
        IConfiguration config,
        IAuditLogService audit,
        IHostEnvironment env,
        ILogger<CoachReferralService> logger)
    {
        _db = db;
        _turnstile = turnstile;
        _config = config;
        _audit = audit;
        _env = env;
        _logger = logger;
    }

    public async Task<Guid> SubmitAsync(SubmitCoachReferralRequest request, string? referrerUserId, string? ipAddress)
    {
        // 1. Turnstile gate — only enforced when not in development AND a secret key is configured.
        if (RequireTurnstile())
        {
            if (string.IsNullOrWhiteSpace(request.TurnstileToken))
                throw new ArgumentException("Turnstile token is required.", nameof(request));
            var ok = await _turnstile.VerifyAsync(request.TurnstileToken, ipAddress ?? string.Empty);
            if (!ok) throw new InvalidOperationException("Turnstile verification failed.");
        }

        // 2. Dedup — same contact (email OR phone) within the rolling 30-day window.
        var normalizedEmail = string.IsNullOrWhiteSpace(request.ContactEmail) ? null : request.ContactEmail.Trim().ToLowerInvariant();
        var normalizedPhone = string.IsNullOrWhiteSpace(request.ContactPhone) ? null : request.ContactPhone.Trim();
        var cutoff = DateTime.UtcNow - DedupWindow;
        var duplicate = await _db.CoachReferrals.AnyAsync(r =>
            r.CreatedAt > cutoff &&
            (
                (normalizedEmail != null && r.ContactEmail != null && r.ContactEmail.ToLower() == normalizedEmail) ||
                (normalizedPhone != null && r.ContactPhone == normalizedPhone)
            ));
        if (duplicate)
            throw new BusinessConflictException("referral_duplicate",
                "A referral for this coach was submitted in the last 30 days.");

        // 3. Referral code lookup — only attach a code that matches an active promoter.
        string? attachedCode = null;
        if (!string.IsNullOrWhiteSpace(request.ReferralCode))
        {
            var promoter = await _db.PromoterProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ReferralCode == request.ReferralCode && p.IsActive);
            if (promoter != null) attachedCode = promoter.ReferralCode;
        }

        var ipHash = HashIp(ipAddress);

        var referral = new CoachReferral
        {
            BusinessName = request.BusinessName.Trim(),
            ContactEmail = normalizedEmail,
            ContactPhone = normalizedPhone,
            ActivityType = request.ActivityType,
            City = request.City.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            ReferrerUserId = referrerUserId,
            ReferralCode = attachedCode,
            Status = CoachReferralStatus.Submitted,
            IpHash = ipHash,
        };
        _db.CoachReferrals.Add(referral);

        // 4. Bump promoter aggregate counter — TotalReferrals reflects ALL submitted referrals.
        if (attachedCode != null)
        {
            var promoterProfile = await _db.PromoterProfiles
                .FirstOrDefaultAsync(p => p.ReferralCode == attachedCode);
            if (promoterProfile != null) promoterProfile.TotalReferrals += 1;
        }

        await _db.SaveChangesAsync();
        await _audit.LogAsync(referrerUserId ?? "anonymous", "Business.CoachReferral.Submitted",
            success: true, targetId: referral.Id.ToString(), ipAddress: ipAddress);
        return referral.Id;
    }

    public async Task<PagedReferralsResult> AdminListAsync(CoachReferralStatus? status, int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.CoachReferrals.AsNoTracking();
        if (status.HasValue) query = query.Where(r => r.Status == status.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(r => r.Status == CoachReferralStatus.Submitted ? 0
                       : r.Status == CoachReferralStatus.Contacted ? 1 : 2)
            .ThenByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Join(
                _db.Users.AsNoTracking(),
                r => r.ReferrerUserId,
                u => u.Id,
                (r, u) => new { r, u.DisplayName })
            .Select(x => new CoachReferralDto
            {
                Id = x.r.Id,
                BusinessName = x.r.BusinessName,
                ContactEmail = x.r.ContactEmail,
                ContactPhone = x.r.ContactPhone,
                ActivityType = x.r.ActivityType,
                City = x.r.City,
                Notes = x.r.Notes,
                ReferrerUserId = x.r.ReferrerUserId,
                ReferrerDisplayName = x.DisplayName,
                ReferralCode = x.r.ReferralCode,
                Status = x.r.Status,
                AdminNote = x.r.AdminNote,
                ActionedByAdminId = x.r.ActionedByAdminId,
                ActionedAt = x.r.ActionedAt,
                CreatedAt = x.r.CreatedAt,
            })
            .ToListAsync();

        // Anonymous submissions (ReferrerUserId == null) get dropped by the inner Join — re-add them.
        if (items.Count < pageSize)
        {
            var fetchedIds = items.Select(i => i.Id).ToHashSet();
            var anonymous = await query
                .Where(r => r.ReferrerUserId == null && !fetchedIds.Contains(r.Id))
                .OrderBy(r => r.Status == CoachReferralStatus.Submitted ? 0
                           : r.Status == CoachReferralStatus.Contacted ? 1 : 2)
                .ThenByDescending(r => r.CreatedAt)
                .Take(pageSize - items.Count)
                .Select(r => new CoachReferralDto
                {
                    Id = r.Id,
                    BusinessName = r.BusinessName,
                    ContactEmail = r.ContactEmail,
                    ContactPhone = r.ContactPhone,
                    ActivityType = r.ActivityType,
                    City = r.City,
                    Notes = r.Notes,
                    ReferrerUserId = null,
                    ReferrerDisplayName = null,
                    ReferralCode = r.ReferralCode,
                    Status = r.Status,
                    AdminNote = r.AdminNote,
                    ActionedByAdminId = r.ActionedByAdminId,
                    ActionedAt = r.ActionedAt,
                    CreatedAt = r.CreatedAt,
                })
                .ToListAsync();
            items.AddRange(anonymous);
        }

        return new PagedReferralsResult
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }

    public async Task UpdateStatusAsync(Guid referralId, UpdateCoachReferralStatusRequest request, string adminId)
    {
        var referral = await _db.CoachReferrals.FirstOrDefaultAsync(r => r.Id == referralId)
            ?? throw new KeyNotFoundException("Coach referral not found.");

        var previousStatus = referral.Status;
        referral.Status = request.Status;
        referral.AdminNote = string.IsNullOrWhiteSpace(request.AdminNote) ? null : request.AdminNote.Trim();
        referral.ActionedByAdminId = adminId;
        referral.ActionedAt = DateTime.UtcNow;

        // Promote-activation counter — bump when an admin marks the referral Onboarded
        // and rewind when un-Onboarding (admin correction).
        if (!string.IsNullOrEmpty(referral.ReferralCode))
        {
            var promoter = await _db.PromoterProfiles
                .FirstOrDefaultAsync(p => p.ReferralCode == referral.ReferralCode);
            if (promoter != null)
            {
                if (previousStatus != CoachReferralStatus.Onboarded && request.Status == CoachReferralStatus.Onboarded)
                    promoter.TotalActivations += 1;
                else if (previousStatus == CoachReferralStatus.Onboarded && request.Status != CoachReferralStatus.Onboarded
                         && promoter.TotalActivations > 0)
                    promoter.TotalActivations -= 1;
            }
        }

        await _db.SaveChangesAsync();
        await _audit.LogAsync(adminId, "Admin.Business.CoachReferral.StatusChanged",
            success: true, targetId: referral.Id.ToString(),
            details: $"{previousStatus}→{request.Status}");
    }

    private bool RequireTurnstile()
    {
        if (_env.IsDevelopment()) return false;
        var key = _config["Cloudflare:TurnstileSecretKey"];
        return !string.IsNullOrWhiteSpace(key);
    }

    private static string HashIp(string? ip)
    {
        if (string.IsNullOrWhiteSpace(ip)) return string.Empty;
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(ip + "|mamvibe-coach-referral"));
        return Convert.ToHexString(bytes);
    }
}
