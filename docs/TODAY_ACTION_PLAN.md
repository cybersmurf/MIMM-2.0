# ⚡ TODAY'S ACTION PLAN - 24. ledna 2026

**Current Time**: Start of Sprint 1  
**Priority**: 🔴 CRITICAL PATH - Database Setup  
**Estimated Time**: 2-3 hours total  
**Blocker Status**: These tasks must complete before any other development

---

## 🎯 Mission: Get Database Running & Test E2E Auth Flow

### Why This Matters

- **60% MVP complete**, but wszystko běží v in-memory DB (fake data)
- Bez real PostgreSQL **nemůžeme testovat production auth flow**
- Frontend + Backend nikdy **nebyly testovány společně**
- **Blokuje všechny další features** (Entry CRUD UI potřebuje real DB)

---

## ✅ Action 1: Database Setup (90 minut)

### Step 1.1: Start PostgreSQL Container (5 min)

```bash
# Navigate to project root
cd /Users/petrsramek/AntigravityProjects/MIMM-2.0

# Start PostgreSQL (+ Redis for future)
docker-compose up -d postgres

# Verify running
docker ps | grep postgres
# Expected: mimm-postgres container with status "Up"

# Check logs
docker logs mimm-postgres --tail 20
# Expected: "database system is ready to accept connections"
```

**Troubleshooting**:

- Error: "port 5432 already in use"
  → Solution: `lsof -i :5432` a kill existing PostgreSQL proces
- Error: "permission denied"
  → Solution: `sudo docker-compose up -d postgres`

---

### Step 1.2: Verify Connection String (10 min)

Open `src/MIMM.Backend/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=mimm;Username=mimmuser;Password=mimmpass;Include Error Detail=true"
  },
  "Jwt": {
    "Key": "your-256-bit-secret-key-change-this-in-production-at-least-32-characters-long",
    "Issuer": "MIMM.Backend",
    "Audience": "MIMM.Frontend",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

**Verify**:

- Host: `localhost` (ne 127.0.0.1, Docker network může mít problémy)
- Port: `5432` (default PostgreSQL)
- Database: `mimm` (podle docker-compose.yml)
- Username: `mimmuser` (podle docker-compose.yml)
- Password: `mimmpass` (podle docker-compose.yml)

**Security Note**: JWT Key je placeholder - pro production použij:

```bash
openssl rand -base64 64
```

---

### Step 1.3: Test PostgreSQL Connection (5 min)

```bash
# Option A: psql command-line
docker exec -it mimm-postgres psql -U mimmuser -d mimm

# Expected:
# mimm=# 

# List databases
\l

# Expected: mimm database exists

# Exit
\q

# Option B: pgAdmin (GUI)
# Download: https://www.pgadmin.org/download/
# Connection:
#   Host: localhost
#   Port: 5432
#   Database: mimm
#   Username: mimmuser
#   Password: mimmpass
```

---

### Step 1.4: Install EF Core Tools (10 min)

```bash
# Check if already installed
dotnet ef --version

# Expected: Entity Framework Core .NET Command-line Tools, 9.0.0

# If not installed:
dotnet tool install --global dotnet-ef --version 9.0.0

# Verify installation
dotnet ef --version
```

---

### Step 1.5: Create First Migration (20 min)

```bash
cd /Users/petrsramek/AntigravityProjects/MIMM-2.0

# Create migration (generates C# migration code)
cd src/MIMM.Backend
dotnet ef migrations add InitialCreate --output-dir Data/Migrations

# Expected output:
# Build started...
# Build succeeded.
# Done. To undo this action, use 'dotnet ef migrations remove'

# Verify files created
ls -la Data/Migrations/
# Expected:
#   20260124_InitialCreate.cs          (migration file)
#   20260124_InitialCreate.Designer.cs (snapshot)
#   ApplicationDbContextModelSnapshot.cs
```

**Migration File Contents** (auto-generated):

- Creates `Users` table (Id, Email, PasswordHash, DisplayName, etc.)
- Creates `JournalEntries` table (Id, UserId, SongTitle, Valence, Arousal, etc.)
- Creates `LastFmTokens` table (Id, UserId, SessionKey, etc.)
- Creates indexes (Email unique, UserId + CreatedAt composite)
- Creates foreign keys (Entries.UserId → Users.Id)

---

### Step 1.6: Apply Migration to Database (10 min)

```bash
# Still in src/MIMM.Backend
dotnet ef database update

# Expected output:
# Build started...
# Build succeeded.
# Applying migration '20260124_InitialCreate'.
# Done.

# Verify tables created
docker exec -it mimm-postgres psql -U mimmuser -d mimm -c "\dt"

# Expected tables:
#   Users
#   JournalEntries
#   LastFmTokens
#   __EFMigrationsHistory
```

**Check Table Schema**:

```sql
-- Run in psql
\d "Users"

-- Expected columns:
-- Id, Email, PasswordHash, DisplayName, Language, TimeZone,
-- EmailVerified, CreatedAt, DeletedAt
```

---

### Step 1.7: Seed Test Data (Optional, 10 min)

Create `src/MIMM.Backend/Data/DbInitializer.cs`:

```csharp
public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        // Create test user (for E2E testing)
        if (!context.Users.Any(u => u.Email == "test@example.com"))
        {
            var testUser = new User
            {
                Email = "test@example.com",
                // Password: Test123!
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
                DisplayName = "Test User",
                Language = "cs",
                TimeZone = "Europe/Prague",
                EmailVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            
            context.Users.Add(testUser);
            context.SaveChanges();
        }
    }
}
```

Update `Program.cs` (před `app.Run()`):

```csharp
// Seed database (development only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    DbInitializer.Initialize(context);
}

app.Run();
```

Run backend:

```bash
dotnet run

# Check logs for seed confirmation
# Expected: Test user created (or already exists)
```

---

### Step 1.8: Verify Backend Starts Successfully (10 min)

```bash
cd src/MIMM.Backend
dotnet run

# Expected output:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: https://localhost:7001
#       Now listening on: http://localhost:5001
# info: Microsoft.Hosting.Lifetime[0]
#       Application started. Press Ctrl+C to shut down.

# Open Swagger UI
open https://localhost:7001/swagger

# Expected: Swagger page with 13 endpoints
#   - AuthController: 6 endpoints (register, login, refresh, verify, /me, logout)
#   - EntriesController: 7 endpoints (GET list, GET by id, POST create, etc.)
```

**Test Auth Endpoint in Swagger**:

1. Click `/api/auth/register` → Try it out
2. Request body:

   ```json
   {
     "email": "swagger@example.com",
     "password": "Swagger123!",
     "displayName": "Swagger Test User",
     "language": "en"
   }
   ```

3. Execute
4. Expected: 200 OK, response with `accessToken` and `refreshToken`

**Stop Backend** (Ctrl+C) - budeme ho spouštět znovu v Action 2.

---

## ✅ Action 2: End-to-End Authentication Test (30 minut)

### Step 2.1: Start Backend & Frontend Together (5 min)

**Terminal 1 (Backend)**:

```bash
cd /Users/petrsramek/AntigravityProjects/MIMM-2.0/src/MIMM.Backend
dotnet run

# Wait for: "Now listening on: https://localhost:7001"
```

**Terminal 2 (Frontend)**:

```bash
cd /Users/petrsramek/AntigravityProjects/MIMM-2.0/src/MIMM.Frontend
dotnet run

# Wait for: "Now listening on: https://localhost:5001"
```

**Keep both terminals running!**

---

### Step 2.2: Test User Registration (5 min)

1. Open browser: <https://localhost:5001>
2. Expected: Login page s toggle "Don't have an account? Register"
3. Click **"Register"** tab
4. Fill form:
   - Email: `e2e@example.com`
   - Password: `Test123!`
   - Confirm Password: `Test123!`
   - Display Name: `E2E Test User`
   - Language: `cs` (Czech)
5. Click **"Register"** button
6. Expected:
   - Loading spinner appears
   - Success snackbar: "Registration successful! Redirecting..."
   - Redirect to `/dashboard`

**If fails**:

- Check browser DevTools Console (F12) for errors
- Check Network tab: POST `/api/auth/register` should be 200 OK
- Check Terminal 1 (backend logs) for exceptions

---

### Step 2.3: Verify Dashboard Access (5 min)

After successful registration, you should see:

**Dashboard Page** (`/dashboard`):

- MudAppBar with "MIMM 2.0" title
- MudDrawer (left sidebar) with navigation
- Main content area with placeholder cards:
  - "Recent Entries" (empty)
  - "Mood Statistics" (no data)
  - "Music Library" (no data)
- Logout button (top-right)

**Check localStorage** (DevTools → Application → Local Storage):

- Key: `mimm_access_token`
- Value: JWT token (long string starting with `eyJ...`)
- Key: `mimm_refresh_token`
- Value: Refresh token UUID

---

### Step 2.4: Test Logout & Login (5 min)

1. Click **"Logout"** button
2. Expected:
   - Tokens cleared from localStorage
   - Redirect to `/login`
3. Fill login form:
   - Email: `e2e@example.com`
   - Password: `Test123!`
4. Click **"Login"** button
5. Expected:
   - Success snackbar: "Welcome back!"
   - Redirect to `/dashboard`

---

### Step 2.5: Test Protected Route Access (5 min)

**Scenario A: User logged in**

1. Navigate to: <https://localhost:5001/dashboard>
2. Expected: Dashboard loads normally

**Scenario B: User logged out**

1. Click Logout
2. Manually navigate to: <https://localhost:5001/dashboard>
3. Expected: **Redirect to `/login`** (auth guard working)

**Scenario C: Invalid token**

1. Open DevTools → Application → Local Storage
2. Edit `mimm_access_token` → change last character
3. Refresh page
4. Expected: Redirect to `/login` (token validation failed)

---

### Step 2.6: Verify Backend Database (5 min)

```bash
# Check Users table
docker exec -it mimm-postgres psql -U mimmuser -d mimm

-- Run SQL
SELECT "Id", "Email", "DisplayName", "CreatedAt" FROM "Users";

-- Expected: 1-2 users (test@example.com, e2e@example.com, swagger@example.com)

-- Check JournalEntries table
SELECT COUNT(*) FROM "JournalEntries";

-- Expected: 0 (žádné entries vytvořeny zatím)

-- Exit psql
\q
```

---

### Step 2.7: Test API with cURL (Optional, 10 min)

**Get Access Token First**:

1. Login via frontend
2. Copy `mimm_access_token` from localStorage

**Test Protected Endpoint**:

```bash
# Replace YOUR_TOKEN with actual token
curl -X GET "https://localhost:7001/api/auth/me" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -k  # -k ignores self-signed certificate warning

# Expected response:
# {
#   "id": 1,
#   "email": "e2e@example.com",
#   "displayName": "E2E Test User",
#   "language": "cs",
#   "emailVerified": false
# }
```

**Test Entries Endpoint**:

```bash
curl -X GET "https://localhost:7001/api/entries?page=1&pageSize=10" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -k

# Expected: Empty array (no entries yet)
# {
#   "items": [],
#   "totalCount": 0,
#   "page": 1,
#   "pageSize": 10
# }
```

---

## ✅ Success Checklist

Before proceeding to Entry CRUD UI (tomorrow), verify:

- [ ] **PostgreSQL container running** (`docker ps | grep postgres`)
- [ ] **3 tables created** (Users, JournalEntries, LastFmTokens)
- [ ] **Backend starts without errors** (`dotnet run` → Swagger at 7001)
- [ ] **Frontend starts without errors** (`dotnet run` → Blazor at 5001)
- [ ] **User can register** (new user in DB)
- [ ] **User can login** (receives JWT token)
- [ ] **Dashboard loads** (shows user data)
- [ ] **Logout works** (clears localStorage, redirects to login)
- [ ] **Protected routes guarded** (redirect to login when not authenticated)
- [ ] **API /me endpoint** returns user data with valid token
- [ ] **API /entries endpoint** returns empty array (no entries yet)

---

## 🚨 Common Issues & Solutions

### Issue 1: "Connection refused to localhost:5432"

**Cause**: PostgreSQL container not running  
**Solution**:

```bash
docker-compose up -d postgres
docker logs mimm-postgres
```

### Issue 2: "The ConnectionString property has not been initialized"

**Cause**: Missing appsettings.Development.json  
**Solution**:

```bash
cd src/MIMM.Backend
ls -la appsettings.*
# Should see: appsettings.json, appsettings.Development.json
```

### Issue 3: "Table 'Users' doesn't exist"

**Cause**: Migration not applied  
**Solution**:

```bash
cd src/MIMM.Backend
dotnet ef database update
```

### Issue 4: "CORS policy blocked"

**Cause**: Frontend origin not allowed  
**Solution**: Check `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            "https://localhost:5001",  // ← Must include this
            "http://localhost:5000"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});
```

### Issue 5: "401 Unauthorized on /api/auth/me"

**Cause**: JWT token not sent in Authorization header  
**Solution**: Check `AuthorizationMessageHandler.cs`:

```csharp
request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
```

---

## 📝 What's Next (Tomorrow - 25. ledna)

**Tomorrow's Focus**: Entry CRUD UI Implementation

**Action 3.1 & 3.2** (4 hours):

- Create `EntryApiService.cs` (HTTP wrapper pro entries endpoints)
- Create `EntryList.razor` (main dashboard s entry list)
- MudDataGrid s pagination
- Search filters (text, date range)

**By end of tomorrow**:

- Dashboard zobrazuje entry list (prázdný, ale funkční)
- "New Entry" button naviguje na `/entries/create`

---

## 🎉 Congratulations

Po dokončení dnešních akcí máš:

- ✅ **Real PostgreSQL database** s 3 tabulkami
- ✅ **End-to-end auth flow** tested and working
- ✅ **Frontend + Backend integration** verified
- ✅ **Production-like environment** (ne in-memory DB)

**You're now ready to build the core MVP feature: Entry CRUD UI!** 🚀

---

**Document Version**: 1.0  
**Created**: 24. ledna 2026  
**Estimated Completion**: 2-3 hours (depends on troubleshooting)  
**Next Action**: STRATEGIC_ACTION_PLAN_2026.md → Action 3 (Entry CRUD UI)
