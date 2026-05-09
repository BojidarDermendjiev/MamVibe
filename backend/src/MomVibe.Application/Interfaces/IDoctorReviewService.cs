namespace MomVibe.Application.Interfaces;

using DTOs.DoctorReviews;

/// <summary>
/// Defines operations for managing doctor reviews, including retrieval,
/// submission, moderation, and deletion.
/// </summary>
public interface IDoctorReviewService
{
    /// <summary>
    /// Retrieves a paginated list of approved doctor reviews matching the given filters.
    /// </summary>
    /// <param name="city">Optional city filter (case-insensitive substring match).</param>
    /// <param name="specialization">Optional medical specialization filter (case-insensitive substring match).</param>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Number of results per page.</param>
    /// <returns>A collection of approved <see cref="DoctorReviewDto"/> instances.</returns>
    Task<IEnumerable<DoctorReviewDto>> GetAllAsync(string? city = null, string? specialization = null, int page = 1, int pageSize = 20);

    /// <summary>
    /// Retrieves a single doctor review by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the review.</param>
    /// <returns>The matching <see cref="DoctorReviewDto"/>, or <c>null</c> if not found.</returns>
    Task<DoctorReviewDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// Retrieves all doctor reviews submitted by a specific user.
    /// </summary>
    /// <param name="userId">The identifier of the user whose reviews to retrieve.</param>
    /// <returns>A collection of <see cref="DoctorReviewDto"/> instances belonging to the user.</returns>
    Task<IEnumerable<DoctorReviewDto>> GetByUserAsync(string userId);

    /// <summary>
    /// Retrieves all doctor reviews that are awaiting administrator approval.
    /// </summary>
    /// <returns>A collection of pending <see cref="DoctorReviewDto"/> instances.</returns>
    Task<IEnumerable<DoctorReviewDto>> GetPendingAsync();

    /// <summary>
    /// Creates a new doctor review on behalf of the specified user.
    /// The review is initially unapproved and requires moderation.
    /// </summary>
    /// <param name="userId">The identifier of the submitting user.</param>
    /// <param name="dto">The review data provided by the user.</param>
    /// <returns>The newly created <see cref="DoctorReviewDto"/>.</returns>
    Task<DoctorReviewDto> CreateAsync(string userId, CreateDoctorReviewDto dto);

    /// <summary>
    /// Marks a pending doctor review as approved, making it publicly visible.
    /// </summary>
    /// <param name="id">The unique identifier of the review to approve.</param>
    Task ApproveAsync(Guid id);

    /// <summary>
    /// Deletes a doctor review. Administrators may delete any review; regular users
    /// may only delete their own submissions.
    /// </summary>
    /// <param name="id">The unique identifier of the review to delete.</param>
    /// <param name="userId">The identifier of the requesting user.</param>
    /// <param name="isAdmin">When <c>true</c>, bypasses ownership checks.</param>
    Task DeleteAsync(Guid id, string userId, bool isAdmin = false);
}
