namespace MomVibe.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using AutoMapper;

using Application.Interfaces;
using Application.DTOs.DoctorReviews;
using Domain.Entities;
using Persistence;

public class DoctorReviewService : IDoctorReviewService
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public DoctorReviewService(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<IEnumerable<DoctorReviewDto>> GetAllAsync(string? city = null, string? specialization = null, int page = 1, int pageSize = 20)
    {
        var query = _db.DoctorReviews.Include(r => r.User).Where(r => r.IsApproved).AsQueryable();
        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(r => r.City.ToLower().Contains(city.ToLower()));
        if (!string.IsNullOrWhiteSpace(specialization))
            query = query.Where(r => r.Specialization.ToLower().Contains(specialization.ToLower()));

        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return reviews.Select(r => MapToDto(r));
    }

    public async Task<DoctorReviewDto?> GetByIdAsync(Guid id)
    {
        var review = await _db.DoctorReviews.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);
        return review == null ? null : MapToDto(review);
    }

    public async Task<IEnumerable<DoctorReviewDto>> GetByUserAsync(string userId)
    {
        var reviews = await _db.DoctorReviews
            .Include(r => r.User)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        return reviews.Select(r => MapToDto(r));
    }

    public async Task<IEnumerable<DoctorReviewDto>> GetPendingAsync()
    {
        var reviews = await _db.DoctorReviews
            .Include(r => r.User)
            .Where(r => !r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        return reviews.Select(r => MapToDto(r));
    }

    public async Task ApproveAsync(Guid id)
    {
        var review = await _db.DoctorReviews.FindAsync(id)
            ?? throw new KeyNotFoundException("Review not found.");
        review.IsApproved = true;
        await _db.SaveChangesAsync();
    }

    public async Task<DoctorReviewDto> CreateAsync(string userId, CreateDoctorReviewDto dto)
    {
        var review = new DoctorReview
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DoctorName = dto.DoctorName,
            Specialization = dto.Specialization,
            ClinicName = dto.ClinicName,
            City = dto.City,
            Rating = dto.Rating,
            Content = dto.Content,
            SuperdocUrl = dto.SuperdocUrl,
            IsAnonymous = dto.IsAnonymous,
            IsApproved = false,
        };
        _db.DoctorReviews.Add(review);
        await _db.SaveChangesAsync();
        await _db.DoctorReviews.Entry(review).Reference(r => r.User).LoadAsync();
        return MapToDto(review);
    }

    public async Task DeleteAsync(Guid id, string userId, bool isAdmin = false)
    {
        var review = await _db.DoctorReviews.FindAsync(id)
            ?? throw new KeyNotFoundException("Review not found.");
        if (!isAdmin && review.UserId != userId)
            throw new UnauthorizedAccessException("Not allowed.");
        _db.DoctorReviews.Remove(review);
        await _db.SaveChangesAsync();
    }

    private static DoctorReviewDto MapToDto(DoctorReview r) => new()
    {
        Id = r.Id,
        UserId = r.UserId,
        AuthorDisplayName = r.IsAnonymous ? null : r.User?.DisplayName,
        AuthorAvatarUrl = r.IsAnonymous ? null : r.User?.AvatarUrl,
        DoctorName = r.DoctorName,
        Specialization = r.Specialization,
        ClinicName = r.ClinicName,
        City = r.City,
        Rating = r.Rating,
        Content = r.Content,
        SuperdocUrl = r.SuperdocUrl,
        IsAnonymous = r.IsAnonymous,
        IsApproved = r.IsApproved,
        CreatedAt = r.CreatedAt,
    };
}
