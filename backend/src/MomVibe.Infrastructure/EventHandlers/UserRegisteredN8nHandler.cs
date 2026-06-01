namespace MomVibe.Infrastructure.EventHandlers;

using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Application.Events;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Configuration;

/// <summary>
/// On <see cref="UserRegisteredEvent"/>, fires the n8n <c>user.registered</c> webhook
/// (Slack alert, CRM sync, etc.). The user's email is masked in the payload to avoid
/// exposing PII to downstream automations.
/// </summary>
public sealed class UserRegisteredN8nHandler : INotificationHandler<UserRegisteredEvent>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IN8nWebhookService _webhook;
    private readonly N8nSettings _n8nSettings;
    private readonly ILogger<UserRegisteredN8nHandler> _logger;

    public UserRegisteredN8nHandler(
        UserManager<ApplicationUser> userManager,
        IN8nWebhookService webhook,
        IOptions<N8nSettings> n8nSettings,
        ILogger<UserRegisteredN8nHandler> logger)
    {
        this._userManager = userManager;
        this._webhook = webhook;
        this._n8nSettings = n8nSettings.Value;
        this._logger = logger;
    }

    public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var user = await this._userManager.FindByIdAsync(notification.UserId);
            if (user is null) return;

            this._webhook.Send(this._n8nSettings.UserRegistered, new
            {
                Event = "user.registered",
                Timestamp = DateTime.UtcNow,
                Email = MaskEmail(user.Email),
                user.DisplayName,
                ProfileType = user.ProfileType.ToString(),
                user.LanguagePreference
            });
        }
        catch (Exception ex)
        {
            this._logger.LogWarning(ex, "n8n user.registered webhook failed for user {UserId}", notification.UserId);
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
