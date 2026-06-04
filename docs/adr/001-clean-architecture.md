# ADR-001: Clean Architecture over layered MVC

**Status**: Accepted  
**Date**: 2025-01-01

## Context

The project needs a long-lived, testable backend. The team considered a simple
layered MVC structure (Controller → Service → Repository) and the more strict
Clean Architecture (Domain / Application / Infrastructure / WebApi).

## Decision

Adopt Clean Architecture with four projects:

| Project | Namespace prefix | Role |
|---------|-----------------|------|
| `MomVibe.Domain` | `MomVibe.Domain` | Entities, enums, constants. Zero external dependencies. |
| `MomVibe.Application` | `MomVibe.Application` | DTOs, interfaces, validators (FluentValidation), AutoMapper profiles. Depends only on Domain. |
| `MomVibe.Infrastructure` | `MomVibe.Infrastructure` | EF Core DbContext, migrations, service implementations, external clients (Stripe, n8n, couriers). Depends on Domain + Application. |
| `MomVibe.WebApi` | `MomVibe.WebApi` | Controllers, SignalR hubs, middleware, DI wiring (`StartUp.cs`). Depends on Application + Infrastructure. |

Dependency injection is wired through `AddApplicationServices()` and
`AddInfrastructureServices()` extension methods, keeping `StartUp.cs` thin.

## Consequences

**Good**  
- Domain entities are free of framework annotations and infrastructure concerns.  
- Application interfaces allow Infrastructure to be swapped without changing business logic.  
- Each layer can be unit-tested independently; Infrastructure uses EF Core InMemory or
  Testcontainers (Postgres) depending on test type.

**Trade-offs**  
- More projects and namespaces than a flat MVC structure; onboarding takes slightly longer.  
- Cross-cutting concerns (e.g. current-user resolution) require an abstraction
  (`ICurrentUserService`) instead of direct `HttpContext` access in service code.
