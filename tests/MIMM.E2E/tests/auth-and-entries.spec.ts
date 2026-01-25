import { test, expect } from '@playwright/test';

const FRONTEND_URL = process.env.FRONTEND_URL || 'http://localhost:5000';
const BACKEND_URL = process.env.BACKEND_URL || 'http://localhost:5001';

const USER_EMAIL = process.env.TEST_EMAIL || 'e2e-auto@example.com';
const USER_PASSWORD = process.env.TEST_PASSWORD || 'Test123!';

test.describe('MIMM E2E - Auth and Entries', () => {
  test('login → dashboard → create entry via API → list shows entry', async ({ page, request }) => {
    // Open login page
    await page.goto(`${FRONTEND_URL}/login`);

    // Fill email & password
    await page.getByLabel('Email').fill(USER_EMAIL);
    await page.getByLabel('Password').fill(USER_PASSWORD);

    // Click Sign In button
    await page.getByRole('button', { name: /sign in/i }).click();

    // Navigate to dashboard and verify welcome text
    await page.waitForURL(/.*\/dashboard$/);
    // Accept either snackbar or heading; prefer heading to avoid strict violation
    await expect(page.getByRole('heading', { name: /welcome back/i })).toBeVisible();

    // Create entry via API to avoid UI flakiness in first test
    const loginRes = await request.post(`${BACKEND_URL}/api/auth/login`, {
      data: { email: USER_EMAIL, password: USER_PASSWORD },
    });
    expect(loginRes.ok()).toBeTruthy();
    const { accessToken } = await loginRes.json();

    const createRes = await request.post(`${BACKEND_URL}/api/entries`, {
      headers: { Authorization: `Bearer ${accessToken}` },
      data: {
        songTitle: 'E2E Test Song',
        artistName: 'E2E Artist',
        albumName: 'E2E Album',
        valence: 0.6,
        arousal: 0.4,
        tensionLevel: 35,
        somaticTags: ['Warm'],
        notes: 'Created by Playwright test',
      },
    });
    expect(createRes.ok()).toBeTruthy();

    // Refresh dashboard and verify the new entry appears
    await page.reload();
    await expect(page.getByText('E2E Test Song')).toBeVisible();
    await expect(page.getByText('E2E Artist')).toBeVisible();
  });
});
