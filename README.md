# MamVibe

A full-stack marketplace platform where families can donate or sell baby items — clothes, strollers, toys, and more.

## Tech Stack

### Backend
- **Runtime:** .NET 8 / ASP.NET Core Web API
- **Architecture:** Clean Architecture (Domain → Application → Infrastructure → WebApi)
- **Database:** PostgreSQL with EF Core (Code-First migrations)
- **Auth:** ASP.NET Core Identity, JWT + Refresh Tokens, Google OAuth
- **Real-time:** SignalR (chat messaging)
- **Payments:** Stripe Checkout
- **Shipping:** Econt Express, Speedy, Box Now (provider pattern)
- **Email:** SMTP service (password reset flow)
- **Security:** Cloudflare Turnstile, security headers middleware, blocked-user middleware

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

# Generate and apply EF Core migrations
dotnet ef migrations add InitialPostgres --project ../MomVibe.Infrastructure --startup-project .
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
| `JwtSettings:Secret` | JWT signing key |
| `JwtSettings:Issuer` / `JwtSettings:Audience` | JWT token configuration |
| `GoogleAuth:ClientId` / `GoogleAuth:ClientSecret` | Google OAuth credentials |
| `Stripe:SecretKey` / `Stripe:WebhookSecret` | Stripe payment keys |
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
- Stripe card payments + pay-on-spot option (test mode when Stripe is not configured)
- Dedicated card input page with animated credit card preview
- Shipping integration (Econt, Speedy, Box Now) with label download and tracking
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
- The checkout flow works end-to-end: `/checkout` → `/checkout/card` (card input page) → `/payment/success`
- n8n webhooks fire with `TestMode: true` so you can distinguish test events

To enable real Stripe payments, set valid keys in `appsettings.json`:

```json
"Stripe": {
  "SecretKey": "sk_live_...",
  "WebhookSecret": "whsec_..."
}
```

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
# Unit tests
cd backend/tests/MamVibe.UnitTests
dotnet test

# Integration tests
cd backend/tests/MamVibe.IntegrationTests
dotnet test

# Frontend lint
cd frontend
npm run lint
```

## License

This project is proprietary. All rights reserved.
