namespace MomVibe.Infrastructure.Services.Business;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Application.Interfaces;
using Application.DTOs.Business;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

/// <summary>
/// Manages the single <c>BusinessProfile</c> owned by the current user. Profile creation
/// performs device-fingerprint duplicate detection, role assignment (<c>Business</c>),
/// and writes the initial <c>BusinessPolicyAcceptance</c> row inside a single SaveChanges call.
/// </summary>
public class BusinessProfileService : IBusinessProfileService
{
    private const string BusinessRoleName = "Business";

    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDeviceFingerprintService _fingerprint;
    private readonly IAuditLogService _audit;
    private readonly ILogger<BusinessProfileService> _logger;

    public BusinessProfileService(
        IApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IDeviceFingerprintService fingerprint,
        IAuditLogService audit,
        ILogger<BusinessProfileService> logger)
    {
        _db = db;
        _userManager = userManager;
        _fingerprint = fingerprint;
        _audit = audit;
        _logger = logger;
    }

    public async Task<BusinessProfileDto?> GetMineAsync(string userId)
    {
        var profile = await _db.BusinessProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null) return null;

        var hasListing = await _db.BusinessListings.AnyAsync(l => l.BusinessProfileId == profile.Id);
        var hasSubscription = await _db.BusinessSubscriptions.AnyAsync(s => s.BusinessProfileId == profile.Id);

        // Resolve current policy id for the profile's user language to drive the re-acceptance banner.
        var userLanguage = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.LanguagePreference)
            .FirstOrDefaultAsync() ?? "en";
        var currentLanguage = userLanguage.StartsWith("bg", StringComparison.OrdinalIgnoreCase) ? "bg" : "en";

        var currentPolicyId = await _db.BusinessPolicyVersions
            .AsNoTracking()
            .Where(v => v.Language == currentLanguage && v.IsCurrent)
            .Select(v => (Guid?)v.Id)
            .FirstOrDefaultAsync();

        var reacceptanceRequired = currentPolicyId.HasValue
                                   && profile.PolicyVersionAcceptedId != currentPolicyId.Value;

        return MapToDto(profile, hasListing, hasSubscription, reacceptanceRequired);
    }

    public async Task<BusinessProfileDto> CreateAsync(string userId, CreateBusinessProfileRequest request, string? ip, string? userAgent)
    {
        // 1. No second profile per user (DB also has UX_BusinessProfiles_UserId; this is the clean 409).
        var profileExists = await _db.BusinessProfiles.AnyAsync(p => p.UserId == userId);
        if (profileExists)
            throw new BusinessConflictException("profile_already_exists", "You already have a business profile.");

        // 2. Policy version must exist.
        var policy = await _db.BusinessPolicyVersions.FirstOrDefaultAsync(v => v.Id == request.PolicyVersionId)
            ?? throw new KeyNotFoundException("Policy version not found.");

        // 3. Device-fingerprint duplicate check.
        var fingerprintHash = _fingerprint.HashVisitorId(request.FingerprintVisitorId);
        if (string.IsNullOrEmpty(fingerprintHash))
            throw new BusinessConflictException("fingerprint_missing",
                "Device fingerprint is required. Disable privacy/tracking blockers and retry.");

        var conflictingUserId = await _fingerprint.GetConflictingUserIdAsync(fingerprintHash, userId);
        if (conflictingUserId != null)
        {
            try
            {
                await _fingerprint.EmitDuplicateSignalAsync(fingerprintHash, userId, conflictingUserId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to emit MultiAccountSameDevice signal for {UserId}", userId);
            }
            await _audit.LogAsync(userId, "Business.Profile.DeviceConflict", success: false, targetId: fingerprintHash, ipAddress: ip);
            throw new DeviceConflictException("device_already_has_business",
                "This device is already linked to an active business account. Contact support if this is a shared family device.");
        }

        var truncatedIp = _fingerprint.TruncateIpForStorage(ip);

        var profile = new BusinessProfile
        {
            UserId = userId,
            Category = request.Category,
            ProfileKind = request.ProfileKind,
            LegalName = request.LegalName.Trim(),
            DisplayName = request.DisplayName.Trim(),
            Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim(),
            ContactEmail = request.ContactEmail.Trim(),
            ContactPhone = string.IsNullOrWhiteSpace(request.ContactPhone) ? null : request.ContactPhone.Trim(),
            Website = string.IsNullOrWhiteSpace(request.Website) ? null : request.Website.Trim(),
            City = request.City.Trim(),
            DeviceFingerprintHash = fingerprintHash,
            IpAtRegistration = string.IsNullOrEmpty(truncatedIp) ? null : truncatedIp,
            UserAgentAtRegistration = string.IsNullOrEmpty(userAgent) ? null : userAgent[..Math.Min(userAgent.Length, 512)],
            Status = BusinessProfileStatus.PendingPayment,
            PolicyVersionAcceptedId = policy.Id,
        };
        _db.BusinessProfiles.Add(profile);

        _db.BusinessPolicyAcceptances.Add(new BusinessPolicyAcceptance
        {
            BusinessProfileId = profile.Id,
            PolicyVersionId = policy.Id,
            AcceptedAt = DateTime.UtcNow,
            Ip = string.IsNullOrEmpty(truncatedIp) ? null : truncatedIp,
            UserAgent = profile.UserAgentAtRegistration,
        });

        await _fingerprint.UpsertLinkAsync(fingerprintHash, userId);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // Unique-constraint race on UserId — concurrent second create attempt.
            _logger.LogWarning(ex, "BusinessProfile create race detected for {UserId}", userId);
            throw new BusinessConflictException("profile_already_exists", "You already have a business profile.");
        }

        // Role assignment is outside EF — if it fails the profile still exists but the user
        // misses the Business role. Operator can re-run; the dashboard still works because
        // GetMineAsync does not depend on the role flag.
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null && !await _userManager.IsInRoleAsync(user, BusinessRoleName))
        {
            var addResult = await _userManager.AddToRoleAsync(user, BusinessRoleName);
            if (!addResult.Succeeded)
            {
                _logger.LogWarning(
                    "Failed to assign Business role to user {UserId}: {Errors}",
                    userId, string.Join("; ", addResult.Errors.Select(e => e.Description)));
            }
        }

        await _audit.LogAsync(userId, "Business.Profile.Created", success: true, targetId: profile.Id.ToString(), ipAddress: ip);

        return MapToDto(profile, hasListing: false, hasSubscription: false, policyReacceptanceRequired: false);
    }

    public async Task<BusinessProfileDto> UpdateAsync(string userId, UpdateBusinessProfileRequest request)
    {
        var profile = await _db.BusinessProfiles.FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new KeyNotFoundException("Business profile not found.");

        profile.ProfileKind = request.ProfileKind;
        profile.LegalName = request.LegalName.Trim();
        profile.DisplayName = request.DisplayName.Trim();
        profile.Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim();
        profile.ContactEmail = request.ContactEmail.Trim();
        profile.ContactPhone = string.IsNullOrWhiteSpace(request.ContactPhone) ? null : request.ContactPhone.Trim();
        profile.Website = string.IsNullOrWhiteSpace(request.Website) ? null : request.Website.Trim();
        profile.City = request.City.Trim();

        await _db.SaveChangesAsync();
        await _audit.LogAsync(userId, "Business.Profile.Updated", success: true, targetId: profile.Id.ToString());

        var hasListing = await _db.BusinessListings.AnyAsync(l => l.BusinessProfileId == profile.Id);
        var hasSubscription = await _db.BusinessSubscriptions.AnyAsync(s => s.BusinessProfileId == profile.Id);

        return MapToDto(profile, hasListing, hasSubscription, policyReacceptanceRequired: false);
    }

    private static BusinessProfileDto MapToDto(BusinessProfile p, bool hasListing, bool hasSubscription, bool policyReacceptanceRequired) => new()
    {
        Id = p.Id,
        UserId = p.UserId,
        Category = p.Category,
        ProfileKind = p.ProfileKind,
        LegalName = p.LegalName,
        DisplayName = p.DisplayName,
        Bio = p.Bio,
        ContactEmail = p.ContactEmail,
        ContactPhone = p.ContactPhone,
        Website = p.Website,
        City = p.City,
        Status = p.Status,
        PolicyReacceptanceRequired = policyReacceptanceRequired,
        HasListing = hasListing,
        HasSubscription = hasSubscription,
        CreatedAt = p.CreatedAt,
    };
}
