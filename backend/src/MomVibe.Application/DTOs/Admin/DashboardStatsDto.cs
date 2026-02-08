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
    public int TotalUsers { get; set; }
    public int TotalItems { get; set; }
    public int ActiveItems { get; set; }
    public int TotalDonations { get; set; }
    public int TotalSales { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalMessages { get; set; }
    public int BlockedUsers { get; set; }
}
