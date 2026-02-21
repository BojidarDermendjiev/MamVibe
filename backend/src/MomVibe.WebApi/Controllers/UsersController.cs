namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

using Domain.Entities;
using Application.DTOs.Users;
using Application.Interfaces;

/// <summary>
/// Public and authenticated endpoints for user profiles and dashboards:
/// - Retrieve a user's public profile by ID
/// - Update the current user's profile (authenticated)
/// - Get the current user's items (authenticated)
/// - Get the current user's liked items (authenticated)
/// </summary>

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly IItemService _itemService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersController"/>.
    /// </summary>
    /// <param name="userManager">ASP.NET Core Identity user manager.</param>
    /// <param name="currentUserService">Service providing current user context.</param>
    /// <param name="itemService">Service for item operations related to users.</param>
    public UsersController(UserManager<ApplicationUser> userManager, ICurrentUserService currentUserService, IItemService itemService)
    {
        this._userManager = userManager;
        this._currentUserService = currentUserService;
        this._itemService = itemService;
    }

    /// <summary>
    /// Retrieves a public user profile by identifier.
    /// </summary>
    /// <param name="id">The identifier of the user to fetch.</param>
    /// <returns>
    /// 404 Not Found if the user does not exist.<br/>
    /// 200 OK with a <see cref="UserDto"/> containing public profile fields on success.
    /// </returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProfile(string id)
    {
        var user = await this._userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
            DisplayName = user.DisplayName,
            ProfileType = user.ProfileType,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio,
            CreatedAt = user.CreatedAt,
        });
    }

    /// <summary>
    /// Updates the authenticated user's profile.
    /// Only provided fields are updated; unspecified fields remain unchanged.
    /// </summary>
    /// <param name="dto">Partial profile update payload.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 404 Not Found if the user cannot be located.<br/>
    /// 200 OK with the updated <see cref="UserDto"/> on success.
    /// </returns>
    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        var user = await this._userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        if (dto.DisplayName != null) user.DisplayName = dto.DisplayName;
        if (dto.Bio != null) user.Bio = dto.Bio;
        if (dto.AvatarUrl != null) user.AvatarUrl = dto.AvatarUrl;
        if (dto.ProfileType.HasValue) user.ProfileType = dto.ProfileType.Value;
        if (dto.LanguagePreference != null) user.LanguagePreference = dto.LanguagePreference;
        if (dto.Iban != null) user.Iban = dto.Iban;

        await this._userManager.UpdateAsync(user);
        return Ok(new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
            DisplayName = user.DisplayName,
            ProfileType = user.ProfileType,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio,
            CreatedAt = user.CreatedAt,
            Iban = user.Iban
        });
    }

    /// <summary>
    /// Retrieves items created by the authenticated user.
    /// </summary>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 200 OK with the user's items on success.
    /// </returns>
    [Authorize]
    [HttpGet("dashboard/items")]
    public async Task<IActionResult> GetMyItems()
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        var items = await this._itemService.GetUserItemsAsync(userId);
        return Ok(items);
    }

    /// <summary>
    /// Retrieves items liked by the authenticated user.
    /// </summary>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 200 OK with the user's liked items on success.
    /// </returns>
    [Authorize]
    [HttpGet("dashboard/liked")]
    public async Task<IActionResult> GetLikedItems()
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        var items = await this._itemService.GetLikedItemsAsync(userId);
        return Ok(items);
    }
}
