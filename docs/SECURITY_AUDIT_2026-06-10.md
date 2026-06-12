# SECURITY AUDIT REPORT — MamVibe — 2026-06-10

> Authorized internal security audit. Conducted as a static code review + threat model. No live exploitation against production infrastructure.

## Executive Summary

MamVibe has a meaningfully above-average security baseline for a startup: CORS is locked to a single origin, JWT validation is fully configured with all four parameters, refresh tokens use httpOnly SameSite=Strict cookies path-scoped to `/api/v1/auth`, rate limiting covers auth/upload/donation endpoints, magic-byte photo validation is implemented, HSTS and all OWASP-recommended security headers are emitted, the admin controller is correctly role-gated on every action, Docker containers run as non-root users, and Stripe webhook signature verification is correctly enforced via `EventUtility.ConstructEvent`. No `dangerouslySetInnerHTML` usage exists in the React frontend. The access token never touches localStorage.

**Five Critical and five High findings require immediate action.** The most severe finding is a hardcoded admin password in `appsettings.Development.json` (`Admin@Dev1234`) that, if the seed is ever enabled, gives full admin access to anyone with repository access. A real IBAN AES-256 encryption key is stored in the `.env` file (`OnunFQHLEn+XD/e8BO76JhXsLQ0Ck8uheAj42WgE418=`); if that file leaks, all stored IBANs are decryptable. An N8n outbox dispatcher concatenates a database-persisted path string directly into a URL without an allowlist, enabling SSRF against internal Docker services. The Stripe `PaymentIntent` flow has no webhook handler for `payment_intent.succeeded` and no item reservation, enabling a double-purchase race condition. SignalR broadcasts every user's online/offline status to all connected clients globally, enabling systematic user ID harvesting.

---

## CRITICAL FINDINGS (P0)

### CRIT-1: Hardcoded Admin Credentials in Developer Config File
- **CWE**: CWE-798
- **File**: `backend/src/MomVibe.WebApi/appsettings.Development.json`, line 39
- **Also at**: `backend/src/MomVibe.WebApi/bin/Release/net8.0/appsettings.Development.json` (build artefact — may be in Docker layer)

```json
"AdminSeed": {
  "Enabled": false,
  "Email": "admin@mamvibe.com",
  "Password": "Admin@Dev1234",
  ...
}
```

The file is in `.gitignore` but exists on disk. The Release build output directory is NOT in `.dockerignore` (only in `.gitignore`). If the `backend/` Docker build context includes `bin/Release/`, this credential is baked into your production image. Even with `Enabled: false`, the email/password pair is known to anyone with repo or image access.

**Attack scenario**: Attacker reads `Admin@Dev1234` from the build artefact or a leaked CI log. Tries it against `POST /api/v1/auth/login`. If this was ever the seeded production password, it works. Full admin access: block users, read all payments, approve fraudulent listings, access audit logs.

**Fix**: Wipe `Email`/`Password` to empty strings. Confirm `backend/**/bin/` is excluded from the Docker build context (add to `backend/.dockerignore`). Rotate the production admin password immediately if it was ever set to this value.

---

### CRIT-2: Real IBAN Encryption Key and N8N Key in `.env`
- **CWE**: CWE-321
- **File**: `.env`, lines 73 and 86

```
IBAN_ENCRYPTION_KEY=OnunFQHLEn+XD/e8BO76JhXsLQ0Ck8uheAj42WgE418=
N8N_ENCRYPTION_KEY=BgDRpea6+r4MM/zLQw50wHuI0e/4N/eOuwubeKUCJ/0=
```

These are real operational keys, not placeholders. The `IBAN_ENCRYPTION_KEY` is used by `AesEncryptionConverter` to decrypt every stored IBAN in the production database. Any exposure of this file gives an attacker the ability to decrypt all user financial data.

The `.env.example` ships this exact key value as a "template", which is wrong — templates should contain only placeholders.

**Fix**: Rotate both keys immediately. Store them in Docker secrets or a proper secrets manager. The `.env.example` template must use `CHANGE_ME` for these values, not real keys.

---

### CRIT-3: N8n Outbox Dispatcher — SSRF via Unvalidated Path from Database
- **CWE**: CWE-918
- **File**: `backend/src/MomVibe.Infrastructure/Outbox/N8nOutboxDispatcher.cs`, line 45

```csharp
var url = this._settings.BaseUrl.TrimEnd('/') + "/" + payload.Path.TrimStart('/');
```

`payload.Path` is deserialised from `OutboxMessages.Payload` in PostgreSQL — a value that originates from code but is persisted in the database. Any attacker who can write to `OutboxMessages` (SQL injection elsewhere, compromised migration, rogue insider) can inject an arbitrary path. The `mamvibe-net` Docker network contains Redis, PgBouncer, Prometheus (`:9090`), Grafana (`:3000`), n8n (`:5678`), and the API itself. A path of `../../../../redis:6379/` or `../../grafana:3000/api/admin/users` would reach internal services.

The HMAC handler signs the payload body, not the URL — so SSRF is not blocked by `N8nHmacHandler`.

**Fix** — add an allowlist to `N8nOutboxDispatcher.cs`:
```csharp
private static readonly HashSet<string> _allowedPaths = new(StringComparer.OrdinalIgnoreCase)
{
    "payment-completed", "payment-failed", "shipment-created", "shipment-delivered",
    "shipment-stuck", "user-registered", "user-blocked", "item-sold", "new-chat-message",
    "stale-items", "daily-summary", "feedback-prompt", "item-pending-approval",
    "shipment-out-for-delivery"
};
```
Reject any path not in this allowlist before constructing the URL.

---

### CRIT-4: Stripe PaymentIntent — No Item Reservation, No Webhook Handler
- **CWE**: CWE-362 (Race Condition)
- **File**: `backend/src/MomVibe.Infrastructure/Services/StripePaymentService.cs`, lines 489-515

`CreatePaymentIntentAsync` creates a Stripe intent without:
1. Checking `item.IsSold` or `item.IsReserved`
2. Reserving the item for the buyer
3. Any `payment_intent.succeeded` event handler exists in `HandleWebhookAsync`

```csharp
// HandleWebhookAsync — only handles CheckoutSessionCompleted:
if (stripeEvent.Type != EventTypes.CheckoutSessionCompleted) return;
```

Two buyers call `POST /payments/create-intent/{itemId}` simultaneously. Both get valid `clientSecret`s. Both complete payment. Stripe fires `payment_intent.succeeded` twice — but the server ignores both. Result: two buyers charged, item still active, no payment record created.

**Fix**: Add `payment_intent.succeeded` handling. Wrap `HandleSingleWebhookAsync` and `HandleBulkWebhookAsync` in a `BeginTransactionAsync(IsolationLevel.Serializable)` with an item availability check + idempotency guard via `Payments.AnyAsync(p => p.StripeSessionId == session.Id)`.

---

### CRIT-5: Google OAuth Client ID Committed in `frontend/.env.local`
- **CWE**: CWE-522
- **File**: `frontend/.env.local`, line 6

```
VITE_GOOGLE_CLIENT_ID=403094627890-qr2pao5uhbmjbt80t4cjti5chrki2htv.apps.googleusercontent.com
```

This is a real credential. While the Client ID is intentionally embedded in the browser bundle (it must be, for Google Sign-In), its exposure in source means: (a) it is now known to anyone who has audited this codebase, (b) it enables constructing a phishing OAuth flow that mimics MamVibe.

**Fix**: Verify in Google Cloud Console that authorized JavaScript origins are strictly `https://mamvibe.bg` only, no wildcards. Verify redirect URIs are explicit and complete.

---

## HIGH FINDINGS (P1)

### HIGH-1: SignalR Broadcasts Online/Offline Presence to All Connected Clients
- **CWE**: CWE-359
- **File**: `backend/src/MomVibe.WebApi/Hubs/ChatHub.cs`, lines 111, 121

```csharp
await Clients.Others.UserOnline(userId);   // → every connected client sees this
await Clients.Others.UserOffline(userId);  // → every connected client sees this
```

Any authenticated user connected to the hub receives real-time online/offline notifications for every other user on the platform. An attacker who opens a long-lived SignalR connection will enumerate all active user IDs over time.

**Fix**: Only notify users who share an existing conversation. Requires `IMessageService.GetConversationPartnerIdsAsync(userId)` and group-targeted pushes.

---

### HIGH-2: Stripe Bulk Webhook Has No Database Transaction
- **CWE**: CWE-366
- **File**: `backend/src/MomVibe.Infrastructure/Services/StripePaymentService.cs`, lines 337-367

`HandleBulkWebhookAsync` creates multiple payment records and marks multiple items sold in a single `SaveChangesAsync` call with no explicit transaction. The bundle handler correctly uses `BeginTransactionAsync`. A failure mid-loop leaves the database in a partially-completed state.

**Fix**: Wrap `HandleBulkWebhookAsync` in `BeginTransactionAsync` / `CommitAsync` / `RollbackAsync` — same pattern as `HandleBundleWebhookAsync`.

---

### HIGH-3: N8n Webhook Secret Empty String in Default `appsettings.json`
- **CWE**: CWE-306
- **File**: `backend/src/MomVibe.WebApi/appsettings.json`, line 100

```json
"N8n": {
  "BaseUrl": "https://mamvibe.app.n8n.cloud/webhook/",
  "Enabled": true,
  "WebhookSecret": "",
```

With `WebhookSecret` empty, `N8nHmacHandler._keyBytes.Length == 0`, and the handler skips signing. Any developer running with the base config fires unsigned webhooks to the live external n8n.cloud instance.

**Fix**: Add a startup guard in `DependencyInjection.cs`:
```csharp
if (settings.Value.Enabled && string.IsNullOrWhiteSpace(n8nSecret))
    throw new InvalidOperationException("N8n:WebhookSecret must be configured when N8n:Enabled is true.");
```

---

### HIGH-4: Cloudflare Turnstile Test Keys Ship in Base Config — No Bot Protection
- **CWE**: CWE-183
- **File**: `backend/src/MomVibe.WebApi/appsettings.json`, lines 29-30

The Cloudflare-documented test keys always return `success: true` regardless of the token submitted. Any non-dev environment that does not override these keys has zero bot protection on registration, login, and forgot-password.

**Fix**: Replace the test keys with empty strings. Throw at startup if `!env.IsDevelopment()` and the secret matches the test key.

---

### HIGH-5: No Per-Email Rate Limit on Forgot-Password / Reset-Password
- **CWE**: CWE-307
- **File**: `backend/src/MomVibe.WebApi/Controllers/AuthController.cs`

Both endpoints share the `Auth` policy (30 req/min per IP) with login and register. An attacker using 30+ IPs can attempt 900+ password-reset tokens per minute. More critically, a high-volume attack on `ForgotPassword` from many IPs consumes the login rate limit for real users — a cheap DoS.

**Fix**: Add a dedicated `ForgotPassword` policy at 3 req/10 min per IP. Apply `[EnableRateLimiting(RateLimitPolicies.ForgotPassword)]` to both endpoints.

---

## MEDIUM FINDINGS (P2)

| # | Finding | File | CWE |
|---|---------|------|-----|
| MED-1 | `SendMessageRequest` DTO in controller — no FluentValidation | `MessagesController.cs:113` | CWE-20 |
| MED-2 | `AvatarUrl` accepts arbitrary URLs, no origin validation | `UsersController.cs:109` | CWE-601 |
| MED-3 | PostgreSQL auth uses MD5 — broken hash algorithm | `docker-compose.yml:7-8` | CWE-916 |
| MED-4 | Redis runs without a password on the internal network | `docker-compose.yml:136` | CWE-306 |
| MED-5 | `RevolutTag` returned in public profile responses | `UsersController.cs:115` | CWE-312 |
| MED-6 | CSP `connect-src` missing explicit `wss:` for SignalR | `SecurityHeadersMiddleware.cs:44` | CWE-16 |
| MED-7 | N8n daily scheduled service sends PII to external n8n.cloud — GDPR | `N8nScheduledService.cs:96-108` | N/A |
| MED-8 | `appsettings.json` default N8n config points at live cloud with `Enabled: true` | `appsettings.json:98-100` | CWE-16 |

---

## LOW / INFORMATIONAL (P3)

| # | Finding | File |
|---|---------|------|
| LOW-1 | N8n BaseUrl points to external cloud with `Enabled: true` by default | `appsettings.json:98` |
| LOW-2 | Turnstile test keys in `frontend/.env.local` | `.env.local:4` |
| LOW-3 | `edoburu/pgbouncer:latest` — unpinned Docker image tag | `docker-compose.yml:149` |
| LOW-4 | `n8nio/n8n:latest` — unpinned Docker image tag | `docker-compose.yml:169` |
| LOW-5 | `curl` in backend Dockerfile HEALTHCHECK | `Dockerfile:38` |
| LOW-6 | Blocked user JWT claims stale until token expiry (15 min) | `BlockedUserMiddleware.cs` |
| LOW-7 | GDPR consent stored client-side only | `CookieConsent.tsx` |
| LOW-8 | AI assistant system prompt discloses internal route map | `AssistantController.cs:43` |

---

## Defense-in-Depth Recommendations (Prioritised)

**Within 24 hours**: Remove admin password from `appsettings.Development.json`. Add `backend/**/bin/` to `backend/.dockerignore`. Rotate production admin password. Rotate IBAN encryption key. Rotate N8N encryption key.

**Within 1 week**: Add N8n path allowlist in `N8nOutboxDispatcher.cs`. Wrap `HandleBulkWebhookAsync` in a database transaction. Add `payment_intent.succeeded` webhook handler with serializable-transaction double-sell guard. Add `ForgotPassword` dedicated rate limit policy. Add N8n secret emptiness guard at startup.

**Within 1 month**: Add Redis authentication. Migrate PostgreSQL auth to scram-sha-256. Restrict `UserOnline`/`UserOffline` to conversation partners only. Add AvatarUrl origin validation. Exclude RevolutTag from public profiles. Review n8n.cloud DPA for GDPR Article 28 compliance. Add Turnstile test-key detection at startup for non-dev environments.

**Backlog**: Pin Docker image tags. Add server-side GDPR consent logging. Add Redis-based real-time blocklist for immediately blocked users. Add explicit `connect-src wss:` to CSP.

---

## Quick Wins (Under 1 Hour Each)

| # | Action | Time |
|---|--------|------|
| 1 | Clear `AdminSeed:Password` in `appsettings.Development.json` | 2 min |
| 2 | Add N8n path allowlist in `N8nOutboxDispatcher.cs` (~15 lines) | 15 min |
| 3 | Wrap `HandleBulkWebhookAsync` in BeginTransactionAsync | 10 min |
| 4 | Add Redis `requirepass` to docker-compose | 5 min |
| 5 | Set `N8n:WebhookSecret` to non-empty placeholder + startup guard | 10 min |
| 6 | Add Turnstile test-key detection at startup | 5 min |
| 7 | Add `connect-src wss:mamvibe.bg` to CSP middleware | 2 min |
| 8 | Add AvatarUrl HTTPS/origin validation to `UpdateProfileValidator` | 20 min |
| 9 | Exclude `RevolutTag` from public profile DTO | 10 min |
| 10 | Pin pgBouncer and n8n Docker image tags | 5 min |

---

## What Is Implemented Correctly (Preserve These)

- JWT: all four validation parameters; minimum 32-char secret enforced at startup; 30-second ClockSkew
- Refresh tokens: httpOnly + Secure + SameSite=Strict + path-scoped to `/api/v1/auth`
- Access token: memory-only in Zustand; never written to localStorage
- Photo upload: extension + content-type + magic bytes + dimension + EXIF strip + 5 MB limit
- Path traversal: canonical path check in `PhotoService.DeletePhotoAsync`
- CORS: single-origin allowlist; HTTPS enforced in non-dev
- Security headers: X-Content-Type-Options, X-Frame-Options DENY, Referrer-Policy, Permissions-Policy, full CSP, HSTS, CORP, COOP
- Rate limiting: global 200/min, auth 30/min, upload 20/min, donation 5/hr, view 30/min
- Admin endpoint: `[Authorize(Policy = "AdminOnly")]` on every single action in `AdminController`
- Stripe webhook: `EventUtility.ConstructEvent` signature verification; body size limit 64KB; error suppression
- N8n outbound HMAC: `N8nHmacHandler` signs every webhook with `X-MamVibe-Signature`
- Exception handling: stack traces suppressed; generic 500 responses; 5xx sanitized in Axios interceptor
- Docker: non-root `appuser` in backend; `nginx-unprivileged` in frontend; observability ports on 127.0.0.1
- Account lockout: 5 failed attempts → 15-minute lockout
- Password policy: 8+ chars, digit, upper, lower, non-alphanumeric
- Password reset token: 30-minute expiry
- Account enumeration prevention: forgot-password swallows all errors
- GDPR: data export (`/me/export`), right-to-erasure (`DELETE /me`), IBAN encrypted at rest, EXIF GPS stripped
- FluentValidation: consistently applied in Application layer for all major item/shipping/offer DTOs
- AES-256-CBC IBAN encryption: per-record random IV; EF Core value converter
- SignalR hub: `[Authorize]` class-level + `RequireAuthorization(ActiveUser)` on hub mapping
- No `dangerouslySetInnerHTML` anywhere in the frontend
- Console logging gated behind `import.meta.env.DEV`
