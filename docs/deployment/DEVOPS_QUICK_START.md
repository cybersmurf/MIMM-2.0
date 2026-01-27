# MIMM 2.0 - DevOps Quick Start Guide

**Cíl:** Rychlá reference pro deploy, update, monitoring, rollback  
**Audience:** DevOps engineers  
**Time:** 5 minutes to learn, 15 minutes to deploy

---

## 🚀 60-Second Deploy (After Code Changes)

```bash
# Spusť na VPS
ssh -p 2222 mimm@188.245.68.164 '
  cd ~/mimm-app && \
  git pull origin main && \
  docker compose -f docker-compose.prod.yml down && \
  docker rmi mimm-backend:latest 2>/dev/null || true && \
  docker build --build-arg CACHEBUST=$(date +%s) -t mimm-backend:latest . && \
  docker compose -f docker-compose.prod.yml up -d && \
  sleep 30 && \
  curl -s http://localhost:8080/health
'
```

**Očekávaný výsledek:**

```json
{"status":"healthy","timestamp":"2026-01-27T..."}
```

---

## 📋 Pre-Deploy Checklist (Local)

```bash
cd ~/MIMM-2.0

# 1. Tests pass?
dotnet test MIMM.sln --configuration Release --no-build -v minimal
# ✅ Expected: X test(s) passed

# 2. Build succeeds?
dotnet build MIMM.sln -c Release
# ✅ Expected: Build succeeded. 0 Warning(s)

# 3. No hardcoded secrets?
git diff HEAD~1 HEAD | grep -i "password\|apikey\|secret" | wc -l
# ✅ Expected: 0

# 4. Ready to go?
echo "✅ All checks passed! Ready for deploy."
```

---

## 🔧 Common Operations

### 1. Deploy New Version

```bash
# Option A: Quick (assumes tests already run)
ssh -p 2222 mimm@188.245.68.164 'cd ~/mimm-app && git pull && docker build --build-arg CACHEBUST=$(date +%s) -t mimm-backend:latest . && docker compose -f docker-compose.prod.yml up -d'

# Option B: Safe (with full cleanup)
ssh -p 2222 mimm@188.245.68.164 '
  cd ~/mimm-app
  git pull origin main
  docker compose -f docker-compose.prod.yml down
  docker system prune -af --volumes
  docker build --build-arg CACHEBUST=$(date +%s) -t mimm-backend:latest .
  docker compose -f docker-compose.prod.yml up -d
  sleep 30
  curl -s http://localhost:8080/health | grep -q healthy && echo "✅ OK" || echo "❌ FAIL"
'
```

### 2. Verify Deployment

```bash
# Health check
curl -s https://api.musicinmymind.app/health | grep -q healthy && echo "✅ PASS" || echo "❌ FAIL"

# Test registration endpoint
curl -s -X POST https://api.musicinmymind.app/api/auth/register \
  -H 'Content-Type: application/json' \
  --data-raw '{"email":"test@example.com","password":"Test1234","displayName":"Test","language":"en"}' \
  | grep -q "accessToken" && echo "✅ JWT working" || echo "❌ JWT broken"
```

### 3. View Logs

```bash
# Last 50 lines
ssh -p 2222 mimm@188.245.68.164 'docker logs -n 50 mimm-backend'

# Follow logs in real-time (Ctrl+C to stop)
ssh -p 2222 mimm@188.245.68.164 'docker logs -f mimm-backend'

# Errors only
ssh -p 2222 mimm@188.245.68.164 'docker logs mimm-backend | grep -i error'
```

### 4. Check Database

```bash
# User count
ssh -p 2222 mimm@188.245.68.164 'docker exec mimm-postgres psql -U mimmuser -d mimm -c "SELECT COUNT(*) FROM \"Users\";"'

# List users
ssh -p 2222 mimm@188.245.68.164 'docker exec mimm-postgres psql -U mimmuser -d mimm -c "SELECT \"Id\", \"Email\", \"DisplayName\", \"CreatedAt\" FROM \"Users\" LIMIT 10;"'

# Backup database
ssh -p 2222 mimm@188.245.68.164 'docker exec mimm-postgres pg_dump -U mimmuser -d mimm > backup-$(date +%Y%m%d-%H%M%S).sql && ls -lh backup-*.sql | tail -1'
```

### 5. Check Resources

```bash
# Disk usage
ssh -p 2222 mimm@188.245.68.164 'df -h /home'

# Docker resource usage
ssh -p 2222 mimm@188.245.68.164 'docker stats --no-stream'

# Container status
ssh -p 2222 mimm@188.245.68.164 'docker compose -f ~/mimm-app/docker-compose.prod.yml ps'
```

### 6. Restart Services

```bash
# Restart one service
ssh -p 2222 mimm@188.245.68.164 'docker compose -f ~/mimm-app/docker-compose.prod.yml restart backend'

# Restart all services
ssh -p 2222 mimm@188.245.68.164 'docker compose -f ~/mimm-app/docker-compose.prod.yml restart'

# Stop all services
ssh -p 2222 mimm@188.245.68.164 'docker compose -f ~/mimm-app/docker-compose.prod.yml down'

# Start all services
ssh -p 2222 mimm@188.245.68.164 'docker compose -f ~/mimm-app/docker-compose.prod.yml up -d'
```

### 7. Rollback to Previous Version

```bash
# Quick rollback
ssh -p 2222 mimm@188.245.68.164 '
  cd ~/mimm-app
  git checkout v26.1.28
  docker compose -f docker-compose.prod.yml down
  docker build --build-arg CACHEBUST=$(date +%s) -t mimm-backend:latest .
  docker compose -f docker-compose.prod.yml up -d
  sleep 30
  curl -s http://localhost:8080/health
  git checkout main
'

# Or with database restore
ssh -p 2222 mimm@188.245.68.164 '
  cd ~/mimm-app
  git checkout v26.1.28
  docker compose -f docker-compose.prod.yml down
  docker build --build-arg CACHEBUST=$(date +%s) -t mimm-backend:latest .
  docker compose -f docker-compose.prod.yml up -d
  sleep 10
  # Restore database
  docker exec -i mimm-postgres psql -U mimmuser -d mimm < backup-latest.sql
  git checkout main
'
```

---

## ⚠️ Emergency Procedures

### Backend Not Starting

```bash
# Check logs
ssh -p 2222 mimm@188.245.68.164 'docker logs mimm-backend | tail -50'

# If config error (missing environment variables)
ssh -p 2222 mimm@188.245.68.164 'docker compose -f ~/mimm-app/docker-compose.prod.yml config'

# If code error
ssh -p 2222 mimm@188.245.68.164 '
  cd ~/mimm-app
  git log --oneline -3
  git diff HEAD~1 HEAD -- src/MIMM.Backend | head -100
'

# Rollback immediately
ssh -p 2222 mimm@188.245.68.164 'cd ~/mimm-app && git checkout v26.1.28 && docker compose -f docker-compose.prod.yml down && docker build --build-arg CACHEBUST=$(date +%s) -t mimm-backend:latest . && docker compose -f docker-compose.prod.yml up -d'
```

### Database Connection Lost

```bash
# Check postgres logs
ssh -p 2222 mimm@188.245.68.164 'docker logs mimm-postgres | tail -50'

# Check disk space (might be full)
ssh -p 2222 mimm@188.245.68.164 'df -h'

# Restart postgres
ssh -p 2222 mimm@188.245.68.164 'docker compose -f ~/mimm-app/docker-compose.prod.yml restart postgres'

# Wait and check
ssh -p 2222 mimm@188.245.68.164 'sleep 10 && docker logs mimm-postgres | tail -20'

# If still failing, restore from backup
ssh -p 2222 mimm@188.245.68.164 '
  ls -lh ~/mimm-app/backup-*.sql | tail -3
  docker exec -i mimm-postgres psql -U mimmuser -d mimm < ~/mimm-app/backup-latest.sql
'
```

### High Memory Usage

```bash
# Check what's using memory
ssh -p 2222 mimm@188.245.68.164 'docker stats --no-stream'

# Likely culprit: old Docker images/containers
ssh -p 2222 mimm@188.245.68.164 'docker system prune -af --volumes'

# If still high: restart backend
ssh -p 2222 mimm@188.245.68.164 'docker compose -f ~/mimm-app/docker-compose.prod.yml restart backend'
```

---

## 📊 Monitoring Commands

### Every Hour

```bash
# Health check
curl -s https://api.musicinmymind.app/health

# Response time
time curl -s https://api.musicinmymind.app/api/entries | wc -c

# Error count in logs (last 1 hour)
ssh -p 2222 mimm@188.245.68.164 'docker logs --since 1h mimm-backend | grep -i error | wc -l'
```

### Every Day

```bash
# Backup database
ssh -p 2222 mimm@188.245.68.164 'cd ~/mimm-app && docker exec mimm-postgres pg_dump -U mimmuser -d mimm | gzip > backups/backup-$(date +\%Y\%m\%d).sql.gz && ls -lh backups/ | tail -5'

# Check disk usage
ssh -p 2222 mimm@188.245.68.164 'df -h | grep home'

# Verify both databases still in sync
ssh -p 2222 mimm@188.245.68.164 'docker exec mimm-postgres psql -U mimmuser -d mimm -c "SELECT COUNT(*) FROM \"Users\", \"Entries\", \"Moods\";"'
```

### Weekly

```bash
# Update Docker images to latest base versions
ssh -p 2222 mimm@188.245.68.164 '
  docker pull mcr.microsoft.com/dotnet/aspnet:9.0
  docker pull postgres:16-alpine
  docker pull redis:7-alpine
  docker images
'

# Check certificate expiration
echo | openssl s_client -servername api.musicinmymind.app -connect api.musicinmymind.app:443 2>/dev/null | openssl x509 -noout -dates
```

---

## 🔐 Security Operations

### SSH Key Setup (First Time)

```bash
# Generate key on your machine
ssh-keygen -t ed25519 -C "devops@musicinmymind.app"

# Add to VPS authorized_keys
ssh-copy-id -p 2222 mimm@188.245.68.164

# Verify passwordless works
ssh -p 2222 mimm@188.245.68.164 'echo "✅ SSH key works"'

# Disable password auth
ssh -p 2222 mimm@188.245.68.164 'sudo sed -i "s/PasswordAuthentication yes/PasswordAuthentication no/g" /etc/ssh/sshd_config && sudo systemctl reload sshd'
```

### Change SSH Password (if needed)

```bash
ssh -p 2222 mimm@188.245.68.164 'passwd'
# Enter old password, then new password twice
```

### View Recent Deployments

```bash
# Show last 5 deployments (git tags)
ssh -p 2222 mimm@188.245.68.164 'cd ~/mimm-app && git tag -l | sort -V | tail -5'

# Show who deployed what when
ssh -p 2222 mimm@188.245.68.164 'cd ~/mimm-app && git log --oneline -20 | grep -E "fix|feat|perf"'
```

---

## 📚 Reference Links

- **Full Production Issues Guide:** [PRODUCTION_ISSUES_AND_FIXES.md](PRODUCTION_ISSUES_AND_FIXES.md)
- **Detailed Update Strategy:** [UPDATE_STRATEGY.md](UPDATE_STRATEGY.md)
- **Docker Operations Guide:** [DOCKER_OPERATIONS.md](DOCKER_OPERATIONS.md)
- **Deployment Checklist:** [DEPLOYMENT_CHECKLIST.md](DEPLOYMENT_CHECKLIST.md)

---

## 🎯 Key Rules

✅ **DO:**

- Always run tests before deploy
- Use `--build-arg CACHEBUST=$(date +%s)` in Docker builds
- Backup database before updates
- Monitor logs for 1 hour after deploy
- Rollback immediately if anything fails

❌ **DON'T:**

- Deploy without tests passing
- Modify code directly on VPS
- Delete database backups
- Use `docker rm -f` without backup
- Deploy during peak hours without notice

---

**Last Updated:** 27. ledna 2026  
**Status:** READY FOR PRODUCTION  
**Questions?** Check full docs in `/docs/deployment/`
