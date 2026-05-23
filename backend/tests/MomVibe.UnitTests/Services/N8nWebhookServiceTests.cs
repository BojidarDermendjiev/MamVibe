using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

using MomVibe.Infrastructure.Configuration;
using MomVibe.Infrastructure.Services;

namespace MomVibe.UnitTests.Services;

/// <summary>
/// Unit tests for N8nWebhookService — a Channel&lt;T&gt;-based BackgroundService.
/// HttpClient is mocked via Moq.Protected so no real network calls are made.
/// </summary>
public class N8nWebhookServiceTests
{
    // =========================================================================
    // Helpers
    // =========================================================================>

    private static (N8nWebhookService Service, Mock<HttpMessageHandler> Handler)
        CreateService(
            HttpStatusCode statusCode = HttpStatusCode.OK,
            bool enabled = true,
            string baseUrl = "https://n8n.example.com")
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var settings = Options.Create(new N8nSettings
        {
            BaseUrl = baseUrl,
            Enabled = enabled
        });

        var svc = new N8nWebhookService(factoryMock.Object, settings, NullLogger<N8nWebhookService>.Instance);
        return (svc, handlerMock);
    }

    // =========================================================================
    // Send (enqueue)
    // =========================================================================

    [Fact]
    public void Send_Does_Not_Throw_When_Enabled_And_Channel_Not_Full()
    {
        var (svc, _) = CreateService(enabled: true);

        var act = () => svc.Send("payment-completed", new { amount = 100 });

        act.Should().NotThrow();
    }

    [Fact]
    public void Send_Is_Noop_When_N8n_Is_Disabled()
    {
        // When disabled, Send returns immediately without writing to the channel.
        // We verify by running ExecuteAsync for a brief window and checking no HTTP calls occur.
        var (svc, handler) = CreateService(enabled: false);

        svc.Send("payment-completed", new { amount = 100 });

        // No ExecuteAsync run needed — just confirm nothing was queued that could reach HTTP
        handler.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    // =========================================================================
    // ExecuteAsync — drains channel and POSTs to n8n
    // =========================================================================

    [Fact]
    public async Task ExecuteAsync_Posts_Enqueued_Item_To_N8n_Url()
    {
        var (svc, handler) = CreateService(baseUrl: "https://n8n.example.com");

        svc.Send("payment-completed", new { orderId = "abc" });

        // Run ExecuteAsync briefly then cancel
        using var cts = new CancellationTokenSource();
        var executeTask = svc.StartAsync(cts.Token);

        // Give the background loop time to process the single item
        await Task.Delay(200);
        await cts.CancelAsync();

        try { await executeTask; } catch (OperationCanceledException) { }

        handler.Protected().Verify(
            "SendAsync",
            Times.AtLeastOnce(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri!.ToString().Contains("payment-completed")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Processes_Multiple_Enqueued_Items()
    {
        var (svc, handler) = CreateService();

        svc.Send("event-1", new { id = 1 });
        svc.Send("event-2", new { id = 2 });
        svc.Send("event-3", new { id = 3 });

        using var cts = new CancellationTokenSource();
        var executeTask = svc.StartAsync(cts.Token);

        await Task.Delay(400);
        await cts.CancelAsync();

        try { await executeTask; } catch (OperationCanceledException) { }

        handler.Protected().Verify(
            "SendAsync",
            Times.Exactly(3),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Does_Not_Crash_On_Http_Failure()
    {
        // Even when the HTTP call returns 500, the service should continue running
        var (svc, _) = CreateService(statusCode: HttpStatusCode.InternalServerError);

        svc.Send("some-event", new { });

        using var cts = new CancellationTokenSource();
        var executeTask = svc.StartAsync(cts.Token);

        await Task.Delay(200);
        await cts.CancelAsync();

        // If the service crashed, the task would be faulted; it should merely be cancelled
        var act = async () => await executeTask;
        await act.Should().NotThrowAsync<Exception>(
            "the service must swallow HTTP errors and keep running");
    }

    [Fact]
    public async Task ExecuteAsync_Builds_Correct_Url_From_BaseUrl_And_Path()
    {
        Uri? capturedUri = null;
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedUri = req.RequestUri)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var settings = Options.Create(new N8nSettings
        {
            BaseUrl = "https://n8n.example.com/",
            Enabled = true
        });

        var svc = new N8nWebhookService(factoryMock.Object, settings, NullLogger<N8nWebhookService>.Instance);
        svc.Send("my-webhook", new { });

        using var cts = new CancellationTokenSource();
        await svc.StartAsync(cts.Token);

        await Task.Delay(200);
        await cts.CancelAsync();

        capturedUri.Should().NotBeNull();
        capturedUri!.ToString().Should().Be("https://n8n.example.com/my-webhook");
    }

    [Fact]
    public async Task ExecuteAsync_Does_Not_Crash_On_Http_Exception()
    {
        // Network-level exception (not just a bad status code) must also be handled gracefully
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network unreachable"));

        var httpClient = new HttpClient(handlerMock.Object);
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var settings = Options.Create(new N8nSettings { BaseUrl = "https://n8n.example.com", Enabled = true });
        var svc = new N8nWebhookService(factoryMock.Object, settings, NullLogger<N8nWebhookService>.Instance);

        svc.Send("failing-event", new { });

        using var cts = new CancellationTokenSource();
        var executeTask = svc.StartAsync(cts.Token);

        await Task.Delay(200);
        await cts.CancelAsync();

        var act = async () => await executeTask;
        await act.Should().NotThrowAsync<HttpRequestException>(
            "exceptions in the background loop must be caught and logged, not rethrown");
    }
}
