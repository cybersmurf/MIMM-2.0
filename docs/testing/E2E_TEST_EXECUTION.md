# 🧪 E2E TEST EXECUTION REPORT

**Date**: 24. ledna 2026, 13:45 UTC  
**Status**: ✅ **SYSTEMS OPERATIONAL - READY FOR TESTING**

---

## 🚀 Services Status

### ✅ Backend API

```bash
Service: ASP.NET Core 9
Port: 7001 (HTTPS)
Status: ✅ RUNNING
Database: PostgreSQL (connected)
Swagger: https://localhost:7001/swagger

Endpoints Available:
  - POST /api/auth/register
  - POST /api/auth/login
  - POST /api/auth/refresh
  - GET /api/auth/me
  - POST /api/auth/verify
  - POST /api/auth/logout
  - GET /api/entries (paginated)
  - POST /api/entries (create)
  - GET /api/entries/{id}
  - PUT /api/entries/{id}
  - DELETE /api/entries/{id}
  - GET /api/entries/search
  - GET /api/entries/statistics
```

### ✅ Frontend Application

```bash
Service: Blazor WebAssembly 9
Port: 5000 (HTTP)
Status: ✅ RUNNING
Framework: MudBlazor 7.0.0
Router: Ready

Pages Available:
  - / (redirect to /login)
  - /login (auth page)
  - /dashboard (protected)
```

### ✅ Database

```yaml
Service: PostgreSQL 16
Port: 5432
Database: mimm
User: mimmuser
Status: ✅ RUNNING
Tables: Users, Entries, LastFmTokens
```

---

## 🧪 E2E Test Scenarios

### Test 1: User Registration (READY TO TEST)

**Manual Steps**:

1. Navigate to: <http://localhost:5000/login>
2. Click "Register" toggle/tab
3. Fill form:
   - Email: `e2e-test@example.com`
   - Password: `Test123!`
   - Confirm: `Test123!`
   - Display Name: `E2E Test User`
   - Language: `en`
4. Click "Register" button

**Expected Results**:

- ✅ No error message
- ✅ Success snackbar: "Registration successful"
- ✅ Redirect to `/dashboard`
- ✅ User appears in database (verify with psql)

**Database Verification**:

```bash
docker exec -it mimm-postgres psql -U mimmuser -d mimm
SELECT "Email", "DisplayName", "CreatedAt" FROM "Users" WHERE "Email" = 'e2e-test@example.com';
```

---

### Test 2: JWT Token Storage (AFTER REGISTRATION)

**Browser DevTools**:

1. Press F12 (Developer Tools)
2. Go to Application → Local Storage → <http://localhost:5000>
3. Look for:
   - `mimm_access_token` (should be JWT string)
   - `mimm_refresh_token` (should be UUID or JWT)

**Expected**:

- ✅ Both tokens present
- ✅ Access token starts with `eyJ`
- ✅ Tokens are non-empty

**JWT Decoder (Optional)**:

- Copy `mimm_access_token` value
- Paste at <https://jwt.io> to see claims
- Should show: `sub` (user ID), `email`, `iat` (issued at)

---

### Test 3: Dashboard Access (AFTER LOGIN)

**Navigation**:

1. You should see dashboard automatically (redirect from login)
2. URL should be: <http://localhost:5000/dashboard>

**Expected UI**:

- ✅ MudAppBar with "MIMM 2.0" title
- ✅ User greeting (if implemented)
- ✅ Navigation menu
- ✅ Placeholder cards or components
- ✅ Logout button

---

### Test 4: Login with Credentials (NEW SESSION)

**Steps**:

1. Click Logout button
2. Verify redirect to login page
3. localStorage cleared (DevTools check)
4. Fill login form:
   - Email: `e2e-test@example.com`
   - Password: `Test123!`
5. Click "Login"

**Expected**:

- ✅ Success snackbar: "Welcome back!"
- ✅ Redirect to dashboard
- ✅ New token in localStorage
- ✅ Dashboard accessible

---

### Test 5: Protected Route Guard (OPTIONAL)

**Steps**:

1. Click Logout
2. Manually type in browser: <http://localhost:5000/dashboard>
3. Press Enter

**Expected**:

- ✅ Auto-redirect to /login (route guard working)
- ✅ Dashboard NOT accessible without token

---

### Test 6: API /me Endpoint (OPTIONAL - WITH CURL)

**Terminal Command**:

```bash
# Step 1: Get token from browser localStorage (copy mimm_access_token)
# Step 2: Replace YOUR_TOKEN with actual value

curl -X GET "https://localhost:7001/api/auth/me" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -k

# Expected 200 OK response:
# {
#   "id": "...",
#   "email": "e2e-test@example.com",
#   "displayName": "E2E Test User",
#   "language": "en",
#   "emailVerified": false
# }
```

---

## 📋 Test Execution Checklist

After manually running tests, mark complete:

### Registration Flow

- [ ] Registration page loads
- [ ] Form submission works
- [ ] Success message appears
- [ ] Redirect to dashboard
- [ ] User in database
- [ ] Token in localStorage

### Login Flow  

- [ ] Login page loads
- [ ] Form submission works
- [ ] Success message appears
- [ ] Redirect to dashboard
- [ ] Dashboard displays

### Logout Flow

- [ ] Logout button works
- [ ] Redirect to login
- [ ] Tokens cleared
- [ ] localStorage empty

### Protection

- [ ] Dashboard redirect when not auth
- [ ] API returns 401 without token
- [ ] API returns 200 with token

---

## 🔗 Quick Links

**Frontend**: <http://localhost:5000/login>  
**Backend Swagger**: <https://localhost:7001/swagger>  
**PostgreSQL CLI**:

```bash
docker exec -it mimm-postgres psql -U mimmuser -d mimm
```

**DevTools Instructions**:

1. Press F12
2. Go to Console (for errors)
3. Go to Network (for API calls)
4. Go to Application → Local Storage (for tokens)

---

## 🎯 What Happens When Test Passes

✅ **Validates**:

1. Frontend + Backend communication works
2. Database integration working
3. JWT token flow correct
4. Authentication flow complete
5. Protected routes enforced

✅ **Proves**:

- 60% → 100% of critical path complete
- Database is functional
- API responds correctly
- Frontend loads and interacts
- No major integration issues

---

## 📊 Expected Result

After successfully running all 6 tests:

**Status**: ✅ **E2E TEST PASSED**

This means:

- ✅ MVP foundation is solid
- ✅ Backend + Frontend + Database integrated
- ✅ Authentication system working
- ✅ Ready for next feature: Entry CRUD UI

---

## 🚨 Troubleshooting

### Issue: "Cannot GET /login"

**Solution**: Frontend not started, try:

```bash
cd src/MIMM.Frontend && dotnet run
```

### Issue: "Connection refused" on backend API

**Solution**: Backend not started, try:

```bash
cd src/MIMM.Backend && dotnet run
```

### Issue: "CORS error" in console

**Solution**: This is expected for localhost dev. CORS should allow localhost:5000 and localhost:5001.
Check `src/MIMM.Backend/Program.cs` for CORS policy.

### Issue: "Database connection failed"

**Solution**: PostgreSQL not running:

```bash
docker-compose up -d postgres
docker logs mimm-postgres
```

### Issue: User not created in database

**Solution**: Check backend logs for SQL errors. Run:

```bash
docker exec -it mimm-postgres psql -U mimmuser -d mimm
# Then query: SELECT * FROM "Users";
```

---

## ✅ Next Steps After E2E Pass

1. **Document Results**: Note what works, what needs fixing
2. **Fix Any Issues**: If tests fail, debug before continuing
3. **Commit Changes**: Git add/commit/push if using version control
4. **Start Action 3**: Begin Entry CRUD UI implementation (tomorrow)

---

## 🎯 Success Criteria

| Criterion | Status | Notes |
|-----------|--------|-------|
| Backend running on 7001 | ✅ | HTTPS + Swagger |
| Frontend running on 5000 | ✅ | Blazor WASM |
| Database connected | ✅ | PostgreSQL 16 |
| Registration works | ⏳ | Manual test required |
| Login works | ⏳ | Manual test required |
| JWT tokens stored | ⏳ | Manual test required |
| Protected routes work | ⏳ | Manual test required |

---

**Test Execution Started**: 24. ledna 2026, 13:45 UTC  
**Next Step**: Manual browser testing  
**Estimated Completion**: 30-45 minutes  
**Success Indicator**: All checkboxes ticked ✅

---

## 📝 Test Log

**13:45** - Backend and Frontend started  
**13:46** - Verified both services running  
**13:47** - Test documentation prepared  
**NEXT** - Await manual test execution via browser

---

*Document prepared for developer manual testing. Once developer executes all test steps, update checklist above.*
