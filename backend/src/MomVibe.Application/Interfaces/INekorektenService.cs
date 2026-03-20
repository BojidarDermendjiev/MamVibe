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
    public bool HasReports { get; init; }
    public int ReportCount { get; init; }
    public List<NekorektenReport> Reports { get; init; } = [];
    /// <summary>True when the external API returned an error or was unreachable.</summary>
    public bool ServiceUnavailable { get; init; }
}

/// <summary>A single fraud report from nekorekten.com.</summary>
public sealed class NekorektenReport
{
    public string? Text { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public int Likes { get; init; }
    public DateTime? CreatedAt { get; init; }
}
