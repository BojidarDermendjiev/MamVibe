namespace MomVibe.Application.DTOs.Payments;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Payload used to initiate a Stripe checkout session for a one-time monetary donation.
/// </summary>
public class DonationCheckoutRequest
{
    /// <summary>Gets or sets the donation amount in BGN (e.g. 10.00). Must be between 1 and 500 BGN.</summary>
    [Range(1, 500, ErrorMessage = "Donation amount must be between 1 and 500 BGN.")]
    public decimal Amount { get; set; }
}
