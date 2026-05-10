using System.ComponentModel.DataAnnotations;

namespace MomVibe.Domain.Entities;

/// <summary>
/// Represents a persistent key/value application setting stored in the database.
/// Used for runtime-configurable settings such as the active AI model identifier.
/// </summary>
public class AppSetting
{
    /// <summary>Gets or sets the unique key identifying this setting (acts as the primary key).</summary>
    [Key]
    public string Key { get; set; } = string.Empty;

    /// <summary>Gets or sets the serialized value of the setting.</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC timestamp when this setting was last modified.</summary>
    public DateTime UpdatedAt { get; set; }
}
