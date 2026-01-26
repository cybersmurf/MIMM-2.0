# VPS Deployment - Real-World Troubleshooting Guide

> **Status:** Based on actual Jan 26, 2026 deployment on Hetzner Ubuntu 24.04 (rootless Docker)
> **Issues Found:** 3 critical gaps in original checklist

---

## Problem #1: Postgres Permissions in Rootless Docker

### ❌ What Went Wrong
```
chmod: /var/lib/postgresql/data: Operation not permitted
chmod: /var/run/postgresql: Operation not permitted
initdb: error: could not change permissions of directory "/var/lib/postgresql/data": Operation not permitted
```

### Root Cause
- Original compose file had `user: "999:999"` on postgres service
- Rootless Docker runs container with UID 1000 (your user), not root
- User 999 (postgres inside container) can't chown a volume mounted from UID 1000

### ✅ Fix Applied
Removed `user: "999:999"` from postgres service in docker-compose.prod.yml
- Allows postgres entrypoint to run as root inside the container
- The container's root (inside UTS namespace) has permissions to chown `/var/lib/postgresql/data`
- Security: Still safe because rootless Docker isolates the namespace

### How to Detect This on Your VPS
```bash
docker compose -f docker-compose.prod.yml logs postgres | grep -i "permission\|chmod"
```

If you see permission errors → check for explicit `user:` overrides on postgres

---

## Problem #2: dotnet ef Not in Runtime Image

### ❌ What Went Wrong
```
The application 'ef' does not exist.
No .NET SDKs were found.
```

### Root Cause
- Production Dockerfile uses `mcr.microsoft.com/dotnet/aspnet:9.0` (runtime only)
- Runtime image doesn't include SDK tools like `dotnet-ef`
- Can't run `docker compose exec backend dotnet ef database update`

### ✅ Workaround: Use SDK Container for Migrations
```bash
docker run --rm \
  --env-file .env \
  --network mimm-app_default \
  -v "$PWD":/src \
  -w /src \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e PATH="/root/.dotnet/tools:$PATH" \
  mcr.microsoft.com/dotnet/sdk:9.0 \
  bash -c "dotnet restore MIMM.sln && dotnet tool install --global dotnet-ef && dotnet ef database update --project src/MIMM.Backend/MIMM.Backend.csproj --startup-project src/MIMM.Backend/MIMM.Backend.csproj --configuration Release"
```

**Why this works:**
- SDK container can access the network (`--network mimm-app_default`)
- Can access .env variables
- Can reach postgres via docker network (postgres:5432)
- SDK has all tools needed for EF Core migrations

### Alternative: Pre-run Migrations in CI/CD
Consider adding a migration step to GitHub Actions before Docker build, so you ship migrations already applied.

---

## Problem #3: Nginx Not Yet Tackled

### What's Missing
1. Nginx config for rootless backend (port 8080, not 5001)
2. SSL/TLS setup (Let's Encrypt)
3. Frontend static files serving
4. Reverse proxy for SignalR (`/hubs/`)
5. Health check endpoint

### Files You Need to Create
- `/etc/nginx/sites-available/mimm-backend` (proxy to localhost:8080)
- `/etc/nginx/sites-available/mimm-frontend` (serve static files)
- Certbot setup for HTTPS
- DNS A records pointing to VPS IP

---

## Critical Checklist Gaps

| Item | Original Checklist | Reality | Fix |
|------|-------------------|---------|-----|
| Postgres permissions | "Just works" | FAILS in rootless | Remove `user:` override |
| EF Core migrations | `docker compose exec` | No SDK in runtime | Use SDK container or pre-run |
| Nginx config | Basic template provided | Port 8080 vs 5001 confusion | Add rootless-specific example |
| DNS setup | Mentioned in theory | Critical blocker | Move to BEFORE Docker up |
| SSL/TLS | "Run certbot" | Needs running Nginx first | Specify order |
| Frontend build | "Already compiled" | WHERE? How compiled? | Document build process |
| .env security | "chmod 600" | Not version-controlled | Provide template only |
| Testing after deploy | Smoke test only | How to debug if fails? | Add debug commands |

---

## What You Actually Need (in order)

### Phase 0: Pre-Flight (Do BEFORE deploying)
- [ ] Domain registered, DNS A record → VPS IP
- [ ] SSH keys generated on local machine
- [ ] `.env` template filled with secrets (keep locally, never in git)
- [ ] Frontend built (Blazor WASM output to `/wwwroot`)

### Phase 1: Server Setup (Do once)
- [ ] SSH hardening (port 2222, no root login, etc.)
- [ ] UFW firewall (allow 22/2222, 80, 443)
- [ ] Rootless Docker installed and tested

### Phase 2: Nginx Setup (BEFORE Docker services)
- [ ] Nginx installed
- [ ] HTTP → HTTPS redirect configured
- [ ] Certbot SSL certs obtained
- [ ] Reverse proxy to backend (port 8080)
- [ ] Frontend static files served
- [ ] SignalR support configured

### Phase 3: Application Deployment
- [ ] Git clone / SFTP upload
- [ ] `.env` file created on VPS (not from git)
- [ ] Docker images built
- [ ] Services up (postgres, redis, backend)
- [ ] **Migrations run** (SDK container method)
- [ ] Backend health check: `curl http://localhost:8080/health`
- [ ] Nginx reverse proxy working: `curl https://api.your-domain.com/health`

### Phase 4: Verification
- [ ] Frontend loads: `https://your-domain.com`
- [ ] Backend API responds: `https://api.your-domain.com/health`
- [ ] Can register new user
- [ ] Can authenticate and get JWT token
- [ ] Logs clean (no errors): `docker compose logs --tail=50`

---

## Commands You'll Actually Run (Copypasta Ready)

### On Local Machine
```bash
# Pre-deployment
ssh-keygen -t ed25519 -C "your@email.com"  # if not done
scp ~/.ssh/id_ed25519.pub user@VPS_IP:.ssh/authorized_keys
```

### On VPS (as root first)
```bash
apt update && apt upgrade -y
adduser mimm && usermod -aG sudo mimm
# Edit /etc/ssh/sshd_config → Port 2222, PermitRootLogin no, PasswordAuthentication no
systemctl restart sshd

ufw allow 2222/tcp && ufw allow 80/tcp && ufw allow 443/tcp && ufw enable
```

### On VPS (as mimm user)
```bash
# Rootless Docker
su - mimm
curl -fsSL https://get.docker.com/rootless | sh
export PATH=/home/mimm/bin:$PATH
export DOCKER_HOST=unix:///run/user/$(id -u)/docker.sock
echo "export PATH=/home/mimm/bin:\$PATH" >> ~/.bashrc
echo "export DOCKER_HOST=unix:///run/user/\$(id -u)/docker.sock" >> ~/.bashrc

# Nginx
sudo apt install -y nginx
sudo systemctl enable nginx

# Clone repo
cd ~ && git clone https://github.com/your/mimm.git mimm-app
cd mimm-app

# Copy .env (create it with your secrets, NEVER commit it)
nano .env
# Fill in: POSTGRES_PASSWORD, REDIS_PASSWORD, JWT_SECRET_KEY, FRONTEND_URL, BACKEND_URL, API keys

# Build images
docker compose -f docker-compose.prod.yml build

# Start postgres + redis (wait for healthy)
docker compose -f docker-compose.prod.yml up -d postgres redis
sleep 30 && docker compose -f docker-compose.prod.yml ps

# Run migrations (THIS IS THE TRICKY PART)
docker run --rm \
  --env-file .env \
  --network mimm-app_default \
  -v "$PWD":/src \
  -w /src \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e PATH="/root/.dotnet/tools:$PATH" \
  mcr.microsoft.com/dotnet/sdk:9.0 \
  bash -c "dotnet restore MIMM.sln && dotnet tool install --global dotnet-ef && dotnet ef database update --project src/MIMM.Backend/MIMM.Backend.csproj --startup-project src/MIMM.Backend/MIMM.Backend.csproj --configuration Release"

# Start backend
docker compose -f docker-compose.prod.yml up -d backend

# Test
curl http://localhost:8080/health

# Setup Nginx (more detailed in separate section)
# [See Phase 2 above for nginx configs]

# Final test
curl https://api.your-domain.com/health
```

---

## How to Debug When Things Go Wrong

### Postgres won't start
```bash
docker compose -f docker-compose.prod.yml logs postgres | tail -50
docker compose -f docker-compose.prod.yml down
docker volume rm mimm-app_postgres_data
docker compose -f docker-compose.prod.yml up -d postgres
```

### Backend won't reach postgres
```bash
docker compose -f docker-compose.prod.yml logs backend | grep -i "connection\|host\|fail"
# Check backend logs for connection string errors
docker exec -it mimm-postgres psql -U mimmuser -d mimm -c "\dt"  # List tables
```

### Backend unhealthy
```bash
docker compose -f docker-compose.prod.yml ps  # Check status
docker compose -f docker-compose.prod.yml logs backend -f  # Follow logs
curl http://localhost:8080/health  # Test locally
```

### Nginx not working
```bash
sudo nginx -t  # Test config
sudo journalctl -u nginx -n 50  # Nginx logs
curl -v http://localhost  # Test locally (should redirect to https)
curl -v https://api.your-domain.com/health  # Test through Nginx
```

### Port conflicts
```bash
sudo lsof -i :80
sudo lsof -i :443
sudo lsof -i :8080
```

---

## Next Steps (Immediate)

1. **Fix the migration command** - run the SDK container command above on your VPS
2. **Verify postgres is healthy** - `docker compose ps`
3. **Verify backend is up** - `curl http://localhost:8080/health`
4. **Setup Nginx** - see detailed guide in separate file
5. **Deploy frontend** - serve static Blazor WASM build

**Do NOT** attempt to move forward until all three containers are healthy.

---

## Lessons Learned

1. **Rootless Docker is finicky with volumes** - User ID mappings matter
2. **EF Core in production** - Need SDK container for migrations, not included in runtime
3. **Sequential vs. Parallel** - Some steps MUST happen in order (DNS → certs → nginx → app)
4. **Testing is non-optional** - Can't just assume containers will start

---

## Key Differences: Dev vs. Production

| Aspect | Development | Production |
|--------|-------------|-----------|
| Docker user | root (usually) | non-root (1000:1000 in rootless) |
| Migrations | `dotnet ef` from IDE | SDK container before `up -d` |
| Networking | localhost + host.docker.internal | Docker internal network only |
| Ports | Direct access (5001, 5432) | Nginx reverse proxy, firewall |
| SSL/TLS | Self-signed or skipped | Let's Encrypt required |
| Secrets | appsettings.json + user-secrets | .env file (gitignored) |
| Monitoring | Visual Studio debug | Container logs + health checks |

