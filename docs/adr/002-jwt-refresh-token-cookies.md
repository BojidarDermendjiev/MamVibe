# ADR-002: JWT access tokens + httpOnly refresh token cookies

**Status**: Accepted  
**Date**: 2025-01-01

## Context

The API serves a browser SPA (React) and potentially mobile clients. Common
authentication strategies were considered:

1. Session cookies (server-side state)
2. Long-lived JWTs stored in `localStorage`
3. Short-lived JWTs in memory + httpOnly refresh token cookie

## Decision

Use short-lived JWT access tokens (15 min) delivered in the JSON response body
and stored in React in-memory state (Zustand store, not persisted to
`localStorage`). Pair them with a long-lived refresh token stored in an
`httpOnly`, `SameSite=Strict` cookie that the browser sends automatically.

The `/api/v1/auth/refresh` endpoint validates the cookie and issues a new access
token. The Axios response interceptor in the frontend transparently retries
failed 401 requests after a refresh.

## Consequences

**Good**  
- The refresh token is inaccessible to JavaScript; XSS cannot steal it.  
- Access tokens are short-lived so a leaked token has a small blast radius.  
- The SPA can detect unauthenticated state on load by calling `/auth/refresh`
  and checking the result.

**Trade-offs**  
- Two-token flow adds complexity compared to a single cookie session.  
- Mobile clients cannot use cookies without extra configuration; they must
  handle token storage themselves.  
- The `SameSite=Strict` cookie blocks cross-site flows (OAuth callbacks on a
  different domain) unless the cookie policy is relaxed to `Lax`.
