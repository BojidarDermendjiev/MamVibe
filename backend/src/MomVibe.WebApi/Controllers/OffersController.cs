namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

using Application.Interfaces;
using Application.DTOs.Offers;

[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/offers")]
[Authorize]
[EnableRateLimiting(RateLimitPolicies.Global)]
public class OffersController : ControllerBase
{
    private readonly IOfferService _service;
    private readonly ICurrentUserService _currentUser;

    public OffersController(IOfferService service, ICurrentUserService currentUser)
    {
        this._service = service;
        this._currentUser = currentUser;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOfferDto dto)
    {
        var userId = this._currentUser.UserId;
        if (userId == null) return Unauthorized();
        try
        {
            var result = await this._service.CreateAsync(dto, userId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
    }

    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> Accept(Guid id)
    {
        var userId = this._currentUser.UserId;
        if (userId == null) return Unauthorized();
        try { return Ok(await this._service.AcceptAsync(id, userId)); }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("{id:guid}/decline")]
    public async Task<IActionResult> Decline(Guid id)
    {
        var userId = this._currentUser.UserId;
        if (userId == null) return Unauthorized();
        try { return Ok(await this._service.DeclineAsync(id, userId)); }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("{id:guid}/counter")]
    public async Task<IActionResult> Counter(Guid id, [FromBody] CounterOfferDto dto)
    {
        var userId = this._currentUser.UserId;
        if (userId == null) return Unauthorized();
        try { return Ok(await this._service.CounterAsync(id, userId, dto.CounterPrice)); }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("{id:guid}/accept-counter")]
    public async Task<IActionResult> AcceptCounter(Guid id)
    {
        var userId = this._currentUser.UserId;
        if (userId == null) return Unauthorized();
        try { return Ok(await this._service.AcceptCounterAsync(id, userId)); }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("{id:guid}/decline-counter")]
    public async Task<IActionResult> DeclineCounter(Guid id)
    {
        var userId = this._currentUser.UserId;
        if (userId == null) return Unauthorized();
        try { return Ok(await this._service.DeclineCounterAsync(id, userId)); }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var userId = this._currentUser.UserId;
        if (userId == null) return Unauthorized();
        try { return Ok(await this._service.CancelAsync(id, userId)); }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpGet("received")]
    public async Task<IActionResult> GetReceived()
    {
        var userId = this._currentUser.UserId;
        if (userId == null) return Unauthorized();
        return Ok(await this._service.GetReceivedAsync(userId));
    }

    [HttpGet("sent")]
    public async Task<IActionResult> GetSent()
    {
        var userId = this._currentUser.UserId;
        if (userId == null) return Unauthorized();
        return Ok(await this._service.GetSentAsync(userId));
    }
}
