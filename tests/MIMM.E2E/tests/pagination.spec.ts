import { test, expect } from '@playwright/test';
import { loginViaUI, loginAndGetToken, createEntryViaAPI } from './utils';

test.describe('Pagination edge cases', () => {
  test('list shows pagination and navigates pages', async ({ page, request }) => {
    // Seed 15 entries via API
    const token = await loginAndGetToken(request);
    for (let i = 1; i <= 15; i++) {
      await createEntryViaAPI(request, token, {
        songTitle: `E2E Page Song ${i}`,
        artistName: `Artist ${i}`,
      });
    }

    await loginViaUI(page);

    // Verify pagination control visible
    const pagination = page.locator('.mud-pagination');
    await expect(pagination).toBeVisible();

    // Page should show max 10 items
    const items = page.locator('.mud-list-item');
    await expect(items).toHaveCount(10);

    // Navigate to next/last page via pagination buttons
    await pagination.locator('button').last().click(); // last page

    // On last page, remaining items should be <= 10
    const count = await items.count();
    expect(count).toBeLessThanOrEqual(10);

    // Verify presence of one of seeded items from later page
    await expect(page.getByText(/E2E Page Song 15/)).toBeVisible();
  });
});
