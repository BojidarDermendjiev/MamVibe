# MamVibe — Skills & Technology Map

A developer reference covering every skill required to contribute to, extend, and deploy MamVibe.

---

## Backend — .NET 8 / ASP.NET Core

| Skill | Proficiency Required | Where Used |
|-------|---------------------|------------|
| C# 12 | Advanced | All backend layers |
| ASP.NET Core 8 Web API | Advanced | Controllers, middleware, startup |
| Clean Architecture | Intermediate | Domain → Application → Infrastructure → WebApi |
| Entity Framework Core 8 (Code-First) | Intermediate | DbContext, migrations, Fluent API |
| PostgreSQL 16 | Intermediate | Primary datastore (Npgsql provider) |
| ASP.NET Core Identity | Intermediate | User management, roles, password policies |
| JWT Bearer Authentication | Intermediate | Access tokens (15 min) + HttpOnly refresh cookies |
| FluentValidation 11 | Intermediate | Request validation in Application layer |
| AutoMapper 12 | Basic | Entity ↔ DTO mapping profiles |
| Serilog | Basic | Structured logging (PII-safe) |
| SignalR | Intermediate | Real-time chat + notifications hub at `/hubs/chat` |
| Background Services | Intermediate | `N8nWebhookService` (Channel<T>), `N8nScheduledService` |
| Rate Limiting | Basic | Global / Auth / Upload / E-Bill policies |
| AES-256 Encryption | Basic | IBAN storage via custom EF value converter |
| HMAC-SHA256 | Basic | TakeANap fiscal receipt API signing |
| xUnit + Moq + FluentAssertions | Intermediate | Unit and integration tests |
| EF Core InMemory | Basic | Integration test fixtures |

---

## Frontend — React 19 + TypeScript + Vite

| Skill | Proficiency Required | Where Used |
|-------|---------------------|------------|
| TypeScript 5.9 | Intermediate | Entire frontend codebase |
| React 19 | Intermediate | All components, hooks, contexts |
| Vite 7 | Basic | Build tool, dev proxy (`/api`, `/hubs`, `/uploads`) |
| Tailwind CSS 4 | Intermediate | All styling |
| Zustand 5 | Basic | Auth state store (`authStore`) |
| Axios 1.x | Intermediate | API clients, interceptors, silent token refresh |
| React Router 6 | Intermediate | SPA routing, protected routes |
| @microsoft/signalr 10 | Basic | Real-time chat/notification connection |
| i18next + react-i18next | Basic | EN/BG translations |
| Radix UI Primitives | Basic | Accessible UI components (avatar, slot) |
| Framer Motion 12 | Basic | Animations |
| Stripe.js + React Stripe 5 | Basic | Payment checkout UI |
| Cloudflare Turnstile | Basic | Bot-protection gating on sensitive forms |

---

## AI Integration — Anthropic Claude API

| Skill | Proficiency Required | Where Used |
|-------|---------------------|------------|
| Anthropic Claude REST API | Intermediate | `AiService` — listing suggestions, moderation, pricing, chat |
| Prompt Engineering | Intermediate | Vision prompts for photo analysis; JSON-structured outputs |
| Claude Vision (image input) | Basic | `SuggestListingAsync` — photo → title/price/category |
| Content Moderation Design | Basic | `ModerateItemAsync` — approve / review / reject pipeline |
| Dynamic Model Selection | Basic | Model stored in `AppSetting` DB table, not hardcoded |

---

## Payments & Fiscal

| Skill | Proficiency Required | Where Used |
|-------|---------------------|------------|
| Stripe.net SDK 50 | Intermediate | Checkout sessions, webhook handling |
| Stripe Webhooks | Intermediate | `checkout.session.completed` event processing |
| Escrow / Wallet Design | Intermediate | TopUp → Purchase → Release/Refund state machine |
| E-Bill Generation | Basic | Idempotent `MV-{YEAR}-{HEX}` numbering, VAT at 20% |
| TakeANap API | Basic | Bulgarian fiscal receipts (HMAC-signed HTTP calls) |

---

## Shipping Integrations (Bulgarian Couriers)

| Skill | Proficiency Required | Where Used |
|-------|---------------------|------------|
| Provider Pattern (C#) | Intermediate | `CourierProviderFactory` + pluggable `ICourierProvider` |
| Econt Express API | Basic | Address + office delivery, label generation, tracking |
| Speedy API | Basic | Same capabilities as Econt |
| BoxNow API | Basic | Locker-based delivery |
| COD / Insurance Modeling | Basic | `Shipment` entity fields |

---

## Infrastructure & DevOps

| Skill | Proficiency Required | Where Used |
|-------|---------------------|------------|
| Docker & Docker Compose | Intermediate | Multi-stage builds (api, frontend, n8n, postgres, pgadmin) |
| Nginx | Basic | SPA fallback, reverse proxy, WebSocket upgrade, gzip, static caching |
| GitHub Actions | Basic | CI pipeline: build → unit tests → integration tests → docker build |
| Cloudflare DNS / Proxy | Basic | HTTPS termination, Turnstile bot protection |
| Hetzner VPS | Basic | Deployment target (CX22, ~€4/mo) |
| PostgreSQL Volumes | Basic | `pgdata` Docker volume, backup strategy |
| Environment Variable Management | Intermediate | `.env` / `appsettings.*.json` secret injection |

---

## Workflow Automation — n8n

| Skill | Proficiency Required | Where Used |
|-------|---------------------|------------|
| n8n Self-Hosting | Basic | Docker service, webhook endpoint, basic auth |
| Webhook Trigger Nodes | Intermediate | 17 pre-built workflows (registration, payment, shipping, etc.) |
| SMTP Email Nodes | Basic | Transactional emails via Brevo |
| Scheduled Triggers | Basic | Daily stale-item checks, weekly seller reports |

---

## Mobile — React Native / Expo

| Skill | Proficiency Required | Where Used |
|-------|---------------------|------------|
| Expo 54 (managed workflow) | Basic | Android / iOS / Web builds |
| React Navigation v7 | Basic | App navigation stack |
| Expo Secure Store | Basic | Secure token storage on device |
| Expo Image Picker | Basic | Photo selection for listings |
| Stripe React Native | Basic | Mobile checkout |
| React Native Async Storage | Basic | Lightweight local persistence |

---

## Security

| Skill | Proficiency Required | Where Used |
|-------|---------------------|------------|
| OWASP Top 10 Awareness | Intermediate | XSS, injection, over-posting, CSRF mitigations |
| JWT Security (issuer/audience/lifetime) | Intermediate | `TokenService` configuration |
| CORS Configuration | Basic | Explicit trusted-origin policy |
| HTTP Security Headers | Basic | CSP, HSTS, X-Frame-Options, Referrer-Policy, CORP, COOP |
| Input Validation | Intermediate | FluentValidation at API boundary |
| Rate Limiting | Basic | Tiered policies per endpoint category |
| Google OAuth 2.0 / PKCE | Basic | `Google.Apis.Auth` server-side token validation |

---

## External Service Accounts Required (Production)

| Service | Purpose |
|---------|---------|
| Google Cloud Console | OAuth 2.0 credentials |
| Stripe Dashboard | Payment processing + webhook secret |
| Cloudflare | DNS, HTTPS, Turnstile site/secret keys |
| Brevo | SMTP (300 free emails/day) |
| Econt Developer Portal | Courier API credentials |
| Speedy API | Courier API credentials |
| BoxNow API | Locker delivery key |
| Anthropic Console | Claude API key |
| TakeANap | Fiscal receipt API key + shop ID |
| Nekorekten API | Bulgarian compliance check key |
