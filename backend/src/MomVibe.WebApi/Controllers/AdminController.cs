namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Application.Interfaces;
using Application.DTOs.Shipping;

/// <summary>
/// Admin-only API controller providing endpoints for:
/// - Dashboard statistics
/// - User management (list, block, unblock)
/// - Item moderation (delete)
/// Secured with the "AdminOnly" authorization policy.
/// </summary>

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IItemService _itemService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IShippingService _shippingService;
    private readonly IPaymentService _paymentService;

    public AdminController(
        IAdminService adminService,
        IItemService itemService,
        ICurrentUserService currentUserService,
        IShippingService shippingService,
        IPaymentService paymentService)
    {
        this._adminService = adminService;
        this._itemService = itemService;
        this._currentUserService = currentUserService;
        this._shippingService = shippingService;
        this._paymentService = paymentService;
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

    [HttpGet("items/pending")]
    public async Task<IActionResult> GetPendingItems()
    {
        var items = await this._adminService.GetPendingItemsAsync();
        return Ok(items);
    }

    [HttpPost("items/{id:guid}/approve")]
    public async Task<IActionResult> ApproveItem(Guid id)
    {
        await this._adminService.ApproveItemAsync(id);
        return NoContent();
    }

    // --- Feature 3: Admin shipping & payments ---

    [HttpGet("shipments")]
    public async Task<IActionResult> GetAllShipments()
    {
        var shipments = await this._shippingService.GetAllShipmentsAsync();
        return Ok(shipments);
    }

    [HttpGet("shipments/{id:guid}/track")]
    public async Task<IActionResult> TrackShipment(Guid id)
    {
        var events = await this._shippingService.TrackShipmentAsync(id, userId: "", isAdmin: true);
        return Ok(events);
    }

    [HttpGet("payments")]
    public async Task<IActionResult> GetAllPayments()
    {
        var payments = await this._paymentService.GetAllPaymentsAsync();
        return Ok(payments);
    }
}
