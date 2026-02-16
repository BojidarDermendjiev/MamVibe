# MamVibe

A full-stack marketplace platform where families can donate or sell baby items — clothes, strollers, toys, and more.

## Tech Stack

### Backend — NuGet Packages

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.* | JWT authentication |
| Npgsql.EntityFrameworkCore.PostgreSQL | 8.0.* | PostgreSQL EF Core provider |
| Microsoft.EntityFrameworkCore.Design | 8.0.* | EF Core migrations tooling |
| Microsoft.EntityFrameworkCore.Tools | 8.0.0 | EF Core CLI tools |
| Stripe.net | 50.3.0 | Stripe payment integration |
| Google.Apis.Auth | 1.73.0 | Google OAuth verification |
| Serilog.AspNetCore | 10.0.0 | Structured logging |
| Swashbuckle.AspNetCore | 6.5.0 | Swagger / OpenAPI docs |

### Frontend — npm Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| react | ^19.2.0 | UI framework |
| react-dom | ^19.2.0 | React DOM renderer |
| react-router-dom | ^6.30.3 | Client-side routing |
| zustand | ^5.0.10 | Lightweight state management |
| axios | ^1.13.4 | HTTP client |
| @microsoft/signalr | ^10.0.0 | Real-time chat (SignalR) |
| @stripe/react-stripe-js | ^5.6.0 | Stripe Elements React bindings |
| @stripe/stripe-js | ^8.7.0 | Stripe.js loader |
| i18next | ^25.8.0 | Internationalization framework |
| react-i18next | ^16.5.4 | React i18n bindings |
| i18next-browser-languagedetector | ^8.2.0 | Auto language detection |
| react-icons | ^5.5.0 | Icon library |
| react-hot-toast | ^2.6.0 | Toast notifications |
| date-fns | ^4.1.0 | Date utilities |
| clsx | ^2.1.1 | Conditional classnames |
| tailwindcss | ^4.1.18 | Utility-first CSS |
| vite | ^7.2.4 | Build tool |
| typescript | ~5.9.3 | Type checking |
| eslint | ^9.39.1 | Linting |

### Infrastructure

- **Containerization:** Docker + Docker Compose
- **Web Server:** Nginx (frontend reverse proxy)
- **Database:** PostgreSQL 16

---

## Backend Architecture

### Clean Architecture Layers

| Layer | Project | Responsibility |
|-------|---------|----------------|
| Domain | `MomVibe.Domain` | Entities, Enums, Constants |
| Application | `MomVibe.Application` | DTOs, Interfaces, Validators, Mapping |
| Infrastructure | `MomVibe.Infrastructure` | EF Core, Services, Persistence, Config |
| Presentation | `MomVibe.WebApi` | Controllers, Hubs, Middleware, Startup |

### Entities (10)

`ApplicationUser` · `Item` · `ItemPhoto` · `Category` · `Like` · `Message` · `Payment` · `Shipment` · `RefreshToken` · `Feedback`

### Enums (8)

`CourierProvider` · `DeliveryType` · `FeedbackCategory` · `ListingType` · `PaymentMethod` · `PaymentStatus` · `ProfileType` · `ShipmentStatus`

### Controllers (11)

| Controller | Endpoints | Key Responsibilities |
|------------|-----------|---------------------|
| AuthController | 9 | Register, login, Google OAuth, refresh, revoke, change/forgot/reset password, me |
| ItemsController | 7 | CRUD, browse with filters/pagination, view tracking, like toggle |
| PaymentsController | 9 | Checkout (single + bulk), booking, on-spot, create intent, webhook, my payments |
| ShippingController | 8 | Calculate, create, label download, track, cancel, offices, by payment, my shipments |
| AdminController | 10 | Dashboard, users (list/block/unblock), items (delete/pending/approve), shipments, payments |
| MessagesController | 4 | Conversations list, thread history, send, mark read |
| UsersController | 4 | Get profile, update profile, dashboard items, dashboard liked |
| CategoriesController | 2 | List all, get by ID |
| PhotosController | 2 | Upload, delete |
| FeedbackController | 3 | List, create, delete |
| TurnstileController | 1 | Verify Cloudflare Turnstile token |

### Services (20)

| Service | Description |
|---------|-------------|
| AuthService | Registration, login, token management, password reset |
| TokenService | JWT + refresh token generation/validation |
| ItemService | Item CRUD, filtering, pagination, likes |
| PhotoService | Photo upload with magic byte validation |
| PaymentService | Stripe checkout, on-spot, bulk payments |
| ShippingService | Multi-carrier shipping orchestration |
| EcontCourierProvider | Econt Express API integration |
| SpeedyCourierProvider | Speedy API integration |
| BoxNowCourierProvider | Box Now API integration |
| CourierProviderFactory | Factory for courier provider selection |
| MessageService | Chat messaging, conversations, read receipts |
| AdminService | Admin dashboard, user/item management |
| FeedbackService | Platform feedback CRUD |
| EmailService | SMTP email sending (password reset) |
| TurnstileService | Cloudflare Turnstile verification |
| CurrentUserService | Extracts authenticated user context from JWT |
| N8nWebhookService | Background webhook dispatcher (Channel-based) |
| N8nScheduledService | Daily scheduled checks (stale items, stuck shipments) |
| UserPresenceTracker | Tracks online/offline users via SignalR |
| TakeANapService | Utility service |

### Middleware (3)

| Middleware | Purpose |
|------------|---------|
| SecurityHeadersMiddleware | CSP, COOP, CORP, X-Content-Type-Options, Referrer-Policy |
| BlockedUserMiddleware | Rejects requests from admin-blocked users |
| ExceptionHandlingMiddleware | Global exception handling with structured error responses |

### Real-time

- **ChatHub** (SignalR) — real-time messaging with presence tracking

### Rate Limiting

- Auth endpoints use `[EnableRateLimiting("auth")]` policy

---

## Frontend Architecture

### Pages (26)

| Category | Pages |
|----------|-------|
| Public | HomePage, BrowseItemsPage, ItemDetailPage, LoginPage, RegisterPage, ForgotPasswordPage, ResetPasswordPage, NotFoundPage |
| Authenticated | CreateItemPage, EditItemPage, ProfilePage, SettingsPage, DashboardPage, CartPage, ChatPage, FeedbackPage |
| Payment | CheckoutPage, CardPaymentPage, PaymentPage, PaymentSuccessPage, PaymentCancelPage |
| Shipping | ShipmentDetailPage |
| Admin | AdminDashboardPage, AdminUsersPage, AdminItemsPage, AdminShippingPage |

### Components (26)

| Folder | Components |
|--------|------------|
| common/ | Avatar, Button, Input, Modal, IbanModal, Pagination, LoadingSpinner, LanguageSwitcher, ProtectedRoute, CookieConsent, CloudflareGate |
| items/ | ItemCard, ItemFilters, LikeButton, PhotoUploader |
| shipping/ | CourierSelector, DeliveryTypeSelector, ShipmentCard, ShipmentTracker, ShippingPricePreview, OfficePicker |
| payment/ | PaymentCard, PaymentCardForm |
| feedback/ | FeedbackCard, StarRating |
| user/ | ProfileTypeSelector |

### State Management

| Store | Purpose |
|-------|---------|
| authStore | Authentication state, tokens, user profile |
| cartStore | Shopping cart items |

### API Clients (10)

`axiosClient` · `authApi` · `itemsApi` · `photosApi` · `paymentsApi` · `shippingApi` · `messagesApi` · `adminApi` · `feedbackApi` · `turnstileApi`

### Hooks (6)

`useAuth` · `useItems` · `useCategories` · `useDashboard` · `useDebounce` · `useSignalR`

### Contexts (4)

`SignalRContext` · `NotificationContext` · `CategoriesContext` · `ThemeContext`

### Layouts (3)

`MainLayout` · `AuthLayout` · `AdminLayout`

### i18n

English and Bulgarian translations in `src/locales/`

---

## API Endpoints

### Auth — `/api/auth`

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/register` | Register new account |
| POST | `/login` | Email/password login |
| POST | `/refresh` | Refresh JWT tokens |
| POST | `/google` | Google OAuth login |
| POST | `/revoke` | Revoke refresh token (logout) |
| POST | `/change-password` | Change password (authenticated) |
| POST | `/forgot-password` | Request password reset email |
| POST | `/reset-password` | Reset password with token |
| GET | `/me` | Get current user profile |

### Items — `/api/items`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | Browse items (filters, pagination, sorting) |
| GET | `/{id}` | Get item details |
| POST | `/{id}/view` | Increment view count |
| POST | `/` | Create item listing |
| PUT | `/{id}` | Update item |
| DELETE | `/{id}` | Delete item |
| POST | `/{id}/like` | Toggle like |

### Payments — `/api/payments`

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/checkout/{itemId}` | Create Stripe checkout session |
| POST | `/onspot/{itemId}` | Pay on spot (single) |
| POST | `/booking/{itemId}` | Book item for pickup |
| POST | `/create-intent/{itemId}` | Create Stripe payment intent |
| POST | `/bulk-checkout` | Bulk Stripe checkout |
| POST | `/bulk-booking` | Bulk booking |
| POST | `/bulk-onspot` | Bulk pay on spot |
| POST | `/webhook` | Stripe webhook handler |
| GET | `/my-payments` | List user's payments |

### Shipping — `/api/shipping`

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/calculate` | Calculate shipping price |
| POST | `/create` | Create shipment |
| GET | `/{id}/label` | Download shipping label |
| GET | `/{id}/track` | Track shipment |
| POST | `/{id}/cancel` | Cancel shipment |
| GET | `/offices` | List courier offices |
| GET | `/payment/{paymentId}` | Get shipment by payment |
| GET | `/my-shipments` | List user's shipments |

### Admin — `/api/admin`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/dashboard` | Admin dashboard stats |
| GET | `/users` | List all users |
| POST | `/users/{userId}/block` | Block user |
| POST | `/users/{userId}/unblock` | Unblock user |
| DELETE | `/items/{id}` | Delete item (admin) |
| GET | `/items/pending` | List pending items |
| POST | `/items/{id}/approve` | Approve item |
| GET | `/shipments` | List all shipments |
| GET | `/shipments/{id}/track` | Track shipment (admin) |
| GET | `/payments` | List all payments |

### Messages — `/api/messages`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/conversations` | List conversations |
| GET | `/{otherUserId}` | Get message thread |
| POST | `/` | Send message |
| PUT | `/{senderId}/read` | Mark messages as read |

### Users — `/api/users`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/{id}` | Get user profile |
| PUT | `/profile` | Update own profile |
| GET | `/dashboard/items` | User's listed items |
| GET | `/dashboard/liked` | User's liked items |

### Categories — `/api/categories`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | List all categories |
| GET | `/{id}` | Get category by ID |

### Photos — `/api/photos`

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/upload` | Upload item photo |
| DELETE | `/` | Delete photo |

### Feedback — `/api/feedback`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | List feedback |
| POST | `/` | Submit feedback |
| DELETE | `/{id}` | Delete feedback |

### Turnstile — `/api/turnstile`

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/verify` | Verify Cloudflare Turnstile token |

---

## Security

| Layer | Mechanism |
|-------|-----------|
| Authentication | JWT access tokens + refresh tokens, Google OAuth |
| Authorization | Role-based (Admin), IDOR checks on item/payment ownership |
| Rate Limiting | ASP.NET Core rate limiting on auth endpoints |
| Bot Protection | Cloudflare Turnstile verification |
| Security Headers | CSP, COOP, CORP, X-Content-Type-Options, Referrer-Policy |
| File Upload | Magic byte validation (JPEG, PNG, GIF, WebP, BMP), path traversal prevention |
| Blocked Users | Middleware rejects all requests from blocked accounts |
| Password Reset | Time-limited tokens, anti-enumeration (always returns 200) |

---

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
│       ├── store/                   # Zustand stores
│       └── types/                   # TypeScript type definitions
├── n8n-workflows/                   # Importable n8n workflow definitions
├── docker-compose.yml
├── .env / .env.example
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
cd backend/tests/MomVibe.UnitTests
dotnet test

# Integration tests
cd backend/tests/MomVibe.IntegrationTests
dotnet test

# Frontend lint
cd frontend
npm run lint

# Type check
npx tsc --noEmit

# Build
npm run build
```

## License

This project is proprietary. All rights reserved.
