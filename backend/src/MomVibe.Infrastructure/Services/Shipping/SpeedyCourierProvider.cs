namespace MomVibe.Infrastructure.Services.Shipping;

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

using Domain.Enums;
using Configuration;
using Application.Interfaces;
using Application.DTOs.Shipping;

/// <summary>
/// Speedy courier provider implementation.
/// Maps application DTOs to Speedy REST API endpoints:
/// - /calculate — calculate shipping price.
/// - /shipment — create shipment.
/// - /track — get tracking events.
/// - /location/office — list offices.
/// - /shipment/cancel — cancel shipment.
/// Uses basic auth via credentials from SpeedySettings.
/// </summary>
public class SpeedyCourierProvider : ICourierProvider
{
    private readonly HttpClient _httpClient;
    private readonly SpeedySettings _settings;

    public SpeedyCourierProvider(IHttpClientFactory httpClientFactory, IOptions<SpeedySettings> settings)
    {
        this._httpClient = httpClientFactory.CreateClient("Speedy");
        this._settings = settings.Value;
    }

    public CourierProvider ProviderType => CourierProvider.Speedy;

    public async Task<ShippingPriceResultDto> CalculatePriceAsync(CalculateShippingDto request)
    {
        var body = new
        {
            userName = this._settings.Username,
            password = this._settings.Password,
            service = new { serviceId = 505, autoAdjustPickupDate = true },
            content = new { parcelsCount = 1, totalWeight = request.Weight, contents = "Baby items" },
            payment = new { courierServicePayer = "SENDER" },
            recipient = new
            {
                addressLocation = request.DeliveryType == DeliveryType.Address
                    ? new { cityName = request.ToCity }
                    : null,
                pickupOfficeId = request.DeliveryType != DeliveryType.Address ? request.OfficeId : null
            }
        };

        var response = await PostSpeedyAsync("/calculate", body);
        var result = JsonSerializer.Deserialize<JsonElement>(response);

        var price = 0m;
        if (result.TryGetProperty("calculations", out var calculations))
        {
            foreach (var calc in calculations.EnumerateArray())
            {
                if (calc.TryGetProperty("price", out var priceObj) && priceObj.TryGetProperty("total", out var total))
                {
                    price = total.GetDecimal();
                    break;
                }
            }
        }

        return new ShippingPriceResultDto
        {
            Price = price,
            Currency = "BGN",
            EstimatedDelivery = "1-2 business days"
        };
    }

    public async Task<(string TrackingNumber, string WaybillId, string? LabelUrl)> CreateShipmentAsync(CreateShipmentDto request)
    {
        var body = new
        {
            userName = this._settings.Username,
            password = this._settings.Password,
            service = new { serviceId = 505, autoAdjustPickupDate = true },
            content = new { parcelsCount = 1, totalWeight = request.Weight, contents = "Baby items" },
            payment = new { courierServicePayer = "SENDER" },
            sender = new { contactName = "MomVibe" },
            recipient = new
            {
                clientName = request.RecipientName,
                phone1 = new { number = request.RecipientPhone },
                addressLocation = request.DeliveryType == DeliveryType.Address
                    ? new { cityName = request.City, streetName = request.DeliveryAddress }
                    : null,
                pickupOfficeId = request.DeliveryType != DeliveryType.Address ? request.OfficeId : null
            }
        };

        var response = await PostSpeedyAsync("/shipment", body);
        var result = JsonSerializer.Deserialize<JsonElement>(response);

        var shipmentId = "";
        if (result.TryGetProperty("id", out var id))
        {
            shipmentId = id.GetString() ?? id.GetRawText();
        }

        return (shipmentId, shipmentId, null);
    }

    public async Task<byte[]> GetLabelAsync(string waybillId)
    {
        var body = new
        {
            userName = this._settings.Username,
            password = this._settings.Password,
            paperSize = "A4",
            parcels = new[] { new { parcel = new { id = waybillId } } }
        };

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{this._settings.BaseUrl}/print", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<List<TrackingEventDto>> TrackAsync(string trackingNumber)
    {
        var body = new
        {
            userName = this._settings.Username,
            password = this._settings.Password,
            parcels = new[] { new { id = trackingNumber } }
        };

        var response = await PostSpeedyAsync("/track", body);
        var result = JsonSerializer.Deserialize<JsonElement>(response);

        var events = new List<TrackingEventDto>();
        if (result.TryGetProperty("parcels", out var parcels))
        {
            foreach (var parcel in parcels.EnumerateArray())
            {
                if (parcel.TryGetProperty("operations", out var operations))
                {
                    foreach (var op in operations.EnumerateArray())
                    {
                        events.Add(new TrackingEventDto
                        {
                            Timestamp = op.TryGetProperty("dateTime", out var dt) ? DateTime.Parse(dt.GetString()!) : DateTime.UtcNow,
                            Description = op.TryGetProperty("comment", out var comment) ? comment.GetString()! : "Status update",
                            Location = op.TryGetProperty("place", out var place) && place.TryGetProperty("name", out var placeName) ? placeName.GetString() : null
                        });
                    }
                }
            }
        }

        return events;
    }

    public async Task CancelShipmentAsync(string waybillId)
    {
        var body = new
        {
            userName = this._settings.Username,
            password = this._settings.Password,
            id = waybillId,
            comment = "Cancelled via MomVibe"
        };

        await PostSpeedyAsync("/shipment/cancel", body);
    }

    public async Task<List<CourierOfficeDto>> GetOfficesAsync(string? city = null)
    {
        var body = new
        {
            userName = this._settings.Username,
            password = this._settings.Password,
            countryId = 100,
            name = city
        };

        var response = await PostSpeedyAsync("/location/office", body);
        var result = JsonSerializer.Deserialize<JsonElement>(response);

        var offices = new List<CourierOfficeDto>();
        if (result.TryGetProperty("offices", out var officeList))
        {
            foreach (var office in officeList.EnumerateArray())
            {
                offices.Add(new CourierOfficeDto
                {
                    Id = office.TryGetProperty("id", out var id) ? id.GetRawText() : "",
                    Name = office.TryGetProperty("name", out var name) ? name.GetString()! : "",
                    City = office.TryGetProperty("address", out var addr) && addr.TryGetProperty("siteName", out var sn) ? sn.GetString() : null,
                    Address = office.TryGetProperty("address", out var addr2) && addr2.TryGetProperty("localAddressString", out var las) ? las.GetString() : null,
                    Lat = office.TryGetProperty("address", out var addr3) && addr3.TryGetProperty("x", out var x) && x.ValueKind == JsonValueKind.Number ? x.GetDouble() : null,
                    Lng = office.TryGetProperty("address", out var addr4) && addr4.TryGetProperty("y", out var y) && y.ValueKind == JsonValueKind.Number ? y.GetDouble() : null,
                    IsLocker = office.TryGetProperty("type", out var type) && type.GetString() == "APT"
                });
            }
        }

        return offices;
    }

    private async Task<string> PostSpeedyAsync(string endpoint, object body)
    {
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{this._settings.BaseUrl}{endpoint}", content);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        // Speedy returns HTTP 200 even on auth errors — check for error in JSON body
        var doc = JsonSerializer.Deserialize<JsonElement>(responseBody);
        if (doc.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object)
        {
            var message = error.TryGetProperty("message", out var msg) ? msg.GetString() : "Unknown Speedy API error";
            throw new InvalidOperationException($"Speedy API error: {message}");
        }

        return responseBody;
    }
}
