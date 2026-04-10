namespace MomVibe.Application.DTOs.Users;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Payload for registering an Expo push notification token from a mobile device.
/// </summary>
public class RegisterPushTokenDto
{
    /// <summary>
    /// The Expo push token string (e.g. "ExponentPushToken[xxxxxxxxxxxxxxxxxxxxxx]").
    /// </summary>
    [Required]
    [MaxLength(200)]
    public required string Token { get; set; }
}
