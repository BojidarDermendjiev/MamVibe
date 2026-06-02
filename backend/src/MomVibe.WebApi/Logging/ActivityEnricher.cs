namespace MomVibe.WebApi.Logging;

using System.Diagnostics;

using Serilog.Core;
using Serilog.Events;

/// <summary>
/// Serilog enricher that copies the current <see cref="Activity"/>'s TraceId and SpanId
/// onto every log event. OpenTelemetry creates the Activity per request; this enricher
/// makes the same correlation IDs visible in plain-text logs without forcing every
/// caller to thread them through manually.
/// </summary>
public sealed class ActivityEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;
        if (activity is null) return;

        // ToString() on these emits the W3C-format hex strings used across the OTel ecosystem.
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceId", activity.TraceId.ToString()));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("SpanId", activity.SpanId.ToString()));
    }
}
