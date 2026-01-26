# MIMM 2.0 – E2E Testing Guide

**Version:** 1.0  
**Status:** Ready for Testing  
**Target:** Phase 3.5 Accessibility + Core Features Validation  
**Date:** January 26, 2026

---

## Overview

This guide provides step-by-step instructions for testing MIMM 2.0 end-to-end:
- **Authentication flow** (Login/Register)
- **Core features** (mood tracking, music search, scrobbling)
- **Accessibility** (keyboard navigation, screen readers, ARIA)
- **Mobile/responsive behavior**

**Testing Environment:**
- Windows: NVDA or JAWS + Chrome/Edge
- macOS: VoiceOver + Safari
- iOS: VoiceOver + Safari
- Android: TalkBack + Chrome

---

## Part I: Pre-Testing Setup

### 1.1 Environment Requirements

```bash
# Terminal 1: Start PostgreSQL + Redis
docker-compose up -d postgres redis

# Terminal 2: Start Backend API
cd src/MIMM.Backend
dotnet run

# Terminal 3: Start Frontend (Blazor WASM)
cd src/MIMM.Frontend
dotnet run
```

**Endpoints:**
- Frontend: `http://localhost:5173` (dev) or `https://localhost:7000` (prod)
- Backend API: `https://localhost:7001`
- PostgreSQL: `localhost:5432`
- Redis: `localhost:6379`

### 1.2 Test Database Reset

```bash
# Reset database to clean state
dotnet ef database drop -p src/MIMM.Backend --force
dotnet ef database update -p src/MIMM.Backend

# Seed test data (if implemented)
# TODO: Add seed data script
```

### 1.3 Browser DevTools Setup

- Open **DevTools** (F12)
- Enable **Lighthouse** for accessibility audit
- Open **Console** to catch any JS errors
- Enable **Network** tab to monitor API calls

---

## Part II: Authentication Testing

### Test 2.1: User Registration

**Scenario:** New user can register successfully

| Step | Action | Expected Result |
|------|--------|---|
| 1 | Open app, click "Create Account" | Registration form appears |
| 2 | Enter email: `test.user@example.com` | Field accepts valid email |
| 3 | Enter password: `SecurePass123!` | Password field masked |
| 4 | Enter confirm password: `SecurePass123!` | Matches password field |
| 5 | Click "Register" | Loading spinner appears |
| 6 | Wait for response | Success message in live region: "Account created successfully!" |
| 7 | Redirect to dashboard | User logged in, sidebar shows username |

**Screen Reader Test:** NVDA announces: "Heading level 1: Create Account", "Email, required, text input", success message.

---

### Test 2.2: User Login

**Scenario:** Existing user can log in

| Step | Action | Expected Result |
|------|--------|---|
| 1 | Open app | Login form visible |
| 2 | Enter email: `test.user@example.com` | Field accepts email |
| 3 | Enter password: `SecurePass123!` | Field masked |
| 4 | Click "Login" | Loading spinner, live region: "Logging in..." |
| 5 | Wait for response | Redirect to Dashboard, live region: "Welcome back!" |
| 6 | Verify sidebar | User icon + logout button visible |

**Keyboard Test:** Tab through form → Email field → Password field → Login button → all reachable.

**Accessibility Check:**
- [ ] Live region announces login success
- [ ] Focus moves to dashboard after login
- [ ] "Welcome back!" message is audible in screen reader

---

### Test 2.3: Form Validation

**Scenario:** Form shows validation errors for invalid input

| Step | Action | Expected Result |
|------|--------|---|
| 1 | Enter invalid email: `notanemail` | Error displayed: "Invalid email format" |
| 2 | Enter password: `short` | Error: "Password must be at least 8 characters" |
| 3 | Enter different confirm password | Error: "Passwords do not match" |
| 4 | Tab to "Login" button | Button disabled (red outline) |
| 5 | Click "Login" | Form submission blocked |

**Screen Reader Test:** Validation summary announced via assertive live region (priority).

---

## Part III: Core Features Testing

### Test 3.1: Dashboard Load & Live Region

**Scenario:** Dashboard loads with mood data, announcements work

| Step | Action | Expected Result |
|------|--------|---|
| 1 | Login successfully | Redirected to Dashboard |
| 2 | Monitor live region | Announces: "Loading dashboard statistics..." |
| 3 | Wait 1-2 seconds | Live region: "Dashboard loaded. [X] mood entries found." |
| 4 | Verify stats display | Cards show: "Total entries", "This week", "Current mood" |
| 5 | Check theme | Light/dark mode applied per settings |

**Accessibility Check:**
- [ ] Announcement appears before visual content (no duplicate announcements)
- [ ] Focus doesn't jump (live region is polite, not assertive)
- [ ] Stats cards have proper contrast (13.5:1 or better)

---

### Test 3.2: Create New Mood Entry

**Scenario:** User can create and save mood entry

| Step | Action | Expected Result |
|------|--------|---|
| 1 | Click "New Entry" button | Dialog opens with modal backdrop |
| 2 | Tab through form | Focus cycles: Song input → Artist → Mood selector → Buttons |
| 3 | Enter song: `Bohemian Rhapsody` | Autocomplete suggests songs |
| 4 | Click suggestion or enter manually | Song populated in field |
| 5 | Adjust mood selector 2D | Click/drag to position (or arrow keys) |
| 6 | Click "Save" | Dialog closes, entry added to list |
| 7 | Check live region in EntryList | Announces: "Entry created. [Song name] saved." |

**Focus Trap Test:**
- [ ] Tab at bottom of dialog → focus returns to first button (focus trap active)
- [ ] Escape key → dialog closes, focus returns to "New Entry" button

**Screen Reader Test:** NVDA announces: "Dialog: Create new entry", reads all labels, announces save success.

---

### Test 3.3: Music Search

**Scenario:** User can search for music with live feedback

| Step | Action | Expected Result |
|------|--------|---|
| 1 | Focus music search box | Field is focused, visible outline |
| 2 | Type: `adele` | Spinner appears (visual feedback) |
| 3 | Wait for results | MudProgressLinear shows, then disappears |
| 4 | Results display | List of songs by Adele shown |
| 5 | Click result | Song selected, dialog closes |
| 6 | Verify EntryList | New entry appears at top |

**Accessibility Check:**
- [ ] Spinner visible for >500ms search (user knows it's loading)
- [ ] Results announced by screen reader (role="list" with items)
- [ ] Keyboard: Arrow keys navigate results, Enter selects

---

### Test 3.4: Last.fm Scrobbling

**Scenario:** User can scrobble entry to Last.fm

| Precondition | Last.fm account linked via OAuth |
|---|---|

| Step | Action | Expected Result |
|------|--------|---|
| 1 | Open mood entry | Edit dialog shows entry details |
| 2 | Check scrobble status | "Scrobble to Last.fm" button visible |
| 3 | Click scrobble button | Loading spinner, API call made |
| 4 | Wait for response | Success message: "Scrobbled [song] to Last.fm" |
| 5 | Check Last.fm | Track appears in recent scrobbles |

**Error Handling:**
- If Last.fm token expired → displays "Reconnect Last.fm" button
- If network error → Snackbar shows error message

---

### Test 3.5: Analytics Page

**Scenario:** Analytics dashboard loads and displays trends

| Step | Action | Expected Result |
|------|--------|---|
| 1 | Click "Analytics" in sidebar | Page loads, live region: "Loading analytics data..." |
| 2 | Wait for charts | Charts populate with mood trends |
| 3 | Live region announces | "Analytics loaded successfully. 5 trends displayed." |
| 4 | Inspect charts | Can read data in text form (alt text or summary) |

**Mobile Test:**
- [ ] Charts responsive on mobile (320px width)
- [ ] Legend doesn't overlap with chart
- [ ] Touch targets for chart interactions ≥44px

---

## Part IV: Accessibility Testing

### Test 4.1: Keyboard Navigation (Tab Order)

**Procedure:** Use **Tab** key only, no mouse.

| Page | Expected Tab Order |
|---|---|
| **Login** | Email → Password → Login button → "Create Account" link |
| **Dashboard** | Skip link (visible on focus) → AppBar items → Drawer toggle → Sidebar links → Main content cards → Entry list |
| **EntryList** | Entry cards → Music search box → Create button → Dialog (if open) |
| **Analytics** | Chart controls → Legend items |

**Verification Steps:**
```
1. Press Tab repeatedly from page top
2. Verify focus visible (outline, highlight, or color change)
3. Verify logical order (not jumping randomly)
4. Test Shift+Tab to go backwards
5. Verify skip link works (press Tab once → "Skip to main content" visible)
```

---

### Test 4.2: Skip Navigation

**Scenario:** Skip link allows quick navigation to main content

| Step | Action | Expected Result |
|------|--------|---|
| 1 | Open any page | Focus on first interactive element (AppBar) |
| 2 | Press Tab once | "Skip to main content" link becomes visible |
| 3 | Press Enter | Focus jumps to `id="main-content"` (main region) |
| 4 | Verify jump | Focus outline visible on main content area |

**Visual Check:**
- [ ] Skip link has high contrast (visible when focused)
- [ ] Hidden by default (not taking up space)
- [ ] Appears above all other content when focused (z-index: high)

---

### Test 4.3: Screen Reader Navigation (NVDA / JAWS)

**Tool:** NVDA (free, Windows) or JAWS (paid, Windows/macOS)

### Step-by-Step (NVDA Windows)

**Setup:**
1. Download NVDA: [www.nvaccess.org](https://www.nvaccess.org)
2. Install and run: `nvda.exe`
3. Open MIMM app in Chrome/Edge
4. Start NVDA (Insert+N or click app icon)

**Testing Landmarks:**

```
Action: Press R (browse mode - NVDA)
Result: "Navigation region" → main content region listed

Action: Press D (browse mode - NVDA)
Result: Lists all landmarks on page:
  - Banner (AppBar)
  - Navigation (Sidebar)
  - Main (Content area)
```

**Testing Form Labels:**

```
Action: Navigate to Login form
NVDA announces: "Heading level 1: Login"
Tab to Email field
NVDA announces: "Email, required, text input, blank"

Tab to Login button
NVDA announces: "Login, button"
```

**Testing Live Regions (Dynamic Content):**

```
Action: Click "New Entry"
NVDA announces: "Dialog: Create new entry"

Action: Wait for modal to appear
NVDA announces: "Window: Create new entry" (via aria-modal)

Action: Click Save
NVDA announces (after state change): "Entry created successfully" (via live region)
```

**Testing Focus Trap (Dialog):**

```
Action: Press Tab repeatedly in open dialog
Expected: Focus cycles through:
  - Cancel button
  - Save button
  - Cancel button (loops back)

Action: Press Escape
Expected: Dialog closes, focus returns to "New Entry" button
NVDA announces: "Dialog closed"
```

---

### Test 4.4: Screen Reader Navigation (macOS VoiceOver)

**Tool:** Built-in VoiceOver (free, included with macOS)

**Setup:**
1. System Preferences → Accessibility → VoiceOver → Enable
2. Open Safari (best VoiceOver support)
3. Navigate to MIMM app

**Testing Navigation:**

```
Action: Press VO+U (rotor - VoiceOver)
Opens rotor with options:
  - Headings
  - Landmarks
  - Links
  - Form controls

Select "Landmarks" → VoiceOver lists all page regions
```

**Testing Form Labels:**

```
Action: Navigate to Login form
VO announces: "Login section, group"
VO+Right arrow to first input
VO announces: "Email text field, required, double tap to edit"
```

---

### Test 4.5: Mobile Screen Reader (iOS VoiceOver)

**Tool:** Built-in VoiceOver (included with iOS)

**Setup:**
1. Settings → Accessibility → VoiceOver → On
2. Open Safari
3. Navigate to MIMM app URL

**Testing Touch Navigation:**

```
Action: Swipe right
Expected: Focus moves to next element, announces: "Button: New Entry"

Action: Swipe right again
Expected: Focus to next element, announces: "Music search box text field"

Action: Swipe left
Expected: Focus moves backwards
```

**Testing Double-Tap:**

```
Action: Focus on "New Entry" button, double-tap
Expected: Dialog opens, focus on first input

Action: Fill form and double-tap Save button
Expected: Entry saved, live region announces success
```

---

### Test 4.6: High Contrast Mode

**Windows High Contrast:**

1. Settings → Ease of Access → High Contrast → Turn on
2. Choose: "High Contrast White" or "High Contrast Black"
3. Reload MIMM app

**Verification:**
- [ ] All text readable (no light text on light bg)
- [ ] Focus indicators visible (yellow outline, high contrast)
- [ ] Buttons distinguishable (not just color-coded)
- [ ] No information lost due to color change

**macOS Increase Contrast:**

1. System Preferences → Accessibility → Display → Increase Contrast → On
2. Reload app

**Verification:**
- [ ] Text contrast ≥ 7:1 (AAA standard)
- [ ] No "color only" communication (e.g., "Green = success" without text)

---

## Part V: Responsive & Mobile Testing

### Test 5.1: Viewport Sizes

| Viewport | Device | Testing |
|---|---|---|
| 320×568 | iPhone SE | Portrait mode, buttons ≥44px, no horizontal scroll |
| 375×667 | iPhone 11/12 | Standard phone, all interactions reachable |
| 768×1024 | iPad | Tablet, sidebar may collapse, layout adjusts |
| 1024×1366 | iPad Pro | Large tablet, sidebar visible, content centered |
| 1920×1080 | Desktop | Full screen, no UI issues, comfortable spacing |

**Test Procedure:**
```
1. Open DevTools (F12)
2. Click Device Emulation toggle (Ctrl+Shift+M)
3. Select each viewport
4. Reload page
5. Verify layout:
   - No horizontal scroll
   - Touch targets ≥44px
   - Text readable (no font size too small)
   - Buttons clickable (thumb-friendly)
```

---

### Test 5.2: Orientation Change

**Scenario:** User rotates device, layout adapts

| Action | Expected Result |
|---|---|
| Open app in Portrait | Layout optimized for tall, narrow screen |
| Rotate to Landscape | Layout reflows, sidebar may minimize, content uses full width |
| Rotate back to Portrait | Layout returns to original state, no UI glitches |

**Touch Device Emulation (Chrome):**
```
DevTools → More tools → Sensors → Orientation
Change from Portrait (0°) to Landscape (90°)
Monitor CSS media queries responding
```

---

### Test 5.3: Touch Interaction

**Test on actual mobile device or Chrome touch emulation:**

| Interaction | Expected Behavior |
|---|---|
| **Tap button** | Ripple effect, no 300ms delay, action triggers immediately |
| **Long press** | Context menu (if implemented), not drag/selection |
| **Swipe on entry list** | Horizontal swipe: delete/edit action (if implemented) |
| **Pinch to zoom** | Optional: allow zoom for readability, or prevent for app experience |
| **Tap form field** | Keyboard appears, field scrolls into view, label visible |

---

## Part VI: Performance Testing

### Test 6.1: Page Load Time

| Page | Target | Actual | Notes |
|---|---|---|---|
| Login | <1s | ? | Measure time from click to form fully rendered |
| Dashboard | <2s | ? | Includes API call to load stats |
| Analytics | <3s | ? | Includes chart rendering |
| EntryList | <1.5s | ? | Includes list load |

**Measure using Chrome DevTools:**
```
1. Open DevTools → Performance tab
2. Click record button (red circle)
3. Navigate to page
4. Stop recording
5. Review metrics:
   - FCP (First Contentful Paint)
   - LCP (Largest Contentful Paint)
   - CLS (Cumulative Layout Shift)
```

**Target Metrics (Web Vitals):**
- FCP: < 1.8s (good)
- LCP: < 2.5s (good)
- CLS: < 0.1 (good)
- FID: < 100ms (good)

---

### Test 6.2: Memory & Network

**Chrome DevTools → Network Tab:**

```
1. Open DevTools → Network tab
2. Reload page
3. Check:
   - Number of requests: < 50
   - Total size: < 5 MB (first load)
   - Cache hit rate: images/CSS should be cached
   - API response time: < 500ms per request
```

**Memory Usage (DevTools → Memory tab):**
```
1. Take heap snapshot before interaction
2. Perform 10 mood entry operations
3. Take second snapshot
4. Compare: memory growth should be < 20 MB
5. Run garbage collection (trash icon)
6. Verify memory returns to baseline
```

---

## Part VII: Error Handling & Edge Cases

### Test 7.1: Network Errors

**Scenario:** Backend API is unavailable

| Step | Action | Expected Result |
|------|--------|---|
| 1 | Stop backend server | API endpoints offline |
| 2 | Try to load Dashboard | Error message shown: "Failed to load dashboard. Please try again." |
| 3 | Check live region | Assertive announcement: "Error: Failed to load dashboard" |
| 4 | Restart backend | Retry button or automatic reconnect |
| 5 | Verify recovery | Content loads successfully |

---

### Test 7.2: Invalid/Expired Token

**Scenario:** JWT token expires mid-session

| Step | Action | Expected Result |
|------|--------|---|
| 1 | Login successfully | Session active |
| 2 | Manually delete token from localStorage | `localStorage.clear()` in console |
| 3 | Try to access protected page | Redirect to login |
| 4 | Login again | New token issued, session restored |

---

### Test 7.3: Database Connection Loss

**Scenario:** PostgreSQL goes offline

| Step | Action | Expected Result |
|------|--------|---|
| 1 | Stop PostgreSQL: `docker-compose down postgres` | Database unavailable |
| 2 | Try to load data | Backend returns 500 error |
| 3 | UI shows error: "Database connection failed. Please try again later." | User informed |
| 4 | Restart PostgreSQL | `docker-compose up -d postgres` |
| 5 | Retry operation | Data loads successfully |

---

### Test 7.4: Missing Last.fm Integration

**Scenario:** Last.fm API key not configured

| Step | Action | Expected Result |
|------|--------|---|
| 1 | Open entry details | Scrobble button hidden or disabled |
| 2 | Hover over disabled button | Tooltip: "Last.fm not configured" |
| 3 | Click settings | Link to Last.fm OAuth setup |

---

## Part VIII: Automated Testing

### Test 8.1: Unit Tests

Run backend unit tests:
```bash
dotnet test tests/MIMM.Tests.Unit/ -v minimal
```

Expected output:
```
Passed: [X] tests
Failed: 0 tests
Skipped: 0 tests
```

---

### Test 8.2: Integration Tests

Run integration tests (with test database):
```bash
dotnet test tests/MIMM.Tests.Integration/ -v minimal
```

Expected output:
```
Passed: [X] tests
Failed: 0 tests
Skipped: 0 tests
```

---

### Test 8.3: Accessibility Audit (Automated)

Run Lighthouse accessibility audit in Chrome DevTools:

```
1. Open DevTools → Lighthouse tab
2. Select "Accessibility" category
3. Click "Analyze page load"
4. Review report:
   - Score ≥ 90 (good)
   - Passing: All automated checks pass
   - Issues: Fix flagged items
```

**Known Automation Limitations:**
- Live region announcements require manual testing (screen reader)
- Focus trap requires manual keyboard testing
- Color contrast checked automatically (good tool: Axe DevTools)

---

## Part IX: Test Results Documentation

### Test Execution Checklist

**Authentication Tests:**
- [ ] Test 2.1: User Registration – PASS / FAIL
- [ ] Test 2.2: User Login – PASS / FAIL
- [ ] Test 2.3: Form Validation – PASS / FAIL

**Core Features:**
- [ ] Test 3.1: Dashboard Load – PASS / FAIL
- [ ] Test 3.2: Create Entry – PASS / FAIL
- [ ] Test 3.3: Music Search – PASS / FAIL
- [ ] Test 3.4: Last.fm Scrobbling – PASS / FAIL
- [ ] Test 3.5: Analytics Page – PASS / FAIL

**Accessibility:**
- [ ] Test 4.1: Keyboard Navigation – PASS / FAIL
- [ ] Test 4.2: Skip Navigation – PASS / FAIL
- [ ] Test 4.3: Screen Reader (NVDA) – PASS / FAIL
- [ ] Test 4.4: Screen Reader (VoiceOver) – PASS / FAIL
- [ ] Test 4.5: Mobile Screen Reader – PASS / FAIL
- [ ] Test 4.6: High Contrast Mode – PASS / FAIL

**Responsive & Mobile:**
- [ ] Test 5.1: Viewport Sizes – PASS / FAIL
- [ ] Test 5.2: Orientation Change – PASS / FAIL
- [ ] Test 5.3: Touch Interaction – PASS / FAIL

**Performance:**
- [ ] Test 6.1: Page Load Time – PASS / FAIL
- [ ] Test 6.2: Memory & Network – PASS / FAIL

**Error Handling:**
- [ ] Test 7.1: Network Errors – PASS / FAIL
- [ ] Test 7.2: Expired Token – PASS / FAIL
- [ ] Test 7.3: Database Connection Loss – PASS / FAIL
- [ ] Test 7.4: Missing Last.fm Integration – PASS / FAIL

**Automated Tests:**
- [ ] Test 8.1: Unit Tests – PASS / FAIL
- [ ] Test 8.2: Integration Tests – PASS / FAIL
- [ ] Test 8.3: Lighthouse Audit – PASS / FAIL

---

### Sample Test Report

```markdown
## MIMM 2.0 – E2E Test Results

**Date:** January 26, 2026  
**Tester:** [Your Name]  
**Environment:** Windows 11, Chrome 120, NVDA 2024.1

### Summary
- Total Tests: 40
- Passed: 38 ✅
- Failed: 2 ❌
- Blocked: 0 ⏸️

### Failed Tests

**Test 4.4: Screen Reader (VoiceOver)**
- Issue: "Live region announcements delayed by 500ms on Safari"
- Severity: Medium
- Fix: Add StateHasChanged() before live region update
- Status: In Progress

**Test 6.1: Dashboard Load Time**
- Issue: "LCP > 2.5s on slow 3G network"
- Severity: Low
- Fix: Implement skeletal loading + code splitting
- Status: Backlog

### Conclusion
Release ready after fixing 2 medium/low priority issues.
```

---

## Part X: Testing Tools & Resources

### Browser Extensions

| Tool | Purpose | Link |
|---|---|---|
| **Axe DevTools** | Accessibility audit | [chromewebstore.google.com](https://chromewebstore.google.com) |
| **WAVE** | Web accessibility evaluation | [wave.webaim.org](https://wave.webaim.org) |
| **Lighthouse** | Performance & accessibility (built-in DevTools) | Built into Chrome |
| **Web Vitals** | Core Web Vitals monitoring | [chromewebstore.google.com](https://chromewebstore.google.com) |

### Screen Readers

| Tool | OS | Cost | Download |
|---|---|---|---|
| **NVDA** | Windows | Free | [nvaccess.org](https://www.nvaccess.org) |
| **JAWS** | Windows, macOS | $90/year | [freedomscientific.com](https://www.freedomscientific.com) |
| **VoiceOver** | macOS, iOS | Free (built-in) | System Preferences |
| **TalkBack** | Android | Free (built-in) | Settings → Accessibility |

### Performance Testing

| Tool | Purpose | Link |
|---|---|---|
| **Chrome DevTools** | Built-in profiling | Press F12 |
| **WebPageTest** | Waterfall analysis | [webpagetest.org](https://www.webpagetest.org) |
| **GTmetrix** | Performance insights | [gtmetrix.com](https://gtmetrix.com) |

---

## Conclusion & Sign-Off

Once all tests pass, MIMM 2.0 is ready for:
- ✅ Phase 3 delivery (UX/UI + Accessibility)
- ✅ User beta testing
- ✅ Phase 4 development (new features)

**Next Steps:**
1. Execute tests using this guide
2. Document failures in GitHub Issues
3. Fix critical/high priority issues
4. Retest and sign off
5. Proceed to Phase 4

---

**Last Updated:** January 26, 2026  
**Review Cycle:** After each major release or feature addition
