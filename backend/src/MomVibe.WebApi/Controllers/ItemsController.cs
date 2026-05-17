namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using FluentValidation;

using Application.DTOs.Items;
using Application.Interfaces;

/// <summary>
/// Public and authenticated endpoints for managing item listings:
/// - Browse items with filtering and pagination (adds X-Pagination header)
/// - Retrieve item details (increments view count)
/// - Create, update, and delete items (authenticated; with authorization checks)
/// - Toggle like status on an item (authenticated)
/// - AI listing assistant: analyze a photo and return prefilled listing suggestions
/// </summary>

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IItemService _itemService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<CreateItemDto> _createItemValidator;
    private readonly IValidator<UpdateItemDto> _updateItemValidator;
    private readonly IAiService _aiService;

    public ItemsController(
        IItemService itemService,
        ICurrentUserService currentUserService,
        IValidator<CreateItemDto> createItemValidator,
        IValidator<UpdateItemDto> updateItemValidator,
        IAiService aiService)
    {
        this._itemService = itemService;
        this._currentUserService = currentUserService;
        this._createItemValidator = createItemValidator;
        this._updateItemValidator = updateItemValidator;
        this._aiService = aiService;
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

    /// <summary>
    /// Increments the view count for the specified item.
    /// Intended to be called by the client after rendering an item detail page.
    /// Rate-limited to 30 increments per IP per minute to prevent artificial view inflation.
    /// </summary>
    /// <param name="id">The GUID of the item whose view count should be incremented.</param>
    /// <returns>
    /// 204 No Content on success.
    /// </returns>
    [HttpPost("{id:guid}/view")]
    [EnableRateLimiting(RateLimitPolicies.IncrementView)]
    public async Task<IActionResult> IncrementView(Guid id)
    {
        await this._itemService.IncrementViewCountAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Checks the seller's reputation on nekorekten.com for the item's owner.
    /// Returns any fraud reports found for the seller.
    /// </summary>
    /// <param name="id">The GUID of the item whose seller should be checked.</param>
    /// <returns>
    /// 404 Not Found if the item does not exist.<br/>
    /// 200 OK with the seller reputation check result on success.
    /// </returns>
    [Authorize]
    [HttpGet("{id:guid}/seller-check")]
    public async Task<IActionResult> CheckSeller(Guid id)
    {
        try
        {
            var result = await this._itemService.CheckSellerAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Analyzes a product photo with Claude AI and returns prefilled listing suggestions.
    /// </summary>
    /// <param name="photo">The product photo to analyze (JPEG, PNG, or WebP, max 5 MB).</param>
    /// <returns>
    /// 400 Bad Request if the photo is missing, too large, or in an unsupported format.<br/>
    /// 200 OK with <see cref="AiListingSuggestionDto"/> on success.
    /// </returns>
    [Authorize]
    [HttpPost("ai-suggest")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AiSuggest(IFormFile photo)
    {
        if (photo is null || photo.Length == 0)
            return BadRequest(new { error = "A photo is required." });

        try
        {
            var suggestion = await this._aiService.SuggestListingAsync(photo);
            return Ok(suggestion);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (HttpRequestException)
        {
            return StatusCode(502, new { error = "AI service is currently unavailable. Please try again later." });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "AI suggestion failed. Please try again later." });
        }
    }

    /// <summary>
    /// Suggests a fair selling price based on comparable active listings and item context.
    /// </summary>
    /// <param name="dto">Request payload describing the item for which a price suggestion is needed.</param>
    /// <returns>
    /// 502 Bad Gateway if the AI service is unavailable.<br/>
    /// 500 Internal Server Error if price suggestion fails unexpectedly.<br/>
    /// 200 OK with the suggested price details on success.
    /// </returns>
    [Authorize]
    [HttpPost("suggest-price")]
    public async Task<IActionResult> SuggestPrice([FromBody] PriceSuggestionRequestDto dto)
    {
        try
        {
            var result = await this._itemService.SuggestPriceAsync(dto);
            return Ok(result);
        }
        catch (HttpRequestException)
        {
            return StatusCode(502, new { error = "AI service is currently unavailable. Please try again later." });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Price suggestion failed. Please try again later." });
        }
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
        var validation = await this._createItemValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

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

        var validation = await this._updateItemValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

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
        catch (Exception)
        {
            return StatusCode(500, new { error = "An unexpected error occurred." });
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
