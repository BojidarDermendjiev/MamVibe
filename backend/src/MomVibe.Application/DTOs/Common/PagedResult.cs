namespace MomVibe.Application.DTOs.Common;

/// <summary>
/// Generic pagination result container:
/// - Items: list of items for the current page.
/// - TotalCount: total number of items across all pages.
/// - Page: current page number (1-based).
/// - PageSize: number of items per page.
/// - TotalPages: ceiling of TotalCount / PageSize.
/// - HasPreviousPage / HasNextPage: navigation helpers.
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
