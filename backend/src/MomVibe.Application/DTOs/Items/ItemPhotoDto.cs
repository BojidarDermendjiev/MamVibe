namespace MomVibe.Application.DTOs.Items;

/// <summary>
/// DTO for an item's photo:
/// - Id: unique identifier of the photo record.
/// - Url: required absolute or relative image URL.
/// - DisplayOrder: numeric order for rendering in galleries/carousels.
/// </summary>
public class ItemPhotoDto
{
    /// <summary>Gets or sets the unique identifier of the photo record.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the absolute or relative URL of the photo resource.</summary>
    public required string Url { get; set; }

    /// <summary>Gets or sets the zero-based display order used to render photos in galleries or carousels.</summary>
    public int DisplayOrder { get; set; }
}
