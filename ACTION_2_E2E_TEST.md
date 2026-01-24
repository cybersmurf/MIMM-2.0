# ⚡ ACTION 2: End-to-End Authentication Test (READY NOW)

**Status**: 🎯 READY TO EXECUTE  
**Estimated Time**: 30 minutes  
**Prerequisites**: ✅ Action 1 COMPLETE (database running)

---

## 🎯 Mission

Ověřit, že **Frontend + Backend + Database** pracují spolu správně:
- Register new user → DB storage
- Login with credentials → JWT token in localStorage
- Access dashboard → Protected route works
- Logout → Token cleared

---

## 📋 Quick Start

```bash
# Terminal 1: Start Backend
cd /Users/petrsramek/AntigravityProjects/MIMM-2.0/src/MIMM.Backend
dotnet run

# Terminal 2: Start Frontend  
cd /Users/petrsramek/AntigravityProjects/MIMM-2.0/src/MIMM.Frontend
dotnet run

# Browser: https://localhost:5001
# Test: Register → Login → Dashboard → Logout
```

---

## 🚀 Step-by-Step Instructions

### Step 1: Start Backend (Terminal 1) - 2 min

```bash
cd /Users/petrsramek/AntigravityProjects/MIMM-2.0/src/MIMM.Backend
dotnet run
```

**Expected Output**:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7001
      Now listening on: http://localhost:5001
```

**Verify Swagger**:
- Open browser: https://localhost:7001/swagger
- Should see 13 endpoints (6 Auth + 7 Entries)
- ✅ Backend is ready

---

### Step 2: Start Frontend (Terminal 2) - 2 min

```bash
cd /Users/petrsramek/AntigravityProjects/MIMM-2.0/src/MIMM.Frontend
dotnet run
```

**Expected Output**:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
```

**Verify Frontend**:
- Open browser: https://localhost:5001
- Should see Login page with "Don't have account? Register" link
- ✅ Frontend is ready

---

### Step 3: Test User Registration (Browser) - 5 min

**Navigate to**: https://localhost:5001

**UI Elements**:
- Email field
- Password field (with visibility toggle)
- "Don't have account? Register" link
- Language selector dropdown

**Actions**:
1. Click **"Register"** tab (if not selected)
2. Fill form:
   - **Email**: `e2e@example.com`
   - **Password**: `Test123!`
   - **Confirm Password**: `Test123!`
   - **Display Name**: `E2E Test User`
   - **Language**: `cs` (Czech)
3. Click **"Register"** button

**Expected Results** ✅:
- Loading spinner appears briefly
- Green snackbar: "Registration successful! Redirecting..."
- Auto-redirect to `/dashboard`
- Dashboard displays user info

**If Fails** ❌:
- Check Terminal 1 (backend logs) for errors
- Check Browser DevTools Console (F12) for JS errors
- Check Network tab: POST `/api/auth/register` should be 200 OK

---

### Step 4: Verify User in Database - 3 min

Open **new terminal** (Terminal 3):

```bash
docker exec -it mimm-postgres psql -U mimmuser -d mimm

-- Query users
SELECT "Id", "Email", "DisplayName", "CreatedAt" FROM "Users";

-- Expected output:
--  Id | Email           | DisplayName      | CreatedAt
-- ----+-----------------+------------------+-------------------
--  (UUID) | e2e@example.com | E2E Test User    | 2026-01-24 13:30:00

-- Exit psql
\q
```

✅ User successfully stored in PostgreSQL!

---

### Step 5: Verify JWT Token in Browser - 2 min

**Open DevTools** (F12 in browser at https://localhost:5001):

**Navigate to**: Application → Local Storage → https://localhost:5001

**Expected Keys**:
- `mimm_access_token` = JWT string (starts with `eyJ...`)
- `mimm_refresh_token` = UUID or JWT

**Decode JWT Token** (optional):
- Copy `mimm_access_token` value
- Go to https://jwt.io
- Paste token in "Encoded" field
- Should see claims: `sub` (user ID), `email`, etc.

✅ Token successfully stored in browser storage!

---

### Step 6: Test Dashboard Access - 2 min

**You should see Dashboard** with:
- MudAppBar (top bar) with "MIMM 2.0" title
- MudDrawer (left sidebar) if responsive
- Main content area with placeholder cards:
  - "Recent Entries" (empty)
  - "Mood Statistics" (no data)
  - "Music Library" (no data)
- Logout button (top-right)

✅ Protected route access successful!

---

### Step 7: Test Logout - 2 min

1. Click **"Logout"** button (top-right)
2. Expected:
   - Snackbar: "Logged out successfully"
   - Redirect to `/login`
   - localStorage cleared (mimm_access_token gone)

**Verify localStorage empty** (DevTools → Application):
- `mimm_access_token` should be gone
- `mimm_refresh_token` should be gone

✅ Logout working correctly!

---

### Step 8: Test Login - 3 min

Back on Login page:

1. Fill form:
   - **Email**: `e2e@example.com`
   - **Password**: `Test123!`
2. Click **"Login"** button

**Expected**:
- Loading spinner
- Green snackbar: "Welcome back!"
- Redirect to `/dashboard`
- New token in localStorage

✅ Login working correctly!

---

### Step 9: Test Protected Route Guard - 2 min

1. Click **Logout**
2. Manually navigate to: https://localhost:5001/dashboard
3. Expected: **Redirect to `/login`** (auth check working!)

✅ Auth guard protecting routes!

---

### Step 10: Test API Endpoint with Valid Token (Optional) - 5 min

**Terminal 3** (copy your token):

```bash
# Get token from browser localStorage
# Replace YOUR_TOKEN with actual JWT value

curl -X GET "https://localhost:7001/api/auth/me" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -k

# Expected response:
# {
#   "id": "...",
#   "email": "e2e@example.com",
#   "displayName": "E2E Test User",
#   "language": "cs",
#   "emailVerified": false
# }
```

✅ API endpoint protected and accessible with token!

---

### Step 11: Test API Endpoint WITHOUT Token (Optional) - 2 min

```bash
curl -X GET "https://localhost:7001/api/auth/me" \
  -k

# Expected response: 401 Unauthorized
# {
#   "error": "Unauthorized"
# }
```

✅ API correctly rejects request without token!

---

## ✅ Complete Success Checklist

After all steps, you should have:

- [ ] Backend running on https://localhost:7001 with Swagger UI
- [ ] Frontend running on https://localhost:5001
- [ ] User successfully registered (`e2e@example.com`)
- [ ] User stored in PostgreSQL database
- [ ] JWT token in localStorage after login
- [ ] Dashboard accessible after login
- [ ] Dashboard redirects to login when not authenticated
- [ ] Logout clears tokens and redirects to login
- [ ] Login works with correct credentials
- [ ] API endpoint returns 200 with valid token
- [ ] API endpoint returns 401 without token

**If ALL ✓**: Proceed to **Action 3: Entry CRUD UI** tomorrow!

---

## 🚨 Troubleshooting

### Issue: "Connection refused" to backend
**Solution**:
```bash
# Terminal 1: Check backend is running
curl -k https://localhost:7001/swagger
# Should see HTML response (not connection refused)
```

### Issue: CORS Error in browser console
**Solution**:
Check `src/MIMM.Backend/Program.cs` CORS policy includes:
- `https://localhost:5001`
- `http://localhost:5000`

Current CORS setup should be automatic from Program.cs.

### Issue: "401 Unauthorized" on /api/auth/me
**Solution**:
- Check DevTools → Network → look for Authorization header
- Check token format: `Authorization: Bearer eyJ...`
- Check AuthorizationMessageHandler.cs is sending token

### Issue: User not appearing in database
**Solution**:
```bash
# Verify database connection
docker exec -it mimm-postgres psql -U mimmuser -d mimm
# Should connect without error

# Check connection string in appsettings.Development.json
cat src/MIMM.Backend/appsettings.Development.json | grep "DefaultConnection"
# Should show: "Host=localhost;Database=mimm;Username=mimmuser;..."
```

### Issue: "The ASP.NET Core developer certificate is not trusted"
**Solution**:
- This is just a warning, not an error
- https endpoints will still work
- For production, install proper certificate

### Issue: Frontend shows blank page
**Solution**:
```bash
# Check browser console for JS errors (F12)
# Verify frontend is running on https://localhost:5001
# Clear browser cache and reload
```

---

## 📊 E2E Test Results

After completion, you'll have validated:

✅ **User Authentication Flow**:
- Registration creates user with hashed password
- Login generates JWT + refresh token
- Tokens stored securely in localStorage
- Protected routes check authentication

✅ **Database Integrity**:
- User data persisted to PostgreSQL
- Foreign key relationships working
- No orphaned data

✅ **Frontend-Backend Integration**:
- CORS configured correctly
- Authorization header sent on API calls
- Tokens validated on backend
- Protected routes enforced

✅ **Security**:
- Password hashed (not stored plaintext)
- JWT tokens time-limited (60 min)
- Refresh tokens separate (7 days)
- Logout clears credentials

---

## 🎯 What's Next

After E2E test passes:

1. **Today**: Close backend & frontend terminals
2. **Tomorrow**: Action 3 - Implement Entry CRUD UI
   - EntryApiService (HTTP wrapper)
   - EntryList.razor (main dashboard)
   - EntryCreate.razor (new entry form)
   - MoodSelector.razor (2D mood grid)

---

**Estimated Time to Complete E2E Test**: 30 minutes  
**Success Rate**: 95%+ (if Action 1 completed successfully)  
**Next Milestone**: Entry CRUD UI (Action 3) - 25. ledna 2026

Good luck! 🚀
