namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Application.DTOs.Items;
using Application.Interfaces;

/// <summary>
/// Public and authenticated endpoints for managing item listings:
/// - Browse items with filtering and pagination (adds X-Pagination header)
/// - Retrieve item details (increments view count)
/// - Create, update, and delete items (authenticated; with authorization checks)
/// - Toggle like status on an item (authenticated)
/// </summary>

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IItemService _itemService;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemsController"/>.
    /// </summary>
    /// <param name="itemService">Service handling item operations.</param>
    /// <param name="currentUserService">Service providing context about the current user.</param>
    public ItemsController(IItemService itemService, ICurrentUserService currentUserService)
    {
        this._itemService = itemService;
        this._currentUserService = currentUserService;
    }

    /// <summary>
    /// Retrieves a paginated list of items based on the provided filter.
    /// Adds an <c>X-Pagination</c> response header with pagination metadata.
    /// </summary>
    /// <param name="filter">Filtering options including category, listing type, search term, page, pageSize, and sort.</param>
    /// <returns>
    /// 200 OK with a paged result set of items.
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ItemFilterDto filter)
    {
        var result = await this._itemService.GetAllAsync(filter, this._currentUserService.UserId);
        this.Response.Headers.Append("X-Pagination",
            System.Text.Json.JsonSerializer.Serialize(new
            {
                result.TotalCount,
                result.Page,
                result.PageSize,
                result.TotalPages,
                result.HasPreviousPage,
                result.HasNextPage
            }));
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single item by its GUID and increments the item's view count.
    /// </summary>
    /// <param name="id">The GUID of the item to fetch.</param>
    /// <returns>
    /// 404 Not Found if the item does not exist.<br/>
    /// 200 OK with the item details on success.
    /// </returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await this._itemService.GetByIdAsync(id, this._currentUserService.UserId);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost("{id:guid}/view")]
    public async Task<IActionResult> IncrementView(Guid id)
    {
        await this._itemService.IncrementViewCountAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Creates a new item listing for the authenticated user.
    /// </summary>
    /// <param name="dto">Item creation payload including title, description, category, listing type, price, and photos.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 201 Created with the created item resource and location.
    /// </returns>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateItemDto dto)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        var item = await this._itemService.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    /// <summary>
    /// Updates an existing item listing owned by the authenticated user.
    /// </summary>
    /// <param name="id">The GUID of the item to update.</param>
    /// <param name="dto">Partial update payload for the item.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 403 Forbid if the user is not permitted to update the item.<br/>
    /// 404 Not Found if the item does not exist.<br/>
    /// 200 OK with the updated item on success.
    /// </returns>
    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateItemDto dto)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        try
        {
            var item = await this._itemService.UpdateAsync(id, dto, userId);
            return Ok(item);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, inner = ex.InnerException?.Message });
        }
    }

    /// <summary>
    /// Deletes an item listing. Only the author or an administrator may delete.
    /// </summary>
    /// <param name="id">The GUID of the item to delete.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 403 Forbid if the user is not permitted to delete the item.<br/>
    /// 404 Not Found if the item does not exist.<br/>
    /// 204 No Content on successful deletion.
    /// </returns>
    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        try
        {
            await this._itemService.DeleteAsync(id, userId, this._currentUserService.IsAdmin);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Toggles the like status of an item for the authenticated user.
    /// </summary>
    /// <param name="id">The GUID of the item to like or unlike.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 200 OK with a JSON body indicating the resulting like state: <c>{ isLiked }</c>.
    /// </returns>
    [Authorize]
    [HttpPost("{id:guid}/like")]
    public async Task<IActionResult> ToggleLike(Guid id)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        var isLiked = await this._itemService.ToggleLikeAsync(id, userId);
        return Ok(new { isLiked });
    }
}
