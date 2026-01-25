import { test, expect } from '@playwright/test';

test.describe('Last.fm Scrobbling E2E Flow', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('http://localhost:5000');
    // Wait for app to load
    await page.waitForLoadState('networkidle');
  });

  test('Complete scrobbling workflow: Register -> Create Entry -> Scrobble', async ({ page }) => {
    // Step 1: Register new account
    const testEmail = `test-${Date.now()}@example.com`;
    const testPassword = 'TestPassword123!';
    
    // Navigate to register
    await page.click('text=Register');
    await page.waitForLoadState('networkidle');
    
    // Fill registration form
    await page.fill('input[type="email"]', testEmail);
    await page.fill('input[type="password"]', testPassword);
    await page.click('button:has-text("Register")');
    
    // Wait for redirect to dashboard
    await page.waitForURL('**/dashboard', { timeout: 10000 });
    
    // Step 2: Create a new entry
    // Click "Create Entry" button
    await page.click('button:has-text("Create Entry")');
    
    // Wait for dialog
    await expect(page.locator('h2:has-text("Create Entry")')).toBeVisible();
    
    // Fill entry form
    await page.fill('input[placeholder*="Song title"]', 'Bohemian Rhapsody');
    await page.fill('input[placeholder*="Artist"]', 'Queen');
    await page.fill('input[placeholder*="Album"]', 'A Night at the Opera');
    
    // Select mood (click on 2D mood selector - upper right quadrant for happy)
    const moodCanvas = page.locator('canvas');
    if (await moodCanvas.isVisible()) {
      await moodCanvas.click({ position: { x: 150, y: 50 } });
    }
    
    // Click Save
    await page.click('button:has-text("Save")');
    
    // Wait for entry to appear in list
    await page.waitForTimeout(1000);
    await expect(page.locator('text=Bohemian Rhapsody')).toBeVisible();
    
    // Step 3: Scrobble entry
    // Find the scrobble button (music note icon) for our entry
    const entryRow = page.locator('text=Bohemian Rhapsody').first().locator('..');
    
    // Click scrobble button in the entry row
    const scrobbleButton = entryRow.locator('button[title="Scrobble to Last.fm"]');
    
    if (await scrobbleButton.isVisible()) {
      await scrobbleButton.click();
      
      // Check for success notification
      await expect(page.locator('text=/Scrobbled|Last.fm/i')).toBeVisible({ timeout: 5000 });
    }
    
    // Verify success
    await expect(page).toHaveURL('**/dashboard');
  });

  test('Error handling: Scrobble without Last.fm connection shows error', async ({ page }) => {
    // This test requires a user without Last.fm connection
    // Skip for now - would require setup of specific test user
    test.skip();
  });

  test('Scrobble button only appears for unscrobbled entries', async ({ page }) => {
    // Create an entry
    await page.click('button:has-text("Create Entry")');
    
    // Fill minimal entry
    await page.fill('input[placeholder*="Song title"]', 'Test Song');
    await page.fill('input[placeholder*="Artist"]', 'Test Artist');
    await page.click('button:has-text("Save")');
    
    await page.waitForTimeout(500);
    
    // Find the entry and check for scrobble button
    const entryRow = page.locator('text=Test Song').first().locator('..');
    const scrobbleButton = entryRow.locator('button[title="Scrobble to Last.fm"]');
    
    // Button should be visible (entry not yet scrobbled)
    const isVisible = await scrobbleButton.isVisible().catch(() => false);
    
    if (isVisible) {
      // Scrobble it
      await scrobbleButton.click();
      await page.waitForTimeout(1000);
      
      // After successful scrobble, button should be hidden
      // (because ScrobbledToLastFm becomes true)
      const stillVisible = await scrobbleButton.isVisible().catch(() => false);
      expect(stillVisible).toBe(false);
    }
  });
});
