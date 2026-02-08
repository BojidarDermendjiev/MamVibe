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
    Task<PagedResult<FeedbackDto>> GetAllAsync(int page = 1, int pageSize = 10);
    Task<FeedbackDto> CreateAsync(CreateFeedbackDto dto, string userId);
    Task DeleteAsync(Guid id, string userId, bool isAdmin = false);
}
