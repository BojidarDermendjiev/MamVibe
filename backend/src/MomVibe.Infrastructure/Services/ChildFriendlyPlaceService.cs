namespace MomVibe.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;

using Application.Interfaces;
using Application.DTOs.ChildFriendlyPlaces;
using Domain.Entities;
using Domain.Enums;

public class ChildFriendlyPlaceService : IChildFriendlyPlaceService
{
    private readonly IApplicationDbContext _db;

    public ChildFriendlyPlaceService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ChildFriendlyPlaceDto>> GetAllAsync(string? city = null, PlaceType? placeType = null, int? maxAgeMonths = null, int page = 1, int pageSize = 20)
    {
        var query = _db.ChildFriendlyPlaces.Include(p => p.User)
            .Where(p => p.IsApproved)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(p => p.City.ToLower().Contains(city.ToLower()));
        if (placeType.HasValue)
            query = query.Where(p => p.PlaceType == placeType.Value);
        if (maxAgeMonths.HasValue)
            query = query.Where(p => !p.AgeToMonths.HasValue || p.AgeToMonths >= maxAgeMonths.Value);

        var places = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return places.Select(MapToDto);
    }

    public async Task<IEnumerable<ChildFriendlyPlaceDto>> GetPendingAsync()
    {
        var places = await _db.ChildFriendlyPlaces
            .Include(p => p.User)
            .Where(p => !p.IsApproved)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        return places.Select(MapToDto);
    }

    public async Task<ChildFriendlyPlaceDto?> GetByIdAsync(Guid id)
    {
        var place = await _db.ChildFriendlyPlaces.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);
        return place == null ? null : MapToDto(place);
    }

    public async Task<ChildFriendlyPlaceDto> CreateAsync(string userId, CreateChildFriendlyPlaceDto dto)
    {
        var place = new ChildFriendlyPlace
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = dto.Name,
            Description = dto.Description,
            Address = dto.Address,
            City = dto.City,
            PlaceType = dto.PlaceType,
            AgeFromMonths = dto.AgeFromMonths,
            AgeToMonths = dto.AgeToMonths,
            PhotoUrl = dto.PhotoUrl,
            Website = dto.Website,
            IsApproved = false,
        };
        _db.ChildFriendlyPlaces.Add(place);
        await _db.SaveChangesAsync();
        place = await _db.ChildFriendlyPlaces.Include(p => p.User).FirstAsync(p => p.Id == place.Id);
        return MapToDto(place);
    }

    public async Task ApproveAsync(Guid id)
    {
        var place = await _db.ChildFriendlyPlaces.FindAsync(id)
            ?? throw new KeyNotFoundException("Place not found.");
        place.IsApproved = true;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, string userId, bool isAdmin = false)
    {
        var place = await _db.ChildFriendlyPlaces.FindAsync(id)
            ?? throw new KeyNotFoundException("Place not found.");
        if (!isAdmin && place.UserId != userId)
            throw new UnauthorizedAccessException("Not allowed.");
        _db.ChildFriendlyPlaces.Remove(place);
        await _db.SaveChangesAsync();
    }

    private static ChildFriendlyPlaceDto MapToDto(ChildFriendlyPlace p) => new()
    {
        Id = p.Id,
        UserId = p.UserId,
        AuthorDisplayName = p.User?.DisplayName,
        Name = p.Name,
        Description = p.Description,
        Address = p.Address,
        City = p.City,
        PlaceType = p.PlaceType,
        AgeFromMonths = p.AgeFromMonths,
        AgeToMonths = p.AgeToMonths,
        PhotoUrl = p.PhotoUrl,
        Website = p.Website,
        IsApproved = p.IsApproved,
        CreatedAt = p.CreatedAt,
    };
}
