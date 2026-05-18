namespace MomVibe.WebApi.Middleware;

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;

using MomVibe.Domain.Entities;

/// <summary>
/// Middleware that blocks access for users whose account has been blocked.
/// Checks the database (via UserManager) so that blocking takes effect immediately
/// without waiting for the user's JWT to expire.
/// Results are cached per-user for up to 1 minute to avoid a DB hit on every request.
/// The cache is explicitly evicted when an admin blocks or unblocks a user.
/// Uses IDistributedCache so the check works correctly across multiple API instances.
/// </summary>
public class BlockedUserMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>Returns the cache key used to store the blocked-status for a given user.</summary>
    public static string CacheKey(string userId) => $"blocked:{userId}";

    /// <summary>Initializes a new instance of <see cref="BlockedUserMiddleware"/> with the next middleware delegate.</summary>
    public BlockedUserMiddleware(RequestDelegate next)
    {
        this._next = next;
    }

    /// <summary>Checks whether the authenticated user is blocked and short-circuits with 403 if so; otherwise forwards the request.</summary>
    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager, IDistributedCache cache)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                var cached = await cache.GetAsync(CacheKey(userId));
                bool isBlocked;

                if (cached is null)
                {
                    var user = await userManager.FindByIdAsync(userId);
                    isBlocked = user?.IsBlocked ?? false;
                    var entry = new byte[] { isBlocked ? (byte)1 : (byte)0 };
                    await cache.SetAsync(CacheKey(userId), entry,
                        new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) });
                }
                else
                {
                    isBlocked = cached[0] == 1;
                }

                if (isBlocked)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new { error = "Your account has been blocked." });
                    return;
                }
            }
        }

        await this._next(context);
    }
}
