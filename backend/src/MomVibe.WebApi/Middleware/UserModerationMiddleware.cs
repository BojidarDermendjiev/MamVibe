namespace MomVibe.WebApi.Middleware;

using System.Security.Claims;
using System.Text.Json;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;

using MomVibe.Domain.Entities;
using MomVibe.Domain.Enums;

/// <summary>
/// Middleware that enforces graded user moderation (Warn / Restrict / Suspend / Ban) on every
/// authenticated request. Replaces the legacy binary <see cref="BlockedUserMiddleware"/>.
/// </summary>
/// <remarks>
/// Pipeline position: MUST run after <c>UseAuthentication</c> (needs the user identity) but
/// BEFORE <c>UseAuthorization</c> so the 403 short-circuit beats policy evaluation.
///
/// Behaviour by level:
/// <list type="bullet">
///   <item><b>None</b> / <b>Warned</b>: pass.</item>
///   <item><b>Restricted</b>: any write verb (POST/PUT/PATCH/DELETE) is blocked unless the path
///         is in <see cref="RestrictedAllowList"/>. GETs always pass.</item>
///   <item><b>Suspended</b>: every request blocked unless <c>ExpiresAt</c> has elapsed
///         (treated as None in that case; the expiry hosted-service will clear the DB row).</item>
///   <item><b>Banned</b>: every request blocked.</item>
/// </list>
///
/// Cached per user in <see cref="IDistributedCache"/> for 60 seconds. The cache is evicted on
/// every moderation action via <see cref="UserModerationCacheKey"/> so the DB-backed enforcement
/// is fresh within ~1 minute. The <c>IsBlocked</c> JWT claim is intentionally not used for
/// decisions because tokens live up to 15 minutes — staleness was the audit's HIGH finding.
/// </remarks>
public class UserModerationMiddleware
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(1);

    /// <summary>Paths a Restricted user is still allowed to POST to — needed to file an appeal and complete the auth cookie dance.</summary>
    private static readonly HashSet<string> RestrictedAllowList = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/v1/users/me/appeals",
        "/api/v1/auth/refresh",
        "/api/v1/auth/logout",
        "/api/v1/auth/revoke"
    };

    /// <summary>Returns the distributed cache key used for the per-user moderation snapshot.</summary>
    public static string UserModerationCacheKey(string userId) => $"moderation:{userId}";

    private readonly RequestDelegate _next;

    public UserModerationMiddleware(RequestDelegate next)
    {
        this._next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager, IDistributedCache cache)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await this._next(context);
            return;
        }

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            await this._next(context);
            return;
        }

        var snapshot = await LoadSnapshotAsync(userManager, cache, userId);

        // Treat an elapsed temporary suspension/restriction as cleared for the purposes of this
        // request; the expiry background service will persist the cleared state shortly.
        var effectiveLevel = snapshot.Level;
        if ((effectiveLevel == UserModerationLevel.Suspended || effectiveLevel == UserModerationLevel.Restricted)
            && snapshot.ExpiresAtUtc.HasValue
            && snapshot.ExpiresAtUtc.Value <= DateTime.UtcNow)
        {
            effectiveLevel = UserModerationLevel.None;
        }

        if (effectiveLevel == UserModerationLevel.None || effectiveLevel == UserModerationLevel.Warned)
        {
            await this._next(context);
            return;
        }

        if (effectiveLevel == UserModerationLevel.Restricted)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            var isWrite = IsWriteVerb(context.Request.Method);
            if (!isWrite || RestrictedAllowList.Contains(path))
            {
                await this._next(context);
                return;
            }
        }

        await WriteModerationForbiddenAsync(context, snapshot, effectiveLevel);
    }

    private static bool IsWriteVerb(string method) =>
        HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsPatch(method) || HttpMethods.IsDelete(method);

    private static async Task<ModerationSnapshot> LoadSnapshotAsync(
        UserManager<ApplicationUser> userManager, IDistributedCache cache, string userId)
    {
        var cached = await cache.GetAsync(UserModerationCacheKey(userId));
        if (cached is not null && ModerationSnapshot.TryDecode(cached, out var fromCache))
            return fromCache;

        var user = await userManager.FindByIdAsync(userId);
        var snapshot = user is null
            ? ModerationSnapshot.None
            : new ModerationSnapshot(user.ModerationLevel, user.ModerationReason, user.ModerationPublicReason, user.ModerationExpiresAt);

        await cache.SetAsync(
            UserModerationCacheKey(userId),
            snapshot.Encode(),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheTtl });

        return snapshot;
    }

    private static async Task WriteModerationForbiddenAsync(HttpContext context, ModerationSnapshot snapshot, UserModerationLevel effectiveLevel)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/json";
        var payload = new
        {
            error = "Your account has a moderation action in effect.",
            moderation = new
            {
                level = effectiveLevel.ToString(),
                reason = snapshot.Reason.ToString(),
                publicReason = snapshot.PublicReason ?? string.Empty,
                expiresAt = snapshot.ExpiresAtUtc
            }
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }

    /// <summary>
    /// Compact serialisable snapshot of the per-user moderation state.
    /// Encoded as JSON in <see cref="IDistributedCache"/> rather than a binary struct so that
    /// schema evolution (extra fields) does not invalidate the cached entries.
    /// </summary>
    private readonly struct ModerationSnapshot
    {
        public static readonly ModerationSnapshot None = new(UserModerationLevel.None, ModerationReason.Unspecified, null, null);

        public UserModerationLevel Level { get; }
        public ModerationReason Reason { get; }
        public string? PublicReason { get; }
        public DateTime? ExpiresAtUtc { get; }

        public ModerationSnapshot(UserModerationLevel level, ModerationReason reason, string? publicReason, DateTime? expiresAtUtc)
        {
            this.Level = level;
            this.Reason = reason;
            this.PublicReason = publicReason;
            this.ExpiresAtUtc = expiresAtUtc;
        }

        public byte[] Encode()
        {
            var dto = new CachedModerationDto((byte)this.Level, (byte)this.Reason, this.PublicReason, this.ExpiresAtUtc);
            return JsonSerializer.SerializeToUtf8Bytes(dto);
        }

        public static bool TryDecode(byte[] bytes, out ModerationSnapshot snapshot)
        {
            try
            {
                var dto = JsonSerializer.Deserialize<CachedModerationDto>(bytes);
                if (dto is null)
                {
                    snapshot = None;
                    return false;
                }
                snapshot = new ModerationSnapshot((UserModerationLevel)dto.L, (ModerationReason)dto.R, dto.P, dto.E);
                return true;
            }
            catch
            {
                snapshot = None;
                return false;
            }
        }

        private sealed record CachedModerationDto(byte L, byte R, string? P, DateTime? E);
    }
}
