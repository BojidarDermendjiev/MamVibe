namespace MomVibe.WebApi.Middleware;

/// <summary>
/// Middleware that appends common security headers to every HTTP response:
/// - X-Content-Type-Options=nosniff
/// - X-Frame-Options=DENY
/// - X-XSS-Protection=1; mode=block
/// - Referrer-Policy=strict-origin-when-cross-origin
/// - Permissions-Policy=camera=(), microphone=(), geolocation=()
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityHeadersMiddleware"/>.
    /// </summary>
    /// <param name="next">The next request delegate in the pipeline.</param>
    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        this._next = next;
    }

    /// <summary>
    /// Appends security headers to the response and continues the middleware pipeline.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");

        await this._next(context);
    }
}
