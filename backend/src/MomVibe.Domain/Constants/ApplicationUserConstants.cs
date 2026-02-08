namespace MomVibe.Domain.Constants;

/// <summary>
/// Centralized constants for <see cref="MomVibe.Domain.Entities.ApplicationUser"/> to avoid magic numbers/strings
/// and keep validation and schema comments consistent across the codebase.
/// </summary>
public static class ApplicationUserConstants
{

    /// <summary>
    /// Length-related constraints.
    /// </summary>
    public static class Lengths
    {
        /// <summary>Minimum length for <c>DisplayName</c>.</summary>
        public const int DisplayNameMin = 2;

        /// <summary>Maximum length for <c>DisplayName</c>.</summary>
        public const int DisplayNameMax = 64;

        /// <summary>Maximum length for <c>Bio</c>.</summary>
        public const int BioMax = 512;

        /// <summary>Maximum length for <c>LanguagePreference</c> (e.g., "en", "en-US").</summary>
        public const int LanguageCodeMax = 10;

        /// <summary>Maximum length for <c>AvatarUrl</c>.</summary>
        public const int AvatarUrlMax = 2048;

        /// <summary>Maximum length for <c>Iban</c>.</summary>
        public const int IbanMax = 34;
    }

    /// <summary>
    /// Default values for properties.
    /// </summary>
    public static class Defaults
    {
        /// <summary>Default language preference (ISO code).</summary>
        public const string Language = "en";
    }

    /// <summary>
    /// Database column comments for EF Core schema.
    /// </summary>
    public static class Comments
    {
        /// <summary>Public display name shown to other users.</summary>
        public const string DisplayName = "Public display name shown to other users.";

        /// <summary>Type/category of the user's profile.</summary>
        public const string ProfileType = "Type/category of the user's profile.";

        /// <summary>Absolute URL to the user's avatar image.</summary>
        public const string AvatarUrl = "Absolute URL to the user's avatar image.";

        /// <summary>Indicates whether the account is blocked from interacting.</summary>
        public const string IsBlocked = "Indicates whether the account is blocked from interacting.";

        /// <summary>User-provided short biography.</summary>
        public const string Bio = "User-provided short biography.";

        /// <summary>Preferred language or locale (e.g., en or en-US).</summary>
        public const string LanguagePreference = "Preferred language or locale (e.g., en or en-US).";

        /// <summary>UTC timestamp when the user account was created.</summary>
        public const string CreatedAt = "UTC timestamp when the user account was created.";

        /// <summary>IBAN for receiving card payments.</summary>
        public const string Iban = "IBAN for receiving card payments.";
    }
}
