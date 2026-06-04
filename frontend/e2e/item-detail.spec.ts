import { test, expect } from '@playwright/test';
import { bypassCloudflarGate, mockPublicApis, mockItemDetail } from './helpers/api-mocks';
import { mockSellItem } from './fixtures/mock-data';

test.describe('Item detail page', () => {
  test.beforeEach(async ({ page }) => {
    await bypassCloudflarGate(page);
    await mockPublicApis(page);
    await mockItemDetail(page);
  });

  test('shows the item title', async ({ page }) => {
    await page.goto(`/items/${mockSellItem.id}`);
    await expect(page.getByRole('heading', { name: 'Baby Winter Jacket' })).toBeVisible();
  });

  test('shows the item price', async ({ page }) => {
    await page.goto(`/items/${mockSellItem.id}`);
    await expect(page.getByText(/45/)).toBeVisible();
  });

  test('shows the seller display name', async ({ page }) => {
    await page.goto(`/items/${mockSellItem.id}`);
    await expect(page.getByText('Jane Doe')).toBeVisible();
  });

  test('shows the item photo', async ({ page }) => {
    await page.goto(`/items/${mockSellItem.id}`);
    const img = page.getByRole('img', { name: 'Baby Winter Jacket' });
    await expect(img).toBeVisible();
  });

  test('category name is visible', async ({ page }) => {
    await page.goto(`/items/${mockSellItem.id}`);
    // categoryName appears in both the breadcrumb and the subtitle — use first()
    // to avoid a "multiple elements" assertion failure.
    await expect(page.getByText('Clothing').first()).toBeVisible();
  });
});
