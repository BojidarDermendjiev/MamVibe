namespace MomVibe.Domain.Constants;

public static class ApplicationUserConstants
{
    public static class Lengths
    {
        public const int DisplayNameMin = 2;
        public const int DisplayNameMax = 64;
        public const int BioMax = 512;
        public const int LanguageCodeMax = 10;
        public const int AvatarUrlMax = 2048;
        public const int RevolutTagMax = 50;
    }

    public static class Defaults
    {
        public const string Language = "en";
    }

    public static class Comments
    {
        public const string DisplayName = "Public display name shown to other users.";
        public const string ProfileType = "Type/category of the user's profile.";
        public const string AvatarUrl = "Absolute URL to the user's avatar image.";
        public const string IsBlocked = "Indicates whether the account is blocked from interacting.";
        public const string Bio = "User-provided short biography.";
        public const string LanguagePreference = "Preferred language or locale (e.g., en or en-US).";
        public const string CreatedAt = "UTC timestamp when the user account was created.";
        public const string RevolutTag = "Revolut username or tag for peer-to-peer payments via the Revolut app.";
    }
}
