# MomVibe

A full-stack marketplace platform where families can donate or sell baby items — clothes, strollers, toys, and more.

## Tech Stack

### Backend
- **Runtime:** .NET 8 / ASP.NET Core Web API
- **Architecture:** Clean Architecture (Domain &rarr; Application &rarr; Infrastructure &rarr; WebApi)
- **Database:** SQL Server with EF Core (Code-First migrations)
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

## Project Structure

```
MomVibe/
├── backend/
│   ├── src/
│   │   ├── MomVibe.Domain/          # Entities, Enums, Constants
│   │   ├── MomVibe.Application/     # DTOs, Interfaces, Validators, Mapping
│   │   ├── MomVibe.Infrastructure/  # EF Core, Services, Persistence, Config
│   │   └── MomVibe.WebApi/          # Controllers, Hubs, Middleware, Startup
│   └── tests/
│       ├── MomVibe.UnitTests/
│       └── MomVibe.IntegrationTests/
├── frontend/
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
├── .gitignore
└── README.md
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/) and npm
- [SQL Server 2019+](https://www.microsoft.com/sql-server) (or LocalDB via Visual Studio)

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/BojidarDermendjiev/MomVibe.git
cd MomVibe
```

### 2. Backend setup

```bash
cd backend/src/MomVibe.WebApi

# Copy and configure settings
cp appsettings.json appsettings.Development.json
# Edit appsettings.Development.json with your connection string, JWT secret, Stripe keys, etc.

# Apply EF Core migrations
dotnet ef database update --project ../MomVibe.Infrastructure --startup-project .

# Run the API
dotnet run
```

The API starts at `https://localhost:5001` by default.

### 3. Frontend setup

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
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `Jwt:Secret` | JWT signing key |
| `Jwt:Issuer` / `Jwt:Audience` | JWT token configuration |
| `Google:ClientId` / `Google:ClientSecret` | Google OAuth credentials |
| `Stripe:SecretKey` / `Stripe:WebhookSecret` | Stripe payment keys |
| `Smtp:Host` / `Smtp:Port` / `Smtp:Username` / `Smtp:Password` | SMTP email configuration |
| `Econt:Username` / `Econt:Password` | Econt courier API credentials |
| `Speedy:Username` / `Speedy:Password` | Speedy courier API credentials |
| `BoxNow:ApiKey` | Box Now API key |
| `Turnstile:SecretKey` | Cloudflare Turnstile secret |

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
