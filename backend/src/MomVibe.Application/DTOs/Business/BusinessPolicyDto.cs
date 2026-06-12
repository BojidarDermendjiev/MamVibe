namespace MomVibe.Application.DTOs.Business;

/// <summary>
/// Read-only projection of a <c>BusinessPolicyVersion</c> returned to clients
/// rendering the acceptance modal.
/// </summary>
public class BusinessPolicyDto
{
    /// <summary>Identifier of the policy version (carried back on acceptance).</summary>
    public Guid Id { get; set; }

    /// <summary>Monotonically increasing version number per language.</summary>
    public int Version { get; set; }

    /// <summary>BCP-47 language code ("en", "bg").</summary>
    public string Language { get; set; } = "en";

    /// <summary>Short title shown at the top of the acceptance modal.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Markdown body rendered inside the modal.</summary>
    public string BodyMarkdown { get; set; } = string.Empty;

    /// <summary>UTC date from which this version became canonical.</summary>
    public DateTime EffectiveFrom { get; set; }
}
