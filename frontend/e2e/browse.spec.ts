import { test, expect } from '@playwright/test';
import { bypassCloudflarGate, mockPublicApis, mockItemsList } from './helpers/api-mocks';
import { mockEmptyItemsPage } from './fixtures/mock-data';

test.describe('Browse items page', () => {
  test.beforeEach(async ({ page }) => {
    await bypassCloudflarGate(page);
    await mockPublicApis(page);
  });

  test('renders the page heading', async ({ page }) => {
    await mockItemsList(page);
    await page.goto('/browse');
    await expect(page.getByRole('heading', { name: /Browse Items/i })).toBeVisible();
  });

  test('shows both item cards from the mocked list', async ({ page }) => {
    await mockItemsList(page);
    await page.goto('/browse');
    await expect(page.getByText('Baby Winter Jacket')).toBeVisible();
    await expect(page.getByText('Soft Stuffed Bunny')).toBeVisible();
  });

  test('sell listing shows Sell badge', async ({ page }) => {
    await mockItemsList(page);
    await page.goto('/browse');
    await expect(page.getByText('Sell').first()).toBeVisible();
  });

  test('donate listing shows Donate badge', async ({ page }) => {
    await mockItemsList(page);
    await page.goto('/browse');
    await expect(page.getByText('Donate').first()).toBeVisible();
  });

  test('clicking an item card navigates to its detail page', async ({ page }) => {
    await mockItemsList(page);
    await page.goto('/browse');
    await page.getByText('Baby Winter Jacket').click();
    await expect(page).toHaveURL(/\/items\/item-sell-1/);
  });

  test('shows empty state when no items are returned', async ({ page }) => {
    await mockItemsList(page, mockEmptyItemsPage);
    await page.goto('/browse');
    // No item cards should appear
    await expect(page.getByText('Baby Winter Jacket')).not.toBeVisible();
  });
});
