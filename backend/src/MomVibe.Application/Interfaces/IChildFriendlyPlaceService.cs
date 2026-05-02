namespace MomVibe.Application.Interfaces;

using DTOs.ChildFriendlyPlaces;
using Domain.Enums;

public interface IChildFriendlyPlaceService
{
    Task<IEnumerable<ChildFriendlyPlaceDto>> GetAllAsync(string? city = null, PlaceType? placeType = null, int? maxAgeMonths = null, int page = 1, int pageSize = 20);
    Task<ChildFriendlyPlaceDto?> GetByIdAsync(Guid id);
    Task<ChildFriendlyPlaceDto> CreateAsync(string userId, CreateChildFriendlyPlaceDto dto);
    Task ApproveAsync(Guid id);
    Task DeleteAsync(Guid id, string userId, bool isAdmin = false);
}
