# MamVibe

A full-stack marketplace platform where families can donate or sell baby items — clothes, strollers, toys, and more.

## Tech Stack

### Backend
- **Runtime:** .NET 8 / ASP.NET Core Web API
- **Architecture:** Clean Architecture (Domain → Application → Infrastructure → WebApi)
- **Database:** PostgreSQL with EF Core (Code-First migrations)
- **Auth:** ASP.NET Core Identity, JWT + Refresh Tokens, Google OAuth
- **Real-time:** SignalR (chat messaging)
- **Payments:** Stripe Checkout (card), in-platform Wallet (escrow), pay-on-spot
- **Fiscal Receipts:** TakeANap (Bulgarian SaaS) — HMAC-SHA256 signed API calls
- **E-Bills:** Electronic payment receipts (PDF-style HTML email, idempotent issuance)
- **Shipping:** Econt Express, Speedy, Box Now (provider pattern)
- **Email:** SMTP service (password reset, e-bill delivery)
- **Automation:** n8n webhook dispatcher (fire-and-forget via bounded `Channel<T>`)
- **Security:** Cloudflare Turnstile, CSP / security headers middleware, rate limiting (global + per-endpoint), blocked-user middleware — **15/15 ASP.NET Core security rules compliant**

### Frontend
- **Framework:** React 19 + TypeScript
- **Build:** Vite
- **Styling:** Tailwind CSS
- **State:** Zustand
- **Routing:** React Router v6
- **i18n:** react-i18next (English / Bulgarian)
- **Real-time:** @microsoft/signalr

### Infrastructure
- **Containerization:** Docker + Docker Compose
- **Web Server:** Nginx (frontend reverse proxy)
- **Database:** PostgreSQL 16

## Project Structure

```
MamVibe/
├── backend/
│   ├── Dockerfile
│   ├── src/
│   │   ├── MomVibe.Domain/          # Entities, Enums, Constants
│   │   ├── MomVibe.Application/     # DTOs, Interfaces, Validators, Mapping
│   │   ├── MomVibe.Infrastructure/  # EF Core, Services, Persistence, Config
│   │   └── MomVibe.WebApi/          # Controllers, Hubs, Middleware, Startup
│   └── tests/
│       ├── MomVibe.UnitTests/
│       └── MomVibe.IntegrationTests/
├── frontend/
│   ├── Dockerfile
│   ├── nginx.conf
│   ├── public/                      # Static assets (logo, avatars)
│   └── src/
│       ├── api/                     # Axios API clients
│       ├── components/              # Reusable UI components
│       ├── contexts/                # React contexts (SignalR, Notifications)
│       ├── hooks/                   # Custom hooks
│       ├── layouts/                 # Main, Auth, Admin layouts
│       ├── locales/                 # EN/BG translation files
│       ├── pages/                   # Route pages
│       ├── services/                # SignalR service wrappers
│       ├── store/                   # Zustand stores
│       ├── types/                   # TypeScript type definitions
│       └── utils/                   # Utility helpers (currency, toast, etc.)
├── n8n-workflows/                   # Importable n8n workflow definitions
│   ├── user-registered.json
│   ├── payment-completed.json
│   ├── payment-failed.json
│   ├── shipment-created.json
│   ├── shipment-delivered.json
│   ├── shipment-stuck.json
│   ├── user-blocked.json
│   ├── item-sold.json
│   ├── new-chat-message.json
│   ├── stale-items.json
│   ├── daily-summary.json
│   ├── feedback-prompt.json
│   ├── weekly-seller-report.json
│   ├── new-item-approval-request.json
│   ├── welcome-series-day3.json
│   ├── payment-receipt.json
│   ├── shipment-tracking-update.json
│   └── admin-new-user-milestone.json
├── docker-compose.yml
├── .env / .env.example
├── .gitignore
└── README.md
```

## Prerequisites

- [Docker](https://docs.docker.com/get-docker/) and [Docker Compose](https://docs.docker.com/compose/install/)

For local development without Docker:
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/) and npm
- [PostgreSQL 16+](https://www.postgresql.org/download/)

## Quick Start (Docker)

```bash
# Clone the repository
git clone https://github.com/BojidarDermendjiev/MamVibe.git
cd MamVibe

# Copy and configure environment variables
cp .env.example .env
# Edit .env with your secrets

# Start all services
docker compose up --build
```

Once running:
- **Frontend:** http://localhost
- **API:** http://localhost:8080
- **Health check:** http://localhost:8080/health
- **Swagger (dev):** http://localhost:8080/swagger

### pgAdmin (optional)

Uncomment the `pgadmin` service in `docker-compose.yml`, then:

```bash
docker compose up --build
```

Access pgAdmin at http://localhost:5050 with:
- Email: `admin@mamvibe.com`
- Password: `admin`

Connect to the database with host `postgres`, port `5432`, and the credentials from your `.env` file.

## Docker Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `POSTGRES_DB` | `MamVibeDb` | PostgreSQL database name |
| `POSTGRES_USER` | `mamvibe` | PostgreSQL username |
| `POSTGRES_PASSWORD` | `changeme` | PostgreSQL password |
| `POSTGRES_PORT` | `5432` | PostgreSQL host port |
| `JWT_SECRET` | (dev default) | JWT signing key (min 32 chars) |
| `FRONTEND_URL` | `http://localhost` | Frontend URL for CORS |

## Local Development (without Docker)

### 1. Backend setup

```bash
cd backend/src/MomVibe.WebApi

# Copy and configure settings
cp appsettings.json appsettings.Development.json
# Edit appsettings.Development.json with your PostgreSQL connection string, JWT secret, etc.

# Apply EF Core migrations
dotnet ef database update --project ../MomVibe.Infrastructure --startup-project .

# Run the API
dotnet run
```

The API starts at `https://localhost:5001` by default.

### 2. Frontend setup

```bash
cd frontend

# Install dependencies
npm install

# Create environment file
cp .env.example .env.local
# Edit .env.local with your API URL

# Start dev server
npm run dev
```

The app starts at `http://localhost:5173` by default.

## Environment Variables

### Backend (`appsettings.Development.json`)

| Key | Description |
|-----|-------------|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string |
| `JwtSettings:Secret` | JWT signing key (min 32 chars) |
| `JwtSettings:Issuer` / `JwtSettings:Audience` | JWT token configuration |
| `GoogleAuth:ClientId` / `GoogleAuth:ClientSecret` | Google OAuth credentials |
| `Stripe:SecretKey` / `Stripe:WebhookSecret` | Stripe payment keys (card checkout) |
| `Stripe:WalletWebhookSecret` | Stripe webhook secret for wallet top-up events |
| `TakeANap:ApiKey` / `TakeANap:ApiSecret` | TakeANap fiscal receipt credentials |
| `Smtp:Host` / `Smtp:Port` / `Smtp:Username` / `Smtp:Password` | SMTP email configuration |
| `Econt:Username` / `Econt:Password` | Econt courier API credentials |
| `Speedy:Username` / `Speedy:Password` | Speedy courier API credentials |
| `BoxNow:ApiKey` | Box Now API key |
| `Cloudflare:TurnstileSecretKey` | Cloudflare Turnstile secret |
| `N8n:BaseUrl` | n8n webhook base URL |
| `N8n:Enabled` | Enable/disable webhook dispatching (`true`/`false`) |

### Frontend (`.env.local`)

| Key | Description |
|-----|-------------|
| `VITE_API_URL` | Backend API base URL |

## Key Features

- User registration/login (email + Google OAuth)
- Password reset via email
- Create, edit, delete item listings (donate or sell)
- Photo upload (up to 5 per item)
- Browse items with filters, sorting, and pagination
- Like/favorite items
- Real-time chat between buyers and sellers (SignalR)
- Unread message notifications with badge
- Stripe card payments + pay-on-spot option
- **In-platform Wallet** — Revolut-style escrow: buyer funds held on payment, released to seller on delivery confirmation
- **E-Bills** — Electronic payment receipts (MV-YYYY-XXXXXXXX format) issued automatically on purchase completion, viewable in the Dashboard and resendable via email
- Shipping integration (Econt, Speedy, Box Now) with label download and tracking
- TakeANap fiscal receipts for Bulgarian compliance
- Admin panel (user management, item approval, shipping overview)
- Multi-language support (English / Bulgarian)
- Profile types (Mom / Dad / Family) with custom avatars
- Cloudflare Turnstile bot protection
- Platform feedback system with star ratings
- n8n webhook integration for automated notifications and alerts

## Payment Test Mode

When Stripe keys are not configured (or contain the placeholder `YOUR_STRIPE`), the payment system automatically switches to **test mode**:

- Card payments are simulated — no real charges are made
- Payment records are created with `Completed` status and a `test_*` session ID
- The checkout flow works end-to-end: `/checkout` → `/checkout/card` → `/payment/success`
- E-bills are still issued in test mode so the full flow can be verified
- n8n webhooks fire with `TestMode: true` so you can distinguish test events

To enable real Stripe payments, set valid keys in `appsettings.json`:

```json
"Stripe": {
  "SecretKey": "sk_live_...",
  "WebhookSecret": "whsec_...",
  "WalletWebhookSecret": "whsec_..."
}
```

## Wallet & Escrow

The in-platform wallet follows a Revolut-style escrow pattern:

1. **Top-up** — buyer adds funds via Stripe PaymentIntent; confirmed via webhook.
2. **Purchase** — buyer's wallet is debited; funds are held in escrow (`WalletTransaction` with `Escrow` type).
3. **Delivery confirmation** — buyer confirms receipt; escrowed funds are released as a credit to the seller's wallet.
4. **Refund** — if delivery is rejected, escrow is returned to the buyer.

TakeANap fiscal receipts are issued for every wallet transaction that generates revenue.

## E-Bills (Electronic Payment Receipts)

E-bills are generated automatically for every completed purchase of a **Sell**-type item (donations are excluded). Key design decisions:

- **Idempotent** — `IssueEBillAsync` checks `EBillNumber != null` before assigning; Stripe webhook replays and delivery confirmations never send duplicate emails.
- **Number format** — `MV-{YEAR}-{first 8 hex chars of paymentId}` (e.g. `MV-2026-A1B2C3D4`), stored in `Payment.EBillNumber`.
- **VAT** — back-calculated at 20% Bulgarian rate from the gross amount and shown in the email receipt.
- **Resend** — rate-limited to 3 resends/minute per user (`POST /api/ebills/{id}/resend`).
- **Dashboard** — buyers can view all their e-bills in the **E-Bills** tab (EN: "E-Bills" / BG: "Е-Фактури").

## Security

MamVibe is fully compliant with all 15 ASP.NET Core security rules:

| # | Rule | Implementation |
|---|------|---------------|
| 1 | Enforce HTTPS | `UseHttpsRedirection` (non-dev) + `Strict-Transport-Security: max-age=31536000; includeSubDomains; preload` |
| 2 | `[Authorize]` + JWT | All protected endpoints decorated; JWT as default auth scheme |
| 3 | Validate JWT tokens | Issuer, audience, lifetime, signing key all validated; `ClockSkew = 30s` |
| 4 | Role/policy/claim-based authz | `AdminOnly` (role), `ActiveUser` (authenticated + not blocked) |
| 5 | Input validation | FluentValidation in Application layer; `ExceptionHandlingMiddleware` maps to 400 |
| 6 | DTOs prevent over-posting | All controllers accept/return DTOs, never entities |
| 7 | CORS trusted origins | `WithOrigins(config["FrontendUrl"])`, explicit headers and methods |
| 8 | Suppress error details | Generic messages in production; stack traces never exposed |
| 9 | Rate limiting | Global (200/min), Auth (30/min), Upload (20/min), EBill resend (3/min) + Identity lockout |
| 10 | No sensitive data in logs | Logs contain only IDs and status codes — no passwords, tokens, or PII |
| 11 | Security headers | CSP, X-Content-Type-Options, X-Frame-Options, Permissions-Policy, CORP, COOP, Referrer-Policy |
| 12 | No vulnerable packages | Verified via `dotnet list package --vulnerable` |
| 13 | Cookie security | N/A — JWT in `Authorization` header, no auth cookies |
| 14 | No unused endpoints | Swagger only in Development; purposeful controller structure |
| 15 | Strong password policy | Digit + Upper + Lower + NonAlphanumeric + Length≥8 + UniqueEmail |

## n8n Webhook Integration

MamVibe includes a lightweight webhook dispatcher that fires business events to an [n8n](https://n8n.io/) instance, enabling automated email notifications, seller alerts, and admin dashboards without adding complexity to the core codebase.

### Architecture

| Component | Role |
|-----------|------|
| `IN8nWebhookService` | Fire-and-forget interface — `void Send(path, payload)` |
| `N8nWebhookService` | `BackgroundService` draining a bounded `Channel<T>` (capacity 500), POSTs JSON to n8n |
| `N8nScheduledService` | Daily 8:00 AM UTC background job for time-based checks |
| `UserPresenceTracker` | Singleton tracking SignalR connections (used by MessageService + ChatHub) |

### Supported Events

| Event | Trigger | Key Payload Fields |
|-------|---------|-------------------|
| `payment.completed` | Stripe checkout or bulk payment succeeds | PaymentId, ItemTitle, BuyerEmail, SellerEmail, Amount |
| `shipment.created` | New shipment created | ShipmentId, TrackingNumber, CourierProvider, ItemTitle |
| `shipment.delivered` | Status sync detects delivery | ShipmentId, TrackingNumber, BuyerEmail, SellerEmail |
| `shipment.stuck` | Daily check: InTransit 7+ days | Shipments[] with DaysInTransit |
| `user.registered` | New user registration | Email, DisplayName, ProfileType, LanguagePreference |
| `user.blocked` | Admin blocks a user | UserId, Email, DisplayName |
| `chat.message_offline` | Message sent to offline user | SenderName, ReceiverEmail, ContentPreview |
| `stale_items` | Daily check: active items listed 30+ days | Items[] with Title, Price, DaysListed, ViewCount |
| `daily_summary` | Daily summary at 8:00 AM UTC | NewItems, NewPayments, NewShipments, ActiveShipments |
| `feedback_prompt` | Daily check: delivered 2+ days without feedback | Deliveries[] with ItemTitle, BuyerEmail |

### Configuration (`appsettings.json`)

```json
"N8n": {
  "BaseUrl": "https://mamvibe.app.n8n.cloud/webhook/",
  "Enabled": true,
  "PaymentCompleted": "payment-completed",
  "ShipmentCreated": "shipment-created",
  "ShipmentDelivered": "shipment-delivered",
  "UserRegistered": "user-registered",
  ...
}
```

Set `Enabled` to `false` to disable all webhook calls without removing code.

### n8n Workflow Setup

Pre-built workflow JSON files are available in the `n8n-workflows/` directory. To import:

1. Go to your n8n dashboard
2. Click **Add workflow** → **...** → **Import from file**
3. Select any workflow JSON from `n8n-workflows/`
4. Configure SMTP credentials in each email node (replace `REPLACE_WITH_SMTP_CREDENTIAL_ID`)
5. Update `admin@mamvibe.com` to your actual admin email in admin-facing workflows
6. Toggle the workflow **Active** (top right)

| Workflow File | Description |
|--------------|-------------|
| `user-registered.json` | Welcome email (BG/EN) + admin notification |
| `payment-completed.json` | Buyer confirmation + seller sale notification |
| `payment-failed.json` | Buyer retry prompt |
| `shipment-created.json` | Buyer shipping notification with tracking |
| `shipment-delivered.json` | Buyer delivery + feedback CTA, seller confirmation |
| `shipment-stuck.json` | Admin alert: shipments InTransit 7+ days |
| `user-blocked.json` | Suspension notice to user + admin log |
| `item-sold.json` | Seller congratulations + shipping CTA |
| `new-chat-message.json` | Offline user email with message preview |
| `stale-items.json` | Per-seller email for items listed 30+ days |
| `daily-summary.json` | Admin dashboard: daily counts |
| `feedback-prompt.json` | Buyer email for deliveries without feedback |
| `weekly-seller-report.json` | Weekly seller performance stats (Monday 9AM) |
| `new-item-approval-request.json` | Admin alert when new item needs approval |
| `welcome-series-day3.json` | Day-3 onboarding email (BG/EN) with feature guide |
| `payment-receipt.json` | HTML receipt generation + email to buyer |
| `shipment-tracking-update.json` | Buyer notification when order is out for delivery |
| `admin-new-user-milestone.json` | Admin celebration email at user milestones (10, 25, 50, 100...) |

## Branching Strategy (GitFlow)

| Branch | Purpose |
|--------|---------|
| `main` | Always deployable. Holds released code. |
| `develop` | Integration branch for upcoming release. |
| `feature/*` | New features. Branch from `develop`, merge back to `develop`. |
| `release/x.y.z` | Stabilization. Only bug fixes and docs. Merge to `main` + `develop`. |
| `hotfix/*` | Urgent production fixes. Branch from `main`, merge to `main` + `develop`. |

## Running Tests

```bash
# All tests
cd backend
dotnet test

# Unit tests only
dotnet test tests/MomVibe.UnitTests

# Integration tests only
dotnet test tests/MomVibe.IntegrationTests

# Frontend lint
cd frontend
npm run lint

# Check for vulnerable NuGet packages
cd backend
dotnet list package --vulnerable
```

## Screenshots

### Landing Page

<img src="docs/screenshots/web-home-light.png" width="49%" alt="Homepage – light mode" /> <img src="docs/screenshots/web-home-dark.png" width="49%" alt="Homepage – dark mode (logged in)" />

<img src="docs/screenshots/web-home-how-it-works.png" width="49%" alt="How It Works & Shop by Age sections" /> <img src="docs/screenshots/web-browse-items.png" width="49%" alt="Browse Items with filters and item grid" />

### Authentication

<img src="docs/screenshots/web-auth-login-dark.png" width="49%" alt="Login – dark mode" /> <img src="docs/screenshots/web-auth-login-light.png" width="49%" alt="Login – light mode" />

<img src="docs/screenshots/web-auth-register-dark.png" width="49%" alt="Register – dark mode" /> <img src="docs/screenshots/web-auth-register-light.png" width="49%" alt="Register – light mode" />

### Create & Manage Listings

<img src="docs/screenshots/web-create-listing.png" width="49%" alt="Create Listing with AI assistant" /> <img src="docs/screenshots/web-messages.png" width="49%" alt="Messages – real-time chat and AI-powered assistant" />

### User Dashboard

<img src="docs/screenshots/web-dashboard-listings.png" width="49%" alt="Dashboard – My Listings" /> <img src="docs/screenshots/web-dashboard-liked.png" width="49%" alt="Dashboard – Liked Items" />

<img src="docs/screenshots/web-dashboard-purchases.png" width="49%" alt="Dashboard – My Purchases" /> <img src="docs/screenshots/web-dashboard-requests.png" width="49%" alt="Dashboard – My Requests with status badges" />

<img src="docs/screenshots/web-dashboard-shipments.png" width="49%" alt="Dashboard – My Shipments" /> <img src="docs/screenshots/web-dashboard-ebills.png" width="49%" alt="Dashboard – E-Bills" />

### Profile, Settings & Wallet

<img src="docs/screenshots/web-profile.png" width="49%" alt="User profile page" /> <img src="docs/screenshots/web-settings.png" width="49%" alt="Settings – display name, bio, IBAN, language, password change" />

<img src="docs/screenshots/web-wallet.png" width="49%" alt="Wallet – balance, top-up, transfer, withdraw" /> <img src="docs/screenshots/web-feedback.png" width="49%" alt="Platform Feedback with star rating form" />

### Donations & Payments

<img src="docs/screenshots/web-donation.png" width="49%" alt="Donation page – amount picker" /> <img src="docs/screenshots/web-donation-payment.png" width="49%" alt="Donation card payment form" />

### Admin Panel

<img src="docs/screenshots/web-admin-dashboard.png" width="49%" alt="Admin Dashboard – users, items, donations, revenue stats" /> <img src="docs/screenshots/web-admin-users.png" width="49%" alt="Admin – Users management with block actions" />

<img src="docs/screenshots/web-admin-items.png" width="49%" alt="Admin – Items approval and management" /> <img src="docs/screenshots/web-admin-shipping.png" width="49%" alt="Admin – Shipping Management (Econt, Speedy, Box Now)" />

<img src="docs/screenshots/web-admin-wallets.png" width="49%" alt="Admin – Wallet Management with freeze controls" />

### Mobile App (Expo / React Native)

**Home & Browse**

<img src="docs/screenshots/mobile/home-dark.jpg" width="30%" alt="Mobile home – dark mode" /> <img src="docs/screenshots/mobile/home-light.jpg" width="30%" alt="Mobile home – light mode" /> <img src="docs/screenshots/mobile/home-shop-by-age.jpg" width="30%" alt="Mobile home – Shop by Age" />

<img src="docs/screenshots/mobile/browse-items.jpg" width="30%" alt="Mobile browse items" /> <img src="docs/screenshots/mobile/item-detail.jpg" width="30%" alt="Mobile item detail" /> <img src="docs/screenshots/mobile/messages.jpg" width="30%" alt="Mobile messages" />

**Profile, Orders & Wallet**

<img src="docs/screenshots/mobile/profile-dark.jpg" width="30%" alt="Mobile profile – dark mode" /> <img src="docs/screenshots/mobile/profile-light.jpg" width="30%" alt="Mobile profile – light mode" /> <img src="docs/screenshots/mobile/wallet.jpg" width="30%" alt="Mobile wallet" />

<img src="docs/screenshots/mobile/orders-incoming.jpg" width="30%" alt="Mobile my orders – incoming" /> <img src="docs/screenshots/mobile/my-items.jpg" width="30%" alt="Mobile my items" /> <img src="docs/screenshots/mobile/settings.jpg" width="30%" alt="Mobile settings" />

**Authentication**

<img src="docs/screenshots/mobile/login.jpg" width="30%" alt="Mobile login screen" />

## License

This project is proprietary. All rights reserved.
