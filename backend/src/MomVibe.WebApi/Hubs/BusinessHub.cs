namespace MomVibe.WebApi.Hubs;

using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

/// <summary>
/// Authenticated SignalR hub for the business owner's live dashboard. Each connection joins
/// a per-user group (<c>business_{userId}</c>) on connect — view / like / comment / subscription
/// deltas are pushed to that group by <c>SignalRBusinessRealtimeNotifier</c>.
/// </summary>
[Authorize]
public class BusinessHub : Hub<IBusinessClient>
{
    private readonly ILogger<BusinessHub> _logger;

    public BusinessHub(ILogger<BusinessHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupFor(userId));
            _logger.LogDebug("BusinessHub: {UserId} connected ({ConnectionId})", userId, Context.ConnectionId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupFor(userId));
        await base.OnDisconnectedAsync(exception);
    }

    internal static string GroupFor(string userId) => $"business_{userId}";
}
