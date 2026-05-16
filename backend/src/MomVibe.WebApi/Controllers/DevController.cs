namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

using Domain.Entities;
using Domain.Enums;
using Application.Interfaces;

/// <summary>
/// Development-only helpers for seeding demo data while testing the UI.
/// All endpoints are blocked in non-Development environments.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DevController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IWebHostEnvironment _env;

    public DevController(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IWebHostEnvironment env)
    {
        _context = context;
        _currentUserService = currentUserService;
        _env = env;
    }

    /// <summary>
    /// Seeds a complete demo order for the current user so the Dashboard
    /// shows both a "To Send" (seller) and an "Incoming" (buyer) shipment.
    /// Development environment only.
    /// </summary>
    [HttpPost("seed-demo-order")]
    public async Task<IActionResult> SeedDemoOrder()
    {
        if (!_env.IsDevelopment())
            return NotFound();

        var userId = _currentUserService.UserId;
        if (userId == null) return Unauthorized();

        const string demoSellerId = "demo-user-sofia-001";
        const string demoBuyerId  = "demo-user-plovdiv-002";

        // ── 1. User as BUYER: demo seller → current user ──────────────────
        // Find any active sell item from the demo seller
        var buyerItem = await _context.Items
            .Where(i => i.UserId == demoSellerId && i.ListingType == ListingType.Sell && i.IsActive)
            .FirstOrDefaultAsync();

        if (buyerItem == null)
        {
            // Demo data not seeded yet — create a minimal placeholder item
            var cat = await _context.Categories.FirstOrDefaultAsync();
            if (cat == null) return BadRequest(new { error = "No categories found. Run the app once to seed them." });

            buyerItem = new Item
            {
                Title    = "Demo Item (Seller)",
                Description = "Seeded for UI demo.",
                CategoryId  = cat.Id,
                ListingType = ListingType.Sell,
                Price       = 25.00m,
                UserId      = demoSellerId,
                IsActive    = true,
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

        var incomingShipment = new Shipment
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
        };
        _context.Shipments.Add(incomingShipment);

        // ── 2. User as SELLER: current user → demo buyer ──────────────────
        // Create a lightweight demo item owned by the current user
        var sellerCat = await _context.Categories.FirstOrDefaultAsync();
        var sellerItem = new Item
        {
            Title       = "Лего Duplo 30 части (Demo)",
            Description = "Seeded for UI demo — seller view.",
            CategoryId  = sellerCat!.Id,
            ListingType = ListingType.Sell,
            Price       = 18.00m,
            UserId      = userId,
            IsActive    = true,
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

        var outboundShipment = new Shipment
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
        };
        _context.Shipments.Add(outboundShipment);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message        = "Demo orders seeded. Open Dashboard → My Shipments.",
            incomingShipmentId = incomingShipment.Id,
            outboundShipmentId = outboundShipment.Id,
        });
    }
}
