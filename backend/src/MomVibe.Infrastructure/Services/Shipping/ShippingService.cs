namespace MomVibe.Infrastructure.Services.Shipping;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Domain.Enums;
using Domain.Entities;
using Application.Interfaces;
using Application.DTOs.Shipping;
using Infrastructure.Configuration;


/// <summary>
/// Application-level shipping orchestrator that delegates courier API calls to the correct
/// provider via <see cref="CourierProviderFactory"/>, persists <see cref="Shipment"/> entities,
/// and provides shipment queries. Integrates EF Core, AutoMapper, and courier providers.
/// </summary>
public class ShippingService : IShippingService
{
    private static readonly System.Text.Json.JsonSerializerOptions OutboxJson = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
    };

    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly CourierProviderFactory _factory;
    private readonly IOutboxWriter _outbox;
    private readonly N8nSettings _n8nSettings;
    private readonly ShippingSettings _shippingSettings;
    private readonly IShipmentNotifier _notifier;
    private readonly ILogger<ShippingService> _logger;

    public ShippingService(
        IApplicationDbContext context,
        IMapper mapper,
        CourierProviderFactory factory,
        IOutboxWriter outbox,
        IOptions<N8nSettings> n8nSettings,
        IOptions<ShippingSettings> shippingSettings,
        IShipmentNotifier notifier,
        ILogger<ShippingService> logger)
    {
        this._context = context;
        this._mapper = mapper;
        this._factory = factory;
        this._outbox = outbox;
        this._n8nSettings = n8nSettings.Value;
        this._shippingSettings = shippingSettings.Value;
        this._notifier = notifier;
        this._logger = logger;
    }

    private void EnqueueWebhook<T>(string path, T body) where T : notnull =>
        this._outbox.Enqueue(OutboxMessageTypes.N8nWebhook, new N8nWebhookOutboxPayload(
            path,
            System.Text.Json.JsonSerializer.Serialize(body, OutboxJson)));

    public async Task<ShippingPriceResultDto> CalculatePriceAsync(CalculateShippingDto request)
    {
        if (this._shippingSettings.MockMode)
            return await Task.FromResult(new ShippingPriceResultDto { Price = 4.99m, Currency = "EUR" });

        var provider = this._factory.GetProvider(request.CourierProvider);
        return await provider.CalculatePriceAsync(request);
    }

    public async Task<ShipmentDto> CreateShipmentAsync(CreateShipmentDto request)
    {
        var payment = await this._context.Payments
            .Include(p => p.Item)
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId);

        if (payment == null)
            throw new KeyNotFoundException("Payment not found.");

        string trackingNumber;
        string waybillId;
        string? labelUrl;

        if (this._shippingSettings.MockMode)
        {
            var mockId = Guid.NewGuid().ToString("N")[..8].ToUpper();
            trackingNumber = $"MOCK-{mockId}";
            waybillId = $"MOCK-{mockId}";
            labelUrl = null;
            this._logger.LogInformation("MockMode: generated fake waybill {WaybillId} for payment {PaymentId}", waybillId, request.PaymentId);
        }
        else
        {
            var provider = _factory.GetProvider(request.CourierProvider);
            (trackingNumber, waybillId, labelUrl) = await provider.CreateShipmentAsync(request);
        }

        // Price was already calculated and shown to the buyer before checkout.
        // Re-fetching it here is informational only — never let a price-calc failure
        // prevent the shipment record from being saved (which would orphan the waybill at the courier).
        decimal shippingPrice = 0m;
        try
        {
            var priceResult = await CalculatePriceAsync(new CalculateShippingDto
            {
                CourierProvider = request.CourierProvider,
                DeliveryType = request.DeliveryType,
                ToCity = request.City,
                OfficeId = request.OfficeId,
                Weight = request.Weight,
                IsCod = request.IsCod,
                CodAmount = request.CodAmount,
                IsInsured = request.IsInsured,
                InsuredAmount = request.InsuredAmount
            });
            shippingPrice = priceResult.Price;
        }
        catch (Exception ex)
        {
            this._logger.LogWarning(ex, "Price calculation after shipment creation failed for waybill {WaybillId}. Shipment will be saved with price 0.", waybillId);
        }

        var shipment = new Shipment
        {
            PaymentId = request.PaymentId,
            CourierProvider = request.CourierProvider,
            DeliveryType = request.DeliveryType,
            Status = ShipmentStatus.Created,
            TrackingNumber = trackingNumber,
            WaybillId = waybillId,
            RecipientName = request.RecipientName,
            RecipientPhone = request.RecipientPhone,
            DeliveryAddress = request.DeliveryAddress,
            City = request.City,
            OfficeId = request.OfficeId,
            OfficeName = request.OfficeName,
            ShippingPrice = shippingPrice,
            IsCod = request.IsCod,
            CodAmount = request.CodAmount,
            IsInsured = request.IsInsured,
            InsuredAmount = request.InsuredAmount,
            Weight = request.Weight,
            LabelUrl = labelUrl
        };

        this._context.Shipments.Add(shipment);
        await this._context.SaveChangesAsync();

        // Queue shipment.created webhook through the transactional outbox
        try
        {
            var buyer = await this._context.Payments
                .Include(p => p.Buyer)
                .Where(p => p.Id == payment.Id)
                .Select(p => p.Buyer)
                .FirstOrDefaultAsync();

            EnqueueWebhook(this._n8nSettings.ShipmentCreated, new
            {
                Event = "shipment.created",
                Timestamp = DateTime.UtcNow,
                ShipmentId = shipment.Id,
                shipment.TrackingNumber,
                shipment.CourierProvider,
                ItemTitle = payment.Item?.Title,
                BuyerEmail = buyer?.Email,
                RecipientName = shipment.RecipientName
            });
            await this._context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            this._logger.LogWarning(ex, "Failed to enqueue n8n shipment.created for shipment {ShipmentId}", shipment.Id);
        }

        shipment.Payment = payment;
        var dto = this._mapper.Map<ShipmentDto>(shipment);
        // Notify the SELLER: waybill is ready to print and attach to the package
        try { await this._notifier.NotifySellerShipmentReadyAsync(payment.SellerId, dto); }
        catch (Exception ex)
        {
            this._logger.LogWarning(ex, "SignalR seller-shipment-ready notification failed for shipment {ShipmentId}", shipment.Id);
        }
        return dto;
    }

    public async Task<byte[]> GetLabelAsync(Guid shipmentId, string userId)
    {
        var shipment = await this._context.Shipments
            .Include(s => s.Payment)
            .FirstOrDefaultAsync(s => s.Id == shipmentId);
        if (shipment == null) throw new KeyNotFoundException("Shipment not found.");
        if (shipment.Payment.BuyerId != userId && shipment.Payment.SellerId != userId)
            throw new UnauthorizedAccessException("You do not have access to this shipment.");
        if (string.IsNullOrEmpty(shipment.WaybillId)) throw new InvalidOperationException("No waybill ID for this shipment.");

        if (shipment.WaybillId.StartsWith("MOCK-", StringComparison.Ordinal))
            return BuildMockLabelPdf(shipment.WaybillId, shipment.TrackingNumber ?? shipment.WaybillId, shipment.RecipientName);

        var provider = this._factory.GetProvider(shipment.CourierProvider);
        return await provider.GetLabelAsync(shipment.WaybillId);
    }

    public async Task<List<TrackingEventDto>> TrackShipmentAsync(Guid shipmentId, string userId, bool isAdmin = false)
    {
        var shipment = await this._context.Shipments
            .Include(s => s.Payment)
            .FirstOrDefaultAsync(s => s.Id == shipmentId);
        if (shipment == null) throw new KeyNotFoundException("Shipment not found.");
        if (!isAdmin && shipment.Payment.BuyerId != userId && shipment.Payment.SellerId != userId)
            throw new UnauthorizedAccessException("You do not have access to this shipment.");
        if (string.IsNullOrEmpty(shipment.TrackingNumber)) throw new InvalidOperationException("No tracking number for this shipment.");

        if (shipment.TrackingNumber.StartsWith("MOCK-", StringComparison.Ordinal))
            return await Task.FromResult(new List<TrackingEventDto>
            {
                new() { Timestamp = DateTime.UtcNow, Description = "Mock shipment created — test mode active." }
            });

        var provider = this._factory.GetProvider(shipment.CourierProvider);
        return await provider.TrackAsync(shipment.TrackingNumber);
    }

    public async Task CancelShipmentAsync(Guid shipmentId, string userId)
    {
        var shipment = await this._context.Shipments
            .Include(s => s.Payment)
            .FirstOrDefaultAsync(s => s.Id == shipmentId);
        if (shipment == null) throw new KeyNotFoundException("Shipment not found.");
        if (shipment.Payment.SellerId != userId)
            throw new UnauthorizedAccessException("Only the seller can cancel a shipment.");
        if (string.IsNullOrEmpty(shipment.WaybillId)) throw new InvalidOperationException("No waybill ID for this shipment.");

        if (!shipment.WaybillId.StartsWith("MOCK-", StringComparison.Ordinal))
        {
            var provider = this._factory.GetProvider(shipment.CourierProvider);
            await provider.CancelShipmentAsync(shipment.WaybillId);
        }

        shipment.Status = ShipmentStatus.Cancelled;
        await this._context.SaveChangesAsync();
    }

    public async Task<List<CourierOfficeDto>> GetOfficesAsync(CourierProvider provider, string? city = null)
    {
        var courierProvider = this._factory.GetProvider(provider);
        return await courierProvider.GetOfficesAsync(city);
    }

    public async Task<ShipmentDto?> GetShipmentByPaymentIdAsync(Guid paymentId, string userId)
    {
        var shipment = await this._context.Shipments
            .Include(s => s.Payment)
                .ThenInclude(p => p.Item)
            .FirstOrDefaultAsync(s => s.PaymentId == paymentId);

        if (shipment == null) return null;
        if (shipment.Payment.BuyerId != userId && shipment.Payment.SellerId != userId)
            throw new UnauthorizedAccessException("You do not have access to this shipment.");

        var dto = this._mapper.Map<ShipmentDto>(shipment);
        dto.IsCurrentUserSeller = dto.SellerId == userId;
        return dto;
    }

    public async Task<List<ShipmentDto>> GetShipmentsByUserAsync(string userId)
    {
        var shipments = await this._context.Shipments
            .AsNoTracking()
            .Include(s => s.Payment)
                .ThenInclude(p => p.Item)
            .Where(s => s.Payment.BuyerId == userId || s.Payment.SellerId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        var dtos = _mapper.Map<List<ShipmentDto>>(shipments);
        foreach (var dto in dtos)
            dto.IsCurrentUserSeller = dto.SellerId == userId;
        return dtos;
    }

    public async Task<List<ShipmentDto>> GetAllShipmentsAsync(int page = 1, int pageSize = 50)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);

        var shipments = await this._context.Shipments
            .AsNoTracking()
            .Include(s => s.Payment)
                .ThenInclude(p => p.Item)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return _mapper.Map<List<ShipmentDto>>(shipments);
    }

    private static byte[] BuildMockLabelPdf(string waybillId, string trackingNumber, string? recipientName)
    {
        // Builds a minimal but spec-compliant PDF using a MemoryStream so xref byte offsets are exact.
        static byte[] Line(string s) => System.Text.Encoding.Latin1.GetBytes(s + "\n");

        var lines = new[]
        {
            $"(MOCK WAYBILL) Tj",
            $"0 -18 Td (Waybill ID   : {EscapePdf(waybillId)}) Tj",
            $"0 -18 Td (Tracking No  : {EscapePdf(trackingNumber)}) Tj",
            $"0 -18 Td (Recipient    : {EscapePdf(recipientName ?? "N/A")}) Tj",
            $"0 -36 Td (*** TEST MODE - NOT A REAL SHIPMENT ***) Tj",
        };
        var streamContent = "BT\n/F1 11 Tf\n50 700 Td\n" + string.Join("\n", lines) + "\nET";
        var streamBytes = System.Text.Encoding.Latin1.GetBytes(streamContent);

        using var ms = new System.IO.MemoryStream();

        void W(string s) { var b = Line(s); ms.Write(b); }

        W("%PDF-1.4");
        var off1 = ms.Position; W("1 0 obj"); W("<< /Type /Catalog /Pages 2 0 R >>"); W("endobj");
        var off2 = ms.Position; W("2 0 obj"); W("<< /Type /Pages /Kids [3 0 R] /Count 1 >>"); W("endobj");
        var off3 = ms.Position; W("3 0 obj");
        W("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 420 595]");
        W("   /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>");
        W("endobj");
        var off4 = ms.Position; W("4 0 obj");
        W($"<< /Length {streamBytes.Length} >>");
        W("stream");
        ms.Write(streamBytes);
        W("");
        W("endstream");
        W("endobj");
        var off5 = ms.Position; W("5 0 obj");
        W("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
        W("endobj");

        var xrefPos = ms.Position;
        W("xref");
        W("0 6");
        // Each xref entry is exactly 20 bytes: "nnnnnnnnnn ggggg f \n" (note the trailing space before LF)
        W("0000000000 65535 f ");
        W($"{off1:D10} 00000 n ");
        W($"{off2:D10} 00000 n ");
        W($"{off3:D10} 00000 n ");
        W($"{off4:D10} 00000 n ");
        W($"{off5:D10} 00000 n ");
        W("trailer");
        W("<< /Size 6 /Root 1 0 R >>");
        W("startxref");
        W(xrefPos.ToString());
        ms.Write(System.Text.Encoding.Latin1.GetBytes("%%EOF"));

        return ms.ToArray();
    }

    private static string EscapePdf(string s) =>
        s.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

    public async Task SyncShipmentStatusesAsync()
    {
        var activeShipments = await this._context.Shipments
            .Include(s => s.Payment).ThenInclude(p => p.Item)
            .Include(s => s.Payment).ThenInclude(p => p.Buyer)
            .Include(s => s.Payment).ThenInclude(p => p.Seller)
            .Where(s => s.Status != ShipmentStatus.Delivered
                     && s.Status != ShipmentStatus.Cancelled
                     && s.Status != ShipmentStatus.Returned
                     && !string.IsNullOrEmpty(s.TrackingNumber))
            .ToListAsync();

        var newlyDelivered = new List<Shipment>();
        var newlyOutForDelivery = new List<Shipment>();
        var newlyPickedUp = new List<Shipment>();

        foreach (var shipment in activeShipments)
        {
            if (shipment.TrackingNumber!.StartsWith("MOCK-", StringComparison.Ordinal))
                continue;

            var previousStatus = shipment.Status;
            var provider = this._factory.GetProvider(shipment.CourierProvider);
            var events = await provider.TrackAsync(shipment.TrackingNumber!);

            if (events.Count > 0)
            {
                var latestEvent = events.OrderByDescending(e => e.Timestamp).First();
                var description = latestEvent.Description.ToLowerInvariant();

                if (description.Contains("deliver"))
                    shipment.Status = ShipmentStatus.Delivered;
                else if (description.Contains("out for delivery"))
                    shipment.Status = ShipmentStatus.OutForDelivery;
                else if (description.Contains("transit") || description.Contains("processing"))
                    shipment.Status = ShipmentStatus.InTransit;
                else if (description.Contains("picked up") || description.Contains("accepted"))
                    shipment.Status = ShipmentStatus.PickedUp;
                else if (description.Contains("return"))
                    shipment.Status = ShipmentStatus.Returned;
            }

            if (previousStatus != ShipmentStatus.Delivered && shipment.Status == ShipmentStatus.Delivered)
                newlyDelivered.Add(shipment);

            if (previousStatus != ShipmentStatus.OutForDelivery && shipment.Status == ShipmentStatus.OutForDelivery)
                newlyOutForDelivery.Add(shipment);

            if (previousStatus != ShipmentStatus.PickedUp && shipment.Status == ShipmentStatus.PickedUp)
                newlyPickedUp.Add(shipment);
        }

        await this._context.SaveChangesAsync();

        // Queue shipment.delivered + shipment.out_for_delivery webhooks through the outbox.
        // All staged rows commit together at the end of the loop.
        var enqueueErrored = false;
        foreach (var shipment in newlyDelivered)
        {
            try
            {
                EnqueueWebhook(this._n8nSettings.ShipmentDelivered, new
                {
                    Event = "shipment.delivered",
                    Timestamp = DateTime.UtcNow,
                    ShipmentId = shipment.Id,
                    shipment.TrackingNumber,
                    ItemTitle = shipment.Payment?.Item?.Title,
                    BuyerEmail = shipment.Payment?.Buyer?.Email,
                    SellerEmail = shipment.Payment?.Seller?.Email
                });
            }
            catch (Exception ex)
            {
                enqueueErrored = true;
                this._logger.LogWarning(ex, "Failed to enqueue n8n shipment.delivered for shipment {ShipmentId}", shipment.Id);
            }
        }

        foreach (var shipment in newlyOutForDelivery)
        {
            try
            {
                EnqueueWebhook(this._n8nSettings.ShipmentOutForDelivery, new
                {
                    Event = "shipment.out_for_delivery",
                    Timestamp = DateTime.UtcNow,
                    ShipmentId = shipment.Id,
                    shipment.TrackingNumber,
                    CourierProvider = shipment.CourierProvider.ToString(),
                    ItemTitle = shipment.Payment?.Item?.Title,
                    BuyerEmail = shipment.Payment?.Buyer?.Email,
                    RecipientName = shipment.RecipientName
                });
            }
            catch (Exception ex)
            {
                enqueueErrored = true;
                this._logger.LogWarning(ex, "Failed to enqueue n8n shipment.out_for_delivery for shipment {ShipmentId}", shipment.Id);
            }
        }

        // Commit every webhook staged above. Skipped if every Enqueue threw (nothing to commit).
        if (!enqueueErrored || newlyDelivered.Count + newlyOutForDelivery.Count > 0)
        {
            try { await this._context.SaveChangesAsync(); }
            catch (Exception ex)
            {
                this._logger.LogWarning(ex, "Failed to commit shipment-status outbox rows");
            }
        }

        // Push real-time SignalR notification for newly picked-up shipments
        foreach (var shipment in newlyPickedUp)
        {
            try
            {
                var dto = _mapper.Map<ShipmentDto>(shipment);
                await _notifier.NotifyShipmentStatusChangedAsync(shipment.Payment.BuyerId, dto);
            }
            catch (Exception ex)
            {
                this._logger.LogWarning(ex, "SignalR shipment-status-changed notification (PickedUp) failed for shipment {ShipmentId}", shipment.Id);
            }
        }
    }
}
