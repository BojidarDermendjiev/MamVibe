namespace MomVibe.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Domain.Entities;
using Application.Interfaces;
using Application.DTOs.SavedSearches;
using Application.DTOs.Items;

public class SavedSearchService : ISavedSearchService
{
    private const int MaxPerUser = 20;

    private readonly IApplicationDbContext _context;
    private readonly ISavedSearchNotifier _notifier;
    private readonly ILogger<SavedSearchService> _logger;

    public SavedSearchService(IApplicationDbContext context, ISavedSearchNotifier notifier, ILogger<SavedSearchService> logger)
    {
        this._context = context;
        this._notifier = notifier;
        this._logger = logger;
    }

    public async Task<SavedSearchDto> CreateAsync(string userId, CreateSavedSearchDto dto)
    {
        var count = await this._context.SavedSearches.CountAsync(s => s.UserId == userId);
        if (count >= MaxPerUser)
            throw new InvalidOperationException($"You can have at most {MaxPerUser} saved searches.");

        var entity = new SavedSearch
        {
            UserId = userId,
            Name = dto.Name,
            CategoryId = dto.CategoryId,
            ListingType = dto.ListingType,
            SearchTerm = string.IsNullOrWhiteSpace(dto.SearchTerm) ? null : dto.SearchTerm.Trim(),
            AgeGroup = dto.AgeGroup,
            ShoeSize = dto.ShoeSize,
            ClothingSize = dto.ClothingSize,
            Condition = dto.Condition,
            MaxPrice = dto.MaxPrice
        };

        this._context.SavedSearches.Add(entity);
        await this._context.SaveChangesAsync();

        var category = entity.CategoryId.HasValue
            ? await this._context.Categories.FindAsync(entity.CategoryId.Value)
            : null;

        return MapToDto(entity, category?.Name);
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        var entity = await this._context.SavedSearches.FindAsync(id);
        if (entity == null) throw new KeyNotFoundException("Saved search not found.");
        if (entity.UserId != userId) throw new UnauthorizedAccessException();

        this._context.SavedSearches.Remove(entity);
        await this._context.SaveChangesAsync();
    }

    public async Task<List<SavedSearchDto>> GetMyAsync(string userId)
    {
        return await this._context.SavedSearches
            .AsNoTracking()
            .Include(s => s.Category)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SavedSearchDto
            {
                Id = s.Id,
                Name = s.Name,
                CategoryId = s.CategoryId,
                CategoryName = s.Category != null ? s.Category.Name : null,
                ListingType = s.ListingType,
                SearchTerm = s.SearchTerm,
                AgeGroup = s.AgeGroup,
                ShoeSize = s.ShoeSize,
                ClothingSize = s.ClothingSize,
                Condition = s.Condition,
                MaxPrice = s.MaxPrice,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();
    }

    public async Task NotifyMatchingSearchesAsync(ItemDto item)
    {
        // DB-side filter on all structured attributes
        var candidates = await this._context.SavedSearches
            .AsNoTracking()
            .Where(s => s.UserId != item.UserId)
            .Where(s => s.CategoryId == null || s.CategoryId == item.CategoryId)
            .Where(s => s.ListingType == null || s.ListingType == item.ListingType)
            .Where(s => s.AgeGroup == null || s.AgeGroup == item.AgeGroup)
            .Where(s => s.ShoeSize == null || s.ShoeSize == item.ShoeSize)
            .Where(s => s.ClothingSize == null || s.ClothingSize == item.ClothingSize)
            .Where(s => s.Condition == null || s.Condition == item.Condition)
            .Where(s => s.MaxPrice == null || item.Price == null || s.MaxPrice >= item.Price)
            .Take(500)
            .ToListAsync();

        // In-memory filter for keyword match
        var matches = candidates.Where(s =>
            s.SearchTerm == null ||
            item.Title.Contains(s.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
            item.Description.Contains(s.SearchTerm, StringComparison.OrdinalIgnoreCase));

        foreach (var match in matches)
        {
            try
            {
                await this._notifier.NotifyAsync(match.UserId, new SavedSearchMatchNotification
                {
                    SavedSearchId = match.Id,
                    SavedSearchName = match.Name,
                    Item = new Application.DTOs.SavedSearches.SavedSearchMatchItemDto
                    {
                        Id         = item.Id,
                        Title      = item.Title,
                        CategoryName = item.CategoryName,
                        ListingType  = item.ListingType,
                        Price        = item.Price,
                        FirstPhotoUrl = item.Photos?.OrderBy(p => p.DisplayOrder).Select(p => p.Url).FirstOrDefault(),
                        AgeGroup   = item.AgeGroup,
                        Condition  = item.Condition
                    }
                });
            }
            catch (Exception ex)
            {
                this._logger.LogWarning(ex, "Saved-search match SignalR notification failed for item {ItemId}", item.Id);
            }
        }
    }

    private static SavedSearchDto MapToDto(SavedSearch s, string? categoryName) => new()
    {
        Id = s.Id,
        Name = s.Name,
        CategoryId = s.CategoryId,
        CategoryName = categoryName,
        ListingType = s.ListingType,
        SearchTerm = s.SearchTerm,
        AgeGroup = s.AgeGroup,
        ShoeSize = s.ShoeSize,
        ClothingSize = s.ClothingSize,
        Condition = s.Condition,
        MaxPrice = s.MaxPrice,
        CreatedAt = s.CreatedAt
    };
}
