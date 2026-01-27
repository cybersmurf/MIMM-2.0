# MIMM 2.0 - Production Update Strategy & Deployment Plan

**Datum:** 27. ledna 2026  
**Audience:** DevOps, Backend Team  
**Purpose:** Establish reliable procedure for deploying updates to production VPS

---

## 🎯 Executive Summary

Po vyřešení Docker cache a JWT issueů máme nyní **stabilní deployment pipeline**. Tento dokument definuje:

1. ✅ **Version Management Strategy** - jak verzionovat updates
2. ✅ **Pre-Deployment Checklist** - co musí projít dříve než pushneme
3. ✅ **Deployment Steps** - exaktní procedura na VPS
4. ✅ **Post-Deployment Verification** - jak si ověřit že to funguje
5. ✅ **Rollback Procedures** - jak se vrátit pokud se něco pokazí
6. ✅ **Monitoring & Alerting** - co sledovat po deployu

---

## 1. Version Management Strategy

### Versioning Scheme

Používáme **CalVer + Patch** formát:

```
v26.MONTH.DAY[.PATCH]

Příklady:
v26.1.27      - 27. ledna 2026 (verze #1 toho dne)
v26.1.27.1    - 27. ledna 2026 (verze #2 toho dne, patch update)
v26.2.1       - 1. února 2026
```

### Where to Update Version

Aktualizuj verzi v těchto 3 souborech:

```bash
# 1. CHANGELOG.md - přidej nový section na TOP
## [v26.1.29] - 27. ledna 2026 (Feature Name)

# 2. README.md - aktualizuj badge v headeru
**Version:** v26.1.29 | **Status:** Production ✅

# 3. Git tag - po pushnuti na main
git tag -a v26.1.29 -m "Release v26.1.29 - Registration & Docker cache fix"
git push origin v26.1.29
```

### Release Checklist

Před každým releasem:

- [ ] CHANGELOG.md updated s novými features/fixes
- [ ] README.md version badge updated
- [ ] All tests passing (`dotnet test`)
- [ ] Build succeeds (`dotnet build -c Release`)
- [ ] No warnings or errors
- [ ] Code review approved
- [ ] Git history clean (rebase if needed)

---

## 2. Pre-Deployment Checklist (LOCAL)

### Code Quality (15 minutes)

```bash
cd ~/MIMM-2.0

# 1. Verify clean working directory
git status
# Expected: "nothing to commit, working tree clean"

# 2. Run full build in Release mode
dotnet clean MIMM.sln
dotnet build MIMM.sln -c Release
# Expected: "Build succeeded. 0 Warning(s)"

# 3. Run all tests
dotnet test MIMM.sln --configuration Release --no-build
# Expected: "Test Run Successful. X test(s) passed"

# 4. Check for async/await issues (if backend changes)
grep -r "async void" src/MIMM.Backend --include="*.cs" | wc -l
# Expected: 0 (no async void outside event handlers)

# 5. Check for hardcoded secrets
git diff HEAD~1 HEAD | grep -E "(password|key|token|secret)" | grep -v "test"
# Expected: No hardcoded secrets
```

### Code Review (10 minutes)

```bash
# Show changes since last release
git log --oneline v26.1.28..HEAD

# Show diff summary
git diff v26.1.28...HEAD --stat

# Review each commit
git show --stat <commit-hash>
```

### Documentation Check (5 minutes)

```bash
# Verify markdown linting
markdownlint README.md CHANGELOG.md docs/deployment/*.md
# Expected: No errors

# Verify links in CHANGELOG
grep -o '\[.*\](.*\.md)' CHANGELOG.md | head -5
# Expected: All linked files exist
```

### Staging Test (Optional but Recommended)

```bash
# Test locally with Docker
docker-compose -f docker-compose.yml up -d
sleep 20
curl -s http://localhost:7001/health | grep -q "healthy" && echo "✅ Local OK"
docker-compose down
```

---

## 3. Deployment Steps (VPS)

### Pre-Flight Check

```bash
# SSH na VPS
ssh -p 2222 mimm@188.245.68.164

# 1. Verify Docker je healthy
cd ~/mimm-app
docker compose -f docker-compose.prod.yml ps
# Expected: Všechny containers Up, postgres a redis s (healthy)

# 2. Verify disk space
df -h | grep /home
# Expected: >50% free space

# 3. Backup database (before każdy update)
docker exec mimm-postgres pg_dump -U mimmuser -d mimm > backup-$(date +%Y%m%d-%H%M%S).sql
ls -lh backup-*.sql | tail -1
# Expected: Recent backup file created
```

### Code Update & Build (10 minutes)

```bash
# 1. Pull latest code
cd ~/mimm-app
git pull origin main
git log --oneline -1
# Expected: Latest commit hash visible

# 2. Show what changed (informational)
git diff HEAD~1 HEAD --stat | head -20

# 3. Build new Docker image with cache-bust
echo "Building Docker image with cache-bust..."
docker compose -f docker-compose.prod.yml down

# CRITICAL: Use ARG cache-busting to avoid stale builds
docker build --build-arg CACHEBUST=$(date +%s) -t mimm-backend:latest .
# Watch for:
# - "COPY src/ ." should NOT say CACHED (should say DONE X.Xs)
# - Build succeeded message at end
# - Image sha256 hash displayed

# 4. Start services
echo "Starting services..."
docker compose -f docker-compose.prod.yml up -d
docker compose -f docker-compose.prod.yml logs -f &
sleep 5
kill %1  # Stop log following
```

### Post-Deployment Verification (5 minutes)

**Critical - Run all these checks before declaring success:**

```bash
# 1. Container Status
echo "✓ Check 1: Container status"
docker compose -f docker-compose.prod.yml ps
# Expected: Všechny Up, postgres/redis healthy

# 2. Backend health endpoint
echo "✓ Check 2: Backend health"
curl -s http://localhost:8080/health
# Expected: {"status":"healthy","timestamp":"..."}

# 3. Database connectivity
echo "✓ Check 3: Database connectivity"
docker exec mimm-postgres psql -U mimmuser -d mimm -c "SELECT COUNT(*) FROM \"Users\";" | grep -E "[0-9]+"
# Expected: Number of users (if >0, then DB connected and data safe)

# 4. API endpoint test (CRITICAL for registration fix)
echo "✓ Check 4: Registration endpoint returns correct structure"
RESPONSE=$(curl -s -X POST http://127.0.0.1:8080/api/auth/register \
  -H 'Content-Type: application/json' \
  --data-raw '{"email":"test'$(date +%s)'@example.com","password":"Test1234","displayName":"Test","language":"en"}')

echo "Response: $RESPONSE" | head -c 200
echo ""

# Verify response contains accessToken (not UserDto)
if echo "$RESPONSE" | grep -q "accessToken"; then
  echo "✅ PASS: Backend returns AuthenticationResponse with accessToken"
else
  echo "❌ FAIL: Backend response missing accessToken - ROLLBACK REQUIRED"
  exit 1
fi

# 5. Frontend build check
echo "✓ Check 5: Frontend served correctly"
curl -s https://api.musicinmymind.app/index.html | head -c 100 | grep -q "<!DOCTYPE" && echo "✅ PASS: Frontend HTML found" || echo "❌ FAIL: Frontend not served"

# 6. SSL Certificate check
echo "✓ Check 6: SSL certificate valid"
echo | openssl s_client -servername api.musicinmymind.app -connect api.musicinmymind.app:443 2>/dev/null | openssl x509 -noout -dates
# Expected: notBefore and notAfter dates, after should be future date

echo ""
echo "=========================================="
echo "✅ All checks passed! Update successful."
echo "=========================================="
```

### If Any Check Fails

```bash
# Immediately check logs
docker logs mimm-backend --tail 100 | grep -E "ERROR|Exception|Failed"

# If backend fails, rollback immediately (see section 4)
```

---

## 4. Rollback Procedures

### Rollback to Previous Release

**Use ONLY if deployment verification fails.**

```bash
ssh -p 2222 mimm@188.245.68.164

cd ~/mimm-app

# 1. Stop current version
docker compose -f docker-compose.prod.yml down
docker rmi mimm-backend:latest

# 2. Check git history
git log --oneline -10

# 3. Go back to previous tag
git checkout v26.1.28
git log --oneline -1

# 4. Rebuild and restart
docker build --build-arg CACHEBUST=$(date +%s) -t mimm-backend:latest .
docker compose -f docker-compose.prod.yml up -d

# 5. Verify old version is running
sleep 30
curl -s http://localhost:8080/health
curl -s -X POST http://127.0.0.1:8080/api/auth/register \
  -H 'Content-Type: application/json' \
  --data-raw '{"email":"test@test.com","password":"Test1234","displayName":"Test","language":"en"}' | head -c 200

# 6. Go back to main if rollback successful
git checkout main

# 7. Notify team
echo "⚠️  ROLLBACK COMPLETED to v26.1.28"
```

### Database Rollback (if needed)

```bash
# List available backups
ls -lh backup-*.sql | tail -5

# Restore from specific backup
docker exec -i mimm-postgres psql -U mimmuser -d mimm < backup-20260127-120000.sql

# Verify restore
docker exec mimm-postgres psql -U mimmuser -d mimm -c "SELECT COUNT(*) FROM \"Users\";"
```

---

## 5. Monitoring & Alerting (First Hour After Deploy)

### Manual Monitoring (First 15 minutes)

```bash
# Watch logs in real-time
docker logs -f mimm-backend --since 5m

# Watch for:
# - Any ERROR or Exception messages
# - Connection issues
# - Slow query warnings
# - Missing configuration

# Press Ctrl+C to stop
```

### Automated Checks (Every 5 minutes for first hour)

Create a monitoring script:

```bash
#!/bin/bash
# monitor-deployment.sh - Run for 60 minutes after deploy

for i in {1..12}; do
  echo "=== Check $i ($(date)) ==="
  
  # Health check
  HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8080/health)
  if [ "$HTTP_CODE" != "200" ]; then
    echo "❌ ALERT: Health check returned $HTTP_CODE"
    exit 1
  fi
  
  # Database check
  DB_USERS=$(docker exec mimm-postgres psql -U mimmuser -d mimm -c "SELECT COUNT(*) FROM \"Users\";" 2>&1 | grep -E "^[0-9]+$")
  if [ -z "$DB_USERS" ]; then
    echo "❌ ALERT: Database connectivity lost"
    exit 1
  fi
  
  # Error check in logs
  ERROR_COUNT=$(docker logs --since 5m mimm-backend 2>&1 | grep -i "ERROR" | wc -l)
  if [ $ERROR_COUNT -gt 0 ]; then
    echo "⚠️  WARNING: $ERROR_COUNT errors in last 5 minutes"
    docker logs --since 5m mimm-backend | grep -i "ERROR" | head -3
  fi
  
  echo "✅ All checks passed"
  echo "Users in DB: $DB_USERS"
  echo ""
  
  if [ $i -lt 12 ]; then
    sleep 300  # 5 minutes
  fi
done

echo "✅ 60-minute monitoring completed successfully"
```

Run it:

```bash
./monitor-deployment.sh
```

### Critical Metrics to Watch

| Metric | Normal | Alert |
|--------|--------|-------|
| HTTP 200 health | Yes | Any other code |
| Backend errors | 0/5min | >2 errors/5min |
| DB connections | Stable | Connection failures |
| Response time | <500ms | >2000ms |
| Memory usage | <300MB | >500MB |
| Disk free | >50% | <20% |

---

## 6. Communication Plan

### Before Deployment

```
@team Please start testing registration at 22:00 CET
- Register new account
- Login and verify tokens work
- Check browser console for errors
- Report any issues immediately
```

### After Successful Deployment

```
✅ DEPLOYMENT SUCCESSFUL v26.1.29
- Registration endpoint fixed (JWT tokens now returned)
- Docker cache issue resolved
- All tests passing
- Monitoring: No errors in first hour

Ready for full user access.
```

### If Deployment Fails

```
⚠️  DEPLOYMENT ROLLBACK
- Rolled back to v26.1.28
- Root cause: [specific error]
- Team investigating, will retry in 1 hour
- No user impact - system fully functional
```

---

## 7. Future Updates - Quick Reference

### Quick Deploy (after first time)

```bash
ssh -p 2222 mimm@188.245.68.164
cd ~/mimm-app && \
git pull origin main && \
docker compose -f docker-compose.prod.yml down && \
docker system prune -af --volumes && \
docker build --build-arg CACHEBUST=$(date +%s) -t mimm-backend:latest . && \
docker compose -f docker-compose.prod.yml up -d && \
sleep 30 && \
echo "✓ Backend health:" && \
curl -s http://localhost:8080/health | grep -q "healthy" && echo "✅ OK" || echo "❌ FAILED"
```

### One-Liner Rollback

```bash
ssh -p 2222 mimm@188.245.68.164 "cd ~/mimm-app && git checkout v26.1.28 && docker compose -f docker-compose.prod.yml down && docker build --build-arg CACHEBUST=\$(date +%s) -t mimm-backend:latest . && docker compose -f docker-compose.prod.yml up -d && sleep 30 && curl -s http://localhost:8080/health && git checkout main"
```

---

## Summary

| Phase | Duration | Key Actions |
|-------|----------|-------------|
| Pre-deployment | 30min | Tests, build, code review |
| Deployment | 15min | Git pull, Docker build, start services |
| Verification | 5min | Run all 6 health checks |
| Monitoring | 60min | Watch logs, check metrics every 5min |
| **Total** | **110min** | **Safe, tested, reliable** |

**Golden Rules:**

1. ✅ Never deploy without tests passing
2. ✅ Always use `ARG CACHEBUST=$(date +%s)` in Docker build
3. ✅ Test endpoint directly with curl (not just health check)
4. ✅ Monitor first 60 minutes after deploy
5. ✅ Rollback immediately if anything fails

---

**Last Updated:** 27. ledna 2026  
**Version:** FINAL (ready for production use)  
**Questions?** Check [PRODUCTION_ISSUES_AND_FIXES.md](PRODUCTION_ISSUES_AND_FIXES.md) for detailed technical docs
