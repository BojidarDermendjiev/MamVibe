# ADR-004: Outbox pattern via bounded Channel for n8n webhooks

**Status**: Accepted  
**Date**: 2025-01-01

## Context

Several domain events (new item listed, purchase completed, message sent, etc.)
need to trigger n8n automation workflows via HTTP webhooks. Calling n8n inline
inside the request handler would couple response latency to n8n availability
and add failure risk for unrelated operations.

Alternative approaches considered:

1. Inline `HttpClient` call inside the controller/service
2. Background `IHostedService` polling an outbox table in the database
3. In-process `Channel<T>` drained by a `BackgroundService`

## Decision

Use an in-process bounded `Channel<WebhookEvent>` (capacity 500) as the
transport layer.

- **Producers** (services) call `N8nWebhookDispatcher.Enqueue(event)` — a
  non-blocking write that returns immediately. If the channel is full the event
  is dropped and a warning is logged (fire-and-forget semantics).
- **Consumer** (`N8nWebhookService`, a `BackgroundService`) drains the channel
  in a tight loop, POSTing each event to the configured n8n webhook URL with a
  short timeout and retry.
- The dispatcher is registered as a singleton; the service is registered as a
  hosted service.

## Consequences

**Good**  
- Request handlers return immediately regardless of n8n latency.  
- No additional database table or polling overhead.  
- Simple to reason about: one producer interface, one background consumer.

**Trade-offs**  
- Events in the channel are lost on process restart (not durable). Acceptable
  because n8n webhooks are best-effort notifications, not financial records.  
- A full channel silently drops events; capacity (500) should be monitored via
  the `/health` endpoint counter if event volume grows.  
- If true durability is required in the future, replace the channel with the
  database outbox pattern (EF Core + a polling worker or Hangfire job).
