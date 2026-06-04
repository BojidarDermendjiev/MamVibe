namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

using Application.Interfaces;
using Application.DTOs.Offers;

/// <summary>
/// Authenticated endpoints for the buyer-seller price negotiation flow.
/// A buyer submits an offer; the seller may accept, decline, or counter with a different price;
/// the buyer may then accept or decline the counter. Either party may cancel at any point.
/// All endpoints require authentication and are subject to the global rate-limit policy.
/// </summary>
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

    /// <summary>
    /// Submits a new price offer from the authenticated buyer for a specific item.
    /// Only one open offer per buyer/item pair is allowed at a time.
    /// </summary>
    /// <param name="dto">The offer details including the target item ID and the proposed price.</param>
    /// <returns>
    /// 200 OK with the created offer.<br/>
    /// 401 Unauthorized if the caller is not authenticated.<br/>
    /// 404 Not Found if the target item does not exist.<br/>
    /// 409 Conflict if the buyer already has an open offer on this item.
    /// </returns>
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

    /// <summary>
    /// Seller accepts the buyer's offer, marking it as <c>Accepted</c>.
    /// Only the item owner may accept an offer directed at their listing.
    /// </summary>
    /// <param name="id">The GUID of the offer to accept.</param>
    /// <returns>
    /// 200 OK with the updated offer.<br/>
    /// 400 Bad Request if the offer is not in an acceptable state.<br/>
    /// 401 Unauthorized if the caller is not authenticated.<br/>
    /// 403 Forbidden if the caller is not the item owner.<br/>
    /// 404 Not Found if the offer does not exist.
    /// </returns>
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

    /// <summary>
    /// Seller declines the buyer's offer, marking it as <c>Declined</c>.
    /// Only the item owner may decline an offer directed at their listing.
    /// </summary>
    /// <param name="id">The GUID of the offer to decline.</param>
    /// <returns>
    /// 200 OK with the updated offer.<br/>
    /// 400 Bad Request if the offer is not in a declinable state.<br/>
    /// 401 Unauthorized if the caller is not authenticated.<br/>
    /// 403 Forbidden if the caller is not the item owner.<br/>
    /// 404 Not Found if the offer does not exist.
    /// </returns>
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

    /// <summary>
    /// Seller counters the buyer's offer with a different price.
    /// Moves the offer into <c>Countered</c> state; the buyer must respond.
    /// </summary>
    /// <param name="id">The GUID of the offer to counter.</param>
    /// <param name="dto">The counter-offer details containing the seller's proposed price.</param>
    /// <returns>
    /// 200 OK with the updated offer including the counter price.<br/>
    /// 400 Bad Request if the offer is not in a counterable state.<br/>
    /// 401 Unauthorized if the caller is not authenticated.<br/>
    /// 403 Forbidden if the caller is not the item owner.<br/>
    /// 404 Not Found if the offer does not exist.
    /// </returns>
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

    /// <summary>
    /// Buyer accepts the seller's counter-offer, finalising the negotiated price.
    /// Only the original buyer may accept a counter directed at them.
    /// </summary>
    /// <param name="id">The GUID of the offer whose counter to accept.</param>
    /// <returns>
    /// 200 OK with the updated offer.<br/>
    /// 400 Bad Request if the offer is not in <c>Countered</c> state.<br/>
    /// 401 Unauthorized if the caller is not authenticated.<br/>
    /// 403 Forbidden if the caller is not the original buyer.<br/>
    /// 404 Not Found if the offer does not exist.
    /// </returns>
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

    /// <summary>
    /// Buyer declines the seller's counter-offer, ending the negotiation.
    /// Only the original buyer may decline a counter directed at them.
    /// </summary>
    /// <param name="id">The GUID of the offer whose counter to decline.</param>
    /// <returns>
    /// 200 OK with the updated offer.<br/>
    /// 400 Bad Request if the offer is not in <c>Countered</c> state.<br/>
    /// 401 Unauthorized if the caller is not authenticated.<br/>
    /// 403 Forbidden if the caller is not the original buyer.<br/>
    /// 404 Not Found if the offer does not exist.
    /// </returns>
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

    /// <summary>
    /// Cancels an open offer. Either the buyer or the seller may cancel.
    /// </summary>
    /// <param name="id">The GUID of the offer to cancel.</param>
    /// <returns>
    /// 200 OK with the updated offer in <c>Cancelled</c> state.<br/>
    /// 400 Bad Request if the offer is already in a terminal state.<br/>
    /// 401 Unauthorized if the caller is not authenticated.<br/>
    /// 403 Forbidden if the caller is neither the buyer nor the seller.<br/>
    /// 404 Not Found if the offer does not exist.
    /// </returns>
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

    /// <summary>
    /// Returns all offers received by the authenticated user as a seller (offers on their listings).
    /// </summary>
    /// <returns>
    /// 200 OK with the list of received offers, ordered by creation date descending.<br/>
    /// 401 Unauthorized if the caller is not authenticated.
    /// </returns>
    [HttpGet("received")]
    public async Task<IActionResult> GetReceived()
    {
        var userId = this._currentUser.UserId;
        if (userId == null) return Unauthorized();
        return Ok(await this._service.GetReceivedAsync(userId));
    }

    /// <summary>
    /// Returns all offers submitted by the authenticated user as a buyer.
    /// </summary>
    /// <returns>
    /// 200 OK with the list of sent offers, ordered by creation date descending.<br/>
    /// 401 Unauthorized if the caller is not authenticated.
    /// </returns>
    [HttpGet("sent")]
    public async Task<IActionResult> GetSent()
    {
        var userId = this._currentUser.UserId;
        if (userId == null) return Unauthorized();
        return Ok(await this._service.GetSentAsync(userId));
    }
}
