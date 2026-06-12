namespace MomVibe.WebApi.Middleware;

/// <summary>
/// Middleware that appends OWASP-recommended security headers to every HTTP response:
/// X-Content-Type-Options, X-Frame-Options, X-XSS-Protection, Referrer-Policy,
/// Permissions-Policy, Content-Security-Policy, cross-origin isolation headers, and
/// (in non-development environments) Strict-Transport-Security.
/// </summary>
/// <remarks>
/// HSTS used to live as an inline middleware in <c>StartUp.cs</c>; consolidating it here
/// means there is exactly one place that owns the security-header surface.
/// </remarks>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly bool _emitHsts;

    /// <summary>Initializes a new instance of <see cref="SecurityHeadersMiddleware"/>.</summary>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="env">Hosting environment — used to suppress HSTS in development so
    /// developers can hit <c>http://localhost</c> without being upgraded.</param>
    public SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment env)
    {
        this._next = next;
        this._emitHsts = !env.IsDevelopment();
    }

    /// <summary>Appends all security headers to the response and forwards the request to the next middleware.</summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers.Append("X-Content-Type-Options", "nosniff");
        headers.Append("X-Frame-Options", "DENY");
        headers.Append("X-XSS-Protection", "0");
        headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=(), payment=()");
        headers.Append("Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self' https://challenges.cloudflare.com https://js.stripe.com https://accounts.google.com; " +
            "style-src 'self'; " +
            "img-src 'self' data: blob: https:; " +
            "font-src 'self'; " +
            "connect-src 'self' wss: https://api.stripe.com https://challenges.cloudflare.com; " +
            "frame-src https://challenges.cloudflare.com https://js.stripe.com https://accounts.google.com; " +
            "object-src 'none'; " +
            "base-uri 'self'; " +
            "form-action 'self'; " +
            "upgrade-insecure-requests");
        headers.Append("Cross-Origin-Opener-Policy", "same-origin-allow-popups");
        headers.Append("Cross-Origin-Resource-Policy", "same-origin");
        headers.Append("X-Permitted-Cross-Domain-Policies", "none");

        if (this._emitHsts)
        {
            // 1 year, includeSubDomains, preload — matches the inline middleware this replaced.
            headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
        }

        await this._next(context);
    }
}
