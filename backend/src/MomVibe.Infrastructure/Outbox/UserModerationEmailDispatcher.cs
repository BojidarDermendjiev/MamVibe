namespace MomVibe.Infrastructure.Outbox;

using System.Text.Json;

using Application.Interfaces;
using Domain.Entities;
using Infrastructure.EmailTemplates;

/// <summary>
/// Dispatcher for <see cref="OutboxMessageTypes.UserModerationEmail"/> outbox messages.
/// Renders the locale-appropriate template and sends via <see cref="IEmailService"/>.
/// Failures bubble out so <c>OutboxProcessor</c> applies its retry + backoff policy.
/// </summary>
public sealed class UserModerationEmailDispatcher : IOutboxMessageDispatcher
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IEmailService _email;

    public UserModerationEmailDispatcher(IEmailService email)
    {
        this._email = email;
    }

    public string MessageType => OutboxMessageTypes.UserModerationEmail;

    public async Task DispatchAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Deserialize<UserModerationEmailOutboxPayload>(message.Payload, JsonOptions)
            ?? throw new InvalidOperationException("Outbox payload could not be deserialised as UserModerationEmailOutboxPayload.");

        if (string.IsNullOrWhiteSpace(payload.ToEmail))
            return;

        var rendered = ModerationEmails.Render(
            templateKey: payload.TemplateKey,
            locale: payload.Locale,
            displayName: payload.DisplayName,
            publicReason: payload.PublicReason,
            expiresAtUtc: payload.ExpiresAtUtc);

        await this._email.SendEmailAsync(payload.ToEmail, rendered.Subject, rendered.HtmlBody);
    }
}
