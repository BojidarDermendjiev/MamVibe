namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Application.Interfaces;
using Application.DTOs.Moderation;
using Domain.Enums;

/// <summary>
/// Admin endpoints for the graded user-moderation system. All endpoints require the
/// <c>AdminOnly</c> policy.
/// </summary>
[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class AdminModerationController : ControllerBase
{
    private readonly IUserModerationService _moderation;
    private readonly IAbuseReportService _reports;
    private readonly IAbuseDetectionService _detection;
    private readonly IModerationAppealService _appeals;
    private readonly ICurrentUserService _currentUser;

    public AdminModerationController(
        IUserModerationService moderation,
        IAbuseReportService reports,
        IAbuseDetectionService detection,
        IModerationAppealService appeals,
        ICurrentUserService currentUser)
    {
        this._moderation = moderation;
        this._reports = reports;
        this._detection = detection;
        this._appeals = appeals;
        this._currentUser = currentUser;
    }

    /// <summary>
    /// Returns the moderation detail view for a single user: current state, history,
    /// and counts of open reports and unacknowledged signals.
    /// </summary>
    [HttpGet("users/{userId}/moderation")]
    public async Task<IActionResult> GetUserModeration(string userId)
    {
        var detail = await this._moderation.GetUserModerationAsync(userId);
        return detail is null ? NotFound() : Ok(detail);
    }

    /// <summary>
    /// Applies a moderation action to the target user. Body specifies new level, reason,
    /// public reason (shown to the user), optional internal note, and optional duration in minutes.
    /// </summary>
    [HttpPost("users/{userId}/moderate")]
    public async Task<IActionResult> ApplyModerationAction(string userId, [FromBody] ModerationActionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PublicReason))
            return BadRequest(new { message = "PublicReason is required." });

        var adminId = this._currentUser.UserId;
        if (adminId is null) return Unauthorized();
        var adminName = User.Identity?.Name ?? adminId;

        try
        {
            var logId = await this._moderation.ApplyActionAsync(userId, request, adminId, adminName);
            return Ok(new { moderationLogId = logId });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Manually clears the user's current moderation state, returning them to None.
    /// </summary>
    [HttpPost("users/{userId}/moderate/clear")]
    public async Task<IActionResult> ClearModerationAction(string userId, [FromBody] ModerationClearRequest request)
    {
        var adminId = this._currentUser.UserId;
        if (adminId is null) return Unauthorized();
        var adminName = User.Identity?.Name ?? adminId;

        try
        {
            await this._moderation.ManualClearAsync(userId, adminId, adminName, request.Reason ?? string.Empty);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Returns a paged slice of the abuse-report admin queue.</summary>
    [HttpGet("reports")]
    public async Task<IActionResult> GetReports(
        [FromQuery] ReportStatus? status = null,
        [FromQuery] ReportTargetType? targetType = null,
        [FromQuery] ModerationReason? reason = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await this._reports.GetAdminQueueAsync(
            new AdminReportFilter(status, targetType, reason, page, pageSize));
        return Ok(result);
    }

    /// <summary>Returns the detail view for a single abuse report.</summary>
    [HttpGet("reports/{id:guid}")]
    public async Task<IActionResult> GetReport(Guid id)
    {
        var report = await this._reports.GetReportAsync(id);
        return report is null ? NotFound() : Ok(report);
    }

    /// <summary>
    /// Resolves a report, optionally chaining a moderation action against the target user in
    /// the same workflow.
    /// </summary>
    [HttpPost("reports/{id:guid}/resolve")]
    public async Task<IActionResult> ResolveReport(Guid id, [FromBody] ResolveReportRequest request)
    {
        var adminId = this._currentUser.UserId;
        if (adminId is null) return Unauthorized();
        var adminName = User.Identity?.Name ?? adminId;

        try
        {
            await this._reports.ResolveAsync(id, request, adminId, adminName);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Returns the auto-detection abuse-signal admin queue.</summary>
    [HttpGet("abuse-signals")]
    public async Task<IActionResult> GetAbuseSignals(
        [FromQuery] bool includeAcknowledged = false,
        [FromQuery] AbuseSignalType? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await this._detection.GetAdminQueueAsync(
            new AbuseSignalFilter(includeAcknowledged, type, page, pageSize));
        return Ok(result);
    }

    /// <summary>Marks an abuse signal as acknowledged by the current admin.</summary>
    [HttpPost("abuse-signals/{id:guid}/acknowledge")]
    public async Task<IActionResult> AcknowledgeSignal(Guid id)
    {
        var adminId = this._currentUser.UserId;
        if (adminId is null) return Unauthorized();
        try
        {
            await this._detection.AcknowledgeAsync(id, adminId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Returns the moderation-appeals admin queue.</summary>
    [HttpGet("appeals")]
    public async Task<IActionResult> GetAppeals(
        [FromQuery] AppealStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await this._appeals.GetAdminQueueAsync(new AdminAppealFilter(status, page, pageSize));
        return Ok(result);
    }

    /// <summary>Returns the detail view for a single appeal.</summary>
    [HttpGet("appeals/{id:guid}")]
    public async Task<IActionResult> GetAppeal(Guid id)
    {
        var appeal = await this._appeals.GetAppealAsync(id);
        return appeal is null ? NotFound() : Ok(appeal);
    }

    /// <summary>Approves or rejects an appeal. Approval also clears the user's active moderation.</summary>
    [HttpPost("appeals/{id:guid}/decide")]
    public async Task<IActionResult> DecideAppeal(Guid id, [FromBody] DecideAppealRequest request)
    {
        var adminId = this._currentUser.UserId;
        if (adminId is null) return Unauthorized();
        var adminName = User.Identity?.Name ?? adminId;
        try
        {
            await this._appeals.DecideAsync(id, request, adminId, adminName);
            return NoContent();
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }
}
