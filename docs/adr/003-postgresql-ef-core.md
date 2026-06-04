# ADR-003: PostgreSQL with EF Core (Code-First)

**Status**: Accepted  
**Date**: 2025-01-01

## Context

The project requires a relational database with full-text search, JSONB
support, and good .NET tooling. SQLite, MySQL, and PostgreSQL were considered.

## Decision

Use **PostgreSQL** as the primary database with **EF Core 8** in code-first
mode (migrations generated from C# entity classes).

Key configuration choices:

- All migrations live in `MomVibe.Infrastructure/Migrations/`.
- The `ApplicationDbContext` is registered with a scoped lifetime.
- `IApplicationDbContext` (Application layer interface) abstracts `DbContext`
  so services never take a direct dependency on EF Core.
- Knowledge articles use PostgreSQL full-text search (FTS) via a generated
  `tsvector` column configured in `KnowledgeArticleConfiguration.cs`.
- Integration tests spin up a real Postgres instance via **Testcontainers**
  (Docker) to catch migration and constraint issues that InMemory misses.

## Consequences

**Good**  
- PostgreSQL FTS eliminates the need for a separate search index for the
  knowledge base feature.  
- JSONB columns are available if unstructured metadata is needed.  
- EF Core migrations provide a reproducible schema history checked into git.

**Trade-offs**  
- Docker is required to run integration tests locally (Testcontainers).  
- InMemory provider is kept only for fast unit tests that do not touch
  migration-specific SQL; teams must be aware of the InMemory limitations
  (no FK enforcement, no raw SQL, Include behaviour differs).
