namespace MomVibe.Infrastructure.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;

using Domain.Entities;
using Domain.Enums;
using Application.Interfaces;
using Application.DTOs.UserRatings;

/// <summary>
/// EF Core-backed implementation of <see cref="IUserRatingService"/>.
/// </summary>
public class UserRatingService : IUserRatingService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of <see cref="UserRatingService"/> with the given dependencies.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="mapper">The AutoMapper instance used for object projection.</param>
    public UserRatingService(IApplicationDbContext context, IMapper mapper)
    {
        this._context = context;
        this._mapper = mapper;
    }

    /// <inheritdoc/>
    public async Task<UserRatingDto> CreateAsync(string raterId, Guid purchaseRequestId, CreateUserRatingDto dto)
    {
        var purchase = await _context.PurchaseRequests.FindAsync(purchaseRequestId)
            ?? throw new KeyNotFoundException("Purchase request not found.");

        if (purchase.BuyerId != raterId)
            throw new UnauthorizedAccessException("Only the buyer can rate the seller.");

        if (purchase.Status != PurchaseRequestStatus.Completed)
            throw new InvalidOperationException("Can only rate after the purchase is completed.");

        var alreadyRated = await _context.UserRatings
            .AnyAsync(r => r.PurchaseRequestId == purchaseRequestId);
        if (alreadyRated)
            throw new InvalidOperationException("This purchase has already been rated.");

        var rating = new UserRating
        {
            RaterId = raterId,
            RatedUserId = purchase.SellerId,
            PurchaseRequestId = purchaseRequestId,
            Rating = dto.Rating,
            Comment = dto.Comment,
        };

        _context.UserRatings.Add(rating);
        await _context.SaveChangesAsync();

        var created = await _context.UserRatings
            .Include(r => r.Rater)
            .FirstAsync(r => r.Id == rating.Id);
        return _mapper.Map<UserRatingDto>(created);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<UserRatingDto>> GetForUserAsync(string userId)
    {
        var ratings = await _context.UserRatings
            .Include(r => r.Rater)
            .Where(r => r.RatedUserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        return _mapper.Map<IEnumerable<UserRatingDto>>(ratings);
    }

    /// <inheritdoc/>
    public async Task<(double? Average, int Count)> GetSummaryAsync(string userId)
    {
        var ratings = await _context.UserRatings
            .Where(r => r.RatedUserId == userId)
            .Select(r => r.Rating)
            .ToListAsync();

        if (ratings.Count == 0)
            return (null, 0);

        return (ratings.Average(), ratings.Count);
    }
}
