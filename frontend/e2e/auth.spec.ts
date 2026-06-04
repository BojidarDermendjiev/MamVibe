import { test, expect } from '@playwright/test';
import {
  bypassCloudflarGate,
  mockPublicApis,
  mockItemsList,
  mockAuthApis,
  mockDashboardApis,
  mockTurnstile,
  seedAuthState,
} from './helpers/api-mocks';

test.describe('Authentication', () => {
  test('login page renders the sign-in form', async ({ page }) => {
    await bypassCloudflarGate(page);
    await page.goto('/login');
    await expect(page.getByPlaceholder(/email/i).first()).toBeVisible();
    await expect(page.getByPlaceholder(/password/i).first()).toBeVisible();
  });

  test('successful login redirects to home', async ({ page }) => {
    await bypassCloudflarGate(page);
    await mockPublicApis(page);
    await mockItemsList(page);
    await mockAuthApis(page);
    await mockTurnstile(page);

    await page.goto('/login');
    await page.getByPlaceholder(/email/i).first().fill('e2e@example.com');
    await page.getByPlaceholder(/password/i).first().fill('Password1');
    await page.getByRole('button', { name: /sign in|log in/i }).first().click();
    await expect(page).toHaveURL('/');
  });

  test('unauthenticated visit to /dashboard redirects to login', async ({ page }) => {
    await bypassCloudflarGate(page);
    await page.goto('/dashboard');
    await expect(page).toHaveURL(/\/login/);
  });

  test('seeded auth state grants access to /dashboard', async ({ page }) => {
    await bypassCloudflarGate(page);
    await mockPublicApis(page);
    await mockAuthApis(page);
    await mockDashboardApis(page);
    await seedAuthState(page);
    await page.goto('/dashboard');
    // Stays on /dashboard — not redirected to /login
    await expect(page).not.toHaveURL(/\/login/);
  });

  test('register page shows the sign-up form', async ({ page }) => {
    await bypassCloudflarGate(page);
    await page.goto('/register');
    await expect(page.getByPlaceholder(/display name/i)).toBeVisible();
    await expect(page.getByPlaceholder(/email/i).first()).toBeVisible();
  });
});
