namespace MomVibe.Infrastructure.Configuration;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Strongly-typed binding of the <c>JwtSettings</c> configuration section.
/// Validated at startup via <c>AddOptions&lt;JwtSettings&gt;().ValidateDataAnnotations().ValidateOnStart()</c>
/// so a misconfigured deployment fails fast instead of crashing on the first request.
/// </summary>
public class JwtSettings
{
    /// <summary>HMAC-SHA256 signing secret. Must be at least 32 characters for adequate security.</summary>
    [Required(AllowEmptyStrings = false)]
    [MinLength(32, ErrorMessage = "JwtSettings:Secret must be at least 32 characters.")]
    public string Secret { get; set; } = string.Empty;

    /// <summary>JWT issuer (<c>iss</c> claim).</summary>
    [Required(AllowEmptyStrings = false)]
    public string Issuer { get; set; } = string.Empty;

    /// <summary>JWT audience (<c>aud</c> claim).</summary>
    [Required(AllowEmptyStrings = false)]
    public string Audience { get; set; } = string.Empty;

    /// <summary>Lifetime of issued access tokens, in minutes.</summary>
    [Range(1, 1440)]
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    /// <summary>Lifetime of issued refresh tokens, in days.</summary>
    [Range(1, 365)]
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
