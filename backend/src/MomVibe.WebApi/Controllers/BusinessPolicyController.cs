namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Application.Interfaces;
using Application.DTOs.Business;

/// <summary>
/// Public read of the current business-vertical policy + authenticated acceptance endpoint.
/// </summary>
[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/business/policy")]
public class BusinessPolicyController : ControllerBase
{
    private readonly IBusinessPolicyService _policy;
    private readonly ICurrentUserService _currentUser;

    public BusinessPolicyController(IBusinessPolicyService policy, ICurrentUserService currentUser)
    {
        _policy = policy;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Returns the current published policy version. Anonymous-safe so the modal can render
    /// during the signup flow before the user is fully authenticated.
    /// </summary>
    [HttpGet("current")]
    [AllowAnonymous]
    public async Task<ActionResult<BusinessPolicyDto>> GetCurrent([FromQuery] string? language = null)
    {
        var lang = language ?? Request.Headers["X-Language"].ToString();
        if (string.IsNullOrWhiteSpace(lang))
            lang = Request.Headers.AcceptLanguage.ToString();
        var dto = await _policy.GetCurrentAsync(lang ?? "en");
        return Ok(dto);
    }

    /// <summary>Records an explicit acceptance for the calling user.</summary>
    [HttpPost("accept")]
    [Authorize]
    public async Task<IActionResult> Accept([FromBody] AcceptPolicyRequest request)
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        await _policy.AcceptAsync(userId, request.PolicyVersionId, ip, userAgent);
        return NoContent();
    }
}

/// <summary>Request body for <see cref="BusinessPolicyController.Accept"/>.</summary>
public class AcceptPolicyRequest
{
    public Guid PolicyVersionId { get; set; }
}
