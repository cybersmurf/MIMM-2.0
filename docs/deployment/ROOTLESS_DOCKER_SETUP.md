# MIMM 2.0 - Rootless Docker Setup Guide

## 🔒 Proč Rootless Docker?

Rootless Docker běží **bez root oprávnění**, což výrazně zvyšuje bezpečnost:

- ✅ Kontejnery nemohou získat root přístup k host systému
- ✅ Minimalizuje attack surface při kompromitaci kontejneru
- ✅ Splňuje best practices pro produkční deployment
- ✅ Doporučeno pro VPS a veřejné servery

---

## 📋 Příprava VPS (Ubuntu 24.04)

### 1. Aktualizace systému

```bash
sudo apt update && sudo apt upgrade -y
sudo apt install -y curl wget git uidmap dbus-user-session
```

### 2. Vytvoření non-root uživatele

```bash
# Vytvoření uživatele pro MIMM aplikaci
sudo adduser mimm
sudo usermod -aG sudo mimm

# Nastavení SSH klíče
sudo mkdir -p /home/mimm/.ssh
sudo cp ~/.ssh/authorized_keys /home/mimm/.ssh/
sudo chown -R mimm:mimm /home/mimm/.ssh
sudo chmod 700 /home/mimm/.ssh
sudo chmod 600 /home/mimm/.ssh/authorized_keys

# Přepnutí na nového uživatele
su - mimm
```

---

## 🐋 Instalace Rootless Docker

### 3. Instalace Docker (rootless mode)

```bash
# Stáhnout rootless setup skript
curl -fsSL https://get.docker.com/rootless | sh

# Přidat do PATH (přidat do ~/.bashrc nebo ~/.zshrc)
export PATH=/home/mimm/bin:$PATH
export DOCKER_HOST=unix:///run/user/$(id -u)/docker.sock

# Aktivovat změny
source ~/.bashrc

# Ověřit instalaci
docker version
docker info | grep -i rootless
```

**Očekávaný výstup:**
```
...
 Security Options:
  seccomp
   Profile: builtin
  rootless
  cgroupns
 Kernel Version: 6.8.0-49-generic
 Operating System: Ubuntu 24.04.1 LTS
...
```

### 4. Povolit start Docker při boot

```bash
# Systemd služba pro rootless Docker
systemctl --user enable docker
systemctl --user start docker

# Povolit lingering (Docker běží i bez přihlášení)
sudo loginctl enable-linger mimm
```

### 5. Ověření rootless režimu

```bash
# Zkontrolovat, že Docker běží bez root
ps aux | grep dockerd

# Mělo by zobrazit proces pod uživatelem "mimm", ne "root"
mimm     12345  ... /home/mimm/bin/dockerd-rootless.sh
```

---

## 🚀 Deployment MIMM 2.0 s Rootless Docker

### 6. Klonování repozitáře

```bash
cd ~
git clone https://github.com/cybersmurf/MIMM-2.0.git
cd MIMM-2.0
```

### 7. Příprava environment variables

```bash
# Vytvoření .env souboru pro produkci
cat > .env << 'EOF'
# Database Configuration
POSTGRES_USER=mimmuser
POSTGRES_PASSWORD=<SECURE_PASSWORD_HERE>
POSTGRES_DB=mimm

# Redis Configuration
REDIS_PASSWORD=<REDIS_PASSWORD_HERE>

# JWT Configuration
JWT_SECRET_KEY=<GENERATE_64_CHAR_KEY>
JWT_ISSUER=https://mimm.yourdomain.com
JWT_AUDIENCE=mimm-frontend

# Frontend URL
FRONTEND_URL=https://mimm.yourdomain.com

# Last.fm API
LASTFM_API_KEY=<YOUR_LASTFM_KEY>
LASTFM_API_SECRET=<YOUR_LASTFM_SECRET>

# Spotify API
SPOTIFY_CLIENT_ID=<YOUR_SPOTIFY_ID>
SPOTIFY_CLIENT_SECRET=<YOUR_SPOTIFY_SECRET>

# Discogs API
DISCOGS_CONSUMER_KEY=<YOUR_DISCOGS_KEY>
DISCOGS_CONSUMER_SECRET=<YOUR_DISCOGS_SECRET>

# Version
VERSION=1.0.0
EOF

# Nastavit správná oprávnění
chmod 600 .env
```

**Generování JWT klíče:**
```bash
# 64-znak náhodný klíč pro JWT
openssl rand -base64 64 | tr -d '\n' && echo
```

### 8. Kontrola UID/GID

```bash
# Zjistit své UID a GID
id

# Výstup: uid=1000(mimm) gid=1000(mimm) groups=...
```

**Poznámka:** V `docker-compose.prod.yml` je nastaveno `user: "1000:1000"`. Pokud máš jiné UID/GID, uprav tento řádek.

### 9. Build a spuštění produkčního Docker Compose

```bash
# Build obrazu
docker compose -f docker-compose.prod.yml build

# Spuštění služeb
docker compose -f docker-compose.prod.yml up -d

# Kontrola logů
docker compose -f docker-compose.prod.yml logs -f backend
```

### 10. Ověření běhu

```bash
# Kontrola běžících kontejnerů
docker ps

# Test backend health endpoint
curl http://localhost:8080/health

# Mělo by vrátit: {"status":"Healthy"}
```

---

## 🌐 Nginx Reverse Proxy (Host OS)

### 11. Instalace Nginx (jako root nebo sudo)

```bash
sudo apt install -y nginx certbot python3-certbot-nginx
```

### 12. Konfigurace Nginx pro MIMM

```bash
sudo nano /etc/nginx/sites-available/mimm
```

**Obsah souboru:**
```nginx
# MIMM 2.0 Nginx Configuration for Rootless Docker

upstream mimm_backend {
    server 127.0.0.1:8080;
    keepalive 32;
}

server {
    listen 80;
    listen [::]:80;
    server_name mimm.yourdomain.com;

    # Redirect HTTP to HTTPS
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name mimm.yourdomain.com;

    # SSL certificates (Let's Encrypt)
    ssl_certificate /etc/letsencrypt/live/mimm.yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/mimm.yourdomain.com/privkey.pem;
    ssl_trusted_certificate /etc/letsencrypt/live/mimm.yourdomain.com/chain.pem;

    # SSL Configuration
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers 'ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384';
    ssl_prefer_server_ciphers off;
    ssl_session_cache shared:SSL:10m;
    ssl_session_timeout 10m;
    ssl_stapling on;
    ssl_stapling_verify on;

    # Security Headers
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "no-referrer-when-downgrade" always;

    # Rate Limiting
    limit_req_zone $binary_remote_addr zone=api_limit:10m rate=10r/s;
    limit_req zone=api_limit burst=20 nodelay;

    # Max Upload Size
    client_max_body_size 10M;

    # Proxy to Backend
    location /api/ {
        proxy_pass http://mimm_backend;
        proxy_http_version 1.1;
        
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header X-Forwarded-Host $host;
        
        proxy_buffering off;
        proxy_request_buffering off;
        
        # Timeouts
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }

    # SignalR WebSockets
    location /hubs/ {
        proxy_pass http://mimm_backend;
        proxy_http_version 1.1;
        
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "Upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        proxy_cache_bypass $http_upgrade;
        
        # WebSocket timeouts
        proxy_connect_timeout 7d;
        proxy_send_timeout 7d;
        proxy_read_timeout 7d;
    }

    # Health Check
    location /health {
        proxy_pass http://mimm_backend;
        access_log off;
    }

    # Static Files (Frontend)
    location / {
        root /home/mimm/MIMM-2.0/wwwroot;
        try_files $uri $uri/ /index.html;
        
        # Caching for static assets
        location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
            expires 1y;
            add_header Cache-Control "public, immutable";
        }
    }

    # Deny access to sensitive files
    location ~ /\. {
        deny all;
        access_log off;
        log_not_found off;
    }
}
```

### 13. Aktivace Nginx konfigurace

```bash
# Symbolický odkaz
sudo ln -s /etc/nginx/sites-available/mimm /etc/nginx/sites-enabled/

# Test konfigurace
sudo nginx -t

# Reload Nginx
sudo systemctl reload nginx
```

### 14. Získání SSL certifikátu (Let's Encrypt)

```bash
# Automatické získání a konfigurace SSL
sudo certbot --nginx -d mimm.yourdomain.com

# Automatické obnovení (cron job)
sudo certbot renew --dry-run
```

---

## 🔥 Firewall (UFW)

### 15. Konfigurace firewallu

```bash
sudo ufw default deny incoming
sudo ufw default allow outgoing

# SSH (pokud na nestandardním portu, uprav)
sudo ufw allow 22/tcp comment 'SSH'

# HTTP/HTTPS
sudo ufw allow 80/tcp comment 'HTTP'
sudo ufw allow 443/tcp comment 'HTTPS'

# Povolit firewall
sudo ufw enable

# Kontrola
sudo ufw status verbose
```

---

## 📊 Monitoring a Údržba

### 16. Kontrola stavu služeb

```bash
# Docker kontejnery
docker ps

# Logy backend
docker compose -f docker-compose.prod.yml logs -f backend

# Logy Nginx
sudo tail -f /var/log/nginx/access.log
sudo tail -f /var/log/nginx/error.log

# Využití zdrojů
docker stats
```

### 17. Restart služeb

```bash
# Restart MIMM služeb
cd ~/MIMM-2.0
docker compose -f docker-compose.prod.yml restart backend

# Restart Nginx
sudo systemctl restart nginx
```

### 18. Aktualizace aplikace

```bash
cd ~/MIMM-2.0

# Pull nejnovější změny
git pull origin main

# Rebuild a restart
docker compose -f docker-compose.prod.yml build --no-cache
docker compose -f docker-compose.prod.yml up -d

# Kontrola logů
docker compose -f docker-compose.prod.yml logs -f backend
```

---

## 🔒 Bezpečnostní Best Practices

### ✅ Checklist

- [x] Docker běží v rootless režimu
- [x] Kontejnery používají non-root uživatele
- [x] Porty bindovány pouze na localhost (127.0.0.1)
- [x] Nginx reverse proxy s SSL (Let's Encrypt)
- [x] Security headers v Nginx
- [x] Rate limiting v Nginx
- [x] Firewall (UFW) aktivní
- [x] SSH hardening (klíče místo hesel)
- [x] `.env` soubor s `chmod 600`
- [x] Fail2Ban pro SSH protection
- [x] Automatické security updaty (unattended-upgrades)

### Doporučené další kroky

```bash
# Fail2Ban instalace
sudo apt install -y fail2ban
sudo systemctl enable fail2ban
sudo systemctl start fail2ban

# Automatické security updaty
sudo apt install -y unattended-upgrades
sudo dpkg-reconfigure -plow unattended-upgrades
```

---

## 📦 Backup Strategie

### 19. PostgreSQL backup

```bash
# Manuální backup
docker exec mimm-postgres pg_dump -U mimmuser mimm > mimm_backup_$(date +%Y%m%d).sql

# Automatický cron job (každý den ve 2:00)
crontab -e
```

**Přidat řádek:**
```cron
0 2 * * * cd ~/MIMM-2.0 && docker exec mimm-postgres pg_dump -U mimmuser mimm > ~/backups/mimm_backup_$(date +\%Y\%m\%d).sql
```

### 20. Redis backup

```bash
# Redis automaticky ukládá snapshot v /data (volume redis_data)
# Pro manuální backup:
docker exec mimm-redis redis-cli --pass <REDIS_PASSWORD> SAVE
```

---

## ❓ Troubleshooting

### Problém: "permission denied while trying to connect to the Docker daemon socket"

**Řešení:**
```bash
# Ověř, že je nastaven správný DOCKER_HOST
echo $DOCKER_HOST
# Mělo by být: unix:///run/user/1000/docker.sock (nebo tvé UID)

export DOCKER_HOST=unix:///run/user/$(id -u)/docker.sock
```

### Problém: "bind: address already in use"

**Řešení:**
```bash
# Najít proces používající port
sudo lsof -i :8080

# Zastavit konfliktní proces nebo změnit port v docker-compose.prod.yml
```

### Problém: Kontejner spadne hned po startu

**Řešení:**
```bash
# Kontrola logů
docker compose -f docker-compose.prod.yml logs backend

# Kontrola UID/GID v docker-compose.prod.yml
id  # Zkontroluj své UID/GID a uprav 'user:' v docker-compose
```

---

## 🎉 Shrnutí

Po dokončení tohoto návodu máš:

✅ Rootless Docker běžící na VPS  
✅ MIMM 2.0 Backend (PostgreSQL + Redis) v produkčním režimu  
✅ Nginx reverse proxy s SSL (HTTPS)  
✅ Bezpečnostní hardening (firewall, non-root, security headers)  
✅ Automatické SSL certifikáty (Let's Encrypt)  
✅ Monitoring a logging  

**Aplikace je přístupná na:** `https://mimm.yourdomain.com`

---

## 📚 Další Zdroje

- [Docker Rootless Mode Documentation](https://docs.docker.com/engine/security/rootless/)
- [Nginx Hardening Guide](https://www.nginx.com/blog/mitigating-ddos-attacks-with-nginx-and-nginx-plus/)
- [Let's Encrypt Best Practices](https://letsencrypt.org/docs/)
- [ASP.NET Core Production Best Practices](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/building-net-docker-images)
