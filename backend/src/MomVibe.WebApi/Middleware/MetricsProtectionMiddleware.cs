namespace MomVibe.WebApi.Middleware;

using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

/// <summary>
/// Blocks external access to the /metrics endpoint by returning 404 for any caller
/// whose IP is not localhost or within Docker's private address ranges.
/// Prometheus already runs on the same internal Docker network (no external port binding),
/// so legitimate scrapes always pass; public clients never see the endpoint exists.
/// </summary>
public class MetricsProtectionMiddleware(RequestDelegate next)
{
    private static readonly IPNetwork[] _internalNetworks =
    [
        new IPNetwork(IPAddress.Parse("172.16.0.0"), 12),
        new IPNetwork(IPAddress.Parse("10.0.0.0"), 8),
        new IPNetwork(IPAddress.Parse("192.168.0.0"), 16),
    ];

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/metrics"))
        {
            var ip = context.Connection.RemoteIpAddress;
            var allowed = ip != null && (
                IPAddress.IsLoopback(ip) ||
                _internalNetworks.Any(n => n.Contains(ip)));

            if (!allowed)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }
        }

        await next(context);
    }
}
