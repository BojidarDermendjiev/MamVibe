namespace MomVibe.Infrastructure.Outbox;

using System.Text.Json;

using Application.Interfaces;
using Domain.Entities;

/// <summary>
/// Stages an <see cref="OutboxMessage"/> on the shared <see cref="IApplicationDbContext"/>.
/// Intentionally does NOT call <c>SaveChangesAsync</c> — the calling service commits the
/// outbox row in the same transaction as the originating business state change, so a
/// process crash between commit and dispatch still leaves a recoverable record.
/// </summary>
public sealed class OutboxWriter : IOutboxWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IApplicationDbContext _context;

    public OutboxWriter(IApplicationDbContext context)
    {
        this._context = context;
    }

    public void Enqueue<T>(string messageType, T payload) where T : notnull
    {
        var now = DateTime.UtcNow;
        this._context.OutboxMessages.Add(new OutboxMessage
        {
            MessageType = messageType,
            Payload = JsonSerializer.Serialize(payload, JsonOptions),
            CreatedAt = now,
            NextAttemptAt = now,
            AttemptCount = 0
        });
    }
}
