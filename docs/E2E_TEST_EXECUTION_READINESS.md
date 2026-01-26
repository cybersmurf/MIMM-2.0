# E2E Test Execution Report – Playwright Suite Analysis

**Date:** January 26, 2026  
**Framework:** Playwright (TypeScript)  
**Test Files:** 6  
**Total Lines:** ~313  
**Status:** Ready for Execution  

---

## 📋 Test Suite Overview

### Files & Coverage

| File | Lines | Test Cases | Focus |
|------|-------|-----------|-------|
| [auth-and-entries.spec.ts](./tests/auth-and-entries.spec.ts) | 54 | 1 | Login → Dashboard → Create entry via API → List verification |
| [entries-ui.spec.ts](./tests/entries-ui.spec.ts) | 90 | 3 | Create/Edit/Delete via dialogs, snackbar validation |
| [mood-and-music.spec.ts](./tests/mood-and-music.spec.ts) | 80 | 2 | Mood drag interaction, music search autocomplete |
| [pagination.spec.ts](./tests/pagination.spec.ts) | 28 | 1 | Pagination controls, 10-item page limits |
| [validation.spec.ts](./tests/validation.spec.ts) | 52 | 3 | Form validation, invalid credentials, password mismatch |
| [utils.ts](./tests/utils.ts) | 44 | — | Helper functions (loginViaUI, loginAndGetToken, createEntryViaAPI) |
| **TOTAL** | **313** | **~10** | End-to-end user flows |

---

## 🎯 Test Scenarios

### ✅ Auth & Data Flow
1. **Login → Dashboard → Create → List**
   - User logs in via UI
   - Navigates to dashboard
   - Creates entry via API
   - Verifies entry appears in list
   - **Validates:** Auth flow, API integration, real-time updates

### ✅ UI Interactions (CRUD)
2. **Create via Dialog**
   - Opens create dialog
   - Fills form (title, artist, album)
   - Submits and verifies snackbar
   - **Validates:** Dialog UX, form inputs, success feedback

3. **Edit via API**
   - Creates entry if needed
   - Updates via API with new artist name
   - Reloads page and verifies changes
   - **Validates:** Edit endpoint, UI refresh

4. **Delete via API**
   - Creates entry if needed
   - Deletes via API
   - Verifies removal from list
   - **Validates:** Delete endpoint, list synchronization

### ✅ Mood & Music Features
5. **Mood Drag Interaction**
   - Opens dialog with MoodSelector2D
   - Drags to position (~272px, ~128px)
   - Verifies mood label updates dynamically
   - **Validates:** Interactive mood selector, real-time feedback

6. **Music Search Autocomplete**
   - Intercepts music search API (mocks response)
   - Types "beatles" in search box
   - Verifies autocomplete list appears
   - **Validates:** Search debounce, autocomplete UX

### ✅ Data Grid & Pagination
7. **Pagination Controls**
   - Seeds 15 entries via API
   - Verifies pagination visible
   - Navigates between pages
   - Confirms max 10 items per page
   - **Validates:** MudDataGrid pagination, page math

### ✅ Form Validation & Error Handling
8. **Missing Required Field**
   - Opens create dialog
   - Clears song title
   - Triggers validation
   - Expects error message
   - **Validates:** Client-side validation, error UX

9. **Invalid Login Credentials**
   - Attempts login with wrong password
   - Expects error snackbar or message
   - **Validates:** Auth error handling, user feedback

10. **Password Mismatch on Register**
    - Switches to Sign Up mode
    - Fills name but mismatches passwords
    - Expects validation error
    - **Validates:** Registration validation

---

## 🛠️ Test Infrastructure

### Configuration
**File:** [playwright.config.ts](./playwright.config.ts)

```typescript
- testDir: './tests'
- timeout: 60_000 ms (per test)
- baseURL: http://localhost:5000 (or FRONTEND_URL env var)
- headless: true (or true for CI)
- Reporters: list, HTML report, JSON report
- Trace: on-first-retry (captures video/trace on failure)
```

### Environment Variables
```bash
FRONTEND_URL        # Default: http://localhost:5000
BACKEND_URL         # Default: http://localhost:5001
TEST_EMAIL          # Default: e2e-auto@example.com
TEST_PASSWORD       # Default: Test123!
```

### Helper Functions (utils.ts)
- `loginViaUI(page)` – UI-based login
- `loginAndGetToken(request)` – API-based login + token retrieval
- `createEntryViaAPI(request, token, overrides)` – Create entry via API

---

## 🚀 Execution Prerequisites

### Backend Running
- MIMM.Backend API on `http://localhost:5001`
- Swagger endpoint: `http://localhost:5001/swagger`
- Required endpoints:
  - `POST /api/auth/login` – Authentication
  - `GET /api/entries` – Fetch entries
  - `POST /api/entries` – Create entry
  - `PUT /api/entries/{id}` – Update entry
  - `DELETE /api/entries/{id}` – Delete entry

### Frontend Running
- MIMM.Frontend (Blazor WASM) on `http://localhost:5000` (or configured port)
- Pages required:
  - `/login` – Login/Register page
  - `/dashboard` – Main dashboard
  - Dialog components must be functional

### Test Data
- Test user should exist or auto-register via `/api/auth/register`
- Database should be resettable or support cleanup

---

## 📊 Expected Results (Baseline)

| Test | Expected | Type | Risk |
|------|----------|------|------|
| Auth & Entries | ✅ PASS | Critical | Low |
| Create Dialog | ✅ PASS | Important | Low |
| Edit via API | ✅ PASS | Important | Low |
| Delete via API | ✅ PASS | Important | Low |
| Mood Drag | ✅ PASS | Feature | Low |
| Music Search | ✅ PASS | Feature | Medium (debounce timing) |
| Pagination | ✅ PASS | Important | Low |
| Form Validation | ✅ PASS | Critical | Low |
| Invalid Login | ✅ PASS | Critical | Low |
| Password Mismatch | ✅ PASS | Important | Low |

---

## 🎬 Execution Steps

### 1. Install & Setup
```bash
cd tests/MIMM.E2E
npm install                    # Install Playwright + browsers
```

### 2. Start Services
```bash
# Terminal 1: Backend
cd src/MIMM.Backend
dotnet run --project MIMM.Backend.csproj

# Terminal 2: Frontend
cd src/MIMM.Frontend
dotnet run --project MIMM.Frontend.csproj

# Terminal 3: E2E Tests
cd tests/MIMM.E2E
npm test                       # Or: npx playwright test
```

### 3. Run Full Suite
```bash
npm test                       # Run all tests in headless mode
npm test -- --headed           # Run with browser visible
npm test -- --debug            # Debug mode with inspector
npm test -- --ui               # Interactive UI mode
```

### 4. Run Specific Test
```bash
npm test -- auth-and-entries   # Run single file
npm test -- --grep "mood"      # Run tests matching pattern
```

### 5. View Reports
```bash
npx playwright show-report     # Open HTML report
```

---

## ✅ Quality Metrics

### Test Characteristics
- **Test Isolation:** Each test logs in independently (no shared state)
- **API Preference:** CRUD operations use API for stability, UI used for UX flows
- **Error Handling:** Tests expect specific error messages (case-insensitive where needed)
- **Timeouts:** 60s per test, 5s for dialogs/snackbars
- **Mocking:** Music search is mocked to avoid external API dependencies

### Coverage
- **Authentication:** Login, register, invalid credentials
- **CRUD:** Create, read (list), update, delete
- **UI Interactions:** Dialog forms, drag interactions, autocomplete
- **Data Grid:** Pagination, item display
- **Validation:** Client-side form validation, error messages
- **Feedback:** Snackbars, status messages

### Not Covered (Acceptable)
- Real Last.fm API integration (scrobbling)
- Spotify connection flow
- Export/Import CSV
- Friend request flows
- Analytics dashboard charts (unit-tested separately)

---

## 🐛 Known Issues & Workarounds

### Issue: baseURL Default
- **Problem:** baseURL defaults to `http://localhost:5000` but Blazor often runs on `5200+`
- **Solution:** Set `FRONTEND_URL=http://localhost:5200` env var before running

### Issue: Mood Selector Click Position
- **Problem:** 320px default size; coordinates must match CSS size
- **Workaround:** Test uses hardcoded position; adjust if size changes

### Issue: Snackbar Timing
- **Problem:** Snackbars disappear quickly
- **Solution:** Tests use `.toBeVisible({ timeout: 5000 })` for reliability

### Issue: Dialog Name Collisions
- **Problem:** Multiple dialogs with similar names
- **Solution:** Tests use `getByRole('dialog', { name: /.../ })` for specificity

---

## 📈 Success Criteria

✅ **All tests pass** (10/10 test cases)
✅ **No timeouts** (< 60s per test)
✅ **Clear error messages** (if failures occur)
✅ **HTML report generated** (playwright-report/index.html)
✅ **Trace collected on failure** (for debugging)

---

## 🔗 Key Resources

- [Playwright Docs](https://playwright.dev)
- [Config File](./playwright.config.ts)
- [Utils Helper Functions](./tests/utils.ts)
- [Backend API](http://localhost:5001/swagger)

---

## 📝 Conclusion

The Playwright E2E test suite is **fully implemented and ready for execution**. All ~10 critical user flows are covered:
- ✅ Authentication
- ✅ Entry CRUD operations
- ✅ UI interactions (dialogs, forms, pagination)
- ✅ Mood & music features
- ✅ Form validation & error handling

**Next Steps:**
1. Start backend API (`dotnet run` in MIMM.Backend)
2. Start frontend (`dotnet run` in MIMM.Frontend)
3. Run tests: `npm test` in tests/MIMM.E2E
4. Review HTML report in `playwright-report/`

**Expected outcome:** All tests pass, providing confidence in frontend-backend integration and user-facing functionality.

