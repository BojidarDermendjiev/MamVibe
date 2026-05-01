namespace MomVibe.Application.DTOs.Items;

/// <summary>
/// Safe projection of a seller reputation check result returned to API consumers.
/// PII fields present on the raw <c>NekorektenReport</c> (Phone, Email, FirstName, LastName)
/// are intentionally excluded; only the report narrative and engagement metadata are exposed.
/// </summary>
public sealed class SellerCheckResultDto
{
    public bool HasReports { get; init; }
    public int ReportCount { get; init; }
    public List<SellerCheckReportDto> Reports { get; init; } = [];
    public bool ServiceUnavailable { get; init; }
}

/// <summary>
/// A single fraud report entry safe for public consumption.
/// </summary>
public sealed class SellerCheckReportDto
{
    public string? Text { get; init; }
    public int Likes { get; init; }
    public DateTime? CreatedAt { get; init; }
}
