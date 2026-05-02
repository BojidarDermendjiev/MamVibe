namespace MomVibe.Domain.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

using Enums;
using Constants;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MinLength(ApplicationUserConstants.Lengths.DisplayNameMin)]
    [MaxLength(ApplicationUserConstants.Lengths.DisplayNameMax)]
    [Comment(ApplicationUserConstants.Comments.DisplayName)]
    public required string DisplayName { get; set; }

    [Comment(ApplicationUserConstants.Comments.ProfileType)]
    public ProfileType ProfileType { get; set; }

    [Url]
    [MaxLength(ApplicationUserConstants.Lengths.AvatarUrlMax)]
    [Comment(ApplicationUserConstants.Comments.AvatarUrl)]
    public string? AvatarUrl { get; set; }

    [Comment(ApplicationUserConstants.Comments.IsBlocked)]
    public bool IsBlocked { get; set; }

    [MaxLength(ApplicationUserConstants.Lengths.BioMax)]
    [Comment(ApplicationUserConstants.Comments.Bio)]
    public string? Bio { get; set; } = string.Empty;

    [Required]
    [MaxLength(ApplicationUserConstants.Lengths.LanguageCodeMax)]
    [Comment(ApplicationUserConstants.Comments.LanguagePreference)]
    public string LanguagePreference { get; set; } = "en";

    [Comment(ApplicationUserConstants.Comments.CreatedAt)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(ApplicationUserConstants.Lengths.RevolutTagMax)]
    [Comment(ApplicationUserConstants.Comments.RevolutTag)]
    public string? RevolutTag { get; set; }

    [MaxLength(200)]
    public string? ExpoPushToken { get; set; }

    public ICollection<Item> Items { get; set; } = [];
    public ICollection<Like> Likes { get; set; } = [];
    public ICollection<Message> SentMessages { get; set; } = [];
    public ICollection<Message> ReceivedMessages { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<DoctorReview> DoctorReviews { get; set; } = [];
    public ICollection<ChildFriendlyPlace> ChildFriendlyPlaces { get; set; } = [];
}
