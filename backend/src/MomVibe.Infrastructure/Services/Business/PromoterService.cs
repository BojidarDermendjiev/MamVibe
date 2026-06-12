namespace MomVibe.Infrastructure.Services.Business;

using System.Security.Cryptography;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Application.Interfaces;
using Application.DTOs.Business;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

/// <summary>
/// EF Core + UserManager-backed implementation of <see cref="IPromoterService"/>. Referral
/// codes are 8-char Base32 strings prefixed with <c>MAMA-</c> (e.g., <c>MAMA-AB12CD34</c>) —
/// the alphabet excludes I/O/0/1 for readability over the phone.
/// </summary>
public class PromoterService : IPromoterService
{
    private const string ReferralAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // 32 chars
    private const int ReferralCodeBodyLength = 8;
    private const int MaxCodeGenerationAttempts = 4;
    private const string PromoterRoleName = "Promoter";
    private const int RecentReferralCount = 10;

    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditLogService _audit;
    private readonly ILogger<PromoterService> _logger;

    public PromoterService(
        IApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IAuditLogService audit,
        ILogger<PromoterService> logger)
    {
        _db = db;
        _userManager = userManager;
        _audit = audit;
        _logger = logger;
    }

    public async Task<PromoterProfileDto?> GetMineAsync(string userId)
    {
        var profile = await _db.PromoterProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId);
        return profile == null ? null : MapToDto(profile);
    }

    public async Task<PromoterProfileDto> CreateAsync(string userId)
    {
        var existing = await _db.PromoterProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (existing != null)
            return MapToDto(existing);

        // Generate a unique referral code — retry on collision (rare; UX_PromoterProfiles_ReferralCode
        // also enforces uniqueness at the DB layer as the second line of defence).
        string? code = null;
        for (var attempt = 0; attempt < MaxCodeGenerationAttempts; attempt++)
        {
            var candidate = GenerateCode();
            var taken = await _db.PromoterProfiles.AnyAsync(p => p.ReferralCode == candidate);
            if (!taken) { code = candidate; break; }
        }
        if (code == null)
            throw new BusinessConflictException("code_collision",
                "Could not generate a unique referral code. Please retry.");

        var profile = new PromoterProfile
        {
            UserId = userId,
            ReferralCode = code,
            IsActive = true,
            TotalReferrals = 0,
            TotalActivations = 0,
        };
        _db.PromoterProfiles.Add(profile);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Promoter profile create race for user {UserId}", userId);
            var existingAfterRace = await _db.PromoterProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (existingAfterRace != null) return MapToDto(existingAfterRace);
            throw;
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user != null && !await _userManager.IsInRoleAsync(user, PromoterRoleName))
        {
            var addResult = await _userManager.AddToRoleAsync(user, PromoterRoleName);
            if (!addResult.Succeeded)
                _logger.LogWarning("Failed to assign Promoter role to {UserId}: {Errors}",
                    userId, string.Join("; ", addResult.Errors.Select(e => e.Description)));
        }

        await _audit.LogAsync(userId, "Promoter.Profile.Created",
            success: true, targetId: profile.Id.ToString(), details: $"code={code}");

        return MapToDto(profile);
    }

    public async Task<PromoterDashboardDto> GetDashboardAsync(string userId)
    {
        var profile = await _db.PromoterProfiles.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new KeyNotFoundException("Promoter profile not found.");

        var byStatus = await _db.CoachReferrals
            .AsNoTracking()
            .Where(r => r.ReferralCode == profile.ReferralCode)
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var counters = byStatus.ToDictionary(x => x.Status, x => x.Count);

        var recent = await _db.CoachReferrals
            .AsNoTracking()
            .Where(r => r.ReferralCode == profile.ReferralCode)
            .OrderByDescending(r => r.CreatedAt)
            .Take(RecentReferralCount)
            .Select(r => new RecentReferralDto
            {
                Id = r.Id,
                BusinessName = r.BusinessName,
                City = r.City,
                ActivityType = r.ActivityType,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
            })
            .ToListAsync();

        return new PromoterDashboardDto
        {
            Profile = MapToDto(profile),
            TotalSubmitted = counters.GetValueOrDefault(CoachReferralStatus.Submitted),
            TotalContacted = counters.GetValueOrDefault(CoachReferralStatus.Contacted),
            TotalOnboarded = counters.GetValueOrDefault(CoachReferralStatus.Onboarded),
            TotalRejected = counters.GetValueOrDefault(CoachReferralStatus.Rejected),
            Recent = recent,
        };
    }

    private static string GenerateCode()
    {
        var buffer = new char[ReferralCodeBodyLength];
        Span<byte> random = stackalloc byte[ReferralCodeBodyLength];
        RandomNumberGenerator.Fill(random);
        for (var i = 0; i < ReferralCodeBodyLength; i++)
            buffer[i] = ReferralAlphabet[random[i] % ReferralAlphabet.Length];
        return $"MAMA-{new string(buffer)}";
    }

    private static PromoterProfileDto MapToDto(PromoterProfile p) => new()
    {
        Id = p.Id,
        UserId = p.UserId,
        ReferralCode = p.ReferralCode,
        IsActive = p.IsActive,
        TotalReferrals = p.TotalReferrals,
        TotalActivations = p.TotalActivations,
        CreatedAt = p.CreatedAt,
    };
}
