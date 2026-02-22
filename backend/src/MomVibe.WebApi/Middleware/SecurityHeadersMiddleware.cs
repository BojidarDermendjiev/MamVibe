namespace MomVibe.WebApi.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        this._next = next;
    }

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
