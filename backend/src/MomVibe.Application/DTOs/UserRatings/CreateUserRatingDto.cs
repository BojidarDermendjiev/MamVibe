namespace MomVibe.Application.DTOs.UserRatings;

using System.ComponentModel.DataAnnotations;

public class CreateUserRatingDto
{
    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(500)]
    public string? Comment { get; set; }
}
