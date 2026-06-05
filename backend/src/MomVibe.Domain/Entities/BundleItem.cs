namespace MomVibe.Domain.Entities;

using Common;

/// <summary>
/// Join entity that links an individual <see cref="Item"/> to a <see cref="Bundle"/>.
/// Each row represents one item slot within a bundle.
/// </summary>
public class BundleItem : BaseEntity
{
    /// <summary>Foreign key referencing the owning bundle.</summary>
    public Guid BundleId { get; set; }

    /// <summary>Foreign key referencing the item that belongs to this bundle slot.</summary>
    public Guid ItemId { get; set; }

    /// <summary>Navigation to the parent bundle.</summary>
    public Bundle Bundle { get; set; } = null!;

    /// <summary>Navigation to the item in this bundle slot.</summary>
    public Item Item { get; set; } = null!;
}
