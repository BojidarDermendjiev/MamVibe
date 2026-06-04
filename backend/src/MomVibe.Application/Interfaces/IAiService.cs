namespace MomVibe.Application.Interfaces;

/// <summary>
/// AI assistant chat: generates conversational replies for the MamVibe chat widget.
/// Listing assistance is in <see cref="IAiListingService"/>;
/// content moderation is in <see cref="IAiModerationService"/>.
/// </summary>
public interface IAiService
{
    Task<string> ChatAsync(
        string systemPrompt,
        IReadOnlyList<(string role, string content)> history);
}
