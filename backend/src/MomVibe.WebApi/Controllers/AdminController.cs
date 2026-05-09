namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using Domain.Enums;
using Domain.Entities;
using Application.Interfaces;
using Application.DTOs.DoctorReviews;
using Application.DTOs.ChildFriendlyPlaces;

/// <summary>
/// Admin-only API controller providing endpoints for:
/// - Dashboard statistics
/// - User management (list, block, unblock)
/// - Item moderation (delete, approve)
/// - Shipping and payments oversight
/// Secured with the "AdminOnly" authorization policy.
/// </summary>

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IItemService _itemService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IShippingService _shippingService;
    private readonly IPaymentService _paymentService;
    private readonly IDoctorReviewService _doctorReviewService;
    private readonly IChildFriendlyPlaceService _childFriendlyPlaceService;
    private readonly IApplicationDbContext _context;

    private static readonly string[] AllowedModels =
    [
        "claude-haiku-4-5-20251001",
        "claude-sonnet-4-6",
        "claude-opus-4-5"
    ];

    public AdminController(
        IAdminService adminService,
        IItemService itemService,
        ICurrentUserService currentUserService,
        IShippingService shippingService,
        IPaymentService paymentService,
        IDoctorReviewService doctorReviewService,
        IChildFriendlyPlaceService childFriendlyPlaceService,
        IApplicationDbContext context)
    {
        this._adminService = adminService;
        this._itemService = itemService;
        this._currentUserService = currentUserService;
        this._shippingService = shippingService;
        this._paymentService = paymentService;
        this._doctorReviewService = doctorReviewService;
        this._childFriendlyPlaceService = childFriendlyPlaceService;
        this._context = context;
    }

    /// <summary>
    /// Retrieves aggregated statistics for the admin dashboard.
    /// </summary>
    /// <returns>
    /// 200 OK with the dashboard statistics payload.
    /// </returns>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var stats = await this._adminService.GetDashboardStatsAsync();
        return Ok(stats);
    }

    /// <summary>
    /// Retrieves a paginated list of users with optional search.
    /// </summary>
    /// <param name="page">1-based page number (default: 1).</param>
    /// <param name="pageSize">Number of users per page (default: 20).</param>
    /// <param name="search">Optional search term to filter users.</param>
    /// <returns>
    /// 200 OK with a paged result of users.
    /// </returns>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var result = await this._adminService.GetAllUsersAsync(page, pageSize, search);
        return Ok(result);
    }

    /// <summary>
    /// Blocks a user account, preventing access to the application.
    /// </summary>
    /// <param name="userId">The identifier of the user to block.</param>
    /// <returns>
    /// 204 No Content on success.
    /// </returns>
    [HttpPost("users/{userId}/block")]
    public async Task<IActionResult> BlockUser(string userId)
    {
        await this._adminService.BlockUserAsync(userId);
        return NoContent();
    }

    /// <summary>
    /// Unblocks a previously blocked user account, restoring access.
    /// </summary>
    /// <param name="userId">The identifier of the user to unblock.</param>
    /// <returns>
    /// 204 No Content on success.
    /// </returns>
    [HttpPost("users/{userId}/unblock")]
    public async Task<IActionResult> UnblockUser(string userId)
    {
        await this._adminService.UnblockUserAsync(userId);
        return NoContent();
    }

    /// <summary>
    /// Deletes an item listing as an administrator.
    /// </summary>
    /// <param name="id">The GUID of the item to delete.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 204 No Content on successful deletion.
    /// </returns>
    [HttpDelete("items/{id:guid}")]
    public async Task<IActionResult> DeleteItem(Guid id)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        await this._itemService.DeleteAsync(id, userId, isAdmin: true);
        return NoContent();
    }

    // --- Feature 2: Item approval ---

    /// <summary>
    /// Retrieves a paginated list of item listings awaiting admin approval.
    /// </summary>
    /// <param name="page">1-based page number (default: 1).</param>
    /// <param name="pageSize">Number of items per page (default: 50).</param>
    /// <returns>
    /// 200 OK with a paged result of pending items.
    /// </returns>
    [HttpGet("items/pending")]
    public async Task<IActionResult> GetPendingItems([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var items = await this._adminService.GetPendingItemsAsync(page, pageSize);
        return Ok(items);
    }

    /// <summary>
    /// Approves a pending item listing, making it publicly visible in the marketplace.
    /// </summary>
    /// <param name="id">The GUID of the item to approve.</param>
    /// <returns>
    /// 204 No Content on success.
    /// </returns>
    [HttpPost("items/{id:guid}/approve")]
    public async Task<IActionResult> ApproveItem(Guid id)
    {
        await this._adminService.ApproveItemAsync(id);
        return NoContent();
    }

    // --- Feature 3: Admin shipping & payments ---

    /// <summary>
    /// Retrieves a paginated list of all shipments across all users.
    /// </summary>
    /// <param name="page">1-based page number (default: 1).</param>
    /// <param name="pageSize">Number of shipments per page (default: 50).</param>
    /// <returns>
    /// 200 OK with a paged result of shipments.
    /// </returns>
    [HttpGet("shipments")]
    public async Task<IActionResult> GetAllShipments([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var shipments = await this._shippingService.GetAllShipmentsAsync(page, pageSize);
        return Ok(shipments);
    }

    /// <summary>
    /// Retrieves tracking events for the specified shipment with admin-level access.
    /// </summary>
    /// <param name="id">The GUID of the shipment to track.</param>
    /// <returns>
    /// 200 OK with a list of tracking events.
    /// </returns>
    [HttpGet("shipments/{id:guid}/track")]
    public async Task<IActionResult> TrackShipment(Guid id)
    {
        var events = await this._shippingService.TrackShipmentAsync(id, userId: this._currentUserService.UserId ?? "", isAdmin: true);
        return Ok(events);
    }

    /// <summary>
    /// Retrieves all payment records across all users for admin oversight.
    /// </summary>
    /// <returns>
    /// 200 OK with the complete list of payments.
    /// </returns>
    [HttpGet("payments")]
    public async Task<IActionResult> GetAllPayments()
    {
        var payments = await this._paymentService.GetAllPaymentsAsync();
        return Ok(payments);
    }

    // --- Community moderation: Doctor Reviews ---

    /// <summary>
    /// Retrieves all doctor reviews that are pending admin approval.
    /// </summary>
    /// <returns>
    /// 200 OK with the list of pending doctor reviews.
    /// </returns>
    [HttpGet("doctor-reviews/pending")]
    public async Task<IActionResult> GetPendingDoctorReviews()
    {
        var reviews = await this._doctorReviewService.GetPendingAsync();
        return Ok(reviews);
    }

    /// <summary>
    /// Approves a pending doctor review, making it publicly visible.
    /// </summary>
    /// <param name="id">The GUID of the doctor review to approve.</param>
    /// <returns>
    /// 404 Not Found if the review does not exist.<br/>
    /// 204 No Content on success.
    /// </returns>
    [HttpPost("doctor-reviews/{id:guid}/approve")]
    public async Task<IActionResult> ApproveDoctorReview(Guid id)
    {
        try
        {
            await this._doctorReviewService.ApproveAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>
    /// Deletes a doctor review as an administrator.
    /// </summary>
    /// <param name="id">The GUID of the doctor review to delete.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 404 Not Found if the review does not exist.<br/>
    /// 204 No Content on successful deletion.
    /// </returns>
    [HttpDelete("doctor-reviews/{id:guid}")]
    public async Task<IActionResult> DeleteDoctorReview(Guid id)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        try
        {
            await this._doctorReviewService.DeleteAsync(id, userId, isAdmin: true);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    // --- Community moderation: Child-Friendly Places ---

    /// <summary>
    /// Retrieves all child-friendly places that are pending admin approval.
    /// </summary>
    /// <returns>
    /// 200 OK with the list of pending child-friendly places.
    /// </returns>
    [HttpGet("child-friendly-places/pending")]
    public async Task<IActionResult> GetPendingChildFriendlyPlaces()
    {
        var places = await this._childFriendlyPlaceService.GetPendingAsync();
        return Ok(places);
    }

    /// <summary>
    /// Approves a pending child-friendly place, making it publicly visible.
    /// </summary>
    /// <param name="id">The GUID of the place to approve.</param>
    /// <returns>
    /// 404 Not Found if the place does not exist.<br/>
    /// 204 No Content on success.
    /// </returns>
    [HttpPost("child-friendly-places/{id:guid}/approve")]
    public async Task<IActionResult> ApproveChildFriendlyPlace(Guid id)
    {
        try
        {
            await this._childFriendlyPlaceService.ApproveAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>
    /// Deletes a child-friendly place as an administrator.
    /// </summary>
    /// <param name="id">The GUID of the place to delete.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 404 Not Found if the place does not exist.<br/>
    /// 204 No Content on successful deletion.
    /// </returns>
    [HttpDelete("child-friendly-places/{id:guid}")]
    public async Task<IActionResult> DeleteChildFriendlyPlace(Guid id)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        try
        {
            await this._childFriendlyPlaceService.DeleteAsync(id, userId, isAdmin: true);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    // --- AI Settings ---

    /// <summary>
    /// Retrieves the current AI model configuration and the list of allowed models.
    /// </summary>
    /// <returns>
    /// 200 OK with the active model identifier and the available model list.
    /// </returns>
    [HttpGet("ai-settings")]
    public async Task<IActionResult> GetAiSettings()
    {
        var setting = await this._context.AppSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == "AI:Model");

        return Ok(new
        {
            model = setting?.Value ?? "claude-haiku-4-5-20251001",
            availableModels = AllowedModels
        });
    }

    /// <summary>
    /// Updates the AI model used by the platform's listing assistant and price suggester.
    /// Only models from the approved allowlist are accepted.
    /// </summary>
    /// <param name="dto">Payload containing the model identifier to activate.</param>
    /// <returns>
    /// 400 Bad Request if the requested model is not in the allowed list.<br/>
    /// 204 No Content on successful update.
    /// </returns>
    [HttpPut("ai-settings")]
    public async Task<IActionResult> UpdateAiSettings([FromBody] AiSettingsUpdateDto dto)
    {
        if (!AllowedModels.Contains(dto.Model))
            return BadRequest("Unknown model. Allowed: " + string.Join(", ", AllowedModels));

        var setting = await this._context.AppSettings
            .FirstOrDefaultAsync(s => s.Key == "AI:Model");

        if (setting is null)
        {
            this._context.AppSettings.Add(new AppSetting
            {
                Key = "AI:Model",
                Value = dto.Model,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            setting.Value = dto.Model;
            setting.UpdatedAt = DateTime.UtcNow;
        }

        await this._context.SaveChangesAsync(CancellationToken.None);
        return NoContent();
    }
}

public record AiSettingsUpdateDto(string Model);
