namespace MomVibe.WebApi.Middleware;

/// <summary>
/// Middleware that blocks access for authenticated users marked as blocked via the <c>"IsBlocked"</c> claim.
/// If the claim value is <c>"true"</c>, responds with 403 Forbidden and a JSON error; otherwise continues the pipeline.
/// </summary>
public class BlockedUserMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockedUserMiddleware"/>.
    /// </summary>
    /// <param name="next">The next request delegate in the pipeline.</param>
    public BlockedUserMiddleware(RequestDelegate next)
    {
        this._next = next;
    }

    /// <summary>
    /// Intercepts the request and denies access for blocked authenticated users.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var isBlocked = context.User.FindFirst("IsBlocked")?.Value;
            if (isBlocked == "true")
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { error = "Your account has been blocked." });
                return;
            }
        }

        await this._next(context);
    }
}
