namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;

using Application.Interfaces;
using Application.DTOs.Turnstile;

/// <summary>
/// Public API controller for Cloudflare Turnstile CAPTCHA verification:
/// - Verify a client-provided Turnstile token and return verification status.
/// </summary>

[ApiController]
[Route("api/[controller]")]
public class TurnstileController : ControllerBase
{
    private readonly ITurnstileService _turnstileService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TurnstileController"/>.
    /// </summary>
    /// <param name="turnstileService">Service responsible for verifying Turnstile tokens.</param>
    public TurnstileController(ITurnstileService turnstileService)
    {
        this._turnstileService = turnstileService;
    }

    /// <summary>
    /// Verifies a Cloudflare Turnstile token.
    /// </summary>
    /// <param name="request">Request payload containing the Turnstile token.</param>
    /// <returns>
    /// 400 Bad Request with an error message if verification fails.<br/>
    /// 200 OK with <c>{ verified = true }</c> on successful verification.
    /// </returns>
    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] TurnstileVerifyRequestDto request)
    {
        var isValid = await this._turnstileService.VerifyTokenAsync(request.Token);

        if (!isValid)
        {
            return BadRequest(new { message = "Verification failed. Please try again." });
        }

        return Ok(new { verified = true });
    }
}
