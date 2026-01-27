# 🚀 VPS Deployment Action Plan - MIMM 2.0 (.NET 9.0)

**Status:** ✅ Ready for Deployment  
**Version:** 2.0.1 (net9.0)  
**Date:** January 27, 2026  
**Target:** musicinmymind.app + api.musicinmymind.app

---

## ✅ Pre-Deployment Checklist (COMPLETED)

- [x] Downgrade entire codebase to .NET 9.0 (from 10.0)
- [x] Fix .NET 9.0 build issues locally
- [x] Update Dockerfile to use .NET 9.0 SDK & aspnet:9.0 runtime
- [x] Verify frontend publishes successfully (WASM build)
- [x] Run all application tests (40/40 passing)
- [x] Commit all changes to GitHub
- [x] Create comprehensive VPS deployment guide
- [x] Update VPS scripts (backend, frontend, full-stack)

---

## 📋 Step-by-Step Deployment (For VPS)

### Phase 1: Preparation (5 min)

#### 1.1 SSH to VPS

```bash
ssh user@musicinmymind.app
cd ~/mimm-app
```

#### 1.2 Verify Git Status

```bash
git status
# Expected: "On branch main" (clean)

git log --oneline -n 3
# Expected: Latest commits from GitHub including .NET 9.0 downgrade
```

#### 1.3 Verify .NET Availability

```bash
dotnet --version
# Expected: .NET 9.0.xxx (or higher in 9.x series)

# If needed, install .NET 9.0
# Ubuntu: sudo apt install dotnet-sdk-9.0
# RHEL: sudo dnf install dotnet-sdk-9.0
```

### Phase 2: Code Update (2 min)

#### 2.1 Pull Latest Code

```bash
git pull origin main
# Expected: "Already up to date" or displays pulled commits
```

#### 2.2 Verify Dockerfile is .NET 9.0

```bash
grep "FROM" Dockerfile
# Expected output:
# FROM mcr.microsoft.com/dotnet/sdk:9.0 as build
# FROM mcr.microsoft.com/dotnet/aspnet:9.0

grep -E "sdk:9|aspnet:9" Dockerfile || echo "ERROR: Not using .NET 9.0"
```

#### 2.3 Clean Previous Build Artifacts

```bash
rm -rf src/*/bin src/*/obj
rm -rf tests/*/bin tests/*/obj
echo "Build cache cleared"
```

### Phase 3: Backend Build & Deployment (10 min)

#### 3.1 Build Docker Image

```bash
docker build -f Dockerfile -t mimm-backend:v2.0.1 .
# Expected: "Successfully tagged mimm-backend:v2.0.1"
# Time: ~3-5 minutes
```

#### 3.2 Stop Old API Container

```bash
docker stop mimm-api || echo "No running container"
docker rm mimm-api || echo "No existing container"
```

#### 3.3 Start New API Container

```bash
docker run -d \
  --name mimm-api \
  --network=mimm-net \
  --restart=unless-stopped \
  -p 8080:8080 \
  -p 8081:8081 \
  -e "ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=mimm_app;Username=mimm_user;Password=$(cat ~/.mimm-secrets/db-password)" \
  -e "Jwt__Key=$(cat ~/.mimm-secrets/jwt-key)" \
  -e "Jwt__Issuer=https://api.musicinmymind.app" \
  -e "Jwt__Audience=https://musicinmymind.app" \
  -e "LastFm__ApiKey=$(cat ~/.mimm-secrets/lastfm-api-key)" \
  -e "ASPNETCORE_ENVIRONMENT=Production" \
  mimm-backend:v2.0.1

# Verify running
docker ps | grep mimm-api
```

#### 3.4 Test API Health

```bash
sleep 3  # Wait for startup
curl -k https://api.musicinmymind.app/health

# Expected response: {"status":"Healthy"} or similar
```

### Phase 4: Frontend Deployment (8 min)

#### 4.1 Publish Frontend Locally (on your machine)

```bash
# On your LOCAL machine (macOS):
cd ~/AntigravityProjects/MIMM-2.0
dotnet publish src/MIMM.Frontend/MIMM.Frontend.csproj \
  -c Release \
  -o ~/publish-frontend-v2.0.1
```

#### 4.2 Copy to VPS (via rsync)

```bash
# From your LOCAL machine:
rsync -avz --delete \
  ~/publish-frontend-v2.0.1/wwwroot/ \
  user@musicinmymind.app:/var/www/mimm-frontend/

# Verify on VPS:
ssh user@musicinmymind.app "ls -lah /var/www/mimm-frontend/ | head -10"
# Should show index.html, appsettings.json, _framework, etc.
```

#### 4.3 Verify appsettings.json on VPS

```bash
ssh user@musicinmymind.app "cat /var/www/mimm-frontend/appsettings.json"

# Should contain:
# {"apiBaseUrl":"https://api.musicinmymind.app"}
```

#### 4.4 Clear Nginx Cache

```bash
ssh user@musicinmymind.app "sudo systemctl reload nginx"

# Verify Nginx status
ssh user@musicinmymind.app "sudo systemctl status nginx"
```

### Phase 5: Verification (5 min)

#### 5.1 Test API Endpoints

```bash
# Health check
curl -k https://api.musicinmymind.app/health
# Expected: 200 OK

# SignalR negotiate
curl -k https://api.musicinmymind.app/signalr/negotiate
# Expected: 200 OK (or 404 if not configured, but should not 500)

# Auth endpoint (should reject invalid creds, not 500)
curl -X POST https://api.musicinmymind.app/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"wrong"}' \
  -k | jq .
# Expected: 401 Unauthorized or similar
```

#### 5.2 Test Frontend

```bash
# In browser, visit: https://musicinmymind.app
# Expected:
#  1. Page loads (no blank screen)
#  2. Login form visible
#  3. Browser console has no critical errors (F12)
#  4. Network tab shows assets loaded from api.musicinmymind.app

# Clear browser cache if needed:
# Chrome: Ctrl+Shift+Delete
# Safari: Cmd+Shift+Delete
# Firefox: Ctrl+Shift+Delete
```

#### 5.3 Check Logs

```bash
# API logs
docker logs mimm-api | tail -20
# Should show: listening on port 8080, no errors

# Nginx logs
ssh user@musicinmymind.app "sudo tail -20 /var/log/nginx/access.log"
# Should show GET / requests with 200 status
```

---

## 🔄 Rollback Procedure (if needed)

If deployment fails:

```bash
# 1. Revert code to previous commit
git log --oneline -n 3  # Find previous stable version
git checkout <previous-commit-hash>

# 2. Rebuild and restart
docker build -f Dockerfile -t mimm-backend:previous .
docker stop mimm-api && docker rm mimm-api
docker run ... (see Phase 3.3) ... mimm-backend:previous

# 3. Restore frontend from backup
# (assuming you kept previous wwwroot backup)
rsync -avz ~/publish-frontend-previous/wwwroot/ \
  user@musicinmymind.app:/var/www/mimm-frontend/

# 4. Verify
curl https://api.musicinmymind.app/health
```

---

## 📊 Deployment Checklist (to track)

- [ ] SSH access confirmed
- [ ] Git pull completed
- [ ] .NET version verified (9.0.xxx)
- [ ] Dockerfile uses .NET 9.0 ✓
- [ ] Build artifacts cleaned
- [ ] Docker image built successfully
- [ ] Old API container stopped
- [ ] New API container running
- [ ] API health check passed
- [ ] Frontend published locally
- [ ] Frontend assets copied to VPS
- [ ] appsettings.json on VPS is correct
- [ ] Nginx reloaded
- [ ] API endpoints responding
- [ ] Frontend page loads
- [ ] Browser console has no critical errors
- [ ] Database accessible
- [ ] All logs clean

---

## 🚨 Troubleshooting Commands

```bash
# API won't start
docker logs mimm-api -f  # Check error logs
docker ps -a            # List all containers

# Frontend blank page
curl -I https://musicinmymind.app/  # Check Nginx
cat /var/www/mimm-frontend/appsettings.json  # Verify config
sudo tail -f /var/log/nginx/error.log  # Check errors

# Database connection fails
docker ps | grep postgres
docker logs mimm-postgres
psql -h localhost -U mimm_user -d mimm_app -c "SELECT 1"

# Clear Docker cache
docker system prune -a  # Remove all unused images/containers
docker volume prune     # Remove unused volumes

# Restart everything
docker-compose down
docker-compose up -d postgres redis
# Then rebuild & restart API
```

---

## 📝 Post-Deployment Tasks

- [ ] Monitor logs for 30 minutes (no errors expected)
- [ ] Test user registration flow (register → login → dashboard)
- [ ] Test mood entry creation
- [ ] Test Last.fm search integration
- [ ] Test notifications (if enabled)
- [ ] Update CHANGELOG.md with deployment date
- [ ] Create GitHub release tag (v2.0.1)
- [ ] Document any configuration changes in `.github/deployment-log.txt`

---

## 🎯 Success Criteria

✅ **Deployment is successful when:**

1. API health endpoint responds 200 OK
2. Frontend loads without errors
3. User can register & login
4. Dashboard displays mood entries
5. No error logs in docker logs or nginx error.log
6. All APIs respond within 200-500ms

❌ **Rollback if:**

1. API crashes on startup (docker logs show errors)
2. Frontend returns blank page
3. Any 5xx errors on API endpoints
4. Database connection fails
5. Authentication broken

---

## 📞 Support

**In case of issues during deployment:**

1. Check logs first: `docker logs mimm-api` + nginx error.log
2. Review VPS_DEPLOYMENT_GUIDE.md for detailed troubleshooting
3. Check GitHub Actions CI for any related failures
4. Verify all environment variables are set correctly
5. Ensure PostgreSQL is running and accessible

---

**Deployment completed by:** _______________  
**Date:** _______________  
**Notes:** _______________
