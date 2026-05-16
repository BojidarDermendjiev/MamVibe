namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Domain.Entities;
using Domain.Enums;
using Application.Interfaces;

/// <summary>
/// Development-only helpers for seeding demo data while testing the UI.
/// All endpoints return 404 in non-Development environments.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DevController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public DevController(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment env)
    {
        _context = context;
        _currentUserService = currentUserService;
        _userManager = userManager;
        _env = env;
    }

    /// <summary>
    /// Seeds a complete demo order for the given user so the Dashboard
    /// shows both a "To Send" (seller) and an "Incoming" (buyer) shipment.
    /// Pass ?userId=xxx to target a specific account, otherwise falls back
    /// to the authenticated user, then to admin@mamvibe.com.
    /// Development environment only.
    /// </summary>
    [HttpPost("seed-demo-order")]
    public async Task<IActionResult> SeedDemoOrder([FromQuery] string? email = null)
    {
        if (!_env.IsDevelopment())
            return NotFound();

        string? userId = _currentUserService.UserId;

        if (userId == null && email != null)
        {
            var byEmail = await _userManager.FindByEmailAsync(email);
            if (byEmail == null)
                return BadRequest(new { error = $"No user found with email '{email}'." });
            userId = byEmail.Id;
        }

        if (userId == null)
            return BadRequest(new { error = "Pass ?email=your@email.com as a query parameter." });

        const string demoSellerId = "demo-user-sofia-001";
        const string demoBuyerId  = "demo-user-plovdiv-002";

        // ── 1. Current user as BUYER: demo seller → current user ────────────
        var buyerItem = await _context.Items
            .Where(i => i.UserId == demoSellerId && i.ListingType == ListingType.Sell && i.IsActive)
            .FirstOrDefaultAsync();

        if (buyerItem == null)
        {
            var cat = await _context.Categories.FirstOrDefaultAsync();
            if (cat == null)
                return BadRequest(new { error = "No categories found — run the app once to seed them." });

            buyerItem = new Item
            {
                Title              = "Demo Item (Incoming)",
                Description        = "Seeded for UI demo.",
                CategoryId         = cat.Id,
                ListingType        = ListingType.Sell,
                Price              = 25.00m,
                UserId             = demoSellerId,
                IsActive           = true,
                AiModerationStatus = AiModerationStatus.AutoApproved,
            };
            _context.Items.Add(buyerItem);
            await _context.SaveChangesAsync();
        }

        var buyerPayment = new Payment
        {
            ItemId        = buyerItem.Id,
            BuyerId       = userId,
            SellerId      = demoSellerId,
            Amount        = buyerItem.Price ?? 25m,
            PaymentMethod = PaymentMethod.Card,
            Status        = PaymentStatus.Completed,
        };
        _context.Payments.Add(buyerPayment);
        await _context.SaveChangesAsync();

        _context.Shipments.Add(new Shipment
        {
            PaymentId       = buyerPayment.Id,
            CourierProvider = CourierProvider.Econt,
            DeliveryType    = DeliveryType.Office,
            Status          = ShipmentStatus.InTransit,
            TrackingNumber  = $"DEMO-IN-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            WaybillId       = $"WB-DEMO-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            RecipientName   = "You (Demo Buyer)",
            RecipientPhone  = "+359888000001",
            OfficeName      = "Еконт — СО, Витоша",
            ShippingPrice   = 5.50m,
            Weight          = 0.5m,
        });

        // ── 2. Current user as SELLER: current user → demo buyer ────────────
        var sellerCat = await _context.Categories.FirstOrDefaultAsync();
        var sellerItem = new Item
        {
            Title              = "Лего Duplo 30 части (Demo)",
            Description        = "Seeded for UI demo — seller view.",
            CategoryId         = sellerCat!.Id,
            ListingType        = ListingType.Sell,
            Price              = 18.00m,
            UserId             = userId,
            IsActive           = true,
            AiModerationStatus = AiModerationStatus.AutoApproved,
        };
        _context.Items.Add(sellerItem);
        await _context.SaveChangesAsync();

        var sellerPayment = new Payment
        {
            ItemId        = sellerItem.Id,
            BuyerId       = demoBuyerId,
            SellerId      = userId,
            Amount        = 18.00m,
            PaymentMethod = PaymentMethod.OnSpot,
            Status        = PaymentStatus.Completed,
        };
        _context.Payments.Add(sellerPayment);
        await _context.SaveChangesAsync();

        _context.Shipments.Add(new Shipment
        {
            PaymentId       = sellerPayment.Id,
            CourierProvider = CourierProvider.Speedy,
            DeliveryType    = DeliveryType.Office,
            Status          = ShipmentStatus.Created,
            TrackingNumber  = $"DEMO-OUT-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            WaybillId       = $"WB-DEMO-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            RecipientName   = "Елена (Demo Buyer)",
            RecipientPhone  = "+359888000002",
            OfficeName      = "Спиди — Пловдив, Тракия",
            ShippingPrice   = 4.80m,
            Weight          = 0.8m,
        });

        await _context.SaveChangesAsync();

        return Ok(new { message = "Demo orders seeded. Open Dashboard → My Shipments." });
    }

    /// <summary>
    /// Removes all demo data created by seed-demo-order.
    /// Development environment only.
    /// </summary>
    [HttpDelete("seed-demo-order")]
    public async Task<IActionResult> CleanupDemoOrder()
    {
        if (!_env.IsDevelopment())
            return NotFound();

        var demoShipments = _context.Shipments
            .Where(s => s.TrackingNumber != null && s.TrackingNumber.StartsWith("DEMO-"));
        _context.Shipments.RemoveRange(demoShipments);

        var demoItemIds = _context.Items
            .Where(i => i.Description != null && i.Description.Contains("Seeded for UI demo"))
            .Select(i => i.Id);

        var demoPayments = _context.Payments.Where(p => demoItemIds.Contains(p.ItemId));
        _context.Payments.RemoveRange(demoPayments);

        var demoItems = _context.Items
            .Where(i => i.Description != null && i.Description.Contains("Seeded for UI demo"));
        _context.Items.RemoveRange(demoItems);

        await _context.SaveChangesAsync();
        return Ok(new { message = "Demo data removed." });
    }
}
