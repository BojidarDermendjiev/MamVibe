namespace MomVibe.Application.DTOs.Business;

/// <summary>
/// Read projection of a <c>BusinessListingComment</c> returned by the comments endpoint.
/// Hidden comments are dropped for anonymous / parent callers; admins see them with the
/// <see cref="HiddenReason"/> populated for moderation context.
/// </summary>
public class BusinessListingCommentDto
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string AuthorDisplayName { get; set; } = string.Empty;
    public string? AuthorAvatarUrl { get; set; }
    public string Body { get; set; } = string.Empty;
    public Guid? ParentCommentId { get; set; }
    public bool IsHidden { get; set; }
    public string? HiddenReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Wrapper for paged comment listings.</summary>
public class PagedCommentsResult
{
    public IEnumerable<BusinessListingCommentDto> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}
