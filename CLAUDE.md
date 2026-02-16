# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Development Commands

### Backend (.NET 8)
```bash
# Run API (from backend/src/MomVibe.WebApi/)
dotnet run

# EF Core migrations (from backend/src/MomVibe.WebApi/)
dotnet ef migrations add <Name> --project ../MomVibe.Infrastructure --startup-project .
dotnet ef database update --project ../MomVibe.Infrastructure --startup-project .

# Run unit tests
dotnet test backend/tests/MomVibe.UnitTests

# Run integration tests
dotnet test backend/tests/MomVibe.IntegrationTests

# Run a single test
dotnet test backend/tests/MomVibe.UnitTests --filter "FullyQualifiedName~TestMethodName"
```

### Frontend (React 19 + Vite)
```bash
# From frontend/
npm run dev       # Vite dev server at http://localhost:5173
npm run build     # TypeScript + Vite production build
npm run lint      # ESLint
npm run preview   # Preview production build
```

### Docker (full stack)
```bash
docker compose up --build   # Build and run all services
docker compose down         # Stop all services
```

## Architecture

**Monorepo** with three top-level directories: `backend/`, `frontend/`, `n8n-workflows/`.

### Backend — Clean Architecture (.NET 8)

Four projects under `backend/src/`, namespace prefix is `MomVibe` (not MamVibe):

| Layer | Project | Responsibility |
|-------|---------|---------------|
| Domain | `MomVibe.Domain` | Entities, Enums, Constants. No dependencies on other layers. |
| Application | `MomVibe.Application` | DTOs, Interfaces, FluentValidation validators, AutoMapper profiles. |
| Infrastructure | `MomVibe.Infrastructure` | EF Core DbContext, migrations, service implementations, external API clients (Stripe, shipping providers, n8n). |
| WebApi | `MomVibe.WebApi` | Controllers, SignalR Hubs, Middleware, DI registration (`StartUp.cs`). |

Key patterns:
- **DI registration** happens via `AddApplicationServices()` and `AddInfrastructureServices()` extension methods, wired in `StartUp.cs`
- **Shipping provider pattern**: pluggable couriers (Econt, Speedy, BoxNow) in `Infrastructure/Services/Shipping/`
- **N8n webhook dispatcher**: fire-and-forget via bounded `Channel<T>` (capacity 500) drained by `N8nWebhookService` BackgroundService
- **SignalR hub** at `/hubs/chat` with `UserPresenceTracker` singleton for online status

### Frontend — React 19 + TypeScript + Vite

- **State management**: Zustand stores in `src/store/`
- **API layer**: Axios clients in `src/api/`
- **Real-time**: SignalR context in `src/contexts/`
- **i18n**: English/Bulgarian translations in `src/locales/`
- **Path alias**: `@` maps to `src/` (configured in `vite.config.ts`)
- **Dev proxy**: Vite proxies `/api`, `/hubs`, `/uploads` to the backend (default `http://localhost:5038`)
- **Layout system**: separate layouts for Auth, Main, and Admin flows in `src/layouts/`

## Configuration

- **Backend config**: `appsettings.json` / `appsettings.Development.json` / `appsettings.Docker.json`
- **Frontend env**: `.env.local` with `VITE_API_URL` (and optional `VITE_API_TARGET`, `VITE_UPLOADS_TARGET`, `VITE_HUBS_TARGET`)
- **Docker**: root `.env` file (see `.env.example` for template)

## Branching Strategy (GitFlow)

- `main` — always deployable
- `develop` — integration branch
- `feature/*` — branch from `develop`, merge back to `develop`
- `release/x.y.z` — stabilization, merge to `main` + `develop`
- `hotfix/*` — from `main`, merge to `main` + `develop`

## Naming Note

The repository and Docker config use **MamVibe** but all .NET project names, namespaces, and C# code use **MomVibe** (e.g., `MomVibe.Domain`, `MomVibe.Application`).
