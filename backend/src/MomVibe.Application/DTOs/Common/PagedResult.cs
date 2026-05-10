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
    /// <summary>Gets or sets the list of items for the current page.</summary>
    public List<T> Items { get; set; } = [];

    /// <summary>Gets or sets the total number of matching items across all pages.</summary>
    public int TotalCount { get; set; }

    /// <summary>Gets or sets the current 1-based page number.</summary>
    public int Page { get; set; }

    /// <summary>Gets or sets the maximum number of items returned per page.</summary>
    public int PageSize { get; set; }

    /// <summary>Gets the total number of pages, calculated as the ceiling of <see cref="TotalCount"/> / <see cref="PageSize"/>.</summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>Gets a value indicating whether there is a page before the current one.</summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>Gets a value indicating whether there is a page after the current one.</summary>
    public bool HasNextPage => Page < TotalPages;
}
