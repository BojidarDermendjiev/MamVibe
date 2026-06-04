using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

using MomVibe.Application.Interfaces;

namespace MomVibe.IntegrationTests;

/// <summary>
/// Integration tests for KnowledgeService that exercise the real Postgres FTS pipeline:
/// tsvector computed column, GIN index, websearch_to_tsquery, ts_rank_cd.
/// Uses CustomWebApplicationFactory so migrations (including seed data) are applied automatically.
/// </summary>
public class KnowledgeServiceIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public KnowledgeServiceIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private IKnowledgeService GetService()
    {
        // Each call creates a fresh scope — avoids cross-test change-tracker pollution.
        var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IKnowledgeService>();
    }

    // =========================================================================
    // Empty / blank input
    // =========================================================================

    [Fact]
    public async Task SearchAsync_EmptyQuery_ReturnsEmpty()
    {
        var results = await GetService().SearchAsync("", "en");
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_WhitespaceQuery_ReturnsEmpty()
    {
        var results = await GetService().SearchAsync("   ", "en");
        results.Should().BeEmpty();
    }

    // =========================================================================
    // No match
    // =========================================================================

    [Fact]
    public async Task SearchAsync_NonMatchingQuery_ReturnsEmpty()
    {
        var results = await GetService().SearchAsync("xyzzy_no_match_ever_9999", "en");
        results.Should().BeEmpty();
    }

    // =========================================================================
    // Seed data is returned for matching queries
    // =========================================================================

    [Fact]
    public async Task SearchAsync_ShippingQuery_ReturnsSeedShippingArticle()
    {
        var results = await GetService().SearchAsync("shipping econt speedy delivery", "en");

        results.Should().NotBeEmpty();
        results.Should().Contain(r =>
            r.Title.Contains("Shipping", StringComparison.OrdinalIgnoreCase) ||
            r.Content.Contains("Econt", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchAsync_PaymentQuery_ReturnsPaymentArticle()
    {
        // "stripe" is an exact lexeme in article 3 content — 'simple' config does no stemming
        var results = await GetService().SearchAsync("stripe wallet", "en");

        results.Should().NotBeEmpty();
        results.Should().Contain(r =>
            r.Content.Contains("Wallet", StringComparison.OrdinalIgnoreCase) ||
            r.Content.Contains("Stripe", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchAsync_BulgarianQuery_ReturnsBgArticle()
    {
        // Use the exact Cyrillic word "еконт" that appears in seed article 2 (BG shipping)
        var results = await GetService().SearchAsync("еконт доставка", "bg");

        results.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchAsync_AllResultsHaveTitleAndContent()
    {
        // "mamvibe" appears in virtually every seed article — safe single-term query
        var results = await GetService().SearchAsync("mamvibe", "en");

        results.Should().NotBeEmpty();
        results.Should().AllSatisfy(r =>
        {
            r.Title.Should().NotBeNullOrEmpty();
            r.Content.Should().NotBeNullOrEmpty();
        });
    }

    // =========================================================================
    // topK limit
    // =========================================================================

    [Fact]
    public async Task SearchAsync_RespectsTopKLimit()
    {
        // "mamvibe" appears in many seed articles — confirm the cap is applied
        var results = await GetService().SearchAsync("mamvibe", "en", topK: 2);
        results.Count.Should().BeLessThanOrEqualTo(2);
    }

    [Fact]
    public async Task SearchAsync_DefaultTopKIsAtMostFour()
    {
        var results = await GetService().SearchAsync("mamvibe buy sell ship pay", "en");
        results.Count.Should().BeLessThanOrEqualTo(4);
    }

    // =========================================================================
    // Fault tolerance — must never throw
    // =========================================================================

    [Fact]
    public async Task SearchAsync_WithSingleSpecialCharQuery_DoesNotThrow()
    {
        // Single punctuation-only queries can produce empty tsquery — service must swallow it
        var act = async () => await GetService().SearchAsync("!@#$", "en");
        await act.Should().NotThrowAsync();
    }
}
