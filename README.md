# MomVibe

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
MomVibe/
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
git clone https://github.com/BojidarDermendjiev/MomVibe.git
cd MomVibe

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
- Email: `admin@momvibe.com`
- Password: `admin`

Connect to the database with host `postgres`, port `5432`, and the credentials from your `.env` file.

## Docker Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `POSTGRES_DB` | `MomVibeDb` | PostgreSQL database name |
| `POSTGRES_USER` | `momvibe` | PostgreSQL username |
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
- Shipping integration (Econt, Speedy, Box Now) with label download and tracking
- Admin panel (user management, item approval, shipping overview)
- Multi-language support (English / Bulgarian)
- Profile types (Mom / Dad / Family) with custom avatars
- Cloudflare Turnstile bot protection
- Platform feedback system with star ratings

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
```

## License

This project is proprietary. All rights reserved.
