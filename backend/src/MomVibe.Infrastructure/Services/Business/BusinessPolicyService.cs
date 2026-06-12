namespace MomVibe.Infrastructure.Services.Business;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using Application.Interfaces;
using Application.DTOs.Business;
using Domain.Entities;

/// <summary>
/// Read + acceptance operations for the versioned business platform policy. The current
/// version per language is cached in-memory for 5 minutes — admin policy updates surface
/// the next time the cache expires (or when the operator restarts the app).
/// </summary>
public class BusinessPolicyService : IBusinessPolicyService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private readonly IApplicationDbContext _db;
    private readonly IMemoryCache _cache;

    public BusinessPolicyService(IApplicationDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<BusinessPolicyDto> GetCurrentAsync(string language)
    {
        var normalized = NormalizeLanguage(language);
        var cacheKey = $"business-policy:{normalized}";

        if (_cache.TryGetValue<BusinessPolicyDto>(cacheKey, out var cached) && cached != null)
            return cached;

        var entity = await _db.BusinessPolicyVersions
            .AsNoTracking()
            .Where(v => v.Language == normalized && v.IsCurrent)
            .FirstOrDefaultAsync();

        // Fall back to English if no version exists for the requested language yet.
        if (entity == null && normalized != "en")
        {
            entity = await _db.BusinessPolicyVersions
                .AsNoTracking()
                .Where(v => v.Language == "en" && v.IsCurrent)
                .FirstOrDefaultAsync();
        }

        if (entity == null)
            throw new KeyNotFoundException("No active business policy version is configured.");

        var dto = MapToDto(entity);
        _cache.Set(cacheKey, dto, CacheTtl);
        return dto;
    }

    public async Task AcceptAsync(string userId, Guid policyVersionId, string? ip, string? userAgent)
    {
        var policy = await _db.BusinessPolicyVersions.FirstOrDefaultAsync(v => v.Id == policyVersionId)
            ?? throw new KeyNotFoundException("Policy version not found.");

        var profile = await _db.BusinessProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
            // No profile yet — acceptance will be written during profile creation. Nothing to do.
            return;

        var existing = await _db.BusinessPolicyAcceptances
            .AnyAsync(a => a.BusinessProfileId == profile.Id && a.PolicyVersionId == policyVersionId);
        if (existing)
        {
            // Idempotent — still update the profile pointer so the UI stops nagging.
            if (profile.PolicyVersionAcceptedId != policyVersionId)
            {
                profile.PolicyVersionAcceptedId = policyVersionId;
                await _db.SaveChangesAsync();
            }
            return;
        }

        _db.BusinessPolicyAcceptances.Add(new BusinessPolicyAcceptance
        {
            BusinessProfileId = profile.Id,
            PolicyVersionId = policyVersionId,
            AcceptedAt = DateTime.UtcNow,
            Ip = ip,
            UserAgent = userAgent,
        });
        profile.PolicyVersionAcceptedId = policyVersionId;
        await _db.SaveChangesAsync();
    }

    private static string NormalizeLanguage(string? language)
    {
        if (string.IsNullOrWhiteSpace(language)) return "en";
        var lower = language.Trim().ToLowerInvariant();
        return lower.StartsWith("bg") ? "bg" : "en";
    }

    private static BusinessPolicyDto MapToDto(BusinessPolicyVersion v) => new()
    {
        Id = v.Id,
        Version = v.Version,
        Language = v.Language,
        Title = v.Title,
        BodyMarkdown = v.BodyMarkdown,
        EffectiveFrom = v.EffectiveFrom,
    };
}
