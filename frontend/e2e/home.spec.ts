import { test, expect } from '@playwright/test';
import { bypassCloudflarGate, mockPublicApis } from './helpers/api-mocks';

test.describe('Home page', () => {
  test.beforeEach(async ({ page }) => {
    await bypassCloudflarGate(page);
    await mockPublicApis(page);
  });

  test('shows the hero title', async ({ page }) => {
    await page.goto('/');
    await expect(page.getByText('Give Baby Items a Second Life')).toBeVisible();
  });

  test('renders the stats bar with all three stat labels', async ({ page }) => {
    await page.goto('/');
    // Stats section uses framer-motion whileInView — scroll it into the viewport first
    // so the IntersectionObserver fires and the section becomes visible.
    const statsLabel = page.getByText('Active listings');
    await statsLabel.scrollIntoViewIfNeeded();
    await expect(statsLabel).toBeVisible();
    await expect(page.getByText('Active sellers')).toBeVisible();
    await expect(page.getByText('Happy families')).toBeVisible();
  });

  test('Browse Items button navigates to /browse', async ({ page }) => {
    await page.goto('/');
    await page.getByRole('link', { name: /Browse Items/i }).first().click();
    await expect(page).toHaveURL('/browse');
  });
});
