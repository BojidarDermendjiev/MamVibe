namespace MomVibe.Application.Interfaces;

/// <summary>
/// Checks a buyer's reputation against the nekorekten.com fraud-report database.
/// </summary>
public interface INekorektenService
{
    /// <summary>
    /// Searches for reports matching any of the supplied identifiers (name, email, phone).
    /// Returns <see cref="BuyerCheckResult.ServiceUnavailable"/> = true if the external API
    /// cannot be reached, so callers can degrade gracefully without blocking the purchase flow.
    /// </summary>
    Task<BuyerCheckResult> CheckAsync(string? name, string? email, string? phone);
}

/// <summary>Result of a buyer reputation check.</summary>
public sealed class BuyerCheckResult
{
    /// <summary>Gets a value indicating whether any fraud reports were found for the queried identifiers.</summary>
    public bool HasReports { get; init; }
    /// <summary>Gets the total number of fraud reports found.</summary>
    public int ReportCount { get; init; }
    /// <summary>Gets the list of individual fraud reports returned by the nekorekten.com API.</summary>
    public List<NekorektenReport> Reports { get; init; } = [];
    /// <summary>True when the external API returned an error or was unreachable.</summary>
    public bool ServiceUnavailable { get; init; }
}

/// <summary>A single fraud report from nekorekten.com.</summary>
public sealed class NekorektenReport
{
    /// <summary>Gets the free-text description of the reported incident.</summary>
    public string? Text { get; init; }
    /// <summary>Gets the phone number associated with the report, if provided.</summary>
    public string? Phone { get; init; }
    /// <summary>Gets the email address associated with the report, if provided.</summary>
    public string? Email { get; init; }
    /// <summary>Gets the first name of the reported person, if provided.</summary>
    public string? FirstName { get; init; }
    /// <summary>Gets the last name of the reported person, if provided.</summary>
    public string? LastName { get; init; }
    /// <summary>Gets the number of community up-votes this report has received.</summary>
    public int Likes { get; init; }
    /// <summary>Gets the date and time the report was submitted, if available.</summary>
    public DateTime? CreatedAt { get; init; }
}
