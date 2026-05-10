namespace MomVibe.WebApi.Middleware;

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

using MomVibe.Domain.Entities;

/// <summary>
/// Middleware that blocks access for users whose account has been blocked.
/// Checks the database (via UserManager) so that blocking takes effect immediately
/// without waiting for the user's JWT to expire.
/// Results are cached per-user for up to 1 minute to avoid a DB hit on every request.
/// The cache is explicitly evicted when an admin blocks or unblocks a user.
/// </summary>
public class BlockedUserMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>Returns the IMemoryCache key used to store the blocked-status for a given user.</summary>
    /// <param name="userId">The user identifier.</param>
    public static string CacheKey(string userId) => $"blocked:{userId}";

    /// <summary>Initializes a new instance of <see cref="BlockedUserMiddleware"/> with the next middleware delegate.</summary>
    public BlockedUserMiddleware(RequestDelegate next)
    {
        this._next = next;
    }

    /// <summary>Checks whether the authenticated user is blocked and short-circuits with 403 if so; otherwise forwards the request.</summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="userManager">The Identity UserManager used to look up the user's blocked status.</param>
    /// <param name="cache">The memory cache used to avoid a DB hit on every request.</param>
    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager, IMemoryCache cache)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                if (!cache.TryGetValue(CacheKey(userId), out bool isBlocked))
                {
                    var user = await userManager.FindByIdAsync(userId);
                    isBlocked = user?.IsBlocked ?? false;
                    cache.Set(CacheKey(userId), isBlocked, TimeSpan.FromMinutes(1));
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
