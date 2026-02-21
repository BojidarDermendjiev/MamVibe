namespace MomVibe.Infrastructure.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Domain.Entities;
using Application.DTOs.Items;
using Application.Interfaces;
using Application.DTOs.Common;
using Configuration;

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
    private readonly IN8nWebhookService _webhook;
    private readonly N8nSettings _n8nSettings;

    public ItemService(
        IApplicationDbContext context,
        IMapper mapper,
        IN8nWebhookService webhook,
        IOptions<N8nSettings> n8nSettings)
    {
        this._context = context;
        this._mapper = mapper;
        this._webhook = webhook;
        this._n8nSettings = n8nSettings.Value;
    }

    public async Task<PagedResult<ItemDto>> GetAllAsync(ItemFilterDto filter, string? currentUserId = null)
    {
        var query = this._context.Items
            .AsNoTracking()
            .Include(i => i.Photos)
            .Include(i => i.User)
            .Include(i => i.Category)
            .Where(i => i.IsActive)
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

        query = filter.SortBy switch
        {
            "price_asc" => query.OrderBy(i => i.Price),
            "price_desc" => query.OrderByDescending(i => i.Price),
            "most_liked" => query.OrderByDescending(i => i.LikeCount),
            "most_viewed" => query.OrderByDescending(i => i.ViewCount),
            _ => query.OrderByDescending(i => i.CreatedAt)
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

        return new PagedResult<ItemDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<ItemDto?> GetByIdAsync(Guid id, string? currentUserId = null)
    {
        var item = await this._context.Items
            .AsNoTracking()
            .Include(i => i.Photos.OrderBy(p => p.DisplayOrder))
            .Include(i => i.User)
            .Include(i => i.Category)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null) return null;

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
            Price = dto.Price,
            UserId = userId,
            Photos = dto.PhotoUrls.Select((url, index) => new ItemPhoto
            {
                Url = url,
                DisplayOrder = index
            }).ToList()
        };

        this._context.Items.Add(item);
        await this._context.SaveChangesAsync();

        // Fire item.pending_approval webhook for admin review
        var createdItem = await this._context.Items
            .Include(i => i.User)
            .Include(i => i.Category)
            .FirstOrDefaultAsync(i => i.Id == item.Id);
        try
        {
            this._webhook.Send(this._n8nSettings.ItemPendingApproval, new
            {
                Event = "item.pending_approval",
                Timestamp = DateTime.UtcNow,
                ItemId = item.Id,
                Title = dto.Title,
                Description = dto.Description,
                Category = createdItem?.Category?.Name,
                ListingType = dto.ListingType.ToString(),
                Price = dto.Price,
                SellerName = createdItem?.User?.DisplayName,
                SellerEmail = createdItem?.User?.Email
            });
        }
        catch { /* Webhook failure must not break item creation */ }

        return (await GetByIdAsync(item.Id))!;
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

        if (dto.Title != null) item.Title = dto.Title;
        if (dto.Description != null) item.Description = dto.Description;
        if (dto.CategoryId.HasValue) item.CategoryId = dto.CategoryId.Value;
        if (dto.ListingType.HasValue) item.ListingType = dto.ListingType.Value;
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
        var item = await this._context.Items.FindAsync(id);
        if (item != null)
        {
            item.ViewCount++;
            await this._context.SaveChangesAsync();
        }
    }

    public async Task<bool> ToggleLikeAsync(Guid itemId, string userId)
    {
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

    public async Task<List<ItemDto>> GetLikedItemsAsync(string userId)
    {
        var items = await this._context.Likes
            .AsNoTracking()
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
}
