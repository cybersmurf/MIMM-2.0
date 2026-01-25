import { test, expect } from '@playwright/test';
import { FRONTEND_URL, BACKEND_URL, loginViaUI } from './utils';

test.describe('Mood & Music interactions', () => {
  test('sets mood via drag and shows correct mood label', async ({ page }) => {
    await loginViaUI(page);
    await page.goto(`${FRONTEND_URL}/dashboard`);

    await page.getByRole('button', { name: /new entry/i }).click();
    const dialog = page.getByRole('dialog', { name: /new entry/i });
    await expect(dialog).toBeVisible();

    // Drag/click near top-right to produce Valence ~0.7, Arousal ~0.6
    const plane = dialog.locator('.mood-plane');
    await expect(plane).toBeVisible();
    await plane.click({ position: { x: 272, y: 128 } }); // Size defaults to 320px

    // MoodSelector chip shows composite label; list chip later maps to concise label (Happy)
    await expect(dialog.getByText(/Excited\s*\/\s*Happy/i)).toBeVisible();

    await dialog.getByLabel('Song title *').fill('Mood Drag Song');
    await dialog.getByLabel('Artist').fill('Mood Tester');
    await dialog.getByLabel('Notes (optional)').fill('Set via drag');

    await dialog.getByRole('button', { name: /create entry/i }).click();
    await expect(page.getByText(/Entry created successfully/i)).toBeVisible();

    // Verify in the list that mood chip reads "Happy"
    await expect(page.getByRole('list').locator('text=Happy').first()).toBeVisible();
  });

  test('music search autocomplete (mocked) populates fields via Use action', async ({ page }) => {
    await loginViaUI(page);
    await page.goto(`${FRONTEND_URL}/dashboard`);

    await page.route('**/api/music/search**', async (route) => {
      const url = new URL(route.request().url());
      const query = url.searchParams.get('query') || '';
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          query,
          items: [
            {
              title: 'Mock Track',
              artist: 'Mock Artist',
              album: 'Mock Album',
              coverUrl: null,
              source: 'mock',
              externalId: 'mock-1',
            },
          ],
        }),
      });
    });

    await page.getByRole('button', { name: /new entry/i }).click();
    const dialog = page.getByRole('dialog', { name: /new entry/i });
    await expect(dialog).toBeVisible();

    await dialog.getByLabel('Song or artist').fill('beatles');
    await dialog.getByLabel('Song or artist').press('Enter');

    // Wait for mocked list to appear and use the first item
    const useButton = dialog.getByRole('button', { name: /^use$/i }).first();
    await expect(useButton).toBeVisible();
    await useButton.click();

    // Fields should be populated according to HandleTrackSelected
    await expect(dialog.getByLabel('Song title *')).toHaveValue('Mock Track');
    await expect(dialog.getByLabel('Artist')).toHaveValue('Mock Artist');
    await expect(dialog.getByLabel('Album')).toHaveValue('Mock Album');

    // Finish creation to ensure stability of flow
    await dialog.getByRole('button', { name: /create entry/i }).click();
    await expect(page.getByText(/Entry created successfully/i)).toBeVisible();
  });
});
