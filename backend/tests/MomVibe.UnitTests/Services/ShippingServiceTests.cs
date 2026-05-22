using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

using MomVibe.Application.DTOs.Shipping;
using MomVibe.Application.Interfaces;
using MomVibe.Domain.Enums;
using MomVibe.Infrastructure.Configuration;
using MomVibe.Infrastructure.Services.Shipping;

namespace MomVibe.UnitTests.Services;

/// <summary>
/// Unit tests for CourierProviderFactory (routing) and each courier provider's
/// GetOfficesAsync / CalculatePriceAsync path.
/// HttpClient is injected via a mocked IHttpClientFactory backed by a
/// MockHttpMessageHandler so no real network calls are made.
/// </summary>
public class ShippingServiceTests
{
    // =========================================================================
    // HttpClient factory helpers
    // =========================================================================

    /// <summary>Creates a mock HttpClient that returns the given JSON body with the given status code.</summary>
    private static HttpClient CreateMockHttpClient(string jsonBody, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            });

        return new HttpClient(handler.Object);
    }

    /// <summary>Wraps a pre-built HttpClient in an IHttpClientFactory mock.</summary>
    private static IHttpClientFactory CreateFactory(HttpClient client, string clientName)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(clientName)).Returns(client);
        return factory.Object;
    }

    // =========================================================================
    // CourierProviderFactory — routing tests
    // =========================================================================

    [Fact]
    public void CourierProviderFactory_Resolves_Econt_Provider()
    {
        var providers = new ICourierProvider[] { CreateEcontProvider() };
        var factory = new CourierProviderFactory(providers);

        var provider = factory.GetProvider(CourierProvider.Econt);

        provider.ProviderType.Should().Be(CourierProvider.Econt);
    }

    [Fact]
    public void CourierProviderFactory_Resolves_Speedy_Provider()
    {
        var providers = new ICourierProvider[] { CreateSpeedyProvider() };
        var factory = new CourierProviderFactory(providers);

        var provider = factory.GetProvider(CourierProvider.Speedy);

        provider.ProviderType.Should().Be(CourierProvider.Speedy);
    }

    [Fact]
    public void CourierProviderFactory_Resolves_BoxNow_Provider()
    {
        var providers = new ICourierProvider[] { CreateBoxNowProvider() };
        var factory = new CourierProviderFactory(providers);

        var provider = factory.GetProvider(CourierProvider.BoxNow);

        provider.ProviderType.Should().Be(CourierProvider.BoxNow);
    }

    [Fact]
    public void CourierProviderFactory_Resolves_PigeonExpress_Provider()
    {
        var providers = new ICourierProvider[] { CreatePigeonProvider() };
        var factory = new CourierProviderFactory(providers);

        var provider = factory.GetProvider(CourierProvider.PigeonExpress);

        provider.ProviderType.Should().Be(CourierProvider.PigeonExpress);
    }

    [Fact]
    public void CourierProviderFactory_Throws_ArgumentException_For_Unknown_Provider()
    {
        var factory = new CourierProviderFactory(Array.Empty<ICourierProvider>());

        var act = () => factory.GetProvider(CourierProvider.Econt);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Econt*");
    }

    // =========================================================================
    // Econt — GetOfficesAsync
    // =========================================================================

    [Fact]
    public async Task EcontProvider_GetOfficesAsync_Happy_Path_Returns_Parsed_Offices()
    {
        var json = """
            {
                "offices": [
                    {
                        "code": "OFF-001",
                        "name": "Sofia Center",
                        "address": {
                            "city": { "name": "Sofia" },
                            "fullAddress": "ul. Vitosha 1",
                            "location": { "latitude": 42.7, "longitude": 23.3 }
                        },
                        "isAPS": false
                    }
                ]
            }
            """;

        var provider = CreateEcontProvider(json);
        var result = await provider.GetOfficesAsync("Sofia");

        result.Should().HaveCount(1);
        result[0].Id.Should().Be("OFF-001");
        result[0].Name.Should().Be("Sofia Center");
        result[0].City.Should().Be("Sofia");
    }

    [Fact]
    public async Task EcontProvider_GetOfficesAsync_Returns_Empty_On_Empty_Response()
    {
        var provider = CreateEcontProvider("");
        var result = await provider.GetOfficesAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task EcontProvider_GetOfficesAsync_Returns_Empty_When_No_Offices_Property()
    {
        var provider = CreateEcontProvider("{}");
        var result = await provider.GetOfficesAsync();

        result.Should().BeEmpty();
    }

    // =========================================================================
    // Econt — CalculatePriceAsync
    // =========================================================================

    [Fact]
    public async Task EcontProvider_CalculatePriceAsync_Happy_Path_Returns_Price()
    {
        var json = """{ "label": { "totalPrice": 7.50 } }""";
        var provider = CreateEcontProvider(json);

        var result = await provider.CalculatePriceAsync(new CalculateShippingDto
        {
            CourierProvider = CourierProvider.Econt,
            DeliveryType = DeliveryType.Office,
            Weight = 1m,
            OfficeId = "OFF-001"
        });

        result.Price.Should().Be(7.50m);
        result.Currency.Should().Be("BGN");
    }

    [Fact]
    public async Task EcontProvider_CalculatePriceAsync_Throws_When_Response_Empty()
    {
        var provider = CreateEcontProvider("");

        var act = async () => await provider.CalculatePriceAsync(new CalculateShippingDto
        {
            CourierProvider = CourierProvider.Econt,
            DeliveryType = DeliveryType.Address,
            Weight = 0.5m,
            ToCity = "Plovdiv"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Econt*");
    }

    // =========================================================================
    // Speedy — GetOfficesAsync
    // =========================================================================

    [Fact]
    public async Task SpeedyProvider_GetOfficesAsync_Happy_Path_Returns_Parsed_Offices()
    {
        var json = """
            {
                "offices": [
                    {
                        "id": 123,
                        "name": "Speedy Plovdiv",
                        "address": {
                            "siteName": "Plovdiv",
                            "localAddressString": "bul. Bulgaria 5"
                        }
                    }
                ]
            }
            """;

        var provider = CreateSpeedyProvider(json);
        var result = await provider.GetOfficesAsync("Plovdiv");

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Speedy Plovdiv");
        result[0].City.Should().Be("Plovdiv");
    }

    [Fact]
    public async Task SpeedyProvider_GetOfficesAsync_Returns_Empty_When_No_Offices_Property()
    {
        var provider = CreateSpeedyProvider("{}");
        var result = await provider.GetOfficesAsync();

        result.Should().BeEmpty();
    }

    // =========================================================================
    // Speedy — CalculatePriceAsync
    // =========================================================================

    [Fact]
    public async Task SpeedyProvider_CalculatePriceAsync_Happy_Path_Returns_Price()
    {
        var json = """
            {
                "calculations": [
                    { "price": { "total": 6.20 } }
                ]
            }
            """;
        var provider = CreateSpeedyProvider(json);

        var result = await provider.CalculatePriceAsync(new CalculateShippingDto
        {
            CourierProvider = CourierProvider.Speedy,
            DeliveryType = DeliveryType.Office,
            Weight = 1m
        });

        result.Price.Should().Be(6.20m);
        result.Currency.Should().Be("BGN");
    }

    [Fact]
    public async Task SpeedyProvider_CalculatePriceAsync_Throws_On_Http_Error()
    {
        var provider = CreateSpeedyProvider("{}", HttpStatusCode.ServiceUnavailable);

        var act = async () => await provider.CalculatePriceAsync(new CalculateShippingDto
        {
            CourierProvider = CourierProvider.Speedy,
            DeliveryType = DeliveryType.Address,
            Weight = 0.5m,
            ToCity = "Sofia"
        });

        await act.Should().ThrowAsync<Exception>();
    }

    // =========================================================================
    // BoxNow — GetOfficesAsync
    // =========================================================================

    [Fact]
    public async Task BoxNowProvider_GetOfficesAsync_Happy_Path_Parses_Array_Response()
    {
        var json = """
            [
                {
                    "id": "BN-001",
                    "name": "BoxNow Locker Varna",
                    "city": "Varna",
                    "address": "ul. Primorski 10",
                    "latitude": 43.2,
                    "longitude": 27.9,
                    "type": "locker"
                }
            ]
            """;

        var provider = CreateBoxNowProvider(json);
        var result = await provider.GetOfficesAsync("Varna");

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("BoxNow Locker Varna");
        result[0].City.Should().Be("Varna");
        result[0].IsLocker.Should().BeTrue();
    }

    [Fact]
    public async Task BoxNowProvider_GetOfficesAsync_Returns_Empty_On_Http_Error()
    {
        var provider = CreateBoxNowProvider("{}", HttpStatusCode.Unauthorized);
        var result = await provider.GetOfficesAsync();

        result.Should().BeEmpty();
    }

    // =========================================================================
    // BoxNow — CalculatePriceAsync
    // =========================================================================

    [Fact]
    public async Task BoxNowProvider_CalculatePriceAsync_Happy_Path_Returns_Price()
    {
        var json = """{ "price": 4.99 }""";
        var provider = CreateBoxNowProvider(json);

        var result = await provider.CalculatePriceAsync(new CalculateShippingDto
        {
            CourierProvider = CourierProvider.BoxNow,
            DeliveryType = DeliveryType.Locker,
            Weight = 0.5m,
            OfficeId = "BN-001"
        });

        result.Price.Should().Be(4.99m);
        result.Currency.Should().Be("BGN");
    }

    [Fact]
    public async Task BoxNowProvider_CalculatePriceAsync_Throws_On_Http_Error()
    {
        var provider = CreateBoxNowProvider("{}", HttpStatusCode.Forbidden);

        var act = async () => await provider.CalculatePriceAsync(new CalculateShippingDto
        {
            CourierProvider = CourierProvider.BoxNow,
            DeliveryType = DeliveryType.Locker,
            Weight = 1m
        });

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // =========================================================================
    // PigeonExpress — GetOfficesAsync
    // =========================================================================

    [Fact]
    public async Task PigeonProvider_GetOfficesAsync_Happy_Path_Parses_Array_Response()
    {
        var json = """
            [
                {
                    "id": "PE-001",
                    "name": "Pigeon Burgas",
                    "city": "Burgas",
                    "address": "ul. Aleksandrovska 22"
                }
            ]
            """;

        var provider = CreatePigeonProvider(json);
        var result = await provider.GetOfficesAsync("Burgas");

        result.Should().HaveCount(1);
        result[0].Id.Should().Be("PE-001");
        result[0].Name.Should().Be("Pigeon Burgas");
    }

    [Fact]
    public async Task PigeonProvider_GetOfficesAsync_Returns_Empty_On_Http_Error()
    {
        var provider = CreatePigeonProvider("{}", HttpStatusCode.NotFound);
        var result = await provider.GetOfficesAsync();

        result.Should().BeEmpty();
    }

    // =========================================================================
    // PigeonExpress — CalculatePriceAsync
    // =========================================================================

    [Fact]
    public async Task PigeonProvider_CalculatePriceAsync_Happy_Path_Returns_Price()
    {
        var json = """{ "price": 5.50 }""";
        var provider = CreatePigeonProvider(json);

        var result = await provider.CalculatePriceAsync(new CalculateShippingDto
        {
            CourierProvider = CourierProvider.PigeonExpress,
            DeliveryType = DeliveryType.Office,
            Weight = 1m,
            OfficeId = "PE-001"
        });

        result.Price.Should().Be(5.50m);
        result.Currency.Should().Be("BGN");
    }

    [Fact]
    public async Task PigeonProvider_CalculatePriceAsync_Throws_On_Http_Error()
    {
        var provider = CreatePigeonProvider("{}", HttpStatusCode.InternalServerError);

        var act = async () => await provider.CalculatePriceAsync(new CalculateShippingDto
        {
            CourierProvider = CourierProvider.PigeonExpress,
            DeliveryType = DeliveryType.Address,
            Weight = 1m,
            ToCity = "Sofia"
        });

        await act.Should().ThrowAsync<Exception>();
    }

    // =========================================================================
    // Provider factory helpers (build real provider objects with mocked HTTP)
    // =========================================================================

    private static EcontCourierProvider CreateEcontProvider(string? responseBody = null, HttpStatusCode status = HttpStatusCode.OK)
    {
        var body = responseBody ?? "{}";
        var client = CreateMockHttpClient(body, status);
        var factory = CreateFactory(client, "Econt");
        var settings = Options.Create(new EcontSettings
        {
            BaseUrl = "https://demo.econt.com/ee/services",
            Username = "test",
            Password = "test"
        });
        return new EcontCourierProvider(factory, settings);
    }

    private static SpeedyCourierProvider CreateSpeedyProvider(string? responseBody = null, HttpStatusCode status = HttpStatusCode.OK)
    {
        var body = responseBody ?? "{}";
        var client = CreateMockHttpClient(body, status);
        var factory = CreateFactory(client, "Speedy");
        var settings = Options.Create(new SpeedySettings
        {
            BaseUrl = "https://api.speedy.bg/v1",
            Username = "test",
            Password = "test"
        });
        return new SpeedyCourierProvider(factory, settings);
    }

    private static BoxNowCourierProvider CreateBoxNowProvider(string? responseBody = null, HttpStatusCode status = HttpStatusCode.OK)
    {
        var body = responseBody ?? "{}";
        var client = CreateMockHttpClient(body, status);
        var factory = CreateFactory(client, "BoxNow");
        var settings = Options.Create(new BoxNowSettings
        {
            BaseUrl = "https://api.boxnow.bg/api/v1",
            ApiKey = "test-key"
        });
        return new BoxNowCourierProvider(factory, settings);
    }

    private static PigeonExpressCourierProvider CreatePigeonProvider(string? responseBody = null, HttpStatusCode status = HttpStatusCode.OK)
    {
        var body = responseBody ?? "{}";
        var client = CreateMockHttpClient(body, status);
        var factory = CreateFactory(client, "PigeonExpress");
        var settings = Options.Create(new PigeonExpressSettings
        {
            BaseUrl = "https://api.pigeonexpress.com/v1",
            ApiKey = "test-key"
        });
        return new PigeonExpressCourierProvider(factory, settings);
    }
}
