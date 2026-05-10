namespace MomVibe.WebApi.Middleware;

/// <summary>
/// Middleware that appends OWASP-recommended security headers to every HTTP response:
/// X-Content-Type-Options, X-Frame-Options, X-XSS-Protection, Referrer-Policy,
/// Permissions-Policy, Content-Security-Policy, and cross-origin isolation headers.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>Initializes a new instance of <see cref="SecurityHeadersMiddleware"/> with the next middleware delegate.</summary>
    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        this._next = next;
    }

    /// <summary>Appends all security headers to the response and forwards the request to the next middleware.</summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "0");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=(), payment=()");
        context.Response.Headers.Append("Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self' https://challenges.cloudflare.com https://js.stripe.com https://accounts.google.com; " +
            "style-src 'self'; " +
            "img-src 'self' data: blob: https:; " +
            "font-src 'self'; " +
            "connect-src 'self' https://api.stripe.com https://challenges.cloudflare.com; " +
            "frame-src https://challenges.cloudflare.com https://js.stripe.com https://accounts.google.com; " +
            "object-src 'none'; " +
            "base-uri 'self'; " +
            "form-action 'self'");
        context.Response.Headers.Append("Cross-Origin-Opener-Policy", "same-origin-allow-popups");
        context.Response.Headers.Append("Cross-Origin-Resource-Policy", "same-origin");
        context.Response.Headers.Append("X-Permitted-Cross-Domain-Policies", "none");

        await this._next(context);
    }
}
