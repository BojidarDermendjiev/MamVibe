namespace MomVibe.Application.Interfaces;

using DTOs.UserRatings;

/// <summary>
/// Defines operations for managing user ratings that buyers submit to sellers
/// after completing a marketplace transaction.
/// </summary>
public interface IUserRatingService
{
    /// <summary>
    /// Creates a rating for the seller associated with the given completed purchase request.
    /// Only the buyer of the purchase may submit a rating, and each purchase may be rated at most once.
    /// </summary>
    /// <param name="raterId">The identifier of the user submitting the rating (must be the buyer).</param>
    /// <param name="purchaseRequestId">The unique identifier of the completed purchase request.</param>
    /// <param name="dto">The rating data provided by the buyer.</param>
    /// <returns>The newly created <see cref="UserRatingDto"/>.</returns>
    Task<UserRatingDto> CreateAsync(string raterId, Guid purchaseRequestId, CreateUserRatingDto dto);

    /// <summary>
    /// Retrieves all ratings received by the specified user, ordered from newest to oldest.
    /// </summary>
    /// <param name="userId">The identifier of the user whose received ratings to retrieve.</param>
    /// <returns>A collection of <see cref="UserRatingDto"/> instances.</returns>
    Task<IEnumerable<UserRatingDto>> GetForUserAsync(string userId);

    /// <summary>
    /// Calculates the average rating and total count of ratings received by the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user to summarise.</param>
    /// <returns>
    /// A tuple containing the average rating (or <c>null</c> if no ratings exist) and the total count.
    /// </returns>
    Task<(double? Average, int Count)> GetSummaryAsync(string userId);
}
