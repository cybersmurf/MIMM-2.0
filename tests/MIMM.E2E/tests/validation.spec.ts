import { test, expect } from '@playwright/test';
import { loginViaUI } from './utils';

test.describe('Form Validation & Error Cases', () => {
  test('create dialog: missing song title shows error', async ({ page }) => {
    await loginViaUI(page);

    // Open create dialog
    await page.getByRole('button', { name: /new entry/i }).click();

    // Leave title empty, trigger validation by blurring the field
    const dialog = page.getByRole('dialog');
    const titleInput = dialog.getByLabel(/song title/i, { exact: false });
    await titleInput.fill('');
    await titleInput.blur();

    // Expect validation error
    await expect(dialog.getByText(/song title is required/i)).toBeVisible();
  });

  test('login: invalid credentials shows error', async ({ page }) => {
    // Navigate to login and attempt invalid login
    await page.goto('/login');
    await page.getByLabel('Email').fill('nonexistent@example.com');
    await page.getByLabel('Password').fill('WrongPass123!');
    await page.getByRole('button', { name: /sign in/i }).click();

    // Snackbar or validation message
    await expect(
      page.getByText(/invalid email or password|an error occurred|failed/i)
    ).toBeVisible();
  });

  test('register: mismatched passwords shows error', async ({ page }) => {
    await page.goto('/login');

    // Switch to Sign Up mode
    await page.getByText(/sign up/i).click();

    await page.getByLabel(/display name/i).fill('E2E New User');
    await page.getByLabel('Email').fill('e2e-new@example.com');
    await page.getByLabel('Password', { exact: true }).fill('Password123!');
    await page.getByLabel(/confirm password/i).fill('Different123!');

    // Submit
    await page.getByRole('button', { name: /create account/i }).click();

    // Expect error message
    await expect(page.getByText(/passwords do not match/i)).toBeVisible();
  });
});
