namespace MomVibe.Application.DTOs.Items;

/// <summary>
/// DTO for an item's photo:
/// - Id: unique identifier of the photo record.
/// - Url: required absolute or relative image URL.
/// - DisplayOrder: numeric order for rendering in galleries/carousels.
/// </summary>
public class ItemPhotoDto
{
    public Guid Id { get; set; }
    public required string Url { get; set; }
    public int DisplayOrder { get; set; }
}
