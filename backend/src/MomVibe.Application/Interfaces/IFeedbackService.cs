namespace MomVibe.Application.Interfaces;

using DTOs.Common;
using DTOs.Feedbacks;

/// <summary>
/// Feedback service contract for:
/// - Paginated retrieval of feedback entries.
/// - Creating feedback authored by a specific user.
/// - Deleting feedback, restricted to the owner or allowed for admins via the isAdmin flag.
/// </summary>
public interface IFeedbackService
{
    /// <summary>Returns a paginated list of all feedback entries, newest first.</summary>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Number of results per page.</param>
    Task<PagedResult<FeedbackDto>> GetAllAsync(int page = 1, int pageSize = 10);

    /// <summary>Creates a new feedback entry on behalf of the specified user.</summary>
    /// <param name="dto">The feedback data provided by the user.</param>
    /// <param name="userId">The identifier of the user submitting the feedback.</param>
    Task<FeedbackDto> CreateAsync(CreateFeedbackDto dto, string userId);

    /// <summary>Deletes a feedback entry. Admins may delete any entry; regular users may only delete their own.</summary>
    /// <param name="id">The unique identifier of the feedback to delete.</param>
    /// <param name="userId">The identifier of the requesting user.</param>
    /// <param name="isAdmin">When <c>true</c>, bypasses ownership checks.</param>
    Task DeleteAsync(Guid id, string userId, bool isAdmin = false);
}
