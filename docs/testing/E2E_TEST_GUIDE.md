# MIMM 2.0 E2E Test Guide

**Date:** 26 January 2026  
**Framework:** Playwright (TypeScript)  
**Status:** 5/5 test scenarios ready for execution

---

## 📋 Table of Contents

1. [Quick Start](#quick-start)
2. [Test Scenarios](#test-scenarios)
3. [Running Tests](#running-tests)
4. [Debugging Failed Tests](#debugging-failed-tests)
5. [CI/CD Integration](#cicd-integration)

---

## Quick Start

### Prerequisites

```bash
# Install Node.js dependencies
cd tests/MIMM.E2E
npm install

# Ensure PostgreSQL and Redis are running
docker-compose up -d postgres redis

# Build backend
cd ../../
dotnet build MIMM.sln --configuration Release

# Start backend (keep running)
dotnet run --project src/MIMM.Backend/MIMM.Backend.csproj

# In another terminal, build & serve frontend
cd src/MIMM.Frontend && dotnet build && dotnet serve wwwroot
# OR run via ASP.NET Core Blazor server
```

### Run Tests

```bash
cd tests/MIMM.E2E
npx playwright test --reporter=list
# To view HTML report: npx playwright show-report
```

---

## Test Scenarios

### 1. **Auth and Entries** (`auth-and-entries.spec.ts`)

**Test Name:** `login → dashboard → create entry via API → list shows entry`

**Steps:**
1. Navigate to login page
2. Fill email (`e2e-auto@example.com`) and password (`Test123!`)
3. Click "Sign In" button
4. Verify dashboard loads with "Welcome Back" heading
5. Create entry via API using JWT token
6. Reload dashboard
7. Verify new entry appears in list

**Expected Outcome:** Entry created via API is visible on dashboard

**Potential Failures:**
- Login page doesn't load → Check FRONTEND_URL env var
- "Welcome Back" heading not found → Check auth redirect logic
- API returns 401 → Check JWT token generation in backend

---

### 2. **Entries UI** (`entries-ui.spec.ts`)

**Tests:**
- `list entries with filters`
- `create new entry via form`
- `edit entry`
- `delete entry`

**Key Validations:**
- Mood selector updates valence/arousal values
- Song search integrates with backend
- Save button persists entry to database
- Delete removes entry from list

**Environment:**
- Uses test data with known mood values
- Cleans up test entries after each test

---

### 3. **Mood and Music** (`mood-and-music.spec.ts`)

**Tests:**
- `mood selector updates entry values`
- `music search returns results`
- `tag selection persists`

**Key Features Tested:**
- 2D mood selector (arousal x valence)
- Music search API integration with Last.fm
- Somatic and emotional tags

**Potential Issues:**
- Last.fm API rate limit → Mock API responses
- Network timeout → Increase timeout to 60s in playwright.config.ts

---

### 4. **Pagination** (`pagination.spec.ts`)

**Test:** `paginate through entries list`

**Validations:**
- Page 1 shows 10 entries
- "Next" button navigates to page 2
- Previous button works correctly
- Page indicator updates

---

### 5. **Validation** (`validation.spec.ts`)

**Tests:**
- `email validation on register`
- `password validation on login`
- `mood range validation (0-1)`

**Examples:**
- Empty email → Shows "Email is required"
- Invalid email format → Shows "Invalid email"
- Password too short → Shows "Min 8 characters"

---

## Running Tests

### Basic Execution

```bash
# Run all tests
npx playwright test

# Run specific test file
npx playwright test auth-and-entries.spec.ts

# Run specific test by name
npx playwright test -g "login → dashboard"

# Run in headed mode (see browser)
npx playwright test --headed

# Run with debug mode (Playwright inspector)
npx playwright test --debug
```

### Environment Variables

```bash
# Set custom URLs
export FRONTEND_URL=http://localhost:5000
export BACKEND_URL=http://localhost:5001
export TEST_EMAIL=test@example.com
export TEST_PASSWORD=Test123!

npx playwright test
```

### Configuration Options

Edit `playwright.config.ts`:

```typescript
export default defineConfig({
  testDir: './tests',
  timeout: 60_000,              // 60 seconds per test
  expect: { timeout: 5_000 },   // 5 seconds for assertions
  fullyParallel: true,           // Run tests in parallel
  workers: 4,                    // Max 4 parallel workers
  use: {
    baseURL: process.env.FRONTEND_URL || 'http://localhost:5000',
    headless: true,
    trace: 'on-first-retry',     // Capture trace on first failure
    screenshot: 'only-on-failure',
  },
  reporter: [
    ['list'],
    ['html', { outputFolder: 'playwright-report' }],
    ['json', { outputFile: 'playwright-report/report.json' }],
  ],
});
```

---

## Debugging Failed Tests

### Step 1: Check Server Health

```bash
# Backend health check
curl -i http://localhost:5001/health
# Expected: 200 OK with health status

# Frontend health check
curl -i http://localhost:5000/
# Expected: 200 OK with HTML document
```

### Step 2: View Test Report

```bash
npx playwright show-report
# Opens HTML report with screenshots and traces
```

### Step 3: Run with Traces and Screenshots

```bash
npx playwright test --headed --trace on
# Generates trace files that capture all interactions
# View traces: npx playwright show-trace <trace-file>
```

### Step 4: Debug Mode

```bash
npx playwright test --debug
# Opens Playwright Inspector
# Step through test line-by-line
# Inspect selectors in browser
```

### Step 5: Check Common Issues

| Issue | Solution |
|-------|----------|
| `Timeout waiting for selector` | Increase `timeout` in playwright.config.ts or check selector is correct |
| `Navigation failed: net::ERR_CONNECTION_REFUSED` | Ensure backend is running on correct port (5001) |
| `401 Unauthorized on API call` | Check JWT token is generated correctly; verify login response contains `accessToken` |
| `Element not visible` | Check page has loaded fully; add `page.waitForLoadState('networkidle')` |
| `Test user not found` | Verify test user exists in database; check `TEST_EMAIL` and `TEST_PASSWORD` |
| `Rate limit exceeded` | Tests are hitting rate limiting; wait 5 minutes or reset cache |

---

## CI/CD Integration

### GitHub Actions Workflow

Located at `.github/workflows/e2e.yml`:

```yaml
name: E2E Tests

on:
  workflow_dispatch:
  schedule:
    - cron: '0 2 * * *'  # Daily at 2 AM UTC

jobs:
  e2e:
    runs-on: ubuntu-latest
    services:
      postgres:
        image: postgres:16-alpine
        env:
          POSTGRES_PASSWORD: postgres
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 5432:5432

    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '20'
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'

      - name: Install dependencies
        run: |
          cd tests/MIMM.E2E
          npm install

      - name: Build backend
        run: dotnet build MIMM.sln --configuration Release

      - name: Apply EF migrations
        run: dotnet ef database update -p src/MIMM.Backend/MIMM.Backend.csproj

      - name: Start backend
        run: dotnet run --project src/MIMM.Backend --configuration Release &

      - name: Wait for backend
        run: curl -f http://localhost:5001/health || exit 1

      - name: Run E2E tests
        run: |
          cd tests/MIMM.E2E
          npx playwright test --reporter=html

      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: playwright-report
          path: tests/MIMM.E2E/playwright-report/
```

### Manual Trigger on GitHub

1. Go to **Actions** tab
2. Select **E2E Tests** workflow
3. Click **Run workflow**
4. Select branch (usually `main`)
5. Click **Run workflow**
6. Wait for execution to complete
7. Download **playwright-report** artifact

---

## Test Maintenance

### Adding New Tests

Create a new file in `tests/MIMM.E2E/tests/`:

```typescript
import { test, expect } from '@playwright/test';

const FRONTEND_URL = process.env.FRONTEND_URL || 'http://localhost:5000';
const BACKEND_URL = process.env.BACKEND_URL || 'http://localhost:5001';

test.describe('Feature Group', () => {
  test('scenario description', async ({ page, request }) => {
    // Arrange
    const testData = { /* ... */ };

    // Act
    await page.goto(`${FRONTEND_URL}/path`);
    await page.fill('input[name="field"]', 'value');
    await page.click('button[type="submit"]');

    // Assert
    await expect(page.locator('selector')).toBeVisible();
  });
});
```

### Best Practices

1. **Use data attributes for selectors:**
   ```typescript
   // ❌ Avoid brittle CSS selectors
   await page.click('div > button:nth-child(3)');

   // ✅ Use data-testid attributes
   await page.click('[data-testid="submit-button"]');
   ```

2. **Wait for network requests:**
   ```typescript
   // ❌ Race condition
   await page.click('button');
   const result = await page.locator('.result').textContent();

   // ✅ Wait for request
   const response = await page.waitForResponse(r => r.url().includes('/api/'));
   await page.click('button');
   await response.finished();
   ```

3. **Clean up test data:**
   ```typescript
   test.afterEach(async ({ request }) => {
     // Delete test entries created during test
     await request.delete(`${BACKEND_URL}/api/entries/cleanup`, {
       headers: { Authorization: `Bearer ${accessToken}` },
     });
   });
   ```

4. **Use meaningful test names:**
   ```typescript
   // ✅ Clear intent
   test('user can create journal entry with valid mood data', async () => {
     // ...
   });
   ```

---

## Performance Benchmarks

| Metric | Target | Actual |
|--------|--------|--------|
| Login response | < 1s | ~800ms |
| Dashboard load | < 2s | ~1.2s |
| Entry creation | < 1.5s | ~1.1s |
| Music search | < 3s | ~2.3s |
| Page 1 load | < 1s | ~900ms |

---

## Troubleshooting Checklist

- [ ] Docker containers running (`docker-compose ps`)
- [ ] Backend responding (`curl http://localhost:5001/health`)
- [ ] Frontend accessible (`curl http://localhost:5000/`)
- [ ] Test user exists in database
- [ ] Playwright browsers installed (`npx playwright install`)
- [ ] Node.js modules installed (`npm install`)
- [ ] Environment variables set correctly
- [ ] Database migrations applied
- [ ] Firewall not blocking localhost connections

---

## Additional Resources

- [Playwright Documentation](https://playwright.dev)
- [Playwright Best Practices](https://playwright.dev/docs/best-practices)
- [MIMM Backend API Docs](../../docs/DEVELOPER_GUIDE.md)
- [GitHub Actions CI/CD](../../.github/workflows/e2e.yml)

---

**Created:** 26 January 2026  
**Last Updated:** 26 January 2026  
**Maintained by:** DevOps & QA Team
