namespace MomVibe.WebApi.Authorization;

using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;

using MomVibe.Domain.Entities;
using MomVibe.Domain.Enums;
using MomVibe.WebApi.Middleware;

/// <summary>
/// Authorization requirement representing "user is allowed to perform write actions".
/// Blocked by Restricted, Suspended, and Banned moderation levels.
/// </summary>
public sealed class WritePermittedRequirement : IAuthorizationRequirement
{
}

/// <summary>
/// Reads the per-user moderation snapshot from <see cref="IDistributedCache"/> (same key the
/// <see cref="UserModerationMiddleware"/> populates) and succeeds only when the user is at
/// <see cref="UserModerationLevel.None"/> or <see cref="UserModerationLevel.Warned"/>.
/// </summary>
public sealed class WritePermittedHandler : AuthorizationHandler<WritePermittedRequirement>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDistributedCache _cache;

    public WritePermittedHandler(UserManager<ApplicationUser> userManager, IDistributedCache cache)
    {
        this._userManager = userManager;
        this._cache = cache;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, WritePermittedRequirement requirement)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return;

        // Read the same cache key the middleware writes; on miss, hit the DB. This is the only
        // safe place to load the level for SignalR Hub method authorization, where the HTTP
        // pipeline middleware doesn't intercept individual invocations.
        UserModerationLevel level;
        DateTime? expiresAt;

        var cached = await this._cache.GetAsync(UserModerationMiddleware.UserModerationCacheKey(userId));
        if (cached is null)
        {
            var user = await this._userManager.FindByIdAsync(userId);
            if (user is null) return;
            level = user.ModerationLevel;
            expiresAt = user.ModerationExpiresAt;
        }
        else
        {
            try
            {
                var dto = System.Text.Json.JsonSerializer.Deserialize<CachedShape>(cached);
                if (dto is null) return;
                level = (UserModerationLevel)dto.L;
                expiresAt = dto.E;
            }
            catch
            {
                return;
            }
        }

        // Mirror the middleware's expiry treatment so the policy and the middleware agree.
        if ((level == UserModerationLevel.Suspended || level == UserModerationLevel.Restricted)
            && expiresAt.HasValue && expiresAt.Value <= DateTime.UtcNow)
        {
            level = UserModerationLevel.None;
        }

        if (level == UserModerationLevel.None || level == UserModerationLevel.Warned)
            context.Succeed(requirement);
    }

    private sealed record CachedShape(byte L, byte R, string? P, DateTime? E);
}
