namespace MomVibe.Application.Interfaces;

/// <summary>
/// Service interface for sending email messages.
/// </summary>
public interface IEmailService
{
    /// <summary>Sends an HTML email to the specified recipient.</summary>
    /// <param name="to">The recipient email address.</param>
    /// <param name="subject">The email subject line.</param>
    /// <param name="htmlBody">The HTML-formatted email body.</param>
    Task SendEmailAsync(string to, string subject, string htmlBody);
}
