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
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly CourierProviderFactory _factory;
    private readonly IN8nWebhookService _webhook;
    private readonly N8nSettings _n8nSettings;
    private readonly IShipmentNotifier _notifier;
    private readonly ILogger<ShippingService> _logger;

    public ShippingService(
        IApplicationDbContext context,
        IMapper mapper,
        CourierProviderFactory factory,
        IN8nWebhookService webhook,
        IOptions<N8nSettings> n8nSettings,
        IShipmentNotifier notifier,
        ILogger<ShippingService> logger)
    {
        this._context = context;
        this._mapper = mapper;
        this._factory = factory;
        this._webhook = webhook;
        this._n8nSettings = n8nSettings.Value;
        this._notifier = notifier;
        this._logger = logger;
    }

    public async Task<ShippingPriceResultDto> CalculatePriceAsync(CalculateShippingDto request)
    {
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

        var provider = _factory.GetProvider(request.CourierProvider);
        var (trackingNumber, waybillId, labelUrl) = await provider.CreateShipmentAsync(request);

        // Price was already calculated and shown to the buyer before checkout.
        // Re-fetching it here is informational only — never let a price-calc failure
        // prevent the shipment record from being saved (which would orphan the waybill at the courier).
        decimal shippingPrice = 0m;
        try
        {
            var priceResult = await provider.CalculatePriceAsync(new CalculateShippingDto
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

        // Fire shipment.created webhook
        try
        {
            var buyer = await this._context.Payments
                .Include(p => p.Buyer)
                .Where(p => p.Id == payment.Id)
                .Select(p => p.Buyer)
                .FirstOrDefaultAsync();

            this._webhook.Send(this._n8nSettings.ShipmentCreated, new
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
        }
        catch { /* Webhook failure must not break shipment flow */ }

        shipment.Payment = payment;
        var dto = this._mapper.Map<ShipmentDto>(shipment);
        // Notify the SELLER: waybill is ready to print and attach to the package
        try { await this._notifier.NotifySellerShipmentReadyAsync(payment.SellerId, dto); } catch { /* notification failure must not break shipment creation */ }
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

        var provider = this._factory.GetProvider(shipment.CourierProvider);
        await provider.CancelShipmentAsync(shipment.WaybillId);

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

        return this._mapper.Map<ShipmentDto>(shipment);
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

        return _mapper.Map<List<ShipmentDto>>(shipments);
    }

    public async Task<List<ShipmentDto>> GetAllShipmentsAsync()
    {
        var shipments = await this._context.Shipments
            .AsNoTracking()
            .Include(s => s.Payment)
                .ThenInclude(p => p.Item)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<ShipmentDto>>(shipments);
    }

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

        // Fire shipment.delivered webhook for newly delivered shipments
        foreach (var shipment in newlyDelivered)
        {
            try
            {
                this._webhook.Send(this._n8nSettings.ShipmentDelivered, new
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
            catch { /* Webhook failure must not break sync flow */ }
        }

        // Fire shipment.out_for_delivery webhook
        foreach (var shipment in newlyOutForDelivery)
        {
            try
            {
                this._webhook.Send(this._n8nSettings.ShipmentOutForDelivery, new
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
            catch { /* Webhook failure must not break sync flow */ }
        }

        // Push real-time SignalR notification for newly picked-up shipments
        foreach (var shipment in newlyPickedUp)
        {
            try
            {
                var dto = _mapper.Map<ShipmentDto>(shipment);
                await _notifier.NotifyShipmentStatusChangedAsync(shipment.Payment.BuyerId, dto);
            }
            catch { /* Notification failure must not break sync flow */ }
        }
    }
}
