namespace MomVibe.Infrastructure.Services;

using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using System.Security.Cryptography;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Domain.Entities;
using Domain.Enums;
using Application.Interfaces;
using Infrastructure.Configuration;

/// <summary>
/// Creates digital receipt orders via the Take a NAP public API (v2).
/// Uses HMAC-SHA256 signature authentication.
/// </summary>
public class TakeANapService : ITakeANapService
{
    private readonly HttpClient _httpClient;
    private readonly TakeANapSettings _settings;
    private readonly ILogger<TakeANapService> _logger;

    public TakeANapService(IHttpClientFactory httpClientFactory, IOptions<TakeANapSettings> settings, ILogger<TakeANapService> logger)
    {
        this._httpClient = httpClientFactory.CreateClient("TakeANap");
        this._settings = settings.Value;
        this._logger = logger;
    }

    public async Task<string?> CreateOrderAndGetReceiptAsync(Payment payment)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(this._settings.ApiKey) || this._settings.ApiKey.Contains("YOUR_"))
            {
                this._logger.LogWarning("TakeANap is not configured. Skipping receipt creation.");
                return null;
            }

            var orderId = await CreateOrderAsync(payment);
            if (orderId == null) return null;

            var receiptUrl = await GetReceiptUrlAsync(orderId);
            return receiptUrl;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to create TakeANap receipt for payment {PaymentId}", payment.Id);
            return null;
        }
    }

    public async Task<string?> CreateWalletReceiptAsync(WalletTransaction transaction, string customerEmail)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(this._settings.ApiKey) || this._settings.ApiKey.Contains("YOUR_"))
            {
                this._logger.LogWarning("TakeANap is not configured. Skipping wallet receipt.");
                return null;
            }

            var lineItemName = transaction.Kind switch
            {
                WalletTransactionKind.TopUp       => "Зареждане на портфейл",
                WalletTransactionKind.Transfer    => "Превод между потребители",
                WalletTransactionKind.ItemPayment => transaction.Description ?? "Плащане за артикул",
                WalletTransactionKind.Refund      => "Възстановяване на плащане",
                _                                 => "Транзакция"
            };

            var body = new
            {
                shopId     = _settings.ShopId,
                internalId = transaction.Id.ToString(),
                currency   = "EUR",
                paymentType = "PAYMENT_PROCESSOR",
                customer   = new { email = customerEmail },
                lineItems  = new[]
                {
                    new
                    {
                        name      = lineItemName,
                        quantity  = 1,
                        price     = transaction.Amount,
                        vatGroup  = "GROUP_B",
                        vatRate   = 20
                    }
                }
            };

            var jsonBody  = JsonSerializer.Serialize(body);
            var path      = "/public-api/v2/order";
            var request   = CreateSignedRequest(HttpMethod.Post, path, jsonBody);
            var response  = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                this._logger.LogWarning("TakeANap CreateOrder (wallet) failed: {Status} {Error}", response.StatusCode, error);
                return null;
            }

            var result  = await response.Content.ReadFromJsonAsync<JsonElement>();
            var orderId = result.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
            return orderId == null ? null : await GetReceiptUrlAsync(orderId);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to create TakeANap receipt for wallet transaction {TxId}", transaction.Id);
            return null;
        }
    }

    private async Task<string?> CreateOrderAsync(Payment payment)
    {
        var path = "/public-api/v2/order";
        var body = new
        {
            shopId = _settings.ShopId,
            internalId = payment.Id.ToString(),
            currency = "BGN",
            paymentType = "PAYMENT_PROCESSOR",
            customer = new
            {
                email = payment.Buyer?.Email ?? "unknown@momvibe.bg"
            },
            lineItems = new[]
            {
                new
                {
                    name = payment.Item?.Title ?? "Item",
                    quantity = 1,
                    price = payment.Amount,
                    vatGroup = "GROUP_B",
                    vatRate = 20
                }
            }
        };

        var jsonBody = JsonSerializer.Serialize(body);
        var request = CreateSignedRequest(HttpMethod.Post, path, jsonBody);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            this._logger.LogWarning("TakeANap CreateOrder failed: {Status} {Error}", response.StatusCode, error);
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
    }

    private async Task<string?> GetReceiptUrlAsync(string orderId)
    {
        var path = $"/public-api/v2/order/{orderId}/receipt/download";
        var request = CreateSignedRequest(HttpMethod.Get, path, "");

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            this._logger.LogWarning("TakeANap GetReceipt failed: {Status} {Error}", response.StatusCode, error);
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result.TryGetProperty("url", out var urlProp) ? urlProp.GetString() : null;
    }

    private HttpRequestMessage CreateSignedRequest(HttpMethod method, string path, string body)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var message = $"{method.Method}{path}{body}{timestamp}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(this._settings.ApiSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        var signature = Convert.ToBase64String(hash);

        var request = new HttpRequestMessage(method, $"{this._settings.BaseUrl}{path}");
        request.Headers.Add("x-api-key", this._settings.ApiKey);
        request.Headers.Add("x-signature", signature);
        request.Headers.Add("x-timestamp", timestamp);

        if (method != HttpMethod.Get && !string.IsNullOrEmpty(body))
        {
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        return request;
    }
}
