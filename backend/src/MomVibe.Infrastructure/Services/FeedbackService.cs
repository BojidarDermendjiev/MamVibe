namespace MomVibe.Infrastructure.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;

using Domain.Entities;
using Application.Interfaces;
using Application.DTOs.Common;
using Application.DTOs.Feedbacks;

/// <summary>
/// Service for managing user feedback: paginated retrieval, creation, and deletion with ownership checks.
/// Uses EF Core and AutoMapper to fetch feedback with user details, map DTOs, and enforce authorization rules.
/// </summary>
public class FeedbackService : IFeedbackService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public FeedbackService(IApplicationDbContext context, IMapper mapper)
    {
        this._context = context;
        this._mapper = mapper;
    }

    public async Task<PagedResult<FeedbackDto>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        var query = _context.Feedbacks
            .Include(f => f.User)
            .OrderByDescending(f => f.CreatedAt);

        var totalCount = await query.CountAsync();
        var feedbacks = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<FeedbackDto>
        {
            Items = this._mapper.Map<List<FeedbackDto>>(feedbacks),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<FeedbackDto> CreateAsync(CreateFeedbackDto dto, string userId)
    {
        var feedback = new Feedback
        {
            UserId = userId,
            Rating = dto.Rating,
            Category = dto.Category,
            Content = dto.Content,
            IsContactable = dto.IsContactable
        };

        this._context.Feedbacks.Add(feedback);
        await this._context.SaveChangesAsync();

        return this._mapper.Map<FeedbackDto>(feedback);
    }

    public async Task DeleteAsync(Guid id, string userId, bool isAdmin = false)
    {
        var feedback = await this._context.Feedbacks.FindAsync(id);
        if (feedback == null)
            throw new KeyNotFoundException("Feedback not found.");

        if (feedback.UserId != userId && !isAdmin)
            throw new UnauthorizedAccessException("You can only delete your own feedback.");

        this._context.Feedbacks.Remove(feedback);
        await this._context.SaveChangesAsync();
    }
}
