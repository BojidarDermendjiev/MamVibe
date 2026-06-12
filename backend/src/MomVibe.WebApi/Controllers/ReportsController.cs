namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using Application.Interfaces;
using Application.DTOs.Moderation;

/// <summary>
/// Authenticated user-facing endpoint for submitting abuse reports against users, items,
/// messages, or message threads. Per-user rate-limited via <see cref="RateLimitPolicies.ReportSubmit"/>.
/// </summary>
[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IAbuseReportService _reports;
    private readonly ICurrentUserService _currentUser;

    public ReportsController(IAbuseReportService reports, ICurrentUserService currentUser)
    {
        this._reports = reports;
        this._currentUser = currentUser;
    }

    private string? ClientIp => this.HttpContext.Connection.RemoteIpAddress?.ToString();

    /// <summary>Submits a new abuse report. Returns 201 with the report id on success.</summary>
    [HttpPost]
    [EnableRateLimiting(RateLimitPolicies.ReportSubmit)]
    public async Task<IActionResult> Submit([FromBody] SubmitReportRequest request)
    {
        var reporterId = this._currentUser.UserId;
        if (reporterId is null) return Unauthorized();

        try
        {
            var id = await this._reports.SubmitAsync(request, reporterId, this.ClientIp);
            return CreatedAtAction(nameof(Submit), new { id }, new { id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
