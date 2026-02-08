namespace MomVibe.Application.Interfaces;

/// <summary>
/// Service interface for sending email messages.
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
}
