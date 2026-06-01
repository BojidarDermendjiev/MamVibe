namespace MomVibe.Infrastructure.EventHandlers;

using System.Text.Json;

using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Application.Events;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Configuration;

/// <summary>
/// On <see cref="UserRegisteredEvent"/>, queues the n8n <c>user.registered</c> webhook through
/// the transactional outbox. The user's email is masked in the payload so the webhook body
/// (forwarded to downstream automations) doesn't leak PII.
/// </summary>
public sealed class UserRegisteredN8nHandler : INotificationHandler<UserRegisteredEvent>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _context;
    private readonly IOutboxWriter _outbox;
    private readonly N8nSettings _n8nSettings;
    private readonly ILogger<UserRegisteredN8nHandler> _logger;

    public UserRegisteredN8nHandler(
        UserManager<ApplicationUser> userManager,
        IApplicationDbContext context,
        IOutboxWriter outbox,
        IOptions<N8nSettings> n8nSettings,
        ILogger<UserRegisteredN8nHandler> logger)
    {
        this._userManager = userManager;
        this._context = context;
        this._outbox = outbox;
        this._n8nSettings = n8nSettings.Value;
        this._logger = logger;
    }

    public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var user = await this._userManager.FindByIdAsync(notification.UserId);
            if (user is null) return;

            var body = new
            {
                Event = "user.registered",
                Timestamp = DateTime.UtcNow,
                Email = MaskEmail(user.Email),
                user.DisplayName,
                ProfileType = user.ProfileType.ToString(),
                user.LanguagePreference
            };

            this._outbox.Enqueue(OutboxMessageTypes.N8nWebhook, new N8nWebhookOutboxPayload(
                this._n8nSettings.UserRegistered,
                JsonSerializer.Serialize(body, JsonOptions)));
            await this._context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            this._logger.LogWarning(ex, "Failed to enqueue n8n user.registered for user {UserId}", notification.UserId);
        }
    }

    private static string MaskEmail(string? email)
    {
        if (string.IsNullOrEmpty(email)) return "***";
        var at = email.IndexOf('@');
        if (at <= 0) return "***";
        var local = email[..at];
        var domain = email[at..];
        return (local.Length <= 2 ? "***" : local[..2] + "***") + domain;
    }
}
