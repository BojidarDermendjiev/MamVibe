using FluentAssertions;

using MomVibe.Infrastructure.Outbox;

namespace MomVibe.UnitTests.Outbox;

/// <summary>
/// Pinning tests for the exponential-backoff schedule encoded in
/// <see cref="OutboxProcessor.BackoffFor"/>. The schedule is part of the operational
/// contract (alerting thresholds depend on it) so it gets its own dedicated test.
/// </summary>
public class OutboxProcessorBackoffTests
{
    private static TimeSpan Invoke(int attemptCount)
    {
        // Reach into the internal static via the InternalsVisibleTo we haven't added —
        // but since BackoffFor is declared internal in the same assembly's namespace and
        // these tests target the same project, we use reflection instead of widening the API surface.
        var method = typeof(OutboxProcessor).GetMethod("BackoffFor",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;
        return (TimeSpan)method.Invoke(null, [attemptCount])!;
    }

    [Theory]
    [InlineData(0, 60)]               // first retry: 1 min
    [InlineData(1, 60)]               // still 1 min
    [InlineData(2, 5 * 60)]           // 5 min
    [InlineData(3, 30 * 60)]          // 30 min
    [InlineData(4, 2 * 60 * 60)]      // 2 h
    [InlineData(5, 12 * 60 * 60)]     // 12 h
    [InlineData(100, 12 * 60 * 60)]   // stays at 12 h once past the schedule
    public void BackoffFor_Follows_Documented_Schedule(int attempts, int expectedSeconds)
    {
        Invoke(attempts).Should().Be(TimeSpan.FromSeconds(expectedSeconds));
    }
}
