namespace MomVibe.Infrastructure.Outbox;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Application.Interfaces;
using Domain.Entities;

/// <summary>
/// BackgroundService that drains the transactional outbox. Polls every <see cref="PollInterval"/>,
/// claims up to <see cref="BatchSize"/> pending messages, and dispatches each via the registered
/// <see cref="IOutboxMessageDispatcher"/> for its <c>MessageType</c>. Successful dispatches set
/// <c>ProcessedAt</c>; failures increment <c>AttemptCount</c> and reschedule with the exponential
/// backoff in <see cref="BackoffFor"/>. Messages that exhaust <see cref="MaxAttempts"/> stay in the
/// table with <c>ProcessedAt = null</c> for manual inspection.
/// </summary>
public sealed class OutboxProcessor : BackgroundService
{
    /// <summary>Time between drain cycles when the queue is empty.</summary>
    public static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);

    /// <summary>Maximum number of messages dispatched per cycle.</summary>
    public const int BatchSize = 50;

    /// <summary>Hard cap on retries before a message is parked for manual triage.</summary>
    public const int MaxAttempts = 5;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger)
    {
        this._scopeFactory = scopeFactory;
        this._logger = logger;
    }

    /// <summary>
    /// Returns the delay before the next attempt for a message that just failed <paramref name="attemptCount"/> times.
    /// Schedule: 1 min → 5 min → 30 min → 2 h → 12 h.
    /// Internal so it can be unit-tested independently of the polling loop.
    /// </summary>
    internal static TimeSpan BackoffFor(int attemptCount) => attemptCount switch
    {
        <= 1 => TimeSpan.FromMinutes(1),
        2    => TimeSpan.FromMinutes(5),
        3    => TimeSpan.FromMinutes(30),
        4    => TimeSpan.FromHours(2),
        _    => TimeSpan.FromHours(12),
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Brief startup grace so EF Core, migrations, and the rest of the host finish wiring
        // before the first poll. Avoids a noisy first cycle on cold start.
        try { await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken); }
        catch (TaskCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "OutboxProcessor poll cycle failed; will retry after the standard interval");
            }

            try { await Task.Delay(PollInterval, stoppingToken); }
            catch (TaskCanceledException) { return; }
        }
    }

    private async Task ProcessBatchAsync(CancellationToken stoppingToken)
    {
        using var scope = this._scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var dispatchers = scope.ServiceProvider
            .GetServices<IOutboxMessageDispatcher>()
            .ToDictionary(d => d.MessageType, StringComparer.Ordinal);

        var now = DateTime.UtcNow;
        var pending = await context.OutboxMessages
            .Where(m => m.ProcessedAt == null
                     && m.NextAttemptAt <= now
                     && m.AttemptCount < MaxAttempts)
            .OrderBy(m => m.NextAttemptAt)
            .Take(BatchSize)
            .ToListAsync(stoppingToken);

        if (pending.Count == 0) return;

        foreach (var message in pending)
        {
            if (stoppingToken.IsCancellationRequested) return;

            if (!dispatchers.TryGetValue(message.MessageType, out var dispatcher))
            {
                // No registered handler — park the message so we don't spin on it.
                message.AttemptCount = MaxAttempts;
                message.LastError = $"No IOutboxMessageDispatcher registered for MessageType '{message.MessageType}'.";
                this._logger.LogError("OutboxProcessor: no dispatcher for MessageType {MessageType} (message {MessageId})", message.MessageType, message.Id);
                continue;
            }

            try
            {
                await dispatcher.DispatchAsync(message, stoppingToken);
                message.ProcessedAt = DateTime.UtcNow;
                message.LastError = null;
            }
            catch (Exception ex)
            {
                message.AttemptCount++;
                message.NextAttemptAt = DateTime.UtcNow + BackoffFor(message.AttemptCount);
                message.LastError = Truncate(ex.Message, 1000);
                this._logger.LogWarning(ex,
                    "OutboxProcessor: dispatch failed for message {MessageId} ({MessageType}), attempt {Attempt}/{MaxAttempts}",
                    message.Id, message.MessageType, message.AttemptCount, MaxAttempts);
            }
        }

        await context.SaveChangesAsync(stoppingToken);
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];
}
