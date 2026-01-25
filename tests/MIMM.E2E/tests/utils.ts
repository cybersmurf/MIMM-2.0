import { Page, APIRequestContext, expect } from '@playwright/test';

export const FRONTEND_URL = process.env.FRONTEND_URL || 'http://localhost:5000';
export const BACKEND_URL = process.env.BACKEND_URL || 'http://localhost:5001';
export const USER_EMAIL = process.env.TEST_EMAIL || 'e2e-auto@example.com';
export const USER_PASSWORD = process.env.TEST_PASSWORD || 'Test123!';

export async function loginViaUI(page: Page) {
  await page.goto(`${FRONTEND_URL}/login`);
  await page.getByLabel('Email').fill(USER_EMAIL);
  await page.getByLabel('Password').fill(USER_PASSWORD);
  await page.getByRole('button', { name: /sign in/i }).click();
  await page.waitForURL(/.*\/dashboard$/);
}

export async function loginAndGetToken(request: APIRequestContext) {
  const res = await request.post(`${BACKEND_URL}/api/auth/login`, {
    data: { email: USER_EMAIL, password: USER_PASSWORD },
  });
  expect(res.ok()).toBeTruthy();
  const json = await res.json();
  return json.accessToken as string;
}

export async function createEntryViaAPI(request: APIRequestContext, accessToken: string, overrides?: Partial<Record<string, unknown>>) {
  const data = {
    songTitle: 'E2E UI Song',
    artistName: 'E2E UI Artist',
    albumName: 'E2E UI Album',
    valence: 0.2,
    arousal: 0.1,
    tensionLevel: 30,
    somaticTags: ['Relaxed'],
    notes: 'UI test created',
    ...overrides,
  };
  const res = await request.post(`${BACKEND_URL}/api/entries`, {
    headers: { Authorization: `Bearer ${accessToken}` },
    data,
  });
  expect(res.ok()).toBeTruthy();
  return res.json();
}
