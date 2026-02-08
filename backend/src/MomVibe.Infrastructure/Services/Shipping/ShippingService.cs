namespace MomVibe.Infrastructure.Services.Shipping;

using AutoMapper;
using Microsoft.EntityFrameworkCore;

using Domain.Enums;
using Domain.Entities;
using Application.Interfaces;
using Application.DTOs.Shipping;


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

    public ShippingService(IApplicationDbContext context, IMapper mapper, CourierProviderFactory factory)
    {
        this._context = context;
        this._mapper = mapper;
        this._factory = factory;
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
            ShippingPrice = priceResult.Price,
            IsCod = request.IsCod,
            CodAmount = request.CodAmount,
            IsInsured = request.IsInsured,
            InsuredAmount = request.InsuredAmount,
            Weight = request.Weight,
            LabelUrl = labelUrl
        };

        this._context.Shipments.Add(shipment);
        await this._context.SaveChangesAsync();

        shipment.Payment = payment;
        return this._mapper.Map<ShipmentDto>(shipment);
    }

    public async Task<byte[]> GetLabelAsync(Guid shipmentId)
    {
        var shipment = await this._context.Shipments.FindAsync(shipmentId);
        if (shipment == null) throw new KeyNotFoundException("Shipment not found.");
        if (string.IsNullOrEmpty(shipment.WaybillId)) throw new InvalidOperationException("No waybill ID for this shipment.");

        var provider = this._factory.GetProvider(shipment.CourierProvider);
        return await provider.GetLabelAsync(shipment.WaybillId);
    }

    public async Task<List<TrackingEventDto>> TrackShipmentAsync(Guid shipmentId)
    {
        var shipment = await this._context.Shipments.FindAsync(shipmentId);
        if (shipment == null) throw new KeyNotFoundException("Shipment not found.");
        if (string.IsNullOrEmpty(shipment.TrackingNumber)) throw new InvalidOperationException("No tracking number for this shipment.");

        var provider = this._factory.GetProvider(shipment.CourierProvider);
        return await provider.TrackAsync(shipment.TrackingNumber);
    }

    public async Task CancelShipmentAsync(Guid shipmentId)
    {
        var shipment = await this._context.Shipments.FindAsync(shipmentId);
        if (shipment == null) throw new KeyNotFoundException("Shipment not found.");
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

    public async Task<ShipmentDto?> GetShipmentByPaymentIdAsync(Guid paymentId)
    {
        var shipment = await this._context.Shipments
            .Include(s => s.Payment)
                .ThenInclude(p => p.Item)
            .FirstOrDefaultAsync(s => s.PaymentId == paymentId);

        return shipment != null ? this._mapper.Map<ShipmentDto>(shipment) : null;
    }

    public async Task<List<ShipmentDto>> GetShipmentsByUserAsync(string userId)
    {
        var shipments = await this._context.Shipments
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
            .Include(s => s.Payment)
                .ThenInclude(p => p.Item)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<ShipmentDto>>(shipments);
    }

    public async Task SyncShipmentStatusesAsync()
    {
        var activeShipments = await this._context.Shipments
            .Where(s => s.Status != ShipmentStatus.Delivered
                     && s.Status != ShipmentStatus.Cancelled
                     && s.Status != ShipmentStatus.Returned
                     && !string.IsNullOrEmpty(s.TrackingNumber))
            .ToListAsync();

        foreach (var shipment in activeShipments)
        {
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
        }

        await this._context.SaveChangesAsync();
    }
}
