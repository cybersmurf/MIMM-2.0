# MIMM 2.0 - Plán Nasazení na VPS (Hetzner Ubuntu 24.04)

> **Datum vytvoření:** 24. ledna 2026  
> **Cílový server:** VPS Hetzner (Ubuntu 24.04 LTS)  
> **Stack:** Docker + Nginx Reverse Proxy + ASP.NET Core 9  
> **Databáze:** PostgreSQL 16 + Redis

---

## 📋 Obsah

1. [Přehled Architektury](#1-přehled-architektury)
2. [Příprava VPS Serveru](#2-příprava-vps-serveru)
3. [Konfigurace Docker a Docker Compose](#3-konfigurace-docker-a-docker-compose)
4. [Nginx Reverse Proxy Setup](#4-nginx-reverse-proxy-setup)
5. [SSL/TLS Certifikáty (Let's Encrypt)](#5-ssltls-certifikáty-lets-encrypt)
6. [Aplikační Konfigurace](#6-aplikační-konfigurace)
7. [Deployment Pipeline](#7-deployment-pipeline)
8. [Monitoring a Logging](#8-monitoring-a-logging)
9. [Bezpečnost](#9-bezpečnost)
10. [Backup Strategie](#10-backup-strategie)
11. [Checklist před nasazením](#11-checklist-před-nasazením)

---

## 1. Přehled Architektury

### Současná Lokální Struktura
```
┌─────────────────────┐
│   Blazor Frontend   │
│   (Port 5000)       │
└─────────┬───────────┘
          │
┌─────────▼───────────┐
│  ASP.NET Backend    │
│   (Port 5001)       │
└─────────┬───────────┘
          │
    ┌─────┴─────┐
    │           │
┌───▼───┐  ┌───▼───┐
│  DB   │  │ Redis │
│ 5432  │  │ 6379  │
└───────┘  └───────┘
```

### Produkční Architektura na VPS
```
Internet (Port 80/443)
         │
         ▼
┌────────────────────┐
│  Nginx Reverse     │ ← SSL Termination
│  Proxy (Host OS)   │ ← Rate Limiting
└─────────┬──────────┘ ← Security Headers
          │
          │ Docker Network
          │
    ┌─────┴─────┐
    │           │
┌───▼───────┐ ┌─▼────────┐
│  Backend  │ │ Frontend │
│ Container │ │Container │
│ (5001)    │ │ (80)     │
└─────┬─────┘ └──────────┘
      │
  ┌───┴────┐
  │        │
┌─▼──────┐ ┌─▼──────┐
│ Postgres│ │ Redis  │
│Container│ │Container│
└─────────┘ └────────┘
```

---

## 2. Příprava VPS Serveru

### 2.1 Prvotní Přístup a Aktualizace

```bash
# SSH připojení k serveru
ssh root@<your-server-ip>

# Aktualizace systému
apt update && apt upgrade -y

# Instalace základních nástrojů
apt install -y \
  curl \
  wget \
  git \
  ufw \
  fail2ban \
  htop \
  nano \
  vim \
  unattended-upgrades
```

### 2.2 Vytvoření Non-Root Uživatele

```bash
# Vytvoření uživatele
adduser mimm
usermod -aG sudo mimm

# Nastavení SSH klíče pro nového uživatele
mkdir -p /home/mimm/.ssh
cp /root/.ssh/authorized_keys /home/mimm/.ssh/
chown -R mimm:mimm /home/mimm/.ssh
chmod 700 /home/mimm/.ssh
chmod 600 /home/mimm/.ssh/authorized_keys

# Testování přihlášení jako mimm (z lokálního PC)
# ssh mimm@<your-server-ip>
```

### 2.3 SSH Hardening

Editovat `/etc/ssh/sshd_config`:

```bash
# Zákaz root přihlášení
PermitRootLogin no

# Jen SSH klíče, ne hesla
PasswordAuthentication no
PubkeyAuthentication yes

# Změna SSH portu (volitelné, ale doporučené)
Port 2222  # nebo jakýkoliv jiný high port

# Další bezpečnostní nastavení
PermitEmptyPasswords no
X11Forwarding no
MaxAuthTries 3
ClientAliveInterval 300
ClientAliveCountMax 2
```

Restart SSH:
```bash
systemctl restart sshd
```

**⚠️ Důležité:** Po restartu SSH otevřete nové SSH spojení pro test PŘED odpojením původního!

### 2.4 Konfigurace Firewallu (UFW)

```bash
# Výchozí politiky
ufw default deny incoming
ufw default allow outgoing

# Povolit SSH (použij správný port!)
ufw allow 2222/tcp comment 'SSH'

# Povolit HTTP/HTTPS
ufw allow 80/tcp comment 'HTTP'
ufw allow 443/tcp comment 'HTTPS'

# Zapnutí firewallu
ufw enable

# Ověření stavu
ufw status verbose
```

### 2.5 Fail2Ban Konfigurace

```bash
# Instalace
apt install -y fail2ban

# Vytvoření lokální konfigurace
cat > /etc/fail2ban/jail.local << 'EOF'
[DEFAULT]
bantime = 3600
findtime = 600
maxretry = 5
destemail = your-email@example.com
sendername = Fail2Ban

[sshd]
enabled = true
port = 2222
logpath = /var/log/auth.log

[nginx-http-auth]
enabled = true
port = http,https
logpath = /var/log/nginx/error.log

[nginx-noscript]
enabled = true
port = http,https
logpath = /var/log/nginx/access.log

[nginx-badbots]
enabled = true
port = http,https
logpath = /var/log/nginx/access.log
maxretry = 2
EOF

# Restart Fail2Ban
systemctl restart fail2ban
systemctl enable fail2ban

# Status check
fail2ban-client status
```

### 2.6 Automatické Bezpečnostní Aktualizace

```bash
# Konfigurace unattended-upgrades
dpkg-reconfigure -plow unattended-upgrades

# Ověření konfigurace
cat /etc/apt/apt.conf.d/50unattended-upgrades
```

---

## 3. Konfigurace Docker a Docker Compose

### 3.1 Instalace Docker

```bash
# Instalace Docker pomocí oficiálního skriptu
curl -fsSL https://get.docker.com -o get-docker.sh
sh get-docker.sh

# Přidání uživatele do docker skupiny
usermod -aG docker mimm

# Zapnutí Docker služby
systemctl enable docker
systemctl start docker

# Ověření instalace
docker --version
docker run hello-world
```

### 3.2 Instalace Docker Compose

```bash
# Instalace Docker Compose
apt install -y docker-compose-plugin

# Ověření
docker compose version
```

### 3.3 Docker Security Hardening

```bash
# Vytvoření daemon.json pro Docker security
cat > /etc/docker/daemon.json << 'EOF'
{
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "10m",
    "max-file": "3"
  },
  "live-restore": true,
  "userland-proxy": false,
  "no-new-privileges": true
}
EOF

# Restart Docker
systemctl restart docker
```

### 3.4 Produkční Docker Compose

Vytvoření `/home/mimm/mimm-app/docker-compose.prod.yml`:

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:16-alpine
    container_name: mimm-postgres
    restart: unless-stopped
    environment:
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
      POSTGRES_DB: ${POSTGRES_DB}
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./backups:/backups
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER}"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - backend
    security_opt:
      - no-new-privileges:true
    cap_drop:
      - ALL
    cap_add:
      - CHOWN
      - SETUID
      - SETGID

  redis:
    image: redis:7-alpine
    container_name: mimm-redis
    restart: unless-stopped
    command: redis-server --requirepass ${REDIS_PASSWORD}
    volumes:
      - redis_data:/data
    healthcheck:
      test: ["CMD", "redis-cli", "--no-auth-warning", "-a", "${REDIS_PASSWORD}", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - backend
    security_opt:
      - no-new-privileges:true
    cap_drop:
      - ALL

  backend:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: mimm-backend
    restart: unless-stopped
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: http://+:5001
      ASPNETCORE_HTTP_PORTS: 5001
      ConnectionStrings__DefaultConnection: "Host=postgres;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD};SSL Mode=Prefer;"
      ConnectionStrings__Redis: "redis:6379,password=${REDIS_PASSWORD}"
      Jwt__Key: ${JWT_SECRET_KEY}
      Jwt__Issuer: ${JWT_ISSUER}
      Jwt__Audience: ${JWT_AUDIENCE}
      Cors__AllowedOrigins__0: ${FRONTEND_URL}
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
    volumes:
      - ./logs:/app/logs
    networks:
      - frontend
      - backend
    security_opt:
      - no-new-privileges:true
    cap_drop:
      - ALL
    read_only: true
    tmpfs:
      - /tmp
      - /app/logs

  frontend:
    image: nginx:alpine
    container_name: mimm-frontend
    restart: unless-stopped
    volumes:
      - ./frontend-build:/usr/share/nginx/html:ro
      - ./nginx-frontend.conf:/etc/nginx/conf.d/default.conf:ro
    networks:
      - frontend
    security_opt:
      - no-new-privileges:true
    cap_drop:
      - ALL
    cap_add:
      - CHOWN
      - SETUID
      - SETGID
      - NET_BIND_SERVICE

networks:
  frontend:
    driver: bridge
  backend:
    driver: bridge
    internal: true

volumes:
  postgres_data:
    driver: local
  redis_data:
    driver: local
```

### 3.5 Environment Variables (.env)

Vytvoření `/home/mimm/mimm-app/.env`:

```bash
# Database
POSTGRES_USER=mimmuser
POSTGRES_PASSWORD=CHANGE_THIS_STRONG_PASSWORD_123!
POSTGRES_DB=mimm

# Redis
REDIS_PASSWORD=CHANGE_THIS_REDIS_PASSWORD_456!

# JWT
JWT_SECRET_KEY=CHANGE_THIS_AT_LEAST_32_CHARACTERS_LONG_SECRET_KEY_789!
JWT_ISSUER=https://your-domain.com
JWT_AUDIENCE=mimm-frontend

# URLs
FRONTEND_URL=https://your-domain.com
BACKEND_URL=https://api.your-domain.com

# Email (SendGrid)
SENDGRID_API_KEY=your-sendgrid-api-key
SENDGRID_FROM_EMAIL=noreply@your-domain.com

# Last.fm
LASTFM_API_KEY=your-lastfm-api-key
LASTFM_SHARED_SECRET=your-lastfm-secret
```

**⚠️ Bezpečnost .env souboru:**
```bash
chmod 600 /home/mimm/mimm-app/.env
chown mimm:mimm /home/mimm/mimm-app/.env
```

---

## 4. Nginx Reverse Proxy Setup

### 4.1 Instalace Nginx na Host OS

```bash
apt install -y nginx

# Zastavení výchozího serveru
systemctl stop nginx
```

### 4.2 Vytvoření Nginx Konfigurace pro Backend

`/etc/nginx/sites-available/mimm-backend`:

```nginx
# Rate limiting zone
limit_req_zone $binary_remote_addr zone=api_limit:10m rate=10r/s;
limit_conn_zone $binary_remote_addr zone=addr:10m;

# Upstream backend
upstream mimm_backend {
    server 127.0.0.1:5001;
    keepalive 32;
}

# HTTP -> HTTPS redirect
server {
    listen 80;
    listen [::]:80;
    server_name api.your-domain.com;

    # ACME challenge pro Let's Encrypt
    location /.well-known/acme-challenge/ {
        root /var/www/certbot;
    }

    location / {
        return 301 https://$server_name$request_uri;
    }
}

# HTTPS server
server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name api.your-domain.com;

    # SSL certificates (po získání Let's Encrypt)
    ssl_certificate /etc/letsencrypt/live/api.your-domain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/api.your-domain.com/privkey.pem;
    ssl_trusted_certificate /etc/letsencrypt/live/api.your-domain.com/chain.pem;

    # SSL configuration (Mozilla Modern Profile)
    ssl_protocols TLSv1.3 TLSv1.2;
    ssl_ciphers 'ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384';
    ssl_prefer_server_ciphers off;
    ssl_session_cache shared:SSL:10m;
    ssl_session_timeout 10m;
    ssl_session_tickets off;

    # OCSP Stapling
    ssl_stapling on;
    ssl_stapling_verify on;
    resolver 8.8.8.8 8.8.4.4 valid=300s;
    resolver_timeout 5s;

    # Security headers
    add_header Strict-Transport-Security "max-age=63072000; includeSubDomains; preload" always;
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin" always;
    add_header Content-Security-Policy "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline';" always;

    # Hide Nginx version
    server_tokens off;

    # Logging
    access_log /var/log/nginx/mimm-backend-access.log;
    error_log /var/log/nginx/mimm-backend-error.log warn;

    # Client body size limit
    client_max_body_size 10M;

    # Rate limiting
    limit_req zone=api_limit burst=20 nodelay;
    limit_conn addr 10;

    # Proxy settings
    location / {
        proxy_pass http://mimm_backend;
        proxy_http_version 1.1;
        
        # Forwarded headers
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header X-Forwarded-Host $host;
        proxy_set_header X-Forwarded-Port $server_port;
        
        # Connection settings
        proxy_set_header Connection "";
        proxy_buffering off;
        proxy_request_buffering off;
        
        # Timeouts
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }

    # SignalR WebSocket support
    location /hubs/ {
        proxy_pass http://mimm_backend;
        proxy_http_version 1.1;
        
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        proxy_cache_bypass $http_upgrade;
        proxy_read_timeout 3600s;
        proxy_send_timeout 3600s;
    }

    # Health check endpoint (bez rate limitingu)
    location /health {
        limit_req off;
        limit_conn off;
        proxy_pass http://mimm_backend;
        access_log off;
    }
}
```

### 4.3 Vytvoření Nginx Konfigurace pro Frontend

`/etc/nginx/sites-available/mimm-frontend`:

```nginx
# HTTP -> HTTPS redirect
server {
    listen 80;
    listen [::]:80;
    server_name your-domain.com www.your-domain.com;

    # ACME challenge
    location /.well-known/acme-challenge/ {
        root /var/www/certbot;
    }

    location / {
        return 301 https://your-domain.com$request_uri;
    }
}

# WWW -> non-WWW redirect (HTTPS)
server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name www.your-domain.com;

    ssl_certificate /etc/letsencrypt/live/your-domain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/your-domain.com/privkey.pem;

    return 301 https://your-domain.com$request_uri;
}

# Main HTTPS server
server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name your-domain.com;

    # SSL configuration (stejné jako backend)
    ssl_certificate /etc/letsencrypt/live/your-domain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/your-domain.com/privkey.pem;
    ssl_trusted_certificate /etc/letsencrypt/live/your-domain.com/chain.pem;

    ssl_protocols TLSv1.3 TLSv1.2;
    ssl_ciphers 'ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384';
    ssl_prefer_server_ciphers off;
    ssl_session_cache shared:SSL:10m;
    ssl_session_timeout 10m;
    ssl_session_tickets off;

    ssl_stapling on;
    ssl_stapling_verify on;
    resolver 8.8.8.8 8.8.4.4 valid=300s;

    # Security headers
    add_header Strict-Transport-Security "max-age=63072000; includeSubDomains; preload" always;
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;

    server_tokens off;

    # Logging
    access_log /var/log/nginx/mimm-frontend-access.log;
    error_log /var/log/nginx/mimm-frontend-error.log warn;

    # Blazor WebAssembly specifics
    root /var/www/mimm-frontend;
    index index.html;

    # Compression
    gzip on;
    gzip_vary on;
    gzip_proxied any;
    gzip_comp_level 6;
    gzip_types text/plain text/css text/xml text/javascript application/json application/javascript application/xml+rss application/rss+xml font/truetype font/opentype application/vnd.ms-fontobject image/svg+xml;

    # Blazor static files with caching
    location / {
        try_files $uri $uri/ /index.html =404;
        
        # Cache static assets
        location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
            expires 1y;
            add_header Cache-Control "public, immutable";
        }
        
        # No cache for HTML
        location ~* \.html$ {
            add_header Cache-Control "no-store, no-cache, must-revalidate";
        }
    }

    # Blazor framework files
    location /_framework/ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }

    # Service Worker
    location /service-worker.js {
        add_header Cache-Control "no-cache, must-revalidate";
    }
}
```

### 4.4 Aktivace Site Konfigurací

```bash
# Vytvoření symlinků
ln -s /etc/nginx/sites-available/mimm-backend /etc/nginx/sites-enabled/
ln -s /etc/nginx/sites-available/mimm-frontend /etc/nginx/sites-enabled/

# Odstranění výchozí konfigurace
rm /etc/nginx/sites-enabled/default

# Test konfigurace
nginx -t

# Restart Nginx (později, po získání SSL certifikátů)
# systemctl restart nginx
```

---

## 5. SSL/TLS Certifikáty (Let's Encrypt)

### 5.1 Instalace Certbot

```bash
apt install -y certbot python3-certbot-nginx
```

### 5.2 Příprava pro ACME Challenge

```bash
# Vytvoření adresáře pro webroot
mkdir -p /var/www/certbot
chown -R www-data:www-data /var/www/certbot
```

### 5.3 Dočasná Nginx Konfigurace (HTTP only)

Pro získání prvních certifikátů upravte dočasně konfigurace aby poslouchaly pouze na portu 80.

Nebo použijte certbot standalone mode:

```bash
# Zastavit Nginx dočasně
systemctl stop nginx

# Získání certifikátů
certbot certonly --standalone \
  -d your-domain.com \
  -d www.your-domain.com \
  -d api.your-domain.com \
  --email your-email@example.com \
  --agree-tos \
  --no-eff-email

# Restart Nginx s plnou konfigurací
systemctl start nginx
```

### 5.4 Automatická Obnova Certifikátů

```bash
# Testování obnovy
certbot renew --dry-run

# Cron job je automaticky vytvořen při instalaci certbotu
# Ověření:
systemctl list-timers | grep certbot
```

### 5.5 Post-Renewal Hook

Vytvoření `/etc/letsencrypt/renewal-hooks/deploy/reload-nginx.sh`:

```bash
#!/bin/bash
systemctl reload nginx
```

```bash
chmod +x /etc/letsencrypt/renewal-hooks/deploy/reload-nginx.sh
```

---

## 6. Aplikační Konfigurace

### 6.1 Aktualizace appsettings.Production.json

`src/MIMM.Backend/appsettings.Production.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    },
    "Console": {
      "IncludeScopes": true
    }
  },
  "AllowedHosts": "your-domain.com;api.your-domain.com",
  
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Port=5432;Database=mimm;Username=mimmuser;Password=OVERRIDE_FROM_ENV",
    "Redis": "redis:6379,password=OVERRIDE_FROM_ENV"
  },
  
  "Jwt": {
    "Key": "OVERRIDE_FROM_ENV",
    "Issuer": "https://api.your-domain.com",
    "Audience": "mimm-frontend",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  
  "Cors": {
    "AllowedOrigins": [
      "https://your-domain.com"
    ]
  },
  
  "App": {
    "Name": "MIMM",
    "FrontendUrl": "https://your-domain.com",
    "EnableEmailVerification": true,
    "RequireEmailConfirmation": true
  },
  
  "SendGrid": {
    "ApiKey": "OVERRIDE_FROM_ENV",
    "FromEmail": "noreply@your-domain.com",
    "FromName": "MIMM Support"
  }
}
```

### 6.2 Aktualizace Program.cs pro Forwarded Headers

`src/MIMM.Backend/Program.cs` (přidání na začátek middleware pipeline):

```csharp
// Pokud ještě není, přidat na začátek souboru
using Microsoft.AspNetCore.HttpOverrides;

// ... existující kód ...

var app = builder.Build();

// DŮLEŽITÉ: Forwarded Headers MUSÍ být první middleware
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | 
                       ForwardedHeaders.XForwardedProto | 
                       ForwardedHeaders.XForwardedHost,
    KnownProxies = { }, // Proxy je na stejném stroji (localhost)
    KnownNetworks = { }
});

// ... zbytek middleware pipeline ...
```

### 6.3 Optimalizace Dockerfile pro Produkci

Aktualizace `Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/MIMM.Backend/MIMM.Backend.csproj", "MIMM.Backend/"]
COPY ["src/MIMM.Shared/MIMM.Shared.csproj", "MIMM.Shared/"]
RUN dotnet restore "MIMM.Backend/MIMM.Backend.csproj"

# Copy all source files
COPY src/ .

# Build the application
WORKDIR "/src/MIMM.Backend"
RUN dotnet build "MIMM.Backend.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "MIMM.Backend.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    --no-restore

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published files
COPY --from=publish /app/publish .

# Create logs directory with proper permissions
RUN mkdir -p /app/logs && chown -R appuser:appuser /app/logs

# Switch to non-root user
USER appuser

EXPOSE 5001

ENV ASPNETCORE_URLS=http://+:5001

ENTRYPOINT ["dotnet", "MIMM.Backend.dll"]
```

---

## 7. Deployment Pipeline

### 7.1 Manuální Deployment Skript

`/home/mimm/mimm-app/deploy.sh`:

```bash
#!/bin/bash
set -e

echo "🚀 Starting MIMM 2.0 Deployment..."

# Navigace do app složky
cd /home/mimm/mimm-app

# Pull nejnovějšího kódu
echo "📥 Pulling latest code from GitHub..."
git pull origin main

# Build Docker images
echo "🏗️ Building Docker images..."
docker compose -f docker-compose.prod.yml build --no-cache

# Zastavení starých kontejnerů
echo "🛑 Stopping old containers..."
docker compose -f docker-compose.prod.yml down

# Spuštění nových kontejnerů
echo "▶️ Starting new containers..."
docker compose -f docker-compose.prod.yml up -d

# Čekání na healthy status
echo "⏳ Waiting for services to be healthy..."
sleep 10

# Health check
echo "🏥 Running health checks..."
curl -f http://localhost:5001/health || echo "❌ Backend health check failed!"

# Clean up old images
echo "🧹 Cleaning up old Docker images..."
docker image prune -f

echo "✅ Deployment completed successfully!"
```

```bash
chmod +x /home/mimm/mimm-app/deploy.sh
```

### 7.2 Database Migrations Skript

`/home/mimm/mimm-app/migrate.sh`:

```bash
#!/bin/bash
set -e

echo "🗃️ Running database migrations..."

cd /home/mimm/mimm-app

# Spuštění migrací přes dočasný kontejner
docker compose -f docker-compose.prod.yml run --rm backend \
  dotnet ef database update --no-build

echo "✅ Migrations completed!"
```

```bash
chmod +x /home/mimm/mimm-app/migrate.sh
```

### 7.3 GitHub Actions CI/CD (volitelné)

`.github/workflows/deploy.yml`:

```yaml
name: Deploy to Production

on:
  push:
    branches: [ main ]
  workflow_dispatch:

jobs:
  deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Deploy to VPS
      uses: appleboy/ssh-action@master
      with:
        host: ${{ secrets.VPS_HOST }}
        username: ${{ secrets.VPS_USERNAME }}
        key: ${{ secrets.VPS_SSH_KEY }}
        port: 2222
        script: |
          cd /home/mimm/mimm-app
          ./deploy.sh
```

**GitHub Secrets potřebné:**
- `VPS_HOST`: IP adresa VPS
- `VPS_USERNAME`: `mimm`
- `VPS_SSH_KEY`: Privátní SSH klíč

---

## 8. Monitoring a Logging

### 8.1 Docker Logs

```bash
# Zobrazení logů všech služeb
docker compose -f docker-compose.prod.yml logs

# Sledování live logů
docker compose -f docker-compose.prod.yml logs -f

# Logy konkrétní služby
docker compose -f docker-compose.prod.yml logs backend

# Posledních 100 řádků
docker compose -f docker-compose.prod.yml logs --tail=100
```

### 8.2 Nginx Logs

```bash
# Access log
tail -f /var/log/nginx/mimm-backend-access.log

# Error log
tail -f /var/log/nginx/mimm-backend-error.log

# Rotace logů (automatická přes logrotate)
cat /etc/logrotate.d/nginx
```

### 8.3 System Monitoring

```bash
# Systémové zdroje
htop

# Docker stats
docker stats

# Disk space
df -h

# Služby status
systemctl status nginx
systemctl status docker
systemctl status fail2ban
```

### 8.4 Application Performance Monitoring (volitelné)

Integrace s Application Insights nebo Seq:

```bash
# Seq (self-hosted logging)
docker run -d \
  --name seq \
  --restart unless-stopped \
  -e ACCEPT_EULA=Y \
  -v /home/mimm/seq-data:/data \
  -p 5341:80 \
  datalust/seq:latest
```

---

## 9. Bezpečnost

### 9.1 Security Checklist

- ✅ Non-root Docker user
- ✅ Read-only filesystems where possible
- ✅ Capability dropping (CAP_DROP ALL)
- ✅ Security contexts (`no-new-privileges`)
- ✅ Network isolation (internal backend network)
- ✅ Secrets v environment variables (ne hardcoded)
- ✅ HTTPS only (HTTP -> HTTPS redirect)
- ✅ HSTS headers
- ✅ Security headers (CSP, X-Frame-Options, etc.)
- ✅ Rate limiting
- ✅ Fail2Ban
- ✅ UFW firewall
- ✅ SSH hardening
- ✅ Automatické security updates

### 9.2 Regular Security Updates

```bash
# Jednou za měsíc
sudo apt update && sudo apt upgrade -y

# Docker images update
cd /home/mimm/mimm-app
docker compose -f docker-compose.prod.yml pull
docker compose -f docker-compose.prod.yml up -d
docker image prune -f
```

### 9.3 Security Audit Tools

```bash
# Docker Bench Security
docker run -it --rm \
  --net host \
  --pid host \
  --userns host \
  --cap-add audit_control \
  -v /var/lib:/var/lib:ro \
  -v /var/run/docker.sock:/var/run/docker.sock:ro \
  -v /etc:/etc:ro \
  docker/docker-bench-security

# Lynis system audit
apt install lynis
lynis audit system
```

---

## 10. Backup Strategie

### 10.1 Database Backup Script

`/home/mimm/mimm-app/backup-db.sh`:

```bash
#!/bin/bash
set -e

BACKUP_DIR="/home/mimm/backups"
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="$BACKUP_DIR/mimm_db_$DATE.sql.gz"

# Vytvoření backup složky
mkdir -p $BACKUP_DIR

# Dump databáze
docker exec mimm-postgres pg_dump -U mimmuser mimm | gzip > $BACKUP_FILE

# Retention: 30 dní
find $BACKUP_DIR -name "mimm_db_*.sql.gz" -mtime +30 -delete

echo "✅ Database backup created: $BACKUP_FILE"
```

```bash
chmod +x /home/mimm/mimm-app/backup-db.sh
```

### 10.2 Automatické Zálohy (Cron)

```bash
# Editace crontabu
crontab -e

# Přidání:
# Denní backup v 2:00 AM
0 2 * * * /home/mimm/mimm-app/backup-db.sh >> /home/mimm/mimm-app/backup.log 2>&1

# Týdenní full backup v neděli 3:00 AM
0 3 * * 0 /home/mimm/mimm-app/full-backup.sh >> /home/mimm/mimm-app/backup.log 2>&1
```

### 10.3 Full System Backup Script

`/home/mimm/mimm-app/full-backup.sh`:

```bash
#!/bin/bash
set -e

BACKUP_DIR="/home/mimm/backups"
DATE=$(date +%Y%m%d_%H%M%S)

# Database backup
/home/mimm/mimm-app/backup-db.sh

# Application files backup
tar -czf $BACKUP_DIR/mimm_app_$DATE.tar.gz \
  /home/mimm/mimm-app \
  --exclude=/home/mimm/mimm-app/logs \
  --exclude=/home/mimm/mimm-app/backups

# Docker volumes backup
docker run --rm \
  -v mimm_postgres_data:/data \
  -v $BACKUP_DIR:/backup \
  alpine tar -czf /backup/postgres_volume_$DATE.tar.gz -C /data .

echo "✅ Full backup completed: $DATE"
```

### 10.4 Offsite Backup (volitelné)

```bash
# rsync na vzdálený server
rsync -avz -e "ssh -p 2222" \
  /home/mimm/backups/ \
  backup-user@backup-server:/backups/mimm/

# Nebo S3 (AWS CLI)
aws s3 sync /home/mimm/backups/ s3://your-bucket/mimm-backups/
```

---

## 11. Checklist před nasazením

### Pre-Deployment

- [ ] VPS je provisionován a přístupný
- [ ] DNS záznamy jsou nastaveny (A records pro your-domain.com a api.your-domain.com)
- [ ] Non-root user je vytvořen
- [ ] SSH klíče jsou nastaveny
- [ ] SSH je hardenovaný (zakázán root login, změněn port)
- [ ] UFW firewall je nakonfigurován
- [ ] Fail2Ban je nastaven
- [ ] Docker a Docker Compose jsou nainstalovány
- [ ] Nginx je nainstalován

### Configuration

- [ ] `.env` soubor je vytvořen s produkčními credentials
- [ ] `.env` má správná oprávnění (600)
- [ ] `docker-compose.prod.yml` je vytvořen
- [ ] Nginx konfigurace jsou vytvořeny
- [ ] `appsettings.Production.json` je aktualizován
- [ ] `Program.cs` obsahuje ForwardedHeaders middleware

### SSL/HTTPS

- [ ] Certbot je nainstalován
- [ ] Let's Encrypt certifikáty jsou získány
- [ ] HTTPS redirect funguje
- [ ] HSTS header je nastaven
- [ ] SSL Labs test = A+ rating (https://www.ssllabs.com/ssltest/)

### Deployment

- [ ] Kód je nahrán na server (git clone)
- [ ] Docker images jsou buildnuty
- [ ] Database migrace jsou spuštěny
- [ ] Kontejnery běží (docker ps)
- [ ] Health check endpoint odpovídá
- [ ] Frontend je dostupný přes HTTPS
- [ ] Backend API odpovídá přes HTTPS
- [ ] CORS je správně nakonfigurován
- [ ] JWT autentizace funguje

### Security

- [ ] Všechny security headers jsou nastaveny
- [ ] Rate limiting funguje
- [ ] Docker security best practices jsou aplikovány
- [ ] Secrets nejsou commitnuty do gitu
- [ ] `.gitignore` obsahuje `.env`

### Monitoring & Backup

- [ ] Logy se zapisují správně
- [ ] Backup skripty jsou vytvořeny a otestovány
- [ ] Cron jobs pro backupy jsou nastaveny
- [ ] Test restore z backup byl proveden

### Testing

- [ ] Registrace nového uživatele funguje
- [ ] Login funguje
- [ ] API endpoints odpovídají
- [ ] WebSocket/SignalR funguje (pokud je použit)
- [ ] Frontend se loaduje správně
- [ ] Browser console neobsahuje errory
- [ ] Performance testing (load testing)

### Documentation

- [ ] Produkční credentials jsou bezpečně uloženy (password manager)
- [ ] Kontaktní informace pro emergency jsou zdokumentovány
- [ ] Runbook pro incident response je vytvořen
- [ ] Deployment proces je zdokumentován

---

## 📞 Troubleshooting

### Backend kontejner se nestaruje

```bash
# Kontrola logů
docker compose -f docker-compose.prod.yml logs backend

# Kontrola health status
docker inspect mimm-backend | grep -A 10 Health

# Manuální start pro debug
docker compose -f docker-compose.prod.yml run --rm backend /bin/bash
```

### Database connection issues

```bash
# Test spojení z backendu
docker exec -it mimm-backend ping postgres

# Kontrola PostgreSQL logů
docker compose -f docker-compose.prod.yml logs postgres

# Přímé připojení do DB
docker exec -it mimm-postgres psql -U mimmuser -d mimm
```

### Nginx errors

```bash
# Test konfigurace
nginx -t

# Reload bez restartu
nginx -s reload

# Kontrola error logů
tail -f /var/log/nginx/error.log
```

### SSL certificate issues

```bash
# Manuální obnova
certbot renew --force-renewal

# Test konfigurace
certbot certificates

# Kontrola expiry
echo | openssl s_client -connect your-domain.com:443 2>/dev/null | openssl x509 -noout -dates
```

---

## 🔗 Reference

- [Docker Security Best Practices](https://docs.docker.com/engine/security/)
- [ASP.NET Core Host on Linux with Nginx](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx)
- [Let's Encrypt Documentation](https://letsencrypt.org/docs/)
- [Mozilla SSL Configuration Generator](https://ssl-config.mozilla.org/)
- [OWASP Security Headers](https://owasp.org/www-project-secure-headers/)
- [Nginx Rate Limiting](https://www.nginx.com/blog/rate-limiting-nginx/)

---

**Autor:** MIMM 2.0 Team  
**Poslední aktualizace:** 24. ledna 2026  
**Verze dokumentu:** 1.0
