namespace MomVibe.Domain.Constants;

/// <summary>
/// Validation lengths, defaults, and EF Core column comment strings for the ApplicationUser entity.
/// </summary>
public static class ApplicationUserConstants
{
    /// <summary>
    /// Maximum and minimum character lengths used for ApplicationUser field validation.
    /// </summary>
    public static class Lengths
    {
        /// <summary>Minimum allowed length for a user's display name.</summary>
        public const int DisplayNameMin = 2;

        /// <summary>Maximum allowed length for a user's display name.</summary>
        public const int DisplayNameMax = 64;

        /// <summary>Maximum allowed length for a user's biography.</summary>
        public const int BioMax = 500;

        /// <summary>Maximum allowed length for a language/locale code (e.g., "en", "en-US").</summary>
        public const int LanguageCodeMax = 10;

        /// <summary>Maximum allowed length for an avatar image URL.</summary>
        public const int AvatarUrlMax = 2048;

        /// <summary>Maximum allowed length for a Revolut payment tag.</summary>
        public const int RevolutTagMax = 50;
    }

    /// <summary>
    /// Default values applied when a user field is not explicitly provided.
    /// </summary>
    public static class Defaults
    {
        /// <summary>Default language preference assigned to new user accounts.</summary>
        public const string Language = "en";
    }

    /// <summary>
    /// Human-readable column comment strings used in EF Core <c>HasComment</c> configurations.
    /// </summary>
    public static class Comments
    {
        /// <summary>Column comment for the DisplayName property.</summary>
        public const string DisplayName = "Public display name shown to other users.";

        /// <summary>Column comment for the ProfileType property.</summary>
        public const string ProfileType = "Type/category of the user's profile.";

        /// <summary>Column comment for the AvatarUrl property.</summary>
        public const string AvatarUrl = "Absolute URL to the user's avatar image.";

        /// <summary>Column comment for the IsBlocked property.</summary>
        public const string IsBlocked = "Indicates whether the account is blocked from interacting.";

        /// <summary>Column comment for the Bio property.</summary>
        public const string Bio = "User-provided short biography.";

        /// <summary>Column comment for the LanguagePreference property.</summary>
        public const string LanguagePreference = "Preferred language or locale (e.g., en or en-US).";

        /// <summary>Column comment for the CreatedAt property.</summary>
        public const string CreatedAt = "UTC timestamp when the user account was created.";

        /// <summary>Column comment for the RevolutTag property.</summary>
        public const string RevolutTag = "Revolut username or tag for peer-to-peer payments via the Revolut app.";
    }
}
