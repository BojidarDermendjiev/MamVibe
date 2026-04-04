namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Domain.Exceptions;
using Application.Interfaces;
using Application.DTOs.Wallet;

/// <summary>
/// User-facing wallet endpoints.
/// All routes require an authenticated user except the Stripe top-up webhook,
/// which is called directly by Stripe and verified via HMAC signature.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;
    private readonly ICurrentUserService _currentUserService;

    public WalletController(IWalletService walletService, ICurrentUserService currentUserService)
    {
        _walletService = walletService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Returns the authenticated user's wallet, creating it automatically on first call.
    /// Response includes the live balance derived from the transaction ledger.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetWallet()
    {
        var userId = _currentUserService.UserId;
        if (userId == null) return Unauthorized();

        var wallet = await _walletService.GetOrCreateWalletAsync(userId);
        return Ok(wallet);
    }

    /// <summary>
    /// Returns a paginated transaction history for the authenticated user's wallet,
    /// newest first.
    /// </summary>
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = _currentUserService.UserId;
        if (userId == null) return Unauthorized();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var result = await _walletService.GetTransactionsAsync(userId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Creates a Stripe PaymentIntent for a wallet top-up.
    /// Returns a <c>clientSecret</c> that the frontend passes to Stripe Elements
    /// to render an inline card form — no redirect required.
    /// The wallet is credited only after the Stripe webhook confirms payment.
    /// </summary>
    [HttpPost("topup")]
    public async Task<IActionResult> CreateTopUp([FromBody] TopUpRequestDto request)
    {
        var userId = _currentUserService.UserId;
        if (userId == null) return Unauthorized();

        try
        {
            var result = await _walletService.CreateTopUpIntentAsync(userId, request.Amount);
            return Ok(result);
        }
        catch (WalletFrozenException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Top-up service error. Please try again later." });
        }
    }

    /// <summary>
    /// Stripe webhook endpoint for wallet top-up PaymentIntent events.
    /// Called directly by Stripe — no user authentication, HMAC-verified instead.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("topup/webhook")]
    [RequestSizeLimit(65_536)]
    public async Task<IActionResult> TopUpWebhook()
    {
        using var reader = new StreamReader(HttpContext.Request.Body);
        var json = await reader.ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault() ?? "";

        try
        {
            await _walletService.HandleTopUpWebhookAsync(json, signature);
            return Ok();
        }
        catch (Stripe.StripeException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Transfers funds from the authenticated user's wallet to another user identified by email.
    /// Runs as a serializable database transaction — safe against concurrent double-spend.
    /// </summary>
    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequestDto request)
    {
        var userId = _currentUserService.UserId;
        if (userId == null) return Unauthorized();

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        try
        {
            var result = await _walletService.TransferAsync(
                userId,
                request.ReceiverEmail,
                request.Amount,
                request.Note,
                ip);
            return Ok(result);
        }
        catch (WalletFrozenException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InsufficientFundsException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Transfer service error. Please try again later." });
        }
    }

    /// <summary>
    /// Pays for a marketplace item directly from the authenticated user's wallet balance.
    /// Creates a Payment record (method = Wallet) and debits the wallet atomically.
    /// A TakeANap fiscal receipt is generated automatically.
    /// </summary>
    [HttpPost("pay/{itemId:guid}")]
    public async Task<IActionResult> PayForItem(Guid itemId)
    {
        var userId = _currentUserService.UserId;
        if (userId == null) return Unauthorized();

        try
        {
            var tx = await _walletService.PayForItemFromWalletAsync(userId, itemId);
            return Ok(tx);
        }
        catch (WalletFrozenException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InsufficientFundsException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Payment service error. Please try again later." });
        }
    }

    /// <summary>
    /// Queues a withdrawal of funds to the authenticated user's registered IBAN.
    /// Reserves the balance immediately (Pending debit) — an admin must approve
    /// before the bank transfer is executed.
    /// Requires an IBAN to be saved on the user's profile.
    /// </summary>
    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawRequestDto request)
    {
        var userId = _currentUserService.UserId;
        if (userId == null) return Unauthorized();

        try
        {
            var tx = await _walletService.WithdrawAsync(userId, request.Amount);
            return Ok(tx);
        }
        catch (WalletFrozenException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InsufficientFundsException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Withdrawal service error. Please try again later." });
        }
    }
}
