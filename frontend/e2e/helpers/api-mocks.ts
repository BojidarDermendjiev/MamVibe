import type { Page } from '@playwright/test';
import {
  mockCategories,
  mockEmptyItemsPage as _mockEmptyItemsPage,
  mockItemsPage,
  mockLoginResponse,
  mockSellItem,
  mockStats,
  mockUser,
} from '../fixtures/mock-data';

// Intercepts all the public-facing API endpoints used on the home and browse pages.
// useAuth fires a silent refresh on every mount; we return 401 so it silently
// calls logout() without redirecting (the interceptor only hard-navigates for
// non-refresh 401s). turnstile/verify is absorbed so the Vite proxy never errors.
export async function mockPublicApis(page: Page) {
  await page.route(
    (url) => url.pathname === '/api/v1/stats',
    (route) => route.fulfill({ json: mockStats }),
  );
  await page.route(
    (url) => url.pathname === '/api/v1/categories',
    (route) => route.fulfill({ json: mockCategories }),
  );
  await page.route(
    (url) => url.pathname === '/api/v1/doctor-reviews',
    (route) => route.fulfill({ json: { items: [], totalCount: 0, totalPages: 0 } }),
  );
  await page.route(
    (url) => url.pathname === '/api/v1/child-friendly-places',
    (route) => route.fulfill({ json: { items: [], totalCount: 0, totalPages: 0 } }),
  );
  // Silent 401 on refresh: interceptor sees /auth/refresh → calls logout() and
  // rejects without triggering a hard navigation to /login.
  await page.route(
    (url) => url.pathname === '/api/v1/auth/refresh',
    (route) => route.fulfill({ status: 401, json: {} }),
  );
  await page.route(
    (url) => url.pathname === '/api/v1/turnstile/verify',
    (route) => route.fulfill({ json: { verified: true } }),
  );
}

// Intercepts GET /items (list endpoint) and returns the default mock page.
export async function mockItemsList(page: Page, payload = mockItemsPage) {
  await page.route(
    (url) => url.pathname === '/api/v1/items',
    (route) => route.fulfill({ json: payload }),
  );
}

// Intercepts GET /items/:id and returns the sell item mock.
export async function mockItemDetail(page: Page, itemId = mockSellItem.id) {
  await page.route(
    (url) => url.pathname === `/api/v1/items/${itemId}`,
    (route) => route.fulfill({ json: mockSellItem }),
  );
  // View-count increment — fire-and-forget, just absorb it
  await page.route(
    (url) => url.pathname === `/api/v1/items/${itemId}/view`,
    (route) => route.fulfill({ status: 204, body: '' }),
  );
}

// Intercepts the auth endpoints needed for the login flow.
export async function mockAuthApis(page: Page) {
  await page.route(
    (url) => url.pathname === '/api/v1/auth/login',
    (route) => route.fulfill({ json: mockLoginResponse }),
  );
  await page.route(
    (url) => url.pathname === '/api/v1/auth/refresh',
    (route) => route.fulfill({ json: mockLoginResponse }),
  );
  await page.route(
    (url) => url.pathname === '/api/v1/auth/me',
    (route) => route.fulfill({ json: mockUser }),
  );
}

// Intercepts dashboard endpoints so a logged-in user can load /dashboard.
// getMyItems() and getLikedItems() return Item[] directly (not a paginated wrapper).
export async function mockDashboardApis(page: Page) {
  await page.route(
    (url) => url.pathname === '/api/v1/users/dashboard/items',
    (route) => route.fulfill({ json: [] }),
  );
  await page.route(
    (url) => url.pathname === '/api/v1/users/dashboard/liked',
    (route) => route.fulfill({ json: [] }),
  );
}

// Injects a fake Turnstile implementation so login/register forms become submittable
// in a test environment where Cloudflare's challenge script is blocked.
export async function mockTurnstile(page: Page) {
  await page.route('**challenges.cloudflare.com**', (route) => route.abort());
  await page.addInitScript(() => {
    (window as Window & { turnstile?: unknown }).turnstile = {
      render: (_container: unknown, options: Record<string, unknown>) => {
        // Call the success callback asynchronously so React state has time to update
        setTimeout(() => {
          if (typeof options?.callback === 'function') options.callback('e2e-turnstile-token');
        }, 50);
        return 'mock-widget-id';
      },
      reset: () => {},
      remove: () => {},
      getResponse: () => 'e2e-turnstile-token',
    };
  });
}

// Seeds sessionStorage so CloudflareGate treats the session as already verified,
// bypassing the Turnstile security check that otherwise blocks the entire app.
export async function bypassCloudflarGate(page: Page) {
  await page.addInitScript(() => {
    sessionStorage.setItem('cf_verified', String(Date.now() + 30 * 60 * 1000));
  });
}

// Seeds the Zustand auth store in localStorage so the page loads as authenticated
// without going through the login form.
// Must use addInitScript (not page.evaluate) so the key is written before React
// initializes and Zustand reads localStorage on the navigated page.
export async function seedAuthState(page: Page) {
  const serialized = JSON.stringify({ state: { user: mockUser }, version: 0 });
  await page.addInitScript((data) => {
    localStorage.setItem('mamvibe-auth', data);
  }, serialized);
}
