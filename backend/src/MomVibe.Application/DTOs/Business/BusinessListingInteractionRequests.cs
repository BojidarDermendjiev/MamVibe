namespace MomVibe.Application.DTOs.Business;

using Domain.Enums;

/// <summary>Request body when a parent posts a comment under a listing.</summary>
public class CreateBusinessListingCommentRequest
{
    public string Body { get; set; } = string.Empty;
    public Guid? ParentCommentId { get; set; }
}

/// <summary>Request body when a parent reports a listing for policy violation.</summary>
public class ReportBusinessListingRequest
{
    public ModerationReason Reason { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>Admin payload when hiding a comment.</summary>
public class HideBusinessListingCommentRequest
{
    public string Reason { get; set; } = string.Empty;
}

/// <summary>Response payload from like/unlike — lets the client render the new heart state without a re-fetch.</summary>
public class ListingLikeStateDto
{
    public bool IsLiked { get; set; }
    public long LikeCount { get; set; }
}
