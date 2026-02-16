namespace MomVibe.Infrastructure.Services.Shipping;

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

using Domain.Enums;
using Configuration;
using Application.Interfaces;
using Application.DTOs.Shipping;


/// <summary>
/// Econt Express courier provider implementation.
/// Maps application DTOs to Econt REST API endpoints:
/// - /Shipments/LabelService.createLabel.json — create shipment and get label.
/// - /Nomenclatures/NomenclaturesService.getOffices.json — list offices.
/// - /Shipments/LabelService.deleteLabels.json — cancel shipment.
/// Uses basic auth via credentials from EcontSettings.
/// </summary>
public class EcontCourierProvider : ICourierProvider
{
    private readonly HttpClient _httpClient;
    private readonly EcontSettings _settings;

    public EcontCourierProvider(IHttpClientFactory httpClientFactory, IOptions<EcontSettings> settings)
    {
        this._httpClient = httpClientFactory.CreateClient("Econt");
        this._settings = settings.Value;
    }

    public CourierProvider ProviderType => CourierProvider.Econt;

    public async Task<ShippingPriceResultDto> CalculatePriceAsync(CalculateShippingDto request)
    {
        var body = new
        {
            label = new
            {
                senderClient = new { name = "MomVibe" },
                receiverClient = new { name = "Recipient" },
                receiverAddress = request.DeliveryType == DeliveryType.Address
                    ? new { city = new { name = request.ToCity }, street = request.ToCity }
                    : null,
                receiverOfficeCode = request.DeliveryType != DeliveryType.Address ? request.OfficeId : null,
                packCount = 1,
                weight = request.Weight,
                shipmentType = "PACK",
                services = new
                {
                    cdType = request.IsCod ? "GET" : (string?)null,
                    cdAmount = request.IsCod ? request.CodAmount : 0,
                    declaredValueAmount = request.IsInsured ? request.InsuredAmount : 0
                }
            },
            mode = "calculate"
        };

        var response = await PostEcontAsync("/Shipments/LabelService.createLabel.json", body);
        if (string.IsNullOrWhiteSpace(response))
            throw new InvalidOperationException("Econt API returned an empty response. Please verify Econt credentials are configured.");

        var result = JsonSerializer.Deserialize<JsonElement>(response);

        var price = 0m;
        if (result.TryGetProperty("label", out var label) && label.TryGetProperty("totalPrice", out var totalPrice))
        {
            price = totalPrice.GetDecimal();
        }

        return new ShippingPriceResultDto
        {
            Price = price,
            Currency = "BGN",
            EstimatedDelivery = "1-3 business days"
        };
    }

    public async Task<(string TrackingNumber, string WaybillId, string? LabelUrl)> CreateShipmentAsync(CreateShipmentDto request)
    {
        var body = new
        {
            label = new
            {
                senderClient = new { name = "MomVibe" },
                receiverClient = new { name = request.RecipientName, phones = new[] { request.RecipientPhone } },
                receiverAddress = request.DeliveryType == DeliveryType.Address
                    ? new { city = new { name = request.City }, street = request.DeliveryAddress }
                    : null,
                receiverOfficeCode = request.DeliveryType != DeliveryType.Address ? request.OfficeId : null,
                packCount = 1,
                weight = request.Weight,
                shipmentType = "PACK",
                services = new
                {
                    cdType = request.IsCod ? "GET" : (string?)null,
                    cdAmount = request.IsCod ? request.CodAmount : 0,
                    declaredValueAmount = request.IsInsured ? request.InsuredAmount : 0
                }
            },
            mode = "create"
        };

        var response = await PostEcontAsync("/Shipments/LabelService.createLabel.json", body);
        if (string.IsNullOrWhiteSpace(response))
            throw new InvalidOperationException("Econt API returned an empty response. Please verify Econt credentials are configured.");

        var result = JsonSerializer.Deserialize<JsonElement>(response);

        var shipmentNumber = "";
        if (result.TryGetProperty("label", out var label) && label.TryGetProperty("shipmentNumber", out var sn))
        {
            shipmentNumber = sn.GetString() ?? "";
        }

        return (shipmentNumber, shipmentNumber, null);
    }

    public async Task<byte[]> GetLabelAsync(string waybillId)
    {
        var body = new { shipmentNumber = waybillId };
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        AddAuth(content);

        var response = await _httpClient.PostAsync($"{this._settings.BaseUrl}/Shipments/LabelService.createLabel.json", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<List<TrackingEventDto>> TrackAsync(string trackingNumber)
    {
        var body = new { shipmentNumbers = new[] { trackingNumber } };
        var response = await PostEcontAsync("/Shipments/ShipmentService.getShipmentStatuses.json", body);
        if (string.IsNullOrWhiteSpace(response))
            return new List<TrackingEventDto>();

        var result = JsonSerializer.Deserialize<JsonElement>(response);

        var events = new List<TrackingEventDto>();
        if (result.TryGetProperty("shipmentStatuses", out var statuses))
        {
            foreach (var status in statuses.EnumerateArray())
            {
                if (status.TryGetProperty("statuses", out var statusList))
                {
                    foreach (var s in statusList.EnumerateArray())
                    {
                        events.Add(new TrackingEventDto
                        {
                            Timestamp = s.TryGetProperty("time", out var time) ? DateTime.Parse(time.GetString()!) : DateTime.UtcNow,
                            Description = s.TryGetProperty("event", out var evt) ? evt.GetString()! : "Status update",
                            Location = s.TryGetProperty("location", out var loc) ? loc.GetString() : null
                        });
                    }
                }
            }
        }

        return events;
    }

    public async Task CancelShipmentAsync(string waybillId)
    {
        var body = new { shipmentNumber = waybillId };
        await PostEcontAsync("/Shipments/LabelService.deleteLabels.json", body);
    }

    public async Task<List<CourierOfficeDto>> GetOfficesAsync(string? city = null)
    {
        var body = new { countryCode = "BGR", cityName = city ?? "" };

        var response = await PostEcontAsync("/Nomenclatures/NomenclaturesService.getOffices.json", body);
        if (string.IsNullOrWhiteSpace(response))
            return new List<CourierOfficeDto>();

        var result = JsonSerializer.Deserialize<JsonElement>(response);

        var offices = new List<CourierOfficeDto>();
        if (result.TryGetProperty("offices", out var officeList))
        {
            foreach (var office in officeList.EnumerateArray())
            {
                offices.Add(new CourierOfficeDto
                {
                    Id = office.TryGetProperty("code", out var code) ? code.GetString()! : "",
                    Name = office.TryGetProperty("name", out var name) ? name.GetString()! : "",
                    City = office.TryGetProperty("address", out var addr) && addr.TryGetProperty("city", out var c) && c.TryGetProperty("name", out var cn) ? cn.GetString() : null,
                    Address = office.TryGetProperty("address", out var addr2) && addr2.TryGetProperty("fullAddress", out var fa) ? fa.GetString() : null,
                    Lat = office.TryGetProperty("address", out var addr3) && addr3.TryGetProperty("location", out var loc) && loc.TryGetProperty("latitude", out var lat) && lat.ValueKind == JsonValueKind.Number ? lat.GetDouble() : null,
                    Lng = office.TryGetProperty("address", out var addr4) && addr4.TryGetProperty("location", out var loc2) && loc2.TryGetProperty("longitude", out var lng) && lng.ValueKind == JsonValueKind.Number ? lng.GetDouble() : null,
                    IsLocker = office.TryGetProperty("isAPS", out var isAps) && isAps.ValueKind == JsonValueKind.True
                });
            }
        }

        return offices;
    }

    private async Task<string> PostEcontAsync(string endpoint, object body)
    {
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        AddAuth(content);

        var response = await this._httpClient.PostAsync($"{this._settings.BaseUrl}{endpoint}", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private void AddAuth(HttpContent content)
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{this._settings.Username}:{this._settings.Password}"));
        this._httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
    }
}
