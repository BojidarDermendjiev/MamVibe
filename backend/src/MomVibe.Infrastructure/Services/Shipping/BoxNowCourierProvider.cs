namespace MomVibe.Infrastructure.Services.Shipping;

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

using Domain.Enums;
using Configuration;
using Application.DTOs.Shipping;
using Application.Interfaces;

/// <summary>
/// Box Now courier provider implementation.
/// Maps application DTOs to Box Now REST API endpoints:
/// - /delivery-requests — create shipment and calculate price.
/// - /lockers — list locker/office locations.
/// - /delivery-requests/{id}/cancel — cancel shipment.
/// - /delivery-requests/{id}/tracking — get tracking events.
/// Uses Bearer token auth via API key from BoxNowSettings.
/// </summary>
public class BoxNowCourierProvider : ICourierProvider
{
    private readonly HttpClient _httpClient;
    private readonly BoxNowSettings _settings;

    public BoxNowCourierProvider(IHttpClientFactory httpClientFactory, IOptions<BoxNowSettings> settings)
    {
        this._httpClient = httpClientFactory.CreateClient("BoxNow");
        this._settings = settings.Value;
    }

    public CourierProvider ProviderType => CourierProvider.BoxNow;

    public async Task<ShippingPriceResultDto> CalculatePriceAsync(CalculateShippingDto request)
    {
        var body = new
        {
            destination = new
            {
                lockerId = request.DeliveryType != DeliveryType.Address ? request.OfficeId : null,
                address = request.DeliveryType == DeliveryType.Address
                    ? new { city = request.ToCity, street = request.ToCity }
                    : (object?)null
            },
            parcel = new
            {
                weight = request.Weight,
                codAmount = request.IsCod ? request.CodAmount : 0,
                insuredAmount = request.IsInsured ? request.InsuredAmount : 0
            }
        };

        var response = await PostBoxNowAsync("/delivery-requests/calculate", body);
        var result = JsonSerializer.Deserialize<JsonElement>(response);

        var price = 0m;
        if (result.TryGetProperty("price", out var priceEl))
        {
            price = priceEl.GetDecimal();
        }
        else if (result.TryGetProperty("totalPrice", out var totalPrice))
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
            destination = new
            {
                lockerId = request.DeliveryType != DeliveryType.Address ? request.OfficeId : null,
                address = request.DeliveryType == DeliveryType.Address
                    ? new { city = request.City, street = request.DeliveryAddress }
                    : (object?)null
            },
            recipient = new
            {
                name = request.RecipientName,
                phone = request.RecipientPhone
            },
            parcel = new
            {
                weight = request.Weight,
                codAmount = request.IsCod ? request.CodAmount : 0,
                insuredAmount = request.IsInsured ? request.InsuredAmount : 0,
                contents = "Baby items"
            },
            sender = new
            {
                name = "MomVibe"
            }
        };

        var response = await PostBoxNowAsync("/delivery-requests", body);
        var result = JsonSerializer.Deserialize<JsonElement>(response);

        var trackingNumber = "";
        var waybillId = "";
        string? labelUrl = null;

        if (result.TryGetProperty("trackingNumber", out var tn))
            trackingNumber = tn.GetString() ?? "";
        if (result.TryGetProperty("id", out var id))
            waybillId = id.GetString() ?? id.GetRawText();
        if (result.TryGetProperty("labelUrl", out var lu))
            labelUrl = lu.GetString();

        if (string.IsNullOrEmpty(trackingNumber))
            trackingNumber = waybillId;

        return (trackingNumber, waybillId, labelUrl);
    }

    public async Task<byte[]> GetLabelAsync(string waybillId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{this._settings.BaseUrl}/delivery-requests/{waybillId}/label");
        AddAuth(request);

        var response = await this._httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<List<TrackingEventDto>> TrackAsync(string trackingNumber)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{this._settings.BaseUrl}/delivery-requests/{trackingNumber}/tracking");
        AddAuth(request);

        var response = await this._httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responseStr = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseStr);

        var events = new List<TrackingEventDto>();

        if (result.TryGetProperty("events", out var eventList))
        {
            foreach (var evt in eventList.EnumerateArray())
            {
                events.Add(new TrackingEventDto
                {
                    Timestamp = evt.TryGetProperty("timestamp", out var ts)
                        ? DateTime.Parse(ts.GetString()!)
                        : DateTime.UtcNow,
                    Description = evt.TryGetProperty("status", out var status)
                        ? status.GetString()!
                        : "Status update",
                    Location = evt.TryGetProperty("location", out var loc)
                        ? loc.GetString()
                        : null
                });
            }
        }

        return events;
    }

    public async Task CancelShipmentAsync(string waybillId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{this._settings.BaseUrl}/delivery-requests/{waybillId}/cancel");
        AddAuth(request);
        request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

        var response = await this._httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<CourierOfficeDto>> GetOfficesAsync(string? city = null)
    {
        var url = $"{this._settings.BaseUrl}/lockers";
        if (!string.IsNullOrEmpty(city))
            url += $"?city={Uri.EscapeDataString(city)}";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        AddAuth(request);

        var response = await this._httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responseStr = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseStr);

        var offices = new List<CourierOfficeDto>();

        var lockers = result.ValueKind == JsonValueKind.Array
            ? result
            : result.TryGetProperty("lockers", out var lockerList)
                ? lockerList
                : result;

        if (lockers.ValueKind == JsonValueKind.Array)
        {
            foreach (var locker in lockers.EnumerateArray())
            {
                offices.Add(new CourierOfficeDto
                {
                    Id = locker.TryGetProperty("id", out var id) ? (id.ValueKind == JsonValueKind.String ? id.GetString()! : id.GetRawText()) : "",
                    Name = locker.TryGetProperty("name", out var name) ? name.GetString()! : "",
                    City = locker.TryGetProperty("city", out var c) ? c.GetString() : null,
                    Address = locker.TryGetProperty("address", out var addr) ? addr.GetString() : null,
                    Lat = locker.TryGetProperty("latitude", out var lat) ? lat.GetDouble() :
                          locker.TryGetProperty("lat", out var lat2) ? lat2.GetDouble() : null,
                    Lng = locker.TryGetProperty("longitude", out var lng) ? lng.GetDouble() :
                          locker.TryGetProperty("lng", out var lng2) ? lng2.GetDouble() : null,
                    IsLocker = locker.TryGetProperty("type", out var type)
                        ? type.GetString()?.Contains("locker", StringComparison.OrdinalIgnoreCase) == true
                        : true
                });
            }
        }

        return offices;
    }

    private async Task<string> PostBoxNowAsync(string endpoint, object body)
    {
        var json = JsonSerializer.Serialize(body);
        var request = new HttpRequestMessage(HttpMethod.Post, $"{this._settings.BaseUrl}{endpoint}")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        AddAuth(request);

        var response = await this._httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private void AddAuth(HttpRequestMessage request)
    {
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this._settings.ApiKey);
    }
}
