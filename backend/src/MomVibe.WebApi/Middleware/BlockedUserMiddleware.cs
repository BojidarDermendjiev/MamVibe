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

    public static string CacheKey(string userId) => $"blocked:{userId}";

    public BlockedUserMiddleware(RequestDelegate next)
    {
        this._next = next;
    }

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
