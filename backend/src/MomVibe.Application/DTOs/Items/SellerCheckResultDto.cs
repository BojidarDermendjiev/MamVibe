namespace MomVibe.Application.DTOs.Items;

/// <summary>
/// Safe projection of a seller reputation check result returned to API consumers.
/// PII fields present on the raw <c>NekorektenReport</c> (Phone, Email, FirstName, LastName)
/// are intentionally excluded; only the report narrative and engagement metadata are exposed.
/// </summary>
public sealed class SellerCheckResultDto
{
    /// <summary>Gets a value indicating whether any fraud reports were found for this seller.</summary>
    public bool HasReports { get; init; }
    /// <summary>Gets the total number of fraud reports found.</summary>
    public int ReportCount { get; init; }
    /// <summary>Gets the list of sanitized fraud report entries safe for public display.</summary>
    public List<SellerCheckReportDto> Reports { get; init; } = [];
    /// <summary>Gets a value indicating whether the nekorekten.com API was unreachable during the check.</summary>
    public bool ServiceUnavailable { get; init; }
}

/// <summary>
/// A single fraud report entry safe for public consumption.
/// </summary>
public sealed class SellerCheckReportDto
{
    /// <summary>Gets the free-text description of the reported incident.</summary>
    public string? Text { get; init; }
    /// <summary>Gets the number of community up-votes this report has received.</summary>
    public int Likes { get; init; }
    /// <summary>Gets the date and time the report was submitted, if available.</summary>
    public DateTime? CreatedAt { get; init; }
}
