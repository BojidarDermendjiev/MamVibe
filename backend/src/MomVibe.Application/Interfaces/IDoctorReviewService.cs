namespace MomVibe.Application.Interfaces;

using DTOs.DoctorReviews;

public interface IDoctorReviewService
{
    Task<IEnumerable<DoctorReviewDto>> GetAllAsync(string? city = null, string? specialization = null, int page = 1, int pageSize = 20);
    Task<DoctorReviewDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<DoctorReviewDto>> GetByUserAsync(string userId);
    Task<DoctorReviewDto> CreateAsync(string userId, CreateDoctorReviewDto dto);
    Task DeleteAsync(Guid id, string userId, bool isAdmin = false);
}
