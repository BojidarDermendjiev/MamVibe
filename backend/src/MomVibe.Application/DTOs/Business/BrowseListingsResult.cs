namespace MomVibe.Application.DTOs.Business;

/// <summary>
/// Paged result for the public browse endpoint. <see cref="Featured"/> is an interleave
/// of up to 2 top-tier listings rendered at the top of page 1 (empty on later pages).
/// </summary>
public class BrowseListingsResult
{
    public IEnumerable<BusinessListingSummaryDto> Featured { get; set; } = [];
    public IEnumerable<BusinessListingSummaryDto> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}
