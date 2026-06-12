namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Application.Interfaces;
using Application.DTOs.Business;
using Domain.Enums;

/// <summary>
/// Admin-only endpoints for the business vertical: profile + listing queues, approve /
/// suspend / remove mutations, and the revenue KPI snapshot. All requests must pass the
/// <c>AdminOnly</c> policy applied at the controller level.
/// </summary>
[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/business")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class AdminBusinessController : ControllerBase
{
    private readonly IBusinessAdminService _admin;
    private readonly ICurrentUserService _currentUser;

    public AdminBusinessController(IBusinessAdminService admin, ICurrentUserService currentUser)
    {
        _admin = admin;
        _currentUser = currentUser;
    }

    [HttpGet("profiles")]
    public async Task<ActionResult<PagedAdminProfilesResult>> ListProfiles(
        [FromQuery] BusinessCategory? category = null,
        [FromQuery] BusinessProfileStatus? status = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var result = await _admin.ListProfilesAsync(new AdminProfileFilter
        {
            Category = category,
            Status = status,
            Search = search,
            Page = page,
            PageSize = pageSize,
        });
        return Ok(result);
    }

    [HttpPost("profiles/{id:guid}/suspend")]
    public async Task<IActionResult> Suspend(Guid id, [FromBody] AdminSuspendProfileRequest request)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        await _admin.SuspendProfileAsync(id, adminId, request.Reason);
        return NoContent();
    }

    [HttpPost("profiles/{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        await _admin.RestoreProfileAsync(id, adminId);
        return NoContent();
    }

    [HttpPost("profiles/{id:guid}/remove")]
    public async Task<IActionResult> Remove(Guid id, [FromBody] AdminSuspendProfileRequest request)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        await _admin.RemoveProfileAsync(id, adminId, request.Reason);
        return NoContent();
    }

    [HttpGet("listings")]
    public async Task<ActionResult<PagedAdminListingsResult>> ListListings(
        [FromQuery] BusinessCategory? category = null,
        [FromQuery] bool? isApproved = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var result = await _admin.ListListingsAsync(new AdminListingFilter
        {
            Category = category,
            IsApproved = isApproved,
            IsActive = isActive,
            Search = search,
            Page = page,
            PageSize = pageSize,
        });
        return Ok(result);
    }

    [HttpPost("listings/{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        await _admin.ApproveListingAsync(id, adminId);
        return NoContent();
    }

    [HttpPost("listings/{id:guid}/unapprove")]
    public async Task<IActionResult> Unapprove(Guid id, [FromBody] AdminSuspendProfileRequest request)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        await _admin.UnapproveListingAsync(id, adminId, request.Reason);
        return NoContent();
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<BusinessRevenueDto>> Revenue()
    {
        var stats = await _admin.GetRevenueAsync();
        return Ok(stats);
    }
}
