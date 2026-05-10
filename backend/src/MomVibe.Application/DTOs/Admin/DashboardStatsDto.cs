namespace MomVibe.Application.DTOs.Admin;

/// <summary>
/// Aggregated platform metrics for admin dashboards:
/// - Users: TotalUsers, BlockedUsers.
/// - Items: TotalItems, ActiveItems.
/// - Transactions: TotalDonations, TotalSales, TotalRevenue.
/// - Messaging: TotalMessages.
/// </summary>
public class DashboardStatsDto
{
    /// <summary>Gets or sets the total number of registered users on the platform.</summary>
    public int TotalUsers { get; set; }

    /// <summary>Gets or sets the total number of item listings across all statuses.</summary>
    public int TotalItems { get; set; }

    /// <summary>Gets or sets the number of item listings that are currently active and publicly visible.</summary>
    public int ActiveItems { get; set; }

    /// <summary>Gets or sets the total number of completed donation transactions.</summary>
    public int TotalDonations { get; set; }

    /// <summary>Gets or sets the total number of completed sale transactions.</summary>
    public int TotalSales { get; set; }

    /// <summary>Gets or sets the total revenue generated from completed sale transactions.</summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>Gets or sets the total number of direct messages exchanged between users.</summary>
    public int TotalMessages { get; set; }

    /// <summary>Gets or sets the number of user accounts that are currently blocked by an administrator.</summary>
    public int BlockedUsers { get; set; }
}
