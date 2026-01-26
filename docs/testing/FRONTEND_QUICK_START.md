# Frontend Integration Testing - Quick Start Guide

## 📋 Prerequisites

✅ Backend API running: `http://localhost:5001`  
✅ Database (PostgreSQL) running in Docker  
✅ E2E auth test passed  

## 🚀 Getting Started (5 minutes)

### Step 1: Start Frontend Dev Server

```bash
# Terminal 1: Frontend
cd /Users/petrsramek/AntigravityProjects/MIMM-2.0/src/MIMM.Frontend
npm install  # First time only
npm run dev
```

**Expected output:**

```
  VITE v5.x.x  ready in xxxx ms

  ➜  Local:   http://localhost:5173/
  ➜  press h to show help
```

### Step 2: Start Backend API (if not already running)

```bash
# Terminal 2: Backend
cd /Users/petrsramek/AntigravityProjects/MIMM-2.0/src/MIMM.Backend
dotnet run
```

**Expected output:**

```
info: Microsoft.AspNetCore.Hosting.Diagnostics[1]
      Now listening on: http://localhost:5001
```

### Step 3: Open Browser

```bash
open http://localhost:5173
```

You should see the MIMM login page.

## ✅ Manual Test Execution

Follow the **10-step test scenario** in `docs/testing/frontend-integration-test-plan.sh`:

### Quick Checklist (Copy-Paste)

```
1. Authentication Flow
   ☐ Navigate to /login
   ☐ Email: e2e-auto@example.com
   ☐ Password: Test123!
   ☐ Verify redirect to /dashboard

2. Dashboard Initial State
   ☐ Welcome message visible
   ☐ EntryList renders
   ☐ Empty state message shows
   ☐ "New Entry" button present

3. Create Entry Flow
   ☐ Click "New Entry"
   ☐ Song Title: Bohemian Rhapsody
   ☐ Artist: Queen
   ☐ Album: A Night at the Opera
   ☐ Set Valence: +0.7
   ☐ Set Arousal: +0.6
   ☐ Tension: 45
   ☐ Tags: [Energetic, Euphoric]
   ☐ Notes: "Epic masterpiece"
   ☐ Click Create
   ☐ Success notification appears
   ☐ Entry appears in list

4. Entry Display
   ☐ Song title visible
   ☐ Artist name visible
   ☐ Mood badge shows
   ☐ Created timestamp displays
   ☐ Edit/Delete buttons present

5. Edit Entry Flow
   ☐ Click Edit button
   ☐ Change Valence to -0.3
   ☐ Change Notes to "Updated"
   ☐ Click Save
   ☐ Success notification
   ☐ List updates

6. Delete Entry Flow
   ☐ Click Delete button
   ☐ Confirm dialog appears
   ☐ Click Confirm
   ☐ Success notification
   ☐ Entry removed from list

7. Pagination (if 15+ entries exist)
   ☐ Pagination controls visible
   ☐ Next/Previous buttons work
   ☐ Page size selector works

8. Form Validation
   ☐ Try submit without Song Title
   ☐ Error message shows
   ☐ Try invalid CoverUrl format
   ☐ URL validation error shows

9. Music Search (if integrated)
   ☐ Type in search field
   ☐ Wait >3 chars
   ☐ Results appear
   ☐ Click result → form populates

10. Token Expiry (optional)
    ☐ Let token expire (~15 min)
    ☐ Make API call
    ☐ Verify automatic refresh
```

## 🐛 Troubleshooting

### Frontend won't start

```bash
# Clear node_modules and reinstall
cd src/MIMM.Frontend
rm -rf node_modules package-lock.json
npm install
npm run dev
```

### API calls failing (CORS)

- Verify backend is running on `http://localhost:5001`
- Check browser Network tab in DevTools
- Verify `Authorization: Bearer {token}` header present

### Can't log in

- Verify backend is healthy: `curl http://localhost:5001/health`
- Check if seed user exists (docker exec mimm-postgres psql...)
- Verify credentials: `e2e-auto@example.com` / `Test123!`

### Components not rendering

- Check browser Console tab for JavaScript errors
- Verify `.razor` files compile without errors
- Check that Services are registered in `Program.cs`

## 📊 Browser DevTools Inspection

### Network Tab

- Monitor API requests to `http://localhost:5001/api/*`
- Verify `Authorization: Bearer {token}` header on protected endpoints
- Check response payloads

### Application Tab

- Look for `access_token` and `refresh_token` in localStorage
- Verify tokens are valid JWT format

### Console Tab

- Should be clean (no errors)
- Verify Blazor interop messages if using JSInterop

## 🔄 Workflow for Testing

1. **Create 3-5 test entries** with different moods
2. **Verify list pagination** (create 15+ entries if testing pagination)
3. **Test edit flow** on each entry
4. **Test delete flow** (finally delete all test entries)
5. **Verify database state** with SQL query:

   ```sql
   SELECT id, song_title, valence, arousal, created_at 
   FROM entries 
   WHERE user_id = (SELECT id FROM users WHERE email = 'e2e-auto@example.com')
   ORDER BY created_at DESC;
   ```

## 📝 Logging Issues

If something fails, capture:

1. **Browser Console** errors (screenshot or copy text)
2. **Network tab** failed request (show Response tab)
3. **Backend logs** (terminal output)
4. **Database state** (SQL query output)

## ✨ Next Steps After Testing

### If All Tests Pass ✅

1. Create Cypress/Playwright automated test suite
2. Integrate into CI/CD pipeline
3. Begin Phase 3.2 (Form validation improvements)

### If Tests Fail ❌

1. Check error logs (browser console + backend logs)
2. Compare with expected behavior in test plan
3. File issues with reproduction steps
4. Roll back or fix bugs

## 🎯 Success Criteria

All tests pass when:

- [ ] Login redirects to dashboard
- [ ] Create entry saves to database
- [ ] Entry list displays correctly
- [ ] Edit updates entry data
- [ ] Delete removes entry
- [ ] Pagination works (if applicable)
- [ ] Form validation prevents invalid input
- [ ] No console errors
- [ ] All API calls return 200/201 status

---

**Estimated Time:** 20-30 minutes for full manual test cycle  
**Last Updated:** 2026-01-25  
**Test Framework:** Manual (Cypress/Playwright TBD)
