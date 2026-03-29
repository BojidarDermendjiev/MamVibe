namespace MomVibe.Infrastructure.Services;

using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

using Application.Interfaces;
using Infrastructure.Configuration;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _settings;

    public EmailService(IOptions<SmtpSettings> settings)
    {
        this._settings = settings.Value;
    }

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
