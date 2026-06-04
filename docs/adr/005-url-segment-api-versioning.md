# ADR-005: URL-segment API versioning (`/api/v{version}/`)

**Status**: Accepted  
**Date**: 2025-01-01

## Context

The API must support multiple versions simultaneously so that existing clients
are not broken when breaking changes are introduced. Three common strategies
exist:

1. **URL segment** — `/api/v1/items`, `/api/v2/items`
2. **Query string** — `/api/items?api-version=1.0`
3. **Media type / header** — `Accept: application/vnd.momvibe.v1+json`

## Decision

Use **URL-segment versioning** via the `Asp.Versioning.Mvc` package.

- All controllers carry `[Asp.Versioning.ApiVersion("1.0")]` and
  `[Route("api/v{version:apiVersion}/...")]`.
- The default version is `1.0`; requests without a version segment are
  rejected (no implicit fallback).
- Swagger/OpenAPI documents are generated per version; the current published
  spec is `v1`.

## Consequences

**Good**  
- Version is explicit and visible in every request — easy to debug, cache, and
  proxy by path prefix.  
- Clients can target a specific version without custom headers; curl, browsers,
  and simple HTTP tools all work out of the box.  
- Adding `/api/v2/` routes requires only new controller registrations; v1 routes
  continue to function unchanged.

**Trade-offs**  
- URL includes a version segment that some REST purists consider non-canonical
  (version belongs in the media type, not the resource identifier).  
- Every breaking change requires a new URL prefix, which means updating all
  client SDKs and Axios calls in the frontend simultaneously.
