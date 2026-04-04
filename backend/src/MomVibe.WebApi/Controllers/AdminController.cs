namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Domain.Enums;
using Domain.Exceptions;
using Application.Interfaces;
using Application.DTOs.Wallet;

/// <summary>
/// Admin-only API controller providing endpoints for:
/// - Dashboard statistics
/// - User management (list, block, unblock)
/// - Item moderation (delete)
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
    private readonly IWalletService _walletService;

    public AdminController(
        IAdminService adminService,
        IItemService itemService,
        ICurrentUserService currentUserService,
        IShippingService shippingService,
        IPaymentService paymentService,
        IWalletService walletService)
    {
        this._adminService = adminService;
        this._itemService = itemService;
        this._currentUserService = currentUserService;
        this._shippingService = shippingService;
        this._paymentService = paymentService;
        this._walletService = walletService;
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
    public async Task<IActionResult> GetPendingItems([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var items = await this._adminService.GetPendingItemsAsync(page, pageSize);
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
    public async Task<IActionResult> GetAllShipments([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var shipments = await this._shippingService.GetAllShipmentsAsync(page, pageSize);
        return Ok(shipments);
    }

    [HttpGet("shipments/{id:guid}/track")]
    public async Task<IActionResult> TrackShipment(Guid id)
    {
        var events = await this._shippingService.TrackShipmentAsync(id, userId: this._currentUserService.UserId ?? "", isAdmin: true);
        return Ok(events);
    }

    [HttpGet("payments")]
    public async Task<IActionResult> GetAllPayments()
    {
        var payments = await this._paymentService.GetAllPaymentsAsync();
        return Ok(payments);
    }

    // =========================================================================
    // Wallet monitoring & control
    // =========================================================================

    /// <summary>
    /// Returns all user wallets, optionally filtered by status.
    /// Includes live balance and transaction count per wallet.
    /// </summary>
    [HttpGet("wallets")]
    public async Task<IActionResult> GetAllWallets(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] WalletStatus? status = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var result = await this._walletService.GetAllWalletsAsync(page, pageSize, status);
        return Ok(result);
    }

    /// <summary>
    /// Returns a single wallet by ID with live balance and transaction count.
    /// </summary>
    [HttpGet("wallets/{id:guid}")]
    public async Task<IActionResult> GetWalletById(Guid id)
    {
        try
        {
            var wallet = await this._walletService.GetWalletByIdAsync(id);
            return Ok(wallet);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Freezes a wallet. All operations on the wallet are blocked immediately.
    /// A reason is required and stored for audit purposes.
    /// </summary>
    [HttpPut("wallets/{id:guid}/freeze")]
    public async Task<IActionResult> FreezeWallet(Guid id, [FromBody] FreezeWalletDto request)
    {
        try
        {
            await this._walletService.FreezeWalletAsync(id, request.Reason);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Restores a frozen or suspended wallet to Active status.
    /// </summary>
    [HttpPut("wallets/{id:guid}/unfreeze")]
    public async Task<IActionResult> UnfreezeWallet(Guid id)
    {
        try
        {
            await this._walletService.UnfreezeWalletAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Returns all wallet transactions across all users with optional filters.
    /// Supports filtering by userId, walletId, kind, status, type, date range, and amount range.
    /// </summary>
    [HttpGet("wallet-transactions")]
    public async Task<IActionResult> GetAllWalletTransactions(
        [FromQuery] string? userId = null,
        [FromQuery] Guid? walletId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] WalletTransactionKind? kind = null,
        [FromQuery] WalletTransactionStatus? status = null,
        [FromQuery] WalletTransactionType? type = null,
        [FromQuery] decimal? minAmount = null,
        [FromQuery] decimal? maxAmount = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var filter = new TransactionFilterDto
        {
            UserId = userId,
            WalletId = walletId,
            DateFrom = dateFrom,
            DateTo = dateTo,
            Kind = kind,
            Status = status,
            Type = type,
            MinAmount = minAmount,
            MaxAmount = maxAmount,
            Page = page,
            PageSize = pageSize
        };

        var result = await this._walletService.GetAllTransactionsAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Reverses a completed debit transaction and credits the funds back to the wallet.
    /// A TakeANap fiscal receipt is generated for the refund credit automatically.
    /// </summary>
    [HttpPost("wallet-transactions/{id:guid}/refund")]
    public async Task<IActionResult> RefundWalletTransaction(Guid id, [FromBody] RefundRequestDto request)
    {
        var adminId = this._currentUserService.UserId;
        if (adminId == null) return Unauthorized();

        try
        {
            var tx = await this._walletService.RefundTransactionAsync(id, adminId, request.Reason);
            return Ok(tx);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Returns all pending withdrawal requests, oldest first (FIFO processing order).
    /// </summary>
    [HttpGet("withdrawals")]
    public async Task<IActionResult> GetPendingWithdrawals(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var result = await this._walletService.GetPendingWithdrawalsAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Marks a pending withdrawal as completed (funds transferred to IBAN by admin).
    /// </summary>
    [HttpPost("withdrawals/{id:guid}/approve")]
    public async Task<IActionResult> ApproveWithdrawal(Guid id)
    {
        var adminId = this._currentUserService.UserId;
        if (adminId == null) return Unauthorized();

        try
        {
            await this._walletService.ApproveWithdrawalAsync(id, adminId);
            return NoContent();
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Rejects a pending withdrawal request and returns the reserved funds to the wallet.
    /// </summary>
    [HttpPost("withdrawals/{id:guid}/reject")]
    public async Task<IActionResult> RejectWithdrawal(Guid id, [FromBody] RefundRequestDto request)
    {
        var adminId = this._currentUserService.UserId;
        if (adminId == null) return Unauthorized();

        try
        {
            await this._walletService.RejectWithdrawalAsync(id, adminId, request.Reason);
            return NoContent();
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
