namespace MomVibe.Domain.Common;

/// <summary>
/// Serves as a base type for domain entities, providing a unique identifier and basic audit timestamps.
/// </summary>
/// <remarks>
/// Inherit from this class to ensure consistent identity and creation/update auditing across domain models.
/// </remarks>
public abstract class BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for this entity.
    /// A new GUID is assigned when the instance is created.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the timestamp when the entity was initially created.
    /// </summary>
    /// <remarks>
    /// It is recommended to store and handle this value in UTC.
    /// </remarks>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the most recent update to this entity,
    /// or <c>null</c> if it has not been updated since creation.
    /// </summary>
    /// <remarks>
    /// It is recommended to store and handle this value in UTC.
    /// </remarks>
    public DateTime? UpdatedAt { get; set; }
}
