using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

using MomVibe.Application.DTOs.Assistant;
using MomVibe.Application.Interfaces;
using MomVibe.WebApi.Controllers;

namespace MomVibe.UnitTests.Controllers;

/// <summary>
/// Unit tests for AssistantController.
/// IAiService and IKnowledgeService are Moq mocks — no real LLM or DB calls.
/// Tests cover: request validation, off-topic guard (zero LLM cost), RAG context injection.
/// </summary>
public class AssistantControllerTests
{
    // =========================================================================
    // Helpers
    // =========================================================================

    private static AssistantController CreateController(
        Mock<IAiService>? aiMock = null,
        Mock<IKnowledgeService>? knowledgeMock = null)
    {
        if (aiMock is null)
        {
            aiMock = new Mock<IAiService>();
            aiMock.Setup(a => a.ChatAsync(
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyList<(string, string)>>()))
                .ReturnsAsync("Test reply");
        }

        if (knowledgeMock is null)
        {
            knowledgeMock = new Mock<IKnowledgeService>();
            knowledgeMock.Setup(k => k.SearchAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(Array.Empty<KnowledgeArticleDto>());
        }

        return new AssistantController(aiMock.Object, knowledgeMock.Object);
    }

    /// <summary>Extracts the anonymous {reply} value from an OkObjectResult.</summary>
    private static string GetReply(IActionResult result)
    {
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = ok.Value!;
        return value.GetType().GetProperty("reply")!.GetValue(value)!.ToString()!;
    }

    // =========================================================================
    // Request validation
    // =========================================================================

    [Fact]
    public async Task Chat_EmptyMessage_Returns400()
    {
        var ctrl = CreateController();
        var result = await ctrl.Chat(new AssistantChatRequest { Message = "" });
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Chat_WhitespaceMessage_Returns400()
    {
        var ctrl = CreateController();
        var result = await ctrl.Chat(new AssistantChatRequest { Message = "   " });
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Chat_MessageExactly601Chars_Returns400()
    {
        var ctrl = CreateController();
        var result = await ctrl.Chat(new AssistantChatRequest { Message = new string('x', 601) });
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Chat_MessageExactly600Chars_Returns200()
    {
        var aiMock = new Mock<IAiService>();
        aiMock.Setup(a => a.ChatAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<(string, string)>>()))
            .ReturnsAsync("OK");
        var ctrl = CreateController(aiMock);
        var result = await ctrl.Chat(new AssistantChatRequest { Message = new string('x', 600) });
        result.Should().BeOfType<OkObjectResult>();
    }

    // =========================================================================
    // Off-topic guard — must reject without calling LLM
    // =========================================================================

    [Theory]
    [InlineData("write me a poem about spring")]
    [InlineData("write a story about a dragon")]
    [InlineData("tell me a joke")]
    [InlineData("what is the weather in Sofia today")]
    [InlineData("give me a recipe for banitsa")]
    [InlineData("who is the president of Bulgaria")]
    [InlineData("what is the capital of France")]
    [InlineData("show me the latest news")]
    [InlineData("explain quantum mechanics")]
    public async Task Chat_OffTopicMessage_Returns200_WithRejection_AndNoAiCall(string offTopicMessage)
    {
        var aiMock = new Mock<IAiService>();
        var ctrl = CreateController(aiMock);

        var result = await ctrl.Chat(new AssistantChatRequest { Message = offTopicMessage, Language = "en" });

        GetReply(result).Should().Contain("MamVibe platform");

        aiMock.Verify(a => a.ChatAsync(
            It.IsAny<string>(),
            It.IsAny<IReadOnlyList<(string, string)>>()),
            Times.Never,
            "off-topic guard must fire before any LLM call");
    }

    [Fact]
    public async Task Chat_OffTopicMessage_WithBulgarianLanguage_Returns200_WithBulgarianRejection()
    {
        var aiMock = new Mock<IAiService>();
        var ctrl = CreateController(aiMock);

        var result = await ctrl.Chat(new AssistantChatRequest { Message = "write me a poem", Language = "bg" });

        var reply = GetReply(result);
        // Bulgarian rejection must contain Cyrillic text, not the English phrase
        reply.Should().Contain("MamVibe");
        reply.Should().NotBe("I can only help with questions about the MamVibe platform.");

        aiMock.Verify(a => a.ChatAsync(
            It.IsAny<string>(),
            It.IsAny<IReadOnlyList<(string, string)>>()),
            Times.Never);
    }

    // =========================================================================
    // RAG context injection
    // =========================================================================

    [Fact]
    public async Task Chat_WhenKnowledgeReturnsArticles_InjectsContextBlockInSystemPrompt()
    {
        var aiMock = new Mock<IAiService>();
        aiMock.Setup(a => a.ChatAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<(string, string)>>()))
            .ReturnsAsync("Shipping reply");

        var knowledgeMock = new Mock<IKnowledgeService>();
        knowledgeMock.Setup(k => k.SearchAsync("How does shipping work?", "en", It.IsAny<int>()))
            .ReturnsAsync([new KnowledgeArticleDto { Title = "Shipping", Content = "Econt and Speedy" }]);

        var ctrl = CreateController(aiMock, knowledgeMock);
        await ctrl.Chat(new AssistantChatRequest { Message = "How does shipping work?", Language = "en" });

        aiMock.Verify(a => a.ChatAsync(
            It.Is<string>(prompt => prompt.Contains("<context>") && prompt.Contains("Econt and Speedy")),
            It.IsAny<IReadOnlyList<(string, string)>>()),
            Times.Once,
            "retrieved articles must appear inside a <context> block in the system prompt");
    }

    [Fact]
    public async Task Chat_WhenKnowledgeReturnsEmpty_NoContextBlockInSystemPrompt()
    {
        var aiMock = new Mock<IAiService>();
        aiMock.Setup(a => a.ChatAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<(string, string)>>()))
            .ReturnsAsync("General reply");

        var knowledgeMock = new Mock<IKnowledgeService>();
        knowledgeMock.Setup(k => k.SearchAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(Array.Empty<KnowledgeArticleDto>());

        var ctrl = CreateController(aiMock, knowledgeMock);
        await ctrl.Chat(new AssistantChatRequest { Message = "Any question?", Language = "en" });

        aiMock.Verify(a => a.ChatAsync(
            It.Is<string>(prompt => !prompt.Contains("<context>")),
            It.IsAny<IReadOnlyList<(string, string)>>()),
            Times.Once,
            "when no articles are returned, the system prompt must not contain a <context> block");
    }

    [Fact]
    public async Task Chat_WhenMultipleArticlesReturned_AllAreSeparatedByDivider_InPrompt()
    {
        var aiMock = new Mock<IAiService>();
        aiMock.Setup(a => a.ChatAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<(string, string)>>()))
            .ReturnsAsync("Reply");

        var knowledgeMock = new Mock<IKnowledgeService>();
        knowledgeMock.Setup(k => k.SearchAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync([
                new KnowledgeArticleDto { Title = "Article A", Content = "Content A" },
                new KnowledgeArticleDto { Title = "Article B", Content = "Content B" },
            ]);

        var ctrl = CreateController(aiMock, knowledgeMock);
        await ctrl.Chat(new AssistantChatRequest { Message = "something", Language = "en" });

        aiMock.Verify(a => a.ChatAsync(
            It.Is<string>(prompt =>
                prompt.Contains("Article A") &&
                prompt.Contains("Article B") &&
                prompt.Contains("---")),
            It.IsAny<IReadOnlyList<(string, string)>>()),
            Times.Once);
    }

    // =========================================================================
    // Knowledge service call arguments
    // =========================================================================

    [Fact]
    public async Task Chat_CallsKnowledgeService_WithOriginalMessageAndCorrectLanguage()
    {
        var aiMock = new Mock<IAiService>();
        aiMock.Setup(a => a.ChatAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<(string, string)>>()))
            .ReturnsAsync("OK");

        var knowledgeMock = new Mock<IKnowledgeService>();
        knowledgeMock.Setup(k => k.SearchAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(Array.Empty<KnowledgeArticleDto>());

        var ctrl = CreateController(aiMock, knowledgeMock);
        await ctrl.Chat(new AssistantChatRequest { Message = "What couriers do you support?", Language = "bg" });

        knowledgeMock.Verify(k => k.SearchAsync(
            "What couriers do you support?",
            "bg",
            It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task Chat_UnknownLanguage_DefaultsToEnglish_ForKnowledgeSearch()
    {
        var aiMock = new Mock<IAiService>();
        aiMock.Setup(a => a.ChatAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<(string, string)>>()))
            .ReturnsAsync("OK");

        var knowledgeMock = new Mock<IKnowledgeService>();
        knowledgeMock.Setup(k => k.SearchAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(Array.Empty<KnowledgeArticleDto>());

        var ctrl = CreateController(aiMock, knowledgeMock);
        await ctrl.Chat(new AssistantChatRequest { Message = "Hello?", Language = "fr" });

        // "fr" is not "bg" so it falls back to "en"
        knowledgeMock.Verify(k => k.SearchAsync("Hello?", "en", It.IsAny<int>()), Times.Once);
    }

    // =========================================================================
    // End-to-end shape of the response
    // =========================================================================

    [Fact]
    public async Task Chat_ValidMessage_ReturnsReplyFromAiService()
    {
        var aiMock = new Mock<IAiService>();
        aiMock.Setup(a => a.ChatAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<(string, string)>>()))
            .ReturnsAsync("We support Econt and Speedy.");

        var ctrl = CreateController(aiMock);
        var result = await ctrl.Chat(new AssistantChatRequest { Message = "Which couriers?" });

        GetReply(result).Should().Be("We support Econt and Speedy.");
    }

    [Fact]
    public async Task Chat_UserMessageIsWrappedInXmlDelimiters_InHistory()
    {
        string? capturedHistory = null;
        var aiMock = new Mock<IAiService>();
        aiMock.Setup(a => a.ChatAsync(
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<(string, string)>>()))
            .Callback<string, IReadOnlyList<(string role, string content)>>((_, h) =>
                capturedHistory = string.Join("|", h.Select(x => x.content)))
            .ReturnsAsync("OK");

        var ctrl = CreateController(aiMock);
        await ctrl.Chat(new AssistantChatRequest { Message = "How to buy?" });

        capturedHistory.Should().Contain("<user_message>How to buy?</user_message>");
    }
}
