namespace MomVibe.Application.Interfaces;

using DTOs.UserRatings;

public interface IUserRatingService
{
    Task<UserRatingDto> CreateAsync(string raterId, Guid purchaseRequestId, CreateUserRatingDto dto);
    Task<IEnumerable<UserRatingDto>> GetForUserAsync(string userId);
    Task<(double? Average, int Count)> GetSummaryAsync(string userId);
}
