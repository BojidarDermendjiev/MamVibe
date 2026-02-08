namespace MomVibe.Domain.Enums;

/// <summary>
/// Specifies the type of user profile for personalization and access logic.
/// </summary>
public enum ProfileType
{
    /// <summary>
    /// Profile for an individual identifying as male.
    /// </summary>
    Male = 0,
    
    /// <summary>
    /// Profile for an individual identifying as female.
    /// </summary>
    Female = 1,

    /// <summary>
    /// Profile representing a household or family account.
    /// </summary>
    Family = 2
}
