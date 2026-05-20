namespace MomVibe.Application.Interfaces;

/// <summary>
/// Abstraction over a text-only LLM backend used by the MamVibe assistant chat widget.
/// Implement this interface to add new providers (Groq, Gemini, OpenAI, Ollama, …).
/// Register implementations as keyed services using the provider name as the key
/// (e.g. "groq", "anthropic") and set AI:ChatProvider in config to select one at runtime.
/// </summary>
public interface ILlmChatProvider
{
    /// <summary>
    /// Sends a conversation turn to the underlying LLM and returns the assistant's reply.
    /// </summary>
    /// <param name="systemPrompt">Behavioural instructions injected as the system turn.</param>
    /// <param name="history">Full conversation so far, including the current user message as the last entry.</param>
    /// <param name="model">Model identifier to use (overrides the provider's configured default).</param>
    Task<string> ChatAsync(
        string systemPrompt,
        IReadOnlyList<(string role, string content)> history,
        string model);
}
