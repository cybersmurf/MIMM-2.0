# MIMM 2.0 – Nginx Setup (Detailní Průvodce pro Rootless Docker)

> **Pro:** Hetzner/DigitalOcean VPS, Ubuntu 24.04, Rootless Docker
> **Backend:** Běží na `127.0.0.1:8080` (localhost, rootless Docker)
> **Frontend:** Statické Blazor WASM soubory
> **SSL:** Let's Encrypt s Certbot

---

## Prerekvizity

- [x] Rootless Docker je nainstalovaný a funguje
- [x] Postgres, Redis, Backend kontejnery běží (nebo aspoň jsou připravené)
- [ ] Doména je zaregistrovaná (např. `example.com`)
- [ ] DNS A record ukazuje na IP serveru (např. `A example.com → 1.2.3.4`)
- [ ] Port 80 a 443 jsou otevřené v UFW: `ufw allow 80/tcp && ufw allow 443/tcp`

**Pokud máš něco z toho chybět, ZASTAV se a dokonči to dřív.**

---

## Krok 1: Nginx instalace

```bash
sudo apt install -y nginx
sudo systemctl enable nginx
sudo systemctl start nginx

# Test, že Nginx běží
sudo systemctl status nginx
```

### Ověření

```bash
curl http://localhost
# Měli byste vidět default Nginx stránku
```

---

## Krok 2: Vytvořit konfiguraci pro Backend (API)

Vytvoř soubor `/etc/nginx/sites-available/mimm-backend`:

```bash
sudo nano /etc/nginx/sites-available/mimm-backend
```

Kopíruj a vlož **celý** obsah níže (buď opatrný na odsazení):

```nginx
# MIMM Backend API Reverse Proxy
# Rootless Docker: Backend běží na 127.0.0.1:8080 (localhost, non-privileged port)
# SSL: Let's Encrypt certificates

upstream mimm_api {
    server 127.0.0.1:8080;
    keepalive 64;
}

# HTTP → HTTPS redirect (nutné pro Let's Encrypt)
server {
    listen 80;
    server_name api.example.com;
    
    # Let's Encrypt verification
    location /.well-known/acme-challenge/ {
        root /var/www/certbot;
    }
    
    # Všechno ostatní přesměruj na HTTPS
    location / {
        return 301 https://$host$request_uri;
    }
}

# HTTPS server
server {
    listen 443 ssl http2;
    server_name api.example.com;
    
    # SSL certificates (Let's Encrypt)
    ssl_certificate /etc/letsencrypt/live/api.example.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/api.example.com/privkey.pem;
    
    # SSL best practices
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;
    ssl_prefer_server_ciphers on;
    
    # Security headers
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains; preload" always;
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    
    # Disable server tokens (security)
    server_tokens off;
    
    # Aumenta limit pro uploads
    client_max_body_size 10M;
    
    # Logging
    access_log /var/log/nginx/mimm-api-access.log;
    error_log /var/log/nginx/mimm-api-error.log;
    
    # ===== REVERSE PROXY KE BACKENDU =====
    location / {
        proxy_pass http://mimm_api;
        
        # Preserve original request info
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        # Timeouts
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
        
        # Buffering
        proxy_buffering on;
        proxy_buffer_size 4k;
        proxy_buffers 8 4k;
        
        # Connection reuse
        proxy_http_version 1.1;
        proxy_set_header Connection "";
    }
    
    # ===== SIGNALR SUPPORT (WebSocket) =====
    location /hubs/ {
        proxy_pass http://mimm_api;
        
        # WebSocket headers
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        
        # Proxy headers
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        # Longer timeouts pro WebSocket
        proxy_read_timeout 3600s;
        proxy_send_timeout 3600s;
    }
    
    # ===== HEALTH CHECK (bez loggingu) =====
    location /health {
        proxy_pass http://mimm_api;
        access_log off;
    }
}
```

**DŮLEŽITĚ:** Nahraď `example.com` svojí doménou (2x na "server_name" řádcích).

Uložit: `Ctrl+O`, `Enter`, `Ctrl+X`

---

## Krok 3: Vytvořit konfiguraci pro Frontend (Web)

Vytvoř soubor `/etc/nginx/sites-available/mimm-frontend`:

```bash
sudo nano /etc/nginx/sites-available/mimm-frontend
```

Kopíruj a vlož:

```nginx
# MIMM Frontend Web App (Blazor WASM static files)
# Serves https://example.com and https://www.example.com

upstream mimm_api {
    server 127.0.0.1:8080;
    keepalive 64;
}

# HTTP → HTTPS redirect
server {
    listen 80;
    server_name example.com www.example.com;
    
    # Let's Encrypt verification
    location /.well-known/acme-challenge/ {
        root /var/www/certbot;
    }
    
    # Všechno ostatní na HTTPS
    location / {
        return 301 https://$host$request_uri;
    }
}

# HTTPS server (with both domain variants)
server {
    listen 443 ssl http2;
    server_name example.com www.example.com;
    
    # SSL certificates
    ssl_certificate /etc/letsencrypt/live/example.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/example.com/privkey.pem;
    
    # SSL best practices
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;
    ssl_prefer_server_ciphers on;
    
    # Security headers
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains; preload" always;
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    
    server_tokens off;
    
    # Logging
    access_log /var/log/nginx/mimm-web-access.log;
    error_log /var/log/nginx/mimm-web-error.log;
    
    # ===== STATIC FRONTEND FILES =====
    root /home/mimm/mimm-app/src/MIMM.Frontend/bin/Release/net9.0/browser-wasm;
    
    # SPA routing: Všechny 404 → index.html (Blazor WASM si sám routuje)
    location / {
        try_files $uri $uri/ /index.html;
        
        # Cache control pro statické assety
        expires 1d;
        add_header Cache-Control "public, immutable";
    }
    
    # CSS, JS, WASM – cachuj déle
    location ~* \.(js|css|wasm|ttf|eot|svg|otf)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
    
    # HTML – necachuj (aby se vždy stáhl nový, pokud je deployment)
    location ~* \.html$ {
        expires -1;
        add_header Cache-Control "no-cache, no-store, must-revalidate";
    }
    
    # ===== API PROXY (pokud frontend potřebuje /api/...) =====
    # Normálně by měl frontend volat https://api.example.com, ale tady je fallback
    location /api/ {
        proxy_pass http://mimm_api;
        
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        proxy_http_version 1.1;
        proxy_set_header Connection "";
    }
}

# www → bez www redirect (optional, ale nice to have)
server {
    listen 443 ssl http2;
    server_name www.example.com;
    
    ssl_certificate /etc/letsencrypt/live/example.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/example.com/privkey.pem;
    
    return 301 https://example.com$request_uri;
}
```

**DŮLEŽITĚ:** Nahraď `example.com` svojí doménou.

Uložit: `Ctrl+O`, `Enter`, `Ctrl+X`

---

## Krok 4: Povolit sites

```bash
# Vytvoř symlink do sites-enabled
sudo ln -s /etc/nginx/sites-available/mimm-backend /etc/nginx/sites-enabled/mimm-backend
sudo ln -s /etc/nginx/sites-available/mimm-frontend /etc/nginx/sites-enabled/mimm-frontend

# Odstraň default site (pokud existuje)
sudo rm /etc/nginx/sites-enabled/default

# Test konfiguraci
sudo nginx -t

# Měla by být zpráva:
# nginx: the configuration file /etc/nginx/nginx.conf syntax is ok
# nginx: configuration file /etc/nginx/nginx.conf test is successful
```

Pokud test selže, vrátí ti error. **Oprav ho.**

---

## Krok 5: Let's Encrypt SSL Certifikáty

### Instalace Certbot

```bash
sudo apt install -y certbot python3-certbot-nginx
```

### Získání Certificátů

```bash
# Hlavní doména + www variant (frontend)
sudo certbot certonly --nginx \
  -d example.com \
  -d www.example.com \
  -e your-email@example.com

# API doména
sudo certbot certonly --nginx \
  -d api.example.com \
  -e your-email@example.com
```

**Průběh:**

1. Certbot se zeptá na email (pro SSL novotnosti + recovery)
2. Stáhne a ověří domény
3. Uloží certy do `/etc/letsencrypt/live/`

### Ověření, že Certy jsou OK

```bash
ls -la /etc/letsencrypt/live/
# Měli bys vidět složky: example.com, api.example.com

# Check expiry
sudo certbot certificates
```

### Auto-renew (DŮLEŽITÉ!)

```bash
# Certifikáty vypršejí za 90 dní – musíš je obnovit
sudo systemctl enable certbot.timer
sudo systemctl start certbot.timer

# Test dry-run
sudo certbot renew --dry-run
```

---

## Krok 6: Reload Nginx

```bash
sudo systemctl reload nginx
# nebo
sudo nginx -s reload

# Ověř status
sudo systemctl status nginx
```

---

## Krok 7: Testing

### Test Backend Proxy

```bash
# Skrz Nginx (HTTPS)
curl -I https://api.example.com/health

# Mělo by vrátit:
# HTTP/1.1 200 OK
# ...

# Detailnější test
curl https://api.example.com/health 2>/dev/null | jq .
```

### Test Frontend

```bash
# V prohlížeči
https://example.com

# Mělo by se načíst Blazor WASM app
```

### Test WebSocket (SignalR)

```bash
# Je to složitější na command line, ale v dev tools by mělo být:
# GET /hubs/notification?access_token=... HTTP/1.1
# Upgrade: websocket
# HTTP/1.1 101 Switching Protocols
```

---

## Troubleshooting Nginx

### Nginx nejde startovat

```bash
sudo nginx -t
# Ukaž error message
```

### Backend není dostupný skrz Nginx

```bash
# 1. Ověř, že backend běží
docker ps | grep backend

# 2. Test lokálně
curl http://localhost:8080/health

# 3. Check Nginx logs
sudo tail -50 /var/log/nginx/mimm-api-error.log
```

### SSL certificate errors

```bash
# Certbot debug
sudo certbot certificates

# Renew force (když si nejsi jistý)
sudo certbot renew --force-renewal

# Check cert expiry
openssl x509 -in /etc/letsencrypt/live/api.example.com/fullchain.pem -noout -dates
```

### Port already in use

```bash
sudo lsof -i :80
sudo lsof -i :443
sudo lsof -i :8080

# Kill na portu (ZÁSADNĚ, znáš-li co děláš)
sudo kill -9 <PID>
```

### Logs

```bash
# Real-time access log
sudo tail -f /var/log/nginx/mimm-api-access.log

# Errory
sudo tail -f /var/log/nginx/mimm-api-error.log

# Všechny Nginx logy
sudo journalctl -u nginx -n 100 -f
```

---

## Checklist na Závěr

- [ ] Nginx je nainstalovaný a běží (`sudo systemctl status nginx`)
- [ ] `/etc/nginx/sites-available/mimm-backend` existuje a má správný obsah
- [ ] `/etc/nginx/sites-available/mimm-frontend` existuje a má správný obsah
- [ ] Oba sites jsou symlinkovány v `sites-enabled/`
- [ ] `nginx -t` prochází bez chyby
- [ ] SSL certy jsou v `/etc/letsencrypt/live/`
- [ ] `curl https://api.example.com/health` vrací 200 OK
- [ ] `https://example.com` se načte v prohlížeči (Blazor app)
- [ ] `certbot renew --dry-run` prochází bez chyby

---

## Chyby, které jsi Udělal (a jak je Opravit)

### "connect() failed (111: Connection refused)"

```
*1 connect() to 127.0.0.1:8080 failed (111: Connection refused)
```

**Příčina:** Backend není spuštěný.
**Řešení:** `docker compose -f docker-compose.prod.yml up -d backend`

---

### "SSL_ERROR_RX_RECORD_TOO_LONG"

V prohlížeči při otevření https://api.example.com

**Příčina:** Nginx se pokouší servírovat HTTP na HTTPS portu.
**Řešení:** Ověř, že backend na portu 8080 běží jako HTTP (ne HTTPS).

---

### "The Markdown is not readable" ve Frontendu

Vše se načte ale obsah je "blank" nebo "404"

**Příčina:** Cesta k frontend build je špatná v nginx config.
**Řešení:**

```bash
# Najdi správnou cestu
find /home/mimm/mimm-app -name "index.html" -type f

# Uprav `root` v mimm-frontend config
root /správná/cesta/zde;
```

---

## Hotovo

Pokud všech 8 bodů z checklistu je ✅, měl bys mít:

- ✅ API dostupné na `https://api.example.com`
- ✅ Frontend dostupný na `https://example.com`
- ✅ SSL/TLS šifrování
- ✅ Auto-renew certifikátů
- ✅ SignalR WebSocket podpora

**Příští kroky:**

1. Test uživatelskýflow (registrace, login, vytvoření entry, apod.)
2. Monitoring (logy, uptime check)
3. Backupy DB
4. Performance tuning (pokud potřeba)
