namespace MomVibe.Application.DTOs.Payments;

using System.ComponentModel.DataAnnotations;

public class DonationCheckoutRequest
{
    /// <summary>Amount in BGN (e.g. 10.00).</summary>
    [Range(1, 500, ErrorMessage = "Donation amount must be between 1 and 500 BGN.")]
    public decimal Amount { get; set; }
}
