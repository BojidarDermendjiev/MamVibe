namespace MomVibe.Infrastructure.Configuration;

/// <summary>
/// Top-level shipping configuration. MockMode bypasses all courier APIs and generates
/// a fake waybill PDF in memory — useful for local development before real credentials exist.
/// </summary>
public class ShippingSettings
{
    /// <summary>
    /// When true, all courier API calls are skipped. A fake tracking number and a
    /// downloadable test PDF are generated instead. Never enable in production.
    /// </summary>
    public bool MockMode { get; set; }
}
