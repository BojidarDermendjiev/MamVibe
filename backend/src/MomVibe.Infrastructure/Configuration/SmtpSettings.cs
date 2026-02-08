namespace MomVibe.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for SMTP email server integration.
/// Bound from the "Smtp" section in appsettings.json.
/// </summary>
public class SmtpSettings
{
    /// <summary>
    /// SMTP server host address (e.g., smtp.gmail.com).
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// SMTP server port number (typically 25, 465, or 587).
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Username for SMTP server authentication.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password for SMTP server authentication.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Email address to use as the sender in outgoing emails.
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>
    /// Display name to use as the sender in outgoing emails.
    /// </summary>
    public string FromName { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether to use SSL/TLS encryption for SMTP connection.
    /// Defaults to true for secure communication.
    /// </summary>
    public bool EnableSsl { get; set; } = true;
}
