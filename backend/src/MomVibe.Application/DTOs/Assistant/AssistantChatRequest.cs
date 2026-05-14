namespace MomVibe.Application.DTOs.Assistant;

/// <summary>Payload sent to the MamVibe assistant chat endpoint.</summary>
public class AssistantChatRequest
{
    /// <summary>The user's new message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Previous turns in the conversation (up to 10 kept).</summary>
    public List<AssistantMessage>? History { get; set; }

    /// <summary>UI language ("en" or "bg") — assistant replies in this language.</summary>
    public string Language { get; set; } = "en";
}

/// <summary>A single turn in the assistant conversation history.</summary>
public class AssistantMessage
{
    /// <summary>"user" or "assistant".</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>Text content of the turn.</summary>
    public string Content { get; set; } = string.Empty;
}
