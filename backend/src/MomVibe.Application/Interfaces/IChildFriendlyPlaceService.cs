namespace MomVibe.Application.Interfaces;

using DTOs.ChildFriendlyPlaces;
using Domain.Enums;

/// <summary>
/// Defines operations for managing child-friendly place submissions,
/// including retrieval, creation, moderation, and deletion.
/// </summary>
public interface IChildFriendlyPlaceService
{
    /// <summary>
    /// Retrieves a paginated list of approved child-friendly places matching the given filters.
    /// </summary>
    /// <param name="city">Optional city name filter (case-insensitive substring match).</param>
    /// <param name="placeType">Optional place type filter.</param>
    /// <param name="maxAgeMonths">Optional maximum child age in months; excludes places whose upper age limit is below this value.</param>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Number of results per page.</param>
    /// <returns>A collection of approved <see cref="ChildFriendlyPlaceDto"/> instances.</returns>
    Task<IEnumerable<ChildFriendlyPlaceDto>> GetAllAsync(string? city = null, PlaceType? placeType = null, int? maxAgeMonths = null, int page = 1, int pageSize = 20);

    /// <summary>
    /// Retrieves a single child-friendly place by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the place.</param>
    /// <returns>The matching <see cref="ChildFriendlyPlaceDto"/>, or <c>null</c> if not found.</returns>
    Task<ChildFriendlyPlaceDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// Retrieves all child-friendly places that are awaiting administrator approval.
    /// </summary>
    /// <returns>A collection of pending <see cref="ChildFriendlyPlaceDto"/> instances.</returns>
    Task<IEnumerable<ChildFriendlyPlaceDto>> GetPendingAsync();

    /// <summary>
    /// Creates a new child-friendly place submission on behalf of the specified user.
    /// The submission is initially unapproved and requires moderation.
    /// </summary>
    /// <param name="userId">The identifier of the submitting user.</param>
    /// <param name="dto">The place data provided by the user.</param>
    /// <returns>The newly created <see cref="ChildFriendlyPlaceDto"/>.</returns>
    Task<ChildFriendlyPlaceDto> CreateAsync(string userId, CreateChildFriendlyPlaceDto dto);

    /// <summary>
    /// Marks a pending child-friendly place as approved, making it publicly visible.
    /// </summary>
    /// <param name="id">The unique identifier of the place to approve.</param>
    Task ApproveAsync(Guid id);

    /// <summary>
    /// Deletes a child-friendly place. Administrators may delete any place; regular users
    /// may only delete their own submissions.
    /// </summary>
    /// <param name="id">The unique identifier of the place to delete.</param>
    /// <param name="userId">The identifier of the requesting user.</param>
    /// <param name="isAdmin">When <c>true</c>, bypasses ownership checks.</param>
    Task DeleteAsync(Guid id, string userId, bool isAdmin = false);
}
