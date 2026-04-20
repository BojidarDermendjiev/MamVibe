namespace MomVibe.Infrastructure.Services.Shipping;

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

using Domain.Enums;
using Configuration;
using Application.DTOs.Shipping;
using Application.Interfaces;

/// <summary>
/// Pigeon Express courier provider.
/// Authentication: Bearer API key in Authorization header.
///
/// TODO: Replace placeholder endpoint paths and field names once official
/// API docs are obtained from pigeonexpress.com business registration.
/// Placeholder endpoints follow conventional REST courier API patterns.
/// </summary>
public class PigeonExpressCourierProvider : ICourierProvider
{
    private readonly HttpClient _httpClient;
    private readonly PigeonExpressSettings _settings;

    public PigeonExpressCourierProvider(IHttpClientFactory httpClientFactory, IOptions<PigeonExpressSettings> settings)
    {
        this._httpClient = httpClientFactory.CreateClient("PigeonExpress");
        this._settings = settings.Value;
    }

    public CourierProvider ProviderType => CourierProvider.PigeonExpress;

    public async Task<ShippingPriceResultDto> CalculatePriceAsync(CalculateShippingDto request)
    {
        // TODO: Replace with actual Pigeon Express price calculation endpoint and field names.
        var body = new
        {
            delivery_type = request.DeliveryType == DeliveryType.Address ? "address" : "office",
            to_city = request.ToCity,
            office_id = request.DeliveryType != DeliveryType.Address ? request.OfficeId : null,
            weight = request.Weight,
            cod = request.IsCod,
            cod_amount = request.IsCod ? request.CodAmount : 0,
            insured_amount = request.IsInsured ? request.InsuredAmount : 0
        };

        var response = await PostAsync("/shipments/calculate", body);
        var result = JsonSerializer.Deserialize<JsonElement>(response);

        var price = 0m;
        if (result.TryGetProperty("price", out var priceEl) && priceEl.ValueKind == JsonValueKind.Number)
            price = priceEl.GetDecimal();

        return new ShippingPriceResultDto
        {
            Price = price,
            Currency = "BGN",
            EstimatedDelivery = "1-3 business days"
        };
    }

    public async Task<(string TrackingNumber, string WaybillId, string? LabelUrl)> CreateShipmentAsync(CreateShipmentDto request)
    {
        // TODO: Replace with actual Pigeon Express shipment creation endpoint and field names.
        var body = new
        {
            recipient = new
            {
                name = request.RecipientName,
                phone = request.RecipientPhone
            },
            delivery = new
            {
                type = request.DeliveryType == DeliveryType.Address ? "address" : "office",
                city = request.City,
                address = request.DeliveryType == DeliveryType.Address ? request.DeliveryAddress : null,
                office_id = request.DeliveryType != DeliveryType.Address ? request.OfficeId : null,
                office_name = request.DeliveryType != DeliveryType.Address ? request.OfficeName : null
            },
            parcel = new
            {
                weight = request.Weight
            },
            payment = new
            {
                cod = request.IsCod,
                cod_amount = request.IsCod ? request.CodAmount : 0,
                insured_amount = request.IsInsured ? request.InsuredAmount : 0
            }
        };

        var response = await PostAsync("/shipments", body);
        var result = JsonSerializer.Deserialize<JsonElement>(response);

        var trackingNumber = result.TryGetProperty("tracking_number", out var tn) ? tn.GetString() ?? "" : "";
        var waybillId      = result.TryGetProperty("waybill_id", out var wb)       ? wb.GetString() ?? trackingNumber : trackingNumber;
        var labelUrl       = result.TryGetProperty("label_url", out var lu)         ? lu.GetString() : null;

        return (trackingNumber, waybillId, labelUrl);
    }

    public async Task<byte[]> GetLabelAsync(string waybillId)
    {
        // TODO: Replace with actual Pigeon Express label download endpoint.
        SetAuthHeader();
        var response = await _httpClient.GetAsync($"{_settings.BaseUrl}/shipments/{waybillId}/label");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<List<TrackingEventDto>> TrackAsync(string trackingNumber)
    {
        // TODO: Replace with actual Pigeon Express tracking endpoint and response field names.
        SetAuthHeader();
        var response = await _httpClient.GetAsync($"{_settings.BaseUrl}/shipments/{trackingNumber}/tracking");
        if (!response.IsSuccessStatusCode)
            return new List<TrackingEventDto>();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(json);

        var events = new List<TrackingEventDto>();
        if (result.TryGetProperty("events", out var eventList))
        {
            foreach (var ev in eventList.EnumerateArray())
            {
                events.Add(new TrackingEventDto
                {
                    Timestamp = ev.TryGetProperty("timestamp", out var ts)
                        ? DateTime.Parse(ts.GetString()!)
                        : DateTime.UtcNow,
                    Description = ev.TryGetProperty("status", out var st) ? st.GetString()! : "Status update",
                    Location    = ev.TryGetProperty("location", out var loc) ? loc.GetString() : null
                });
            }
        }

        return events;
    }

    public async Task CancelShipmentAsync(string waybillId)
    {
        // TODO: Replace with actual Pigeon Express cancellation endpoint.
        SetAuthHeader();
        var response = await _httpClient.DeleteAsync($"{_settings.BaseUrl}/shipments/{waybillId}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<CourierOfficeDto>> GetOfficesAsync(string? city = null)
    {
        // TODO: Replace with actual Pigeon Express offices/APS endpoint and response field names.
        SetAuthHeader();
        var url = string.IsNullOrWhiteSpace(city)
            ? $"{_settings.BaseUrl}/offices"
            : $"{_settings.BaseUrl}/offices?city={Uri.EscapeDataString(city)}";

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return new List<CourierOfficeDto>();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(json);

        var offices = new List<CourierOfficeDto>();
        var list = result.ValueKind == JsonValueKind.Array
            ? result
            : result.TryGetProperty("offices", out var ol) ? ol : default;

        if (list.ValueKind == JsonValueKind.Array)
        {
            foreach (var office in list.EnumerateArray())
            {
                offices.Add(new CourierOfficeDto
                {
                    Id       = office.TryGetProperty("id", out var id)           ? id.GetString()!           : "",
                    Name     = office.TryGetProperty("name", out var name)       ? name.GetString()!         : "",
                    City     = office.TryGetProperty("city", out var c)          ? c.GetString()             : null,
                    Address  = office.TryGetProperty("address", out var addr)    ? addr.GetString()          : null,
                    Lat      = office.TryGetProperty("lat", out var lat) && lat.ValueKind == JsonValueKind.Number ? lat.GetDouble() : null,
                    Lng      = office.TryGetProperty("lng", out var lng) && lng.ValueKind == JsonValueKind.Number ? lng.GetDouble() : null,
                    IsLocker = office.TryGetProperty("is_aps", out var aps) && aps.ValueKind == JsonValueKind.True
                });
            }
        }

        return offices;
    }

    private async Task<string> PostAsync(string endpoint, object body)
    {
        SetAuthHeader();
        var json    = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{_settings.BaseUrl}{endpoint}", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private void SetAuthHeader()
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.ApiKey);
    }
}
