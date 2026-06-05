namespace MomVibe.Infrastructure.Services;

using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using Domain.Enums;
using Domain.Entities;
using Application.DTOs.Items;
using Application.DTOs.Stats;
using Application.Events;
using Application.Interfaces;
using Application.DTOs.Common;

/// <summary>
/// Service for managing marketplace items: filtered and paginated listing, detailed retrieval,
/// creation, update, and deletion with ownership checks; view count increments; like toggling;
/// and fetching user-owned and liked items. Uses EF Core and AutoMapper to load related data
/// (photos, user, category) and map entities to DTOs.
/// </summary>
public class ItemService : IItemService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IAiModerationService _aiModeration;
    private readonly IAiListingService _aiListing;
    private readonly INekorektenService _nekorekten;
    private readonly IMemoryCache _cache;
    private readonly IPublisher _publisher;
    private readonly ILogger<ItemService> _logger;

    private const double AutoApproveThreshold = 0.85;
    private const double NewSellerAutoApproveThreshold = 0.95;
    private const int TrustedSellerMinSales = 3;
    private const string CacheKeyPrefix = "items_browse:";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);

    public ItemService(
        IApplicationDbContext context,
        IMapper mapper,
        IAiModerationService aiModeration,
        IAiListingService aiListing,
        INekorektenService nekorekten,
        IMemoryCache cache,
        IPublisher publisher,
        ILogger<ItemService> logger)
    {
        this._context = context;
        this._mapper = mapper;
        this._aiModeration = aiModeration;
        this._aiListing = aiListing;
        this._nekorekten = nekorekten;
        this._cache = cache;
        this._publisher = publisher;
        this._logger = logger;
    }

    private static string BuildCacheKey(ItemFilterDto filter) =>
        $"{CacheKeyPrefix}{filter.CategoryId}|{filter.ListingType}|{filter.SearchTerm}|{filter.Brand}|{filter.AgeGroup}|{filter.ShoeSize}|{filter.ClothingSize}|{filter.Condition}|{filter.SortBy}|{filter.Page}|{filter.PageSize}";

    public async Task<PagedResult<ItemDto>> GetAllAsync(ItemFilterDto filter, string? currentUserId = null)
    {
        // Serve anonymous browse requests from cache — avoids DB hit on every page load
        if (currentUserId == null)
        {
            var key = BuildCacheKey(filter);
            if (this._cache.TryGetValue(key, out PagedResult<ItemDto>? cached))
                return cached!;
        }

        var query = this._context.Items
            .AsNoTracking()
            .Include(i => i.Photos)
            .Include(i => i.User)
            .Include(i => i.Category)
            .Where(i => i.IsActive && !i.User.IsOnHoliday)
            .AsQueryable();

        if (filter.CategoryId.HasValue)
            query = query.Where(i => i.CategoryId == filter.CategoryId.Value);

        if (filter.ListingType.HasValue)
            query = query.Where(i => i.ListingType == filter.ListingType.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            query = query.Where(i => i.Title.ToLower().Contains(term) || i.Description.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(filter.Brand))
        {
            var brand = filter.Brand.ToLower();
            query = query.Where(i => i.Title.ToLower().Contains(brand) || i.Description.ToLower().Contains(brand));
        }

        if (filter.AgeGroup.HasValue)
            query = query.Where(i => i.AgeGroup == filter.AgeGroup.Value);

        if (filter.ShoeSize.HasValue)
            query = query.Where(i => i.ShoeSize == filter.ShoeSize.Value);

        if (filter.ClothingSize.HasValue)
            query = query.Where(i => i.ClothingSize == filter.ClothingSize.Value);

        if (filter.Condition.HasValue)
            query = query.Where(i => i.Condition == filter.Condition.Value);

        // Bumped items (within the last 24 h) float to the top of any sort order
        var bumpCutoff = DateTime.UtcNow.AddHours(-24);
        query = filter.SortBy switch
        {
            "price_asc"   => query.OrderByDescending(i => i.BumpedAt != null && i.BumpedAt > bumpCutoff).ThenBy(i => i.Price),
            "price_desc"  => query.OrderByDescending(i => i.BumpedAt != null && i.BumpedAt > bumpCutoff).ThenByDescending(i => i.Price),
            "most_liked"  => query.OrderByDescending(i => i.BumpedAt != null && i.BumpedAt > bumpCutoff).ThenByDescending(i => i.LikeCount),
            "most_viewed" => query.OrderByDescending(i => i.BumpedAt != null && i.BumpedAt > bumpCutoff).ThenByDescending(i => i.ViewCount),
            "oldest"      => query.OrderByDescending(i => i.BumpedAt != null && i.BumpedAt > bumpCutoff).ThenBy(i => i.CreatedAt),
            _             => query.OrderByDescending(i => i.BumpedAt != null && i.BumpedAt > bumpCutoff).ThenByDescending(i => i.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var dtos = this._mapper.Map<List<ItemDto>>(items);

        if (currentUserId != null)
        {
            var itemIds = items.Select(i => i.Id).ToList();
            var likedItemIds = await this._context.Likes
                .Where(l => l.UserId == currentUserId && itemIds.Contains(l.ItemId))
                .Select(l => l.ItemId)
                .ToListAsync();

            foreach (var dto in dtos)
            {
                dto.IsLikedByCurrentUser = likedItemIds.Contains(dto.Id);
            }
        }

        var result = new PagedResult<ItemDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };

        if (currentUserId == null)
            this._cache.Set(BuildCacheKey(filter), result, CacheTtl);

        return result;
    }

    public async Task<ItemDto?> GetByIdAsync(Guid id, string? currentUserId = null)
    {
        var item = await this._context.Items
            .AsNoTracking()
            .Include(i => i.Photos)
            .Include(i => i.User)
            .Include(i => i.Category)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null) return null;

        if (item.Photos != null)
            item.Photos = item.Photos.OrderBy(p => p.DisplayOrder).ToList();

        var dto = this._mapper.Map<ItemDto>(item);

        if (currentUserId != null)
        {
            dto.IsLikedByCurrentUser = await this._context.Likes
                .AnyAsync(l => l.ItemId == id && l.UserId == currentUserId);
        }

        return dto;
    }

    public async Task<ItemDto> CreateAsync(CreateItemDto dto, string userId)
    {
        var item = new Item
        {
            Title = dto.Title,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            ListingType = dto.ListingType,
            AgeGroup = dto.AgeGroup,
            ShoeSize = dto.ShoeSize,
            ClothingSize = dto.ClothingSize,
            Price = dto.Price,
            Condition = dto.Condition,
            UserId = userId,
            Photos = dto.PhotoUrls.Select((url, index) => new ItemPhoto
            {
                Url = url,
                DisplayOrder = index
            }).ToList()
        };

        this._context.Items.Add(item);
        await this._context.SaveChangesAsync();

        // Load navigation properties (needed for moderation context + webhook payload)
        var createdItem = await this._context.Items
            .Include(i => i.User)
            .Include(i => i.Category)
            .FirstOrDefaultAsync(i => i.Id == item.Id);

        // AI Moderation — runs synchronously; failure must never block item creation
        try
        {
            var completedSales = await this._context.Payments
                .CountAsync(p => p.SellerId == userId && p.Status == PaymentStatus.Completed);
            var threshold = completedSales >= TrustedSellerMinSales
                ? AutoApproveThreshold
                : NewSellerAutoApproveThreshold;

            var firstPhotoUrl = dto.PhotoUrls.Count > 0 ? dto.PhotoUrls[0] : null;

            var modResult = await this._aiModeration.ModerateItemAsync(
                dto.Title,
                dto.Description,
                createdItem?.Category?.Name ?? string.Empty,
                dto.ListingType,
                dto.Price,
                firstPhotoUrl);

            item.AiModerationScore = (float)modResult.Confidence;
            item.AiModerationNotes = modResult.Reason;

            if (modResult.Recommendation == "approve" && modResult.Confidence >= threshold)
            {
                item.IsActive = true;
                item.AiModerationStatus = AiModerationStatus.AutoApproved;
            }
            else
            {
                item.AiModerationStatus = modResult.Recommendation == "reject"
                    ? AiModerationStatus.FlaggedForReview
                    : AiModerationStatus.NeedsReview;
            }

            await this._context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            this._logger.LogWarning(ex, "AI moderation failed for item {ItemId}; item left in NeedsReview state", item.Id);
        }

        // Side-effects that used to live here — n8n item.pending_approval webhook (admin alert
        // for items still in review), follower fan-out, saved-search match fan-out — are now
        // INotificationHandler<ItemCreatedEvent> implementations in Infrastructure.EventHandlers.
        // Each handler re-reads the item state, so it correctly sees whether AI moderation
        // auto-approved (IsActive=true) or held the item for review.
        var result = (await GetByIdAsync(item.Id))!;
        await this._publisher.Publish(new ItemCreatedEvent(item.Id, userId));
        return result;
    }

    public async Task<ItemDto> UpdateAsync(Guid id, UpdateItemDto dto, string userId)
    {
        var item = await this._context.Items
            .Include(i => i.Photos)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null)
            throw new KeyNotFoundException("Item not found.");

        if (item.UserId != userId)
            throw new UnauthorizedAccessException("You can only edit your own items.");

        var oldPrice = item.Price;

        if (dto.Title != null) item.Title = dto.Title;
        if (dto.Description != null) item.Description = dto.Description;
        if (dto.CategoryId.HasValue) item.CategoryId = dto.CategoryId.Value;
        if (dto.ListingType.HasValue) item.ListingType = dto.ListingType.Value;
        item.AgeGroup = dto.AgeGroup;
        item.ShoeSize = dto.ShoeSize;
        item.ClothingSize = dto.ClothingSize;
        if (dto.Condition.HasValue) item.Condition = dto.Condition.Value;
        if (dto.Price.HasValue) item.Price = dto.Price;
        if (dto.IsActive.HasValue) item.IsActive = dto.IsActive.Value;

        if (dto.PhotoUrls != null)
        {
            // Mark old photos for deletion via DbSet (not navigation property)
            foreach (var photo in item.Photos.ToList())
            {
                this._context.ItemPhotos.Remove(photo);
            }

            // Add new photos via DbSet directly to avoid navigation fixup issues
            foreach (var (url, index) in dto.PhotoUrls.Select((u, i) => (u, i)))
            {
                this._context.ItemPhotos.Add(new ItemPhoto
                {
                    Url = url,
                    DisplayOrder = index,
                    ItemId = item.Id
                });
            }
        }

        await this._context.SaveChangesAsync();

        // Price-drop fan-out to likers (with concurrency cap) lives in
        // INotificationHandler<ItemPriceDroppedEvent>. We just publish the event here.
        if (dto.Price.HasValue && oldPrice.HasValue && dto.Price.Value < oldPrice.Value)
        {
            await this._publisher.Publish(new ItemPriceDroppedEvent(item.Id, oldPrice.Value, dto.Price.Value));
        }

        return (await GetByIdAsync(item.Id))!;
    }

    public async Task DeleteAsync(Guid id, string userId, bool isAdmin = false)
    {
        var item = await this._context.Items.FindAsync(id);
        if (item == null)
            throw new KeyNotFoundException("Item not found.");

        if (item.UserId != userId && !isAdmin)
            throw new UnauthorizedAccessException("You can only delete your own items.");

        this._context.Items.Remove(item);
        await this._context.SaveChangesAsync();
    }

    public async Task IncrementViewCountAsync(Guid id)
    {
        await this._context.Items
            .Where(i => i.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.ViewCount, i => i.ViewCount + 1));
    }

    public async Task<bool> ToggleLikeAsync(Guid itemId, string userId)
    {
        // IApplicationDbContext already exposes Database (the DatabaseFacade); no cast needed.
        await using var transaction = await this._context.Database.BeginTransactionAsync();
        try
        {
            var existingLike = await this._context.Likes
                .FirstOrDefaultAsync(l => l.ItemId == itemId && l.UserId == userId);

            var item = await this._context.Items.FindAsync(itemId);
            if (item == null)
                throw new KeyNotFoundException("Item not found.");

            if (existingLike != null)
            {
                this._context.Likes.Remove(existingLike);
                item.LikeCount = Math.Max(0, item.LikeCount - 1);
                await this._context.SaveChangesAsync();
                await transaction.CommitAsync();
                return false;
            }

            this._context.Likes.Add(new Like { ItemId = itemId, UserId = userId });
            item.LikeCount++;
            await this._context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<ItemDto>> GetUserItemsAsync(string userId)
    {
        var items = await this._context.Items
            .AsNoTracking()
            .Include(i => i.Photos)
            .Include(i => i.User)
            .Include(i => i.Category)
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        var dtos = this._mapper.Map<List<ItemDto>>(items);

        var itemIds = items.Select(i => i.Id).ToList();
        var likedItemIds = await this._context.Likes
            .Where(l => l.UserId == userId && itemIds.Contains(l.ItemId))
            .Select(l => l.ItemId)
            .ToListAsync();

        foreach (var dto in dtos)
        {
            dto.IsLikedByCurrentUser = likedItemIds.Contains(dto.Id);
        }

        return dtos;
    }

    public async Task<PriceSuggestionResultDto> SuggestPriceAsync(PriceSuggestionRequestDto dto)
    {
        var category = await this._context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId);

        var query = this._context.Items
            .AsNoTracking()
            .Where(i => i.IsActive
                     && i.CategoryId == dto.CategoryId
                     && i.ListingType == ListingType.Sell
                     && i.Price != null);

        if (dto.AgeGroup.HasValue)
            query = query.Where(i => i.AgeGroup == dto.AgeGroup.Value);
        if (dto.ClothingSize.HasValue)
            query = query.Where(i => i.ClothingSize == dto.ClothingSize.Value);
        if (dto.ShoeSize.HasValue)
            query = query.Where(i => i.ShoeSize == dto.ShoeSize.Value);

        var comparablePrices = await query
            .OrderByDescending(i => i.CreatedAt)
            .Take(20)
            .Select(i => i.Price!.Value)
            .ToListAsync();

        return await this._aiListing.SuggestPriceAsync(
            dto.Title,
            dto.Description,
            category?.Name ?? string.Empty,
            dto.AgeGroup,
            dto.ClothingSize,
            dto.ShoeSize,
            comparablePrices);
    }

    public async Task<List<ItemDto>> GetLikedItemsAsync(string userId)
    {
        var items = await this._context.Likes
            .AsNoTrackingWithIdentityResolution()
            .Where(l => l.UserId == userId)
            .Include(l => l.Item)
                .ThenInclude(i => i.Photos)
            .Include(l => l.Item)
                .ThenInclude(i => i.User)
            .Include(l => l.Item)
                .ThenInclude(i => i.Category)
            .Select(l => l.Item)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        var dtos = this._mapper.Map<List<ItemDto>>(items);
        foreach (var dto in dtos)
        {
            dto.IsLikedByCurrentUser = true;
        }
        return dtos;
    }

    public async Task<SellerCheckResultDto> CheckSellerAsync(Guid itemId)
    {
        var item = await this._context.Items
            .Include(i => i.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == itemId);

        if (item == null)
            throw new KeyNotFoundException("Item not found.");

        var raw = await this._nekorekten.CheckAsync(
            item.User?.DisplayName,
            item.User?.Email,
            item.User?.PhoneNumber);

        return new SellerCheckResultDto
        {
            HasReports = raw.HasReports,
            ReportCount = raw.ReportCount,
            ServiceUnavailable = raw.ServiceUnavailable,
            Reports = raw.Reports
                .Select(r => new SellerCheckReportDto
                {
                    Text = r.Text,
                    Likes = r.Likes,
                    CreatedAt = r.CreatedAt,
                })
                .ToList(),
        };
    }

    private static readonly TimeSpan BumpDuration = TimeSpan.FromHours(24);
    private static readonly TimeSpan BumpCooldown = TimeSpan.FromDays(7);

    public async Task<ItemDto> BumpAsync(Guid itemId, string userId)
    {
        var item = await this._context.Items
            .Include(i => i.Photos)
            .Include(i => i.User)
            .Include(i => i.Category)
            .FirstOrDefaultAsync(i => i.Id == itemId);

        if (item == null)
            throw new KeyNotFoundException("Item not found.");
        if (item.UserId != userId)
            throw new UnauthorizedAccessException("You can only bump your own items.");

        var now = DateTime.UtcNow;
        if (item.BumpedAt.HasValue && item.BumpedAt.Value > now - BumpCooldown)
        {
            var hoursLeft = (int)Math.Ceiling((item.BumpedAt.Value + BumpCooldown - now).TotalHours);
            throw new InvalidOperationException($"You can bump this item again in {hoursLeft} hour(s).");
        }

        item.BumpedAt = now;
        await this._context.SaveChangesAsync();

        return this._mapper.Map<ItemDto>(item);
    }

    public async Task<PublicStatsDto> GetPublicStatsAsync()
    {
        const string cacheKey = "public:stats";
        if (this._cache.TryGetValue(cacheKey, out PublicStatsDto? cached))
            return cached!;

        var activeItems = this._context.Items.Where(i => i.IsActive && !i.User!.IsOnHoliday);

        var stats = new PublicStatsDto
        {
            ActiveListings = await activeItems.CountAsync(),
            TotalSellers = await activeItems.Select(i => i.UserId).Distinct().CountAsync(),
            HappyFamilies = await this._context.Payments
                .Where(p => p.Status == Domain.Enums.PaymentStatus.Completed)
                .Select(p => p.BuyerId)
                .Distinct()
                .CountAsync(),
        };

        this._cache.Set(cacheKey, stats, TimeSpan.FromMinutes(10));
        return stats;
    }
}
