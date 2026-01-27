# VPS Deployment Guide - MIMM 2.0 (.NET 9.0)

## Prerequisites

- SSH access to VPS (`ssh user@musicinmymind.app`)
- Docker and Docker Compose installed
- PostgreSQL 16+ (or via Docker)
- Git installed
- .NET 9.0 SDK (if building locally on VPS, not required if using Docker)

## Quick Start (Recommended: Docker)

### 1. On Your Local Machine

Pull latest code and push to GitHub:

```bash
cd ~/AntigravityProjects/MIMM-2.0
git pull origin main
git status  # Verify all committed
```

### 2. SSH to VPS

```bash
ssh user@musicinmymind.app
cd ~/mimm-app
```

### 3. Pull Latest Code from GitHub

```bash
git pull origin main
# Verify Dockerfile uses .NET 9.0 SDK and aspnet 9.0 runtime
grep -E "sdk:9.0|aspnet:9.0" Dockerfile
```

### 4. Build and Start Containers (Docker)

```bash
# Clean any old images/containers
docker-compose down

# Build backend API (Dockerfile uses .NET 9.0)
docker build -f Dockerfile -t mimm-backend:v1.0 .

# Start PostgreSQL + Redis (from docker-compose.yml)
docker-compose up -d postgres redis

# Run backend API container
docker run -d \
  --name mimm-api \
  --network=mimm-net \
  -p 8080:8080 \
  -p 8081:8081 \
  -e ConnectionStrings__DefaultConnection="Host=postgres;Database=mimm_app;Username=mimm_user;Password=SECURE_PASSWORD_HERE" \
  -e Jwt__Key="SECURE_JWT_KEY_HERE" \
  -e Jwt__Issuer="https://api.musicinmymind.app" \
  -e Jwt__Audience="https://musicinmymind.app" \
  mimm-backend:v1.0
```

### 5. Deploy Frontend (Static Assets via Nginx)

```bash
# Publish Blazor WASM frontend locally
dotnet publish src/MIMM.Frontend/MIMM.Frontend.csproj -c Release -o ~/publish-output

# Copy wwwroot to Nginx on VPS
scp -r ~/publish-output/wwwroot/* user@musicinmymind.app:/var/www/mimm-frontend/

# Or via rsync (faster for updates)
rsync -av ~/publish-output/wwwroot/ user@musicinmymind.app:/var/www/mimm-frontend/
```

### 6. Verify Nginx Configuration

Ensure `/etc/nginx/sites-available/musicinmymind` contains:

```nginx
server {
    listen 443 ssl http2;
    server_name musicinmymind.app;

    ssl_certificate /etc/letsencrypt/live/musicinmymind.app/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/musicinmymind.app/privkey.pem;

    # Frontend (Blazor WASM)
    location / {
        root /var/www/mimm-frontend;
        index index.html;
        try_files $uri /index.html;  # SPA routing
    }

    # Backend API proxy
    location /api/ {
        proxy_pass http://localhost:8080/api/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # SignalR hub
    location /signalr {
        proxy_pass http://localhost:8080/signalr;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    }

    # Health check endpoint
    location /health {
        proxy_pass http://localhost:8080/health;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
    }
}

server {
    listen 80;
    server_name musicinmymind.app;
    return 301 https://$server_name$request_uri;
}
```

### 7. Test Endpoints

```bash
# API Health Check
curl -k https://api.musicinmymind.app/health

# Frontend Login Page
curl -s https://musicinmymind.app/login | grep -o '<title>.*</title>'

# SignalR Hub Test (should respond with 200 if available)
curl -k -X GET https://api.musicinmymind.app/signalr/negotiate
```

## Troubleshooting

### Docker Image Build Fails

**Error:** `NETSDK1045: Current .NET SDK does not support net10.0`

**Solution:** Verify Dockerfile uses .NET 9.0:

```bash
grep FROM Dockerfile
# Should show: mcr.microsoft.com/dotnet/sdk:9.0 and mcr.microsoft.com/dotnet/aspnet:9.0
```

### Frontend Shows Blank Page

**Causes:**
1. Browser cache - Clear cache (Ctrl+Shift+Delete)
2. API URL incorrect - Check `appsettings.json` in wwwroot:

```bash
cat /var/www/mimm-frontend/appsettings.json
# Should contain: "ApiBaseUrl": "https://api.musicinmymind.app"
```

3. Nginx not serving `index.html` - Verify `try_files $uri /index.html;` in nginx config

### API Returns 404 for /api/* Endpoints

**Cause:** Nginx proxy_pass misconfigured

**Solution:** Verify:

```bash
curl -v -k https://api.musicinmymind.app/api/auth/register

# Nginx logs
sudo tail -20 /var/log/nginx/error.log
```

### Database Connection Fails

**Cause:** PostgreSQL not running or credentials incorrect

**Solution:**

```bash
# Check Docker container
docker ps | grep postgres

# Test connection
docker exec mimm-postgres psql -U mimm_user -d mimm_app -c "SELECT 1"

# If failed, restart
docker-compose down && docker-compose up -d postgres
```

## Monitoring

### View Logs

```bash
# Backend API logs
docker logs mimm-api -f

# Nginx logs
sudo tail -f /var/log/nginx/access.log

# PostgreSQL logs
docker logs mimm-postgres -f
```

### Restart Components

```bash
# Restart API
docker restart mimm-api

# Restart all services
docker-compose restart postgres redis
```

### Health Checks

```bash
# API Health
curl -k https://api.musicinmymind.app/health

# Frontend Availability
curl -I https://musicinmymind.app/

# Database
psql -h localhost -U mimm_user -d mimm_app -c "SELECT version();"
```

## Rollback Procedure

If deployment fails:

```bash
# Stop new container
docker stop mimm-api
docker rm mimm-api

# Restore previous version
git checkout HEAD~1

# Rebuild and restart
docker build -f Dockerfile -t mimm-backend:v1.0-prev .
docker run ... mimm-backend:v1.0-prev
```

## Notes

- **Version:** .NET 9.0 (stable LTS)
- **Domains:** API = `api.musicinmymind.app`, Frontend = `musicinmymind.app`
- **Ports:** API = 8080/8081 (rootless), PostgreSQL = 5432 (internal), Redis = 6379 (internal)
- **SSL:** Let's Encrypt (auto-renew via certbot)
- **Database:** PostgreSQL 16 (Docker container or external)
- **Cache:** Redis (optional, for session/notifications)
