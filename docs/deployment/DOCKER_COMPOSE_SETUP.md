# Docker Compose Setup Guide - MIMM 2.0

**Status:** ✅ Working & Tested (27.1.2026)  
**Tested on:** macOS 14.x, Docker Desktop, Docker Compose v2.x

---

## Prerequisites

- Docker Desktop installed and running
- `.env` file created with production secrets (see below)
- Git repository cloned locally

---

## Step 1: Create `.env` File

Create file `.env` in project root with your credentials:

```bash
# PostgreSQL Configuration
POSTGRES_USER=mimm
POSTGRES_PASSWORD=YOUR_SECURE_PASSWORD_HERE
POSTGRES_DB=mimm

# JWT Authentication
# Generate key: openssl rand -base64 64
JWT_SECRET_KEY=HaBTAZysXEcWLMaXMeelHiHHcJu/8+fwhm81kD3aKGc=
JWT_ISSUER=https://api.musicinmymind.app
JWT_AUDIENCE=mimm-frontend

# Application URLs
FRONTEND_URL=https://musicinmymind.app
BACKEND_URL=https://api.musicinmymind.app

# Environment (Development = auto-migrate DB, Production = manual)
ASPNETCORE_ENVIRONMENT=Development
```

**⚠️ IMPORTANT:**
- Add `.env` to `.gitignore` - **NEVER commit secrets**
- `POSTGRES_USER` and `POSTGRES_PASSWORD` are used by PostgreSQL container to create credentials
- Same values are passed to backend via environment variables for connection string building
- `JWT_SECRET_KEY` must be at least 32 characters (can generate with `openssl rand -base64 64`)

---

## Step 2: Build and Start Services

```bash
# Build backend image (includes SDK and publish)
docker compose build

# Start all services (postgres, redis, backend)
docker compose up -d

# Check container status
docker compose ps
```

**Expected Output:**
```
NAME            STATUS
mimm-backend    Up 5 seconds
mimm-postgres   Up 15 seconds (healthy)
mimm-redis      Up 15 seconds (healthy)
```

---

## Step 3: Verify Backend Health

```bash
# Health check endpoint
curl http://localhost:8080/health

# Expected response:
{"status":"healthy","timestamp":"2026-01-27T19:17:07.1053324Z"}

# Check database tables (auto-migrated)
docker compose exec postgres psql -U mimm -d mimm -c "SELECT tablename FROM pg_tables WHERE schemaname='public' ORDER BY tablename;"

# Expected: 9 tables (Users, Entries, Friendships, MusicBrainzArtists, etc.)
```

---

## Step 4: Test API Endpoints

```bash
# Register new user
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "TestPassword123!",
    "displayName": "Test User"
  }'

# Expected response:
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "...",
  "user": {
    "id": "...",
    "email": "test@example.com",
    "displayName": "Test User"
  }
}

# Login
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "TestPassword123!"
  }'
```

---

## Troubleshooting

### Problem: "password authentication failed for user"

**Cause:** Old PostgreSQL volume still has old credentials

**Solution:**
```bash
docker compose down -v  # Remove all volumes including DB data
docker compose up -d    # Fresh start with new credentials
```

### Problem: Backend not connecting to PostgreSQL

**Symptoms:** Backend healthy but DB shows no tables

**Checks:**
```bash
# Verify env variables are passed to backend
docker compose exec backend env | grep POSTGRES

# Should see:
# POSTGRES_HOST=postgres
# POSTGRES_USER=mimm
# POSTGRES_PASSWORD=...
# POSTGRES_DB=mimm

# Check backend logs
docker compose logs backend --tail 50
```

### Problem: Migrations not running

**Cause:** `ASPNETCORE_ENVIRONMENT` must be `Development` for auto-migration

**Fix:** Check `.env` contains:
```
ASPNETCORE_ENVIRONMENT=Development
```

Then:
```bash
docker compose restart backend
docker compose logs backend | grep -i "migrat"
```

### Problem: Port already in use

**Solution:**
```bash
# Find process using port 8080
lsof -i :8080

# Or change port in docker-compose.yml:
# ports:
#   - "8081:8080"  # Maps host:8081 to container:8080
```

---

## Useful Commands

```bash
# View logs for specific service
docker compose logs backend --tail 100 -f

# Execute command in running container
docker compose exec backend ls -la /app

# Database shell
docker compose exec postgres psql -U mimm -d mimm

# Stop services (keeps volumes)
docker compose stop

# Stop and remove everything (keeps volumes)
docker compose down

# Stop and remove everything INCLUDING data
docker compose down -v

# Restart specific service
docker compose restart backend

# Rebuild specific service
docker compose build --no-cache backend
```

---

## Architecture

```
docker-compose.yml configuration:
├── postgres:16-alpine
│   ├── Port: 5432
│   ├── Credentials: ${POSTGRES_USER} / ${POSTGRES_PASSWORD}
│   └── Volume: postgres_data (persistent)
├── redis:7-alpine
│   ├── Port: 6379
│   └── Volume: redis_data (persistent)
└── backend (custom image)
    ├── Port: 8080
    ├── Environment: POSTGRES_* variables (passed from .env)
    ├── Depends on: postgres (healthcheck)
    └── Volumes: ./logs (container logs)
```

---

## Network Communication

Inside containers:
- Backend connects to PostgreSQL at `postgres:5432` (service name resolves via Docker DNS)
- Backend connects to Redis at `redis:6379`
- Frontend (future) will connect to backend at `http://localhost:8080` (host machine) or `http://backend:8080` (from other containers)

**CORS Configuration in Backend:**
- `http://localhost:5000` - Local development frontend
- `http://localhost:8080` - Allow from backend itself
- `${FRONTEND_URL}` - Production frontend URL (from `.env`)

---

## Next: Frontend Deployment

After backend is running and verified:

```bash
# Publish frontend
dotnet publish src/MIMM.Frontend/MIMM.Frontend.csproj -c Release -o ./publish/frontend

# Deploy to VPS Nginx:
# 1. Copy wwwroot to `/var/www/mimm-frontend`
# 2. Update Nginx reverse proxy configuration
# 3. Test: https://musicinmymind.app
```

See [NGINX_SETUP_DETAILED.md](./NGINX_SETUP_DETAILED.md) for complete frontend deployment guide.

---

## Maintenance

### Regular Operations

```bash
# Check all services running
docker compose ps

# View resource usage
docker stats mimm-backend mimm-postgres

# Clear logs (from all stopped containers)
docker container prune
```

### Upgrading

```bash
# Update base images
docker compose pull

# Rebuild application
docker compose build --no-cache

# Restart services
docker compose down
docker compose up -d
```

### Backup Database

```bash
# Backup PostgreSQL
docker compose exec postgres pg_dump -U mimm mimm > backup_$(date +%Y%m%d_%H%M%S).sql

# Restore from backup
docker compose exec -T postgres psql -U mimm mimm < backup_20260127_191500.sql
```

---

## Production Checklist

- [ ] `.env` file created and git-ignored
- [ ] `.env` values are production-safe (strong JWT key, DB password, etc.)
- [ ] `ASPNETCORE_ENVIRONMENT=Production` before production deploy
- [ ] Database backups scheduled
- [ ] Health checks monitoring in place
- [ ] Logs aggregation configured (see backend log file at `./logs/`)
- [ ] Frontend deployed and CORS configured
- [ ] SSL/TLS certificates configured at Nginx reverse proxy
- [ ] Rate limiting enabled on API endpoints
- [ ] Database read replicas or clustering for HA

---

**Last Updated:** 27. ledna 2026 | **Version:** 1.0
