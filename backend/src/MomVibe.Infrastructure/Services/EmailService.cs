namespace MomVibe.Infrastructure.Services;

using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

using Application.Interfaces;
using Infrastructure.Configuration;

/// <summary>
/// SMTP-based implementation of <see cref="IEmailService"/> that sends transactional emails
/// using the configured SMTP server credentials.
/// </summary>
public class EmailService : IEmailService
{
    private readonly SmtpSettings _settings;

    /// <summary>
    /// Initializes a new instance of <see cref="EmailService"/> with the given SMTP settings.
    /// </summary>
    /// <param name="settings">The bound SMTP configuration options.</param>
    public EmailService(IOptions<SmtpSettings> settings)
    {
        this._settings = settings.Value;
    }

    /// <inheritdoc/>
    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        using var message = new MailMessage();
        message.From = new MailAddress(this._settings.FromEmail, this._settings.FromName);
        message.To.Add(to);
        message.Subject = subject;
        message.Body = htmlBody;
        message.IsBodyHtml = true;

        using var client = new SmtpClient(this._settings.Host, this._settings.Port);
        client.Credentials = new NetworkCredential(this._settings.Username, this._settings.Password);
        client.EnableSsl = this._settings.EnableSsl;

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await client.SendMailAsync(message, cts.Token);
    }
}
