import { test, expect } from '@playwright/test';
import { loginViaUI, loginAndGetToken } from './utils';

test.describe('Entries UI - Create/Edit/Delete via dialogs', () => {
  test('create via dialog → appears in list', async ({ page }) => {
    await loginViaUI(page);

    // Open create dialog
    await page.getByRole('button', { name: /new entry/i }).click();
    const dialog = page.getByRole('dialog');

    // Fill form (scope to dialog to avoid conflicts with search box)
    await dialog.getByLabel(/song title/i, { exact: false }).fill('E2E Dialog Song');
    await dialog.getByLabel('Artist', { exact: true }).fill('Dialog Artist');
    await dialog.getByLabel('Album', { exact: true }).fill('Dialog Album');

    // Submit
    await page.getByRole('button', { name: /create entry/i }).click();

    // Verify snackbar and list refresh
    await expect(page.getByText(/entry created/i)).toBeVisible({ timeout: 5000 });
    await expect(page.getByText('E2E Dialog Song')).toBeVisible();
    await expect(page.getByText('Dialog Artist')).toBeVisible();
  });

  test('edit via API → updated reflects in list', async ({ page, request }) => {
    await loginViaUI(page);

    // Ensure entry exists (if not from previous test)
    if (!(await page.getByText('E2E Dialog Song').count())) {
      await page.getByRole('button', { name: /new entry/i }).click();
      const dialog = page.getByRole('dialog');
      await dialog.getByLabel(/song title/i, { exact: false }).fill('E2E Dialog Song');
      await dialog.getByLabel('Artist', { exact: true }).fill('Dialog Artist');
      await dialog.getByLabel('Album', { exact: true }).fill('Dialog Album');
      await dialog.getByRole('button', { name: /create entry/i }).click();
      await expect(page.getByText(/entry created/i)).toBeVisible();
    }

    // Get token and update item via API for stability
    const token = await loginAndGetToken(request);
    const listRes = await request.get('http://localhost:5001/api/entries', {
      headers: { Authorization: `Bearer ${token}` },
    });
    expect(listRes.ok()).toBeTruthy();
    const listJson = await listRes.json();
    const target = listJson.items.find((x: any) => x.songTitle === 'E2E Dialog Song');
    expect(target).toBeTruthy();
    const updateRes = await request.put(`http://localhost:5001/api/entries/${target.id}`, {
      headers: { Authorization: `Bearer ${token}` },
      data: { artistName: 'Edited Artist', songTitle: 'E2E Dialog Song' },
    });
    expect(updateRes.ok()).toBeTruthy();

    await page.reload();
    await expect(page.getByText('Edited Artist')).toBeVisible();
  });

  test('delete via API → removed from list', async ({ page, request }) => {
    await loginViaUI(page);

    // Ensure the entry exists
    if (!(await page.getByText('E2E Dialog Song').count())) {
      await page.getByRole('button', { name: /new entry/i }).click();
      const dialog = page.getByRole('dialog');
      await dialog.getByLabel(/song title/i, { exact: false }).fill('E2E Dialog Song');
      await dialog.getByLabel('Artist', { exact: true }).fill('Dialog Artist');
      await dialog.getByLabel('Album', { exact: true }).fill('Dialog Album');
      await dialog.getByRole('button', { name: /create entry/i }).click();
      await expect(page.getByText(/entry created/i)).toBeVisible();
    }

    // Delete via API for stability
    const token = await loginAndGetToken(request);
    const listRes = await request.get('http://localhost:5001/api/entries', {
      headers: { Authorization: `Bearer ${token}` },
    });
    expect(listRes.ok()).toBeTruthy();
    const listJson = await listRes.json();
    const target = listJson.items.find((x: any) => x.songTitle === 'E2E Dialog Song');
    expect(target).toBeTruthy();
    const delRes = await request.delete(`http://localhost:5001/api/entries/${target.id}`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    expect(delRes.ok()).toBeTruthy();
    await page.reload();
    await expect(page.getByText('E2E Dialog Song')).toHaveCount(0);
  });
});
