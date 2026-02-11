namespace MomVibe.Application.DTOs.Payments;

public class BulkCheckoutRequest
{
    public List<Guid> ItemIds { get; set; } = [];
}
