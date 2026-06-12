namespace MomVibe.Infrastructure.Services.Business;

using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

/// <summary>
/// EF Core-backed implementation of <see cref="IDeviceFingerprintService"/>. All persistence
/// writes go through the injected <see cref="IApplicationDbContext"/> so callers can
/// compose the link upsert into a larger transactional unit of work.
/// </summary>
public class DeviceFingerprintService : IDeviceFingerprintService
{
    private readonly IApplicationDbContext _db;

    public DeviceFingerprintService(IApplicationDbContext db)
    {
        _db = db;
    }

    public string HashVisitorId(string visitorId)
    {
        if (string.IsNullOrEmpty(visitorId))
            return string.Empty;
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(visitorId));
        return Convert.ToHexString(bytes);
    }

    public async Task<string?> GetConflictingUserIdAsync(string fingerprintHash, string currentUserId)
    {
        if (string.IsNullOrEmpty(fingerprintHash)) return null;

        return await _db.DeviceFingerprintUsers
            .AsNoTracking()
            .Where(dfu => dfu.FingerprintHash == fingerprintHash && dfu.UserId != currentUserId)
            .Join(_db.BusinessProfiles.AsNoTracking(),
                  dfu => dfu.UserId,
                  bp => bp.UserId,
                  (dfu, bp) => new { dfu.UserId, bp.Status, bp.DeviceCheckBypassedByAdminId })
            .Where(x => x.Status != BusinessProfileStatus.Removed
                     && x.DeviceCheckBypassedByAdminId == null)
            .Select(x => x.UserId)
            .FirstOrDefaultAsync();
    }

    public async Task UpsertLinkAsync(string fingerprintHash, string userId)
    {
        if (string.IsNullOrEmpty(fingerprintHash) || string.IsNullOrEmpty(userId)) return;

        var now = DateTime.UtcNow;

        var fingerprint = await _db.DeviceFingerprints.FirstOrDefaultAsync(f => f.Hash == fingerprintHash);
        var isNewFingerprint = fingerprint == null;
        if (isNewFingerprint)
        {
            fingerprint = new DeviceFingerprint
            {
                Hash = fingerprintHash,
                FirstSeenAt = now,
                LastSeenAt = now,
                LinkedUserCount = 1,
            };
            _db.DeviceFingerprints.Add(fingerprint);
        }
        else
        {
            fingerprint!.LastSeenAt = now;
        }

        var existingLink = await _db.DeviceFingerprintUsers
            .FirstOrDefaultAsync(dfu => dfu.FingerprintHash == fingerprintHash && dfu.UserId == userId);
        if (existingLink == null)
        {
            _db.DeviceFingerprintUsers.Add(new DeviceFingerprintUser
            {
                FingerprintHash = fingerprintHash,
                UserId = userId,
                FirstSeenAt = now,
            });
            // Only bump the counter for existing fingerprints — new ones were initialised to 1 above.
            if (!isNewFingerprint)
                fingerprint!.LinkedUserCount += 1;
        }
    }

    public async Task EmitDuplicateSignalAsync(string fingerprintHash, string currentUserId, string conflictingUserId)
    {
        var details = JsonSerializer.Serialize(new
        {
            fingerprintHash,
            conflictingUserId,
        });

        _db.AbuseSignals.Add(new AbuseSignal
        {
            Type = AbuseSignalType.MultiAccountSameDevice,
            SubjectUserId = currentUserId,
            Score = 60,
            Details = details,
            EvidenceTargetId = fingerprintHash,
        });
        await _db.SaveChangesAsync();
    }

    public string TruncateIpForStorage(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress)) return string.Empty;
        if (!IPAddress.TryParse(ipAddress, out var ip)) return ipAddress;

        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            // IPv4 /24 — drop the last octet.
            var bytes = ip.GetAddressBytes();
            bytes[3] = 0;
            return new IPAddress(bytes).ToString();
        }

        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            // IPv6 /64 — keep first 8 bytes, zero the remaining 8.
            var bytes = ip.GetAddressBytes();
            for (int i = 8; i < 16; i++) bytes[i] = 0;
            return new IPAddress(bytes).ToString();
        }

        return ipAddress;
    }
}
