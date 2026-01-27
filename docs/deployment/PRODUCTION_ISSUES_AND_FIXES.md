# MIMM 2.0 - VPS Production Deployment - Real-World Issues & Solutions

**Datum:** 27. ledna 2026  
**Status:** ✅ **Hotovo** - Backend na VPS komunikuje správně  
**Cíl:** Dokumentace skutečných problémů a řešení na VPS (Hetzner Ubuntu 24.04)

---

## 🎯 Executive Summary

Deployment MIMM 2.0 na VPS (Hetzner) byl problematický kvůli:

1. **Docker build cache issues** - multi-stage build nepoužíval nový zdrojový kód
2. **JWT authentication response structure mismatch** - backend vracíl `UserDto` místo `AuthenticationResponse`
3. **Frontend deserialization errors** - JsonException kvůli chybné struktuře odpovědi
4. **SSH session management** - interaktivní hesla v shell skriptech

Všechny problémy jsou vyřešeny. Tento dokument slúži jako learned lessons pro budoucí updatey.

---

## 1. Docker Build Cache Issue (KRITICKÝ PROBLÉM)

### Symptom

```
Spuštěn: git pull origin main ✅
Build: docker build --no-cache -t mimm-backend:latest . ✅
Backend: vrací starou strukturu odpovědi ❌
```

Backend container běž, ale servíroval **stare DLL** z předchozího buildu.

### Root Cause

Dockerfile multi-stage build používal `COPY src/ .` cache bez invalidace:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/MIMM.Backend/MIMM.Backend.csproj", "MIMM.Backend/"]
COPY ["src/MIMM.Shared/MIMM.Shared.csproj", "MIMM.Shared/"]
RUN dotnet restore "MIMM.Backend/MIMM.Backend.csproj"
COPY src/ .                    # ← PROBLÉM: Docker cachuje tuto vrstvu
RUN dotnet build "MIMM.Backend.csproj" -c Release -o /app/build
```

I s `--no-cache`, Docker cachuje COPY příkaz na základě inode/mtime. Při git pull se soubory změní, ale COPY layer cache jestě existuje.

### Solution (TESTED & WORKING)

#### Řešení #1: ARG Cache-Bust (Recommended)

Přidej ARG na začátek Dockerfile:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG CACHEBUST=1              # ← Nový řádek
WORKDIR /src
# ... rest of Dockerfile
```

Build s unikátním ARG hodnotou:

```bash
docker build --build-arg CACHEBUST=$(date +%s) -t mimm-backend:latest .
```

**Proč to funguje:** Docker invaliduje ALL cache layers po ARG změně.

**Výsledek na VPS:**

```
#12 [build 6/8] COPY src/ .
#12 DONE 1.2s              # ← Vidíš že COPY proběhla, ne cached!
```

#### Řešení #2: Full Prune (Overkill ale jisté)

```bash
docker compose -f docker-compose.prod.yml down
docker system prune -af --volumes
docker build --build-arg CACHEBUST=$(date +%s) -t mimm-backend:latest .
docker compose -f docker-compose.prod.yml up -d
```

**Proč:** `docker system prune -af` smaže všechny nepoužívané layers.

#### Řešení #3: New Image Tag per Commit (CI/CD friendly)

```bash
# V CI/CD pipeline:
docker build -t mimm-backend:$(git rev-parse --short HEAD) .
docker tag mimm-backend:$(git rev-parse --short HEAD) mimm-backend:latest
docker compose -f docker-compose.prod.yml up -d
```

**Proč:** Každý commit = nový image tag, auto-invaliduje cache.

### Prevention

- ✅ Vždycky přidej `ARG CACHEBUST=1` na začátek každého Dockerfile
- ✅ V CI/CD: vždycky build s `--build-arg CACHEBUST=$(date +%s)`
- ✅ Monitoruj backend `/health` endpoint po deployu
- ✅ Test API endpoint po deployu (nejen health check)

---

## 2. JWT Authentication Response Structure Mismatch

### Symptom

**Frontend error:**

```javascript
System.Text.Json.JsonException: JsonRequiredPropertiesMissing, 
MIMM.Shared.Dtos.AuthenticationResponse, 'accessToken'; 'user'
```

**Network inspection:**

```json
{
  "id": "21e0714c-ee71-4357-8795-716481f024a0",
  "email": "test@example.com",
  "displayName": "Test User",
  "language": "en",
  "emailVerified": false
}
```

❌ Missing: `accessToken`, `refreshToken`, `user` wrapper, `accessTokenExpiresAt`

✅ Expected:

```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "QMicKJM...",
  "user": {
    "id": "...",
    "email": "...",
    "displayName": "...",
    "language": "en",
    "emailVerified": false
  },
  "accessTokenExpiresAt": "2026-01-26T23:59:12.6263878Z"
}
```

### Root Cause

Backend `RegisterAsync` vracela `UserDto` místo `AuthenticationResponse`:

**OLD CODE:**

```csharp
public async Task<(bool Success, string? ErrorMessage, UserDto? User)> RegisterAsync(
    RegisterRequest request,
    CancellationToken cancellationToken = default)
{
    // ... validation
    var userDto = new UserDto { Id = user.Id, Email = user.Email, ... };
    return (true, null, userDto);  // ← WRONG! No tokens!
}
```

Controller pak vrátil `UserDto` JSON bez JWT.

### Solution

**FIX APPLIED:** src/MIMM.Backend/Services/AuthService.cs (commit 411290c)

```csharp
public async Task<(bool Success, string? ErrorMessage, AuthenticationResponse? Response)> RegisterAsync(
    RegisterRequest request,
    CancellationToken cancellationToken = default)
{
    // ... validation & user creation
    
    // Generate JWT tokens
    var (accessToken, accessTokenExpiration) = GenerateAccessToken(user);
    var refreshToken = GenerateRefreshToken();
    
    // Persist refresh token
    user.RefreshToken = refreshToken;
    user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(
        _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7)
    );
    await _dbContext.SaveChangesAsync(cancellationToken);
    
    // Return proper AuthenticationResponse
    var authResponse = new AuthenticationResponse
    {
        AccessToken = accessToken,
        RefreshToken = refreshToken,
        AccessTokenExpiresAt = accessTokenExpiration,
        User = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            Language = user.Language,
            EmailVerified = user.EmailVerified
        }
    };
    
    return (true, null, authResponse);
}
```

**Controller update:** src/MIMM.Backend/Controllers/AuthController.cs

```csharp
[HttpPost("register")]
[ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]  // ← Changed from 201
public async Task<ActionResult<AuthenticationResponse>> Register(
    [FromBody] RegisterRequest request,
    CancellationToken cancellationToken = default)
{
    var (success, errorMessage, response) = await _authService.RegisterAsync(request, cancellationToken);
    
    if (!success)
        return BadRequest(new { error = errorMessage });
    
    return Ok(response);  // ← Changed from CreatedAtAction
}
```

### Testing

VPS test command:

```bash
curl -s -X POST http://127.0.0.1:8080/api/auth/register \
  -H 'Content-Type: application/json' \
  --data-raw '{"email":"test@example.com","password":"Test1234","displayName":"Test","language":"en"}'
```

✅ **Expected Response (VERIFIED):**

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "QMicKJMO6Rd137BoZpXjoCTI55zV/4QpFmK9d6Ag0oaG2lSxz...",
  "user": {
    "id": "c9a86452-8448-484c-995c-191cd71dc53e",
    "email": "test@example.com",
    "displayName": "Test",
    "language": "en",
    "emailVerified": false
  },
  "accessTokenExpiresAt": "2026-01-26T23:59:12.6263878Z"
}
```

### Prevention

- ✅ Vždycky vrací `AuthenticationResponse` z auth endpoints
- ✅ Kontroluj `[ProducesResponseType(...)]` atribut - měl by odpovídat vrácenému objektu
- ✅ Test auth endpoints pomocí curl/Postman po deployu
- ✅ V frontend error handling: loguj raw response body pro debugging

---

## 3. Frontend JsonException Handling

### Symptom

```
MIMM.Frontend.Services.AuthApiService[0]
Error deserializing registration response
System.Text.Json.JsonException: JsonRequiredPropertiesMissing...
```

Frontend crashnul, protože se pokusil deserializovat `UserDto` do `AuthenticationResponse` struktury.

### Root Cause

Frontend `AuthApiService` nedělal žádnou validaci Content-Type ani exception handling:

**OLD CODE:**

```csharp
public async Task<AuthenticationResponse?> RegisterAsync(RegisterRequest request)
{
    var response = await _httpClient.PostAsJsonAsync("/api/auth/register", request);
    return await response.Content.ReadFromJsonAsync<AuthenticationResponse>();  // ← CRASH if wrong type
}
```

### Solution

**FIX APPLIED:** src/MIMM.Frontend/Services/AuthApiService.cs (commit 7ac6561)

```csharp
public async Task<AuthenticationResponse?> RegisterAsync(RegisterRequest request)
{
    var response = await _httpClient.PostAsJsonAsync("/api/auth/register", request);
    
    if (response.IsSuccessStatusCode)
    {
        // Validate Content-Type before deserialization
        var contentType = response.Content.Headers.ContentType?.MediaType;
        if (contentType is null || !contentType.Contains("json", StringComparison.OrdinalIgnoreCase))
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogWarning(
                "Expected JSON response but got non-JSON body. Content-Type: {ContentType}, Body snippet: {Snippet}",
                contentType,
                body[..Math.Min(200, body.Length)]
            );
            return null;
        }
        
        // Safe deserialization with exception handling
        try
        {
            return await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
        }
        catch (Exception ex)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                ex,
                "Error deserializing registration response. Body snippet: {Snippet}",
                body[..Math.Min(500, body.Length)]
            );
            return null;
        }
    }
    
    return null;
}
```

**Benefits:**

- ✅ Content-Type validation prevents non-JSON mismatches
- ✅ Exception handling gracefully handles deserialization errors
- ✅ Detailed logging (body snippet) helps debugging
- ✅ Returns `null` instead of crashing

### Prevention

- ✅ Vždycky validuj Content-Type v HTTP responses
- ✅ Wrap deserialization do try-catch
- ✅ Loguj response body snippet (ne celý body - může být velký)
- ✅ Pokud je response null, UI by měla zobrazit generic error message

---

## 4. Update Strategy for Production

### Phase 1: Preparation (0.5 hours)

1. **Code Validation lokálně:**

   ```bash
   cd ~/mimm-app
   git status                 # Ensure clean working directory
   dotnet build -c Release    # Full build test
   dotnet test                # Run all tests
   ```

2. **Commit to main:**

   ```bash
   git add -A
   git commit -m "feat(auth): update registration endpoint with proper JWT handling"
   git push origin main
   ```

3. **Verify GitHub Actions passing:**
   - Check CI workflow status
   - All tests must pass
   - No warnings or errors

### Phase 2: VPS Deployment (1 hour)

```bash
# SSH na VPS
ssh -p 2222 mimm@188.245.68.164

# Deployment script
cd ~/mimm-app

# 1. Pull latest code
git pull origin main

# 2. Show what changed
git diff HEAD~1 HEAD -- src/MIMM.Backend src/MIMM.Frontend

# 3. Verify code contains fixes
grep -n "AuthenticationResponse" src/MIMM.Backend/Services/AuthService.cs | head -5

# 4. Docker rebuild with cache-bust
docker compose -f docker-compose.prod.yml down
docker system prune -af --volumes
docker build --build-arg CACHEBUST=$(date +%s) -t mimm-backend:latest .
docker compose -f docker-compose.prod.yml up -d

# 5. Wait for initialization
sleep 30

# 6. Health check
curl -s http://localhost:8080/health | grep -q "healthy" && echo "✅ Backend ready" || echo "❌ Backend not ready"

# 7. Test endpoint directly
curl -s -X POST http://127.0.0.1:8080/api/auth/register \
  -H 'Content-Type: application/json' \
  --data-raw '{"email":"test'$(date +%s)'@example.com","password":"Test1234","displayName":"Test","language":"en"}' \
  | grep -q "accessToken" && echo "✅ Backend returns new structure" || echo "❌ Backend still returns old structure"

# 8. Check database
docker exec mimm-postgres psql -U mimmuser -d mimm -c "SELECT COUNT(*) FROM \"Users\";"
```

### Phase 3: Frontend Testing (0.5 hours)

1. **Clear browser cache:**
   - Hard refresh (Cmd+Shift+R or Ctrl+Shift+R)
   - DevTools → Application → Clear Storage

2. **Test registration flow:**
   - Go to https://musicinmymind.app
   - Register with new email (e.g., `test-$(date +%s)@example.com`)
   - **Expected:** No JsonException, redirect to dashboard
   - **Verify:** DevTools → Network tab shows `{"accessToken":"...","refreshToken":"...","user":{...}}`

3. **Test login flow:**
   - Logout (if already logged in)
   - Login with registered credentials
   - **Expected:** Redirect to dashboard, no errors

4. **Monitor console for errors:**
   - DevTools → Console
   - Expected: No red errors, only normal logs

### Phase 4: Rollback Plan (if something fails)

```bash
# SSH na VPS
ssh -p 2222 mimm@188.245.68.164
cd ~/mimm-app

# Go back to previous commit
git reset --hard HEAD~1
git push origin main -f        # Careful! Only for emergency rollback

# Redeploy old version
docker compose -f docker-compose.prod.yml down
docker rmi mimm-backend:latest
docker build --build-arg CACHEBUST=$(date +%s) -t mimm-backend:latest .
docker compose -f docker-compose.prod.yml up -d

# Verify
sleep 30
curl -s http://localhost:8080/health
```

---

## 5. Checklist for Future Updates

### Before Deployment

- [ ] All tests passing locally (`dotnet test`)
- [ ] Build succeeds in Release mode (`dotnet build -c Release`)
- [ ] Code review completed (at least 1 approved)
- [ ] No breaking changes in API contracts
- [ ] Database migrations tested (`dotnet ef database update`)
- [ ] Git history clean (no merge commits unless necessary)

### During Deployment

- [ ] SSH session stable (no interruptions)
- [ ] Docker build completes without errors
- [ ] Backend container reaches healthy status (check logs)
- [ ] Health check passes (`/health` returns 200)
- [ ] API endpoint test passes (registration returns `accessToken`)
- [ ] Database migrations applied successfully

### After Deployment

- [ ] Frontend hard refresh works (no old WASM DLL cached)
- [ ] Registration flow succeeds end-to-end
- [ ] Login flow succeeds end-to-end
- [ ] No console errors in DevTools
- [ ] No 5xx errors in backend logs
- [ ] Response times normal (<500ms for API calls)
- [ ] Monitor backend logs for 1 hour (watch for any errors)

### Rollback Trigger

Rollback immediately if:

- ❌ Backend won't start (Error in logs)
- ❌ API returns 500 errors (Internal Server Error)
- ❌ Database connection fails
- ❌ Multiple registration failures in logs
- ❌ Frontend shows persistent JsonException

---

## 6. VPS SSH Session Management

### Problem

Shell scripts with interactive prompts hang when run via SSH non-interactively.

### Solution

**Use single-line commands instead of multi-line scripts:**

```bash
# ❌ BAD - hangs on multi-line
ssh -p 2222 mimm@188.245.68.164 << 'EOF'
cd ~/mimm-app
git pull
docker compose down
EOF

# ✅ GOOD - single command
ssh -p 2222 mimm@188.245.68.164 'cd ~/mimm-app && git pull && docker compose -f docker-compose.prod.yml down'

# ✅ GOOD - echo pipe for non-interactive input
echo "MIMMpassword" | ssh -p 2222 mimm@188.245.68.164 'sudo -S docker ps' 2>/dev/null
```

### Recommended Pattern

```bash
set -e  # Exit on first error

SSH_CMD="ssh -p 2222 mimm@188.245.68.164"

echo "Step 1: Pull latest code"
$SSH_CMD 'cd ~/mimm-app && git pull origin main'

echo "Step 2: Docker build"
$SSH_CMD 'cd ~/mimm-app && docker build --build-arg CACHEBUST=$(date +%s) -t mimm-backend:latest .'

echo "Step 3: Restart services"
$SSH_CMD 'cd ~/mimm-app && docker compose -f docker-compose.prod.yml up -d'

echo "Step 4: Verify"
$SSH_CMD 'sleep 30 && curl -s http://localhost:8080/health'
```

---

## Summary

| Issue | Solution | Status |
|-------|----------|--------|
| Docker cache not invalidating | ARG CACHEBUST=$(date +%s) | ✅ Implemented |
| Backend returns UserDto not AuthenticationResponse | Updated RegisterAsync + Controller | ✅ Implemented |
| Frontend JsonException on registration | Added Content-Type validation + try-catch | ✅ Implemented |
| SSH session hangs | Use single-line commands with && chaining | ✅ Documented |

**Production status:** ✅ **Ready for users**

- Backend correctly returns JWT tokens
- Frontend handles responses gracefully
- Docker deployment reliable with cache-bust
- All errors documented and handled

---

**Last updated:** 27. ledna 2026  
**Next review:** After next production update
