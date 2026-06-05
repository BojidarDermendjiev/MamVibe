namespace MomVibe.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Application.Interfaces;

/// <summary>
/// Implements GDPR Article 17 (erasure) and Article 20 (data portability) obligations.
/// Financial records (Payments) are intentionally retained for the 5-year fiscal period
/// required under Bulgarian accounting law.
/// </summary>
public class GdprService : IGdprService
{
    private readonly IApplicationDbContext _db;
    private readonly IPhotoService _photoService;
    private readonly ILogger<GdprService> _logger;

    public GdprService(IApplicationDbContext db, IPhotoService photoService, ILogger<GdprService> logger)
    {
        _db = db;
        _photoService = photoService;
        _logger = logger;
    }

    public async Task<object> ExportDataAsync(string userId)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            throw new KeyNotFoundException("User not found.");

        var items = await _db.Items
            .AsNoTracking()
            .Include(i => i.Photos)
            .Where(i => i.UserId == userId)
            .Select(i => new
            {
                i.Id,
                i.Title,
                i.Description,
                i.Price,
                i.CategoryId,
                i.ListingType,
                i.Condition,
                i.IsSold,
                i.CreatedAt,
                Photos = i.Photos.Select(p => p.Url).ToList()
            })
            .ToListAsync();

        var sentMessages = await _db.Messages
            .AsNoTracking()
            .Where(m => m.SenderId == userId)
            .Select(m => new { m.Id, m.ReceiverId, m.Content, m.CreatedAt })
            .ToListAsync();

        var receivedMessages = await _db.Messages
            .AsNoTracking()
            .Where(m => m.ReceiverId == userId)
            .Select(m => new { m.Id, m.SenderId, m.Content, m.CreatedAt })
            .ToListAsync();

        var payments = await _db.Payments
            .AsNoTracking()
            .Where(p => p.BuyerId == userId || p.SellerId == userId)
            .Select(p => new { p.Id, p.BuyerId, p.SellerId, p.Amount, p.CreatedAt, p.EBillNumber })
            .ToListAsync();

        var likes = await _db.Likes
            .AsNoTracking()
            .Where(l => l.UserId == userId)
            .Select(l => new { l.ItemId, l.CreatedAt })
            .ToListAsync();

        var feedbacks = await _db.Feedbacks
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .Select(f => new { f.Id, f.Rating, f.Category, f.Content, f.IsContactable, f.CreatedAt })
            .ToListAsync();

        var doctorReviews = await _db.DoctorReviews
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .Select(r => new { r.Id, r.DoctorName, r.Specialization, r.ClinicName, r.City, r.Rating, r.Content, r.IsAnonymous, r.CreatedAt })
            .ToListAsync();

        var childFriendlyPlaces = await _db.ChildFriendlyPlaces
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => new { p.Id, p.Name, p.PlaceType, p.City, p.Address, p.Description, p.CreatedAt })
            .ToListAsync();

        var ratingsGiven = await _db.UserRatings
            .AsNoTracking()
            .Where(r => r.RaterId == userId)
            .Select(r => new { r.Id, r.RatedUserId, r.Rating, r.Comment, r.CreatedAt })
            .ToListAsync();

        var offers = await _db.Offers
            .AsNoTracking()
            .Where(o => o.BuyerId == userId)
            .Select(o => new { o.Id, o.ItemId, o.OfferedPrice, o.Status, o.CreatedAt })
            .ToListAsync();

        var follows = await _db.Follows
            .AsNoTracking()
            .Where(f => f.FollowerId == userId)
            .Select(f => new { f.FolloweeId, f.CreatedAt })
            .ToListAsync();

        var savedSearches = await _db.SavedSearches
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .Select(s => new { s.Id, s.Name, s.SearchTerm, s.MaxPrice, s.CreatedAt })
            .ToListAsync();

        return new
        {
            ExportedAt = DateTime.UtcNow,
            Profile = new
            {
                user.Id,
                user.Email,
                user.DisplayName,
                user.ProfileType,
                user.AvatarUrl,
                user.Bio,
                user.LanguagePreference,
                user.RevolutTag,
                user.IsOnHoliday,
                user.CreatedAt,
            },
            Items = items,
            SentMessages = sentMessages,
            ReceivedMessages = receivedMessages,
            Payments = payments,
            Likes = likes,
            Feedbacks = feedbacks,
            DoctorReviews = doctorReviews,
            ChildFriendlyPlaces = childFriendlyPlaces,
            RatingsGiven = ratingsGiven,
            Offers = offers,
            Follows = follows,
            SavedSearches = savedSearches,
        };
    }

    public async Task ErasePersonalDataAsync(string userId)
    {
        // Delete item photos from storage, then remove items from DB
        var userItems = await _db.Items
            .Include(i => i.Photos)
            .Where(i => i.UserId == userId)
            .ToListAsync();

        foreach (var item in userItems)
        {
            foreach (var photo in item.Photos)
            {
                try { await _photoService.DeletePhotoAsync(photo.Url); }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "GDPR erasure: failed to delete photo {PhotoUrl} (orphaned, continuing)", photo.Url);
                }
            }
        }

        _db.Items.RemoveRange(userItems);

        // Anonymize sent messages — preserve thread structure for the other party
        var sentMessages = await _db.Messages
            .Where(m => m.SenderId == userId)
            .ToListAsync();

        foreach (var msg in sentMessages)
            msg.Content = "[deleted]";

        // Hard-delete everything else owned by this user
        var likes = await _db.Likes.Where(l => l.UserId == userId).ToListAsync();
        _db.Likes.RemoveRange(likes);

        var follows = await _db.Follows.Where(f => f.FollowerId == userId || f.FolloweeId == userId).ToListAsync();
        _db.Follows.RemoveRange(follows);

        var savedSearches = await _db.SavedSearches.Where(s => s.UserId == userId).ToListAsync();
        _db.SavedSearches.RemoveRange(savedSearches);

        var offers = await _db.Offers.Where(o => o.BuyerId == userId).ToListAsync();
        _db.Offers.RemoveRange(offers);

        var feedbacks = await _db.Feedbacks.Where(f => f.UserId == userId).ToListAsync();
        _db.Feedbacks.RemoveRange(feedbacks);

        var doctorReviews = await _db.DoctorReviews.Where(r => r.UserId == userId).ToListAsync();
        _db.DoctorReviews.RemoveRange(doctorReviews);

        var places = await _db.ChildFriendlyPlaces.Where(p => p.UserId == userId).ToListAsync();
        _db.ChildFriendlyPlaces.RemoveRange(places);

        var ratingsGiven = await _db.UserRatings.Where(r => r.RaterId == userId || r.RatedUserId == userId).ToListAsync();
        _db.UserRatings.RemoveRange(ratingsGiven);

        var refreshTokens = await _db.RefreshTokens.Where(t => t.UserId == userId).ToListAsync();
        _db.RefreshTokens.RemoveRange(refreshTokens);

        // Payments are intentionally retained (Bulgarian 5-year fiscal retention obligation)

        await _db.SaveChangesAsync();
    }
}
