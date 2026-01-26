# MIMM 2.0 – Deployment Checklist (Lite, Detailní verze pro nelinuxáře)

> Copy–paste návod pro jedno VPS (Hetzner Ubuntu 24.04). Každý krok má příkaz a krátké vysvětlení.

**Poznámky:**

- Připoj se přes SSH z Windows (PowerShell nebo PuTTY) / z macOS (Terminal).
- `IP` nahraď veřejnou IP serveru, `your-domain.com` svou doménou.
- Pokud se po změně SSH portu odpojíš, připoj se znovu s `-p 2222`.

---

## Co sakra potřebuješ mít ready PŘED začátkem

Než začneš cokoliv instalovat, měl bys mít:

1. **VPS běží** – Hetzner/DigitalOcean/cokoliv, Ubuntu 24.04, min. 2 GB RAM, máš root přístup
2. **Doménu** – koupená a DNS A records nastaveny na IP serveru (pro `your-domain.com`, `www`, `api`)
3. **SSH klíče** – vygenerované na svém PC: `ssh-keygen -t ed25519 -C "tvuj@email.com"`
4. **Silná hesla připravená:**
   - Database password (min. 16 znaků, random)
   - Redis password (min. 16 znaků)
   - JWT secret key (min. 32 znaků, použij `openssl rand -base64 32`)
5. **Email** – platný email pro Let's Encrypt notifikace
6. **Git repo s kódem** – buď public, nebo si nastav SSH deploy key
7. **Frontend build** – zkompilovaný Blazor WASM (složka `dist/` nebo `wwwroot/`)
8. **Volitelné API klíče** (pokud používáš):
   - SendGrid API key (pro emaily)
   - Last.fm API key + shared secret
   - Discogs token

**Pro hesla použij password manager** (Bitwarden, 1Password, KeePass) – nepiš si je na papír nebo do plaintext souboru.

Pokud tohle nemáš, zastav se TEĎKA a připrav si to. Jinak budeš deployment zdržovat a hledat věci v půlce procesu.

---

## Fáze A: Základ serveru (15–20 min)

1) Přihlášení jako root

```bash
ssh root@IP
```

1) Aktualizace balíčků (stáhne a nainstaluje dostupné updaty)

```bash
apt update && apt upgrade -y
```

1) Vytvoření uživatele `mimm` a přidání do sudo (aby nemusel používat root)

```bash
adduser mimm            # nastavte heslo, stačí Enter pro volitelné údaje
usermod -aG sudo mimm   # dá uživateli práva sudo
```

1) SSH hardening (změna portu, zákaz root a hesel)

```bash
nano /etc/ssh/sshd_config
# změňte nebo přidejte řádky:
# Port 2222
# PermitRootLogin no
# PasswordAuthentication no
# Uložte: Ctrl+O, Enter, ukončete: Ctrl+X
systemctl restart sshd
```

> Co to dělá: port 2222 sníží šum botů, zakáže login jako root a zakáže hesla (jen klíče).

1) Firewall UFW (povolí jen SSH+HTTP+HTTPS, zbytek blokne)

```bash
ufw allow 2222/tcp
ufw allow 80/tcp
ufw allow 443/tcp
ufw enable   # potvrďte "y"
ufw status verbose
```

1) Fail2Ban (ochrana proti brute force na SSH a Nginx)

```bash
apt install -y fail2ban
systemctl enable --now fail2ban
fail2ban-client status
```

1) Přihlášení jako nový uživatel (otestuj, že vše funguje)

```bash
ssh mimm@IP -p 2222
```

---

## Fáze B: Docker + Nginx (15–20 min)

### 🔒 Rootless Docker Setup (DOPORUČENO pro produkci)

**Proč rootless?** Docker běží bez root oprávnění → kontejnery nemohou získat root přístup k serveru → vyšší bezpečnost.

1) **Příprava pro rootless mode**

```bash
# Instalace potřebných balíčků
sudo apt install -y uidmap dbus-user-session
```

1) **Instalace Rootless Docker (jako uživatel mimm, NE root!)**

```bash
# Přepni se na uživatele mimm (pokud jsi root)
su - mimm

# Stáhni a spusť rootless setup
curl -fsSL https://get.docker.com/rootless | sh

# Přidej do PATH (vlož do ~/.bashrc nebo ~/.zshrc)
export PATH=/home/mimm/bin:$PATH
export DOCKER_HOST=unix:///run/user/$(id -u)/docker.sock

# Aktivuj změny
source ~/.bashrc

# Ověř instalaci
docker version
docker info | grep -i rootless   # mělo by zobrazit "rootless"
```

1) **Povolit Docker start při bootu**

```bash
systemctl --user enable docker
systemctl --user start docker

# Povolit lingering (Docker běží i bez přihlášení)
sudo loginctl enable-linger mimm
```

1) **Test Dockeru**

```bash
docker run hello-world
# Mělo by úspěšně stáhnout a spustit test kontejner
```

### ⚠️ Pokud NECHCEŠ rootless (klasický Docker s root)

Použij tento postup (méně bezpečný, ale jednodušší):

1) **Docker + compose plugin (instalační skript Dockeru + plugin Compose)**

```bash
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo apt install -y docker-compose-plugin
```

1) **Přidání uživatele do docker group (aby mohl spouštět docker bez sudo)**

```bash
sudo usermod -aG docker mimm
# pak se odhlásit a přihlásit, jinak se skupina neprojeví
newgrp docker   # nebo logout + login
```

1) **Test Dockeru**

```bash
docker run hello-world
```

---

### Nginx instalace (pro oba režimy stejné)

1) **Nginx instalace a vypnutí default site**

```bash
sudo apt install -y nginx
sudo rm /etc/nginx/sites-enabled/default
sudo nginx -t   # test konfigurace (zatím prázdná, ale ok)
```

---

## Fáze C: Certy (Let's Encrypt, 10 min)

1) Certbot instalace

```bash
sudo apt install -y certbot python3-certbot-nginx
```

1) Získání certů (NGINX musí běžet)

```bash
sudo certbot certonly --nginx \
  -d your-domain.com \
  -d www.your-domain.com \
  -d api.your-domain.com
```

> Co to dělá: vystaví HTTPS certifikáty pro tři hostname.

1) Ověření obnovy

```bash
sudo certbot renew --dry-run
```

---

## Fáze D: Aplikace (25–35 min)

1) **Repo stáhnout / nahrát**

```bash
cd /home/mimm
git clone <repo-url> mimm-app   # nebo nahrát SFTP do /home/mimm/mimm-app
cd mimm-app
```

1) **`.env` vytvořit a zamknout (jen na serveru)**

```bash
nano .env
# vlož hodnoty (viz níže)
chmod 600 .env
```

Ukázka obsahu `.env` (nahraď vlastními hodnotami):

```bash
# Database
POSTGRES_HOST=postgres          # Docker hostname (NE localhost!)
POSTGRES_PORT=5432             # Docker port
POSTGRES_USER=mimmuser
POSTGRES_PASSWORD=STRONG_DB_PASS  # min. 16 chars, random
POSTGRES_DB=mimm

# Redis
REDIS_HOST=redis               # Docker hostname
REDIS_PORT=6379
REDIS_PASSWORD=STRONG_REDIS_PASS  # min. 16 chars, random

# JWT Authentication (generuj: openssl rand -base64 64)
JWT_SECRET_KEY=AT_LEAST_64_CHARS_SECRET_KEY_FOR_PRODUCTION
JWT_ISSUER=https://api.your-domain.com
JWT_AUDIENCE=mimm-frontend

# URLs
FRONTEND_URL=https://your-domain.com
BACKEND_URL=https://api.your-domain.com

# External APIs (volitelné)
LASTFM_API_KEY=your_lastfm_key
LASTFM_API_SECRET=your_lastfm_secret
SPOTIFY_CLIENT_ID=your_spotify_id
SPOTIFY_CLIENT_SECRET=your_spotify_secret
DISCOGS_CONSUMER_KEY=your_discogs_key
DISCOGS_CONSUMER_SECRET=your_discogs_secret

# Docker image version
VERSION=1.0.0
```

2) **Build Docker images**

```bash
docker compose -f docker-compose.prod.yml build
```

3) **Spusť Postgres a Redis (bez backendu, aby se aplikovaly migrace)**

```bash
docker compose -f docker-compose.prod.yml up -d postgres redis
sleep 15  # čekej, až jsou healthy

# Ověř, že běží
docker compose -f docker-compose.prod.yml ps
# Měly by být "Up" a "healthy" oba
```

4) **Aplikuj databázové migrace** (KRITICKÉ - tento krok se často pokazí)

⚠️ **Pozor:** Runtime obraz (backend) nemá .NET SDK, proto musíš použít SDK container:

Nejdřív si zkontroluj, jaký je Docker network name (obvykle je to jméno složky + `_default`):

```bash
docker network ls | grep mimm
# Mělo by být něco jako: mimm-app_default
```

Pak spusť migrations (nahraď `<network-name>` správným network jménem):

```bash
docker run --rm \
  --env-file .env \
  --network <network-name> \
  -v "$PWD":/src \
  -w /src \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e POSTGRES_HOST=postgres \
  -e POSTGRES_PORT=5432 \
  -e PATH="/root/.dotnet/tools:$PATH" \
  mcr.microsoft.com/dotnet/sdk:9.0 \
  bash -c "dotnet restore MIMM.sln && dotnet tool install --global dotnet-ef && \
    dotnet ef database update \
    --project src/MIMM.Backend/MIMM.Backend.csproj \
    --startup-project src/MIMM.Backend/MIMM.Backend.csproj \
    --configuration Release"
```

**Pokud máš složku `/home/mimm/mimm-app`, network se jmenuje `mimm-app_default`:**

```bash
docker run --rm \
  --env-file .env \
  --network mimm-app_default \
  -v "$PWD":/src \
  -w /src \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e POSTGRES_HOST=postgres \
  -e POSTGRES_PORT=5432 \
  -e PATH="/root/.dotnet/tools:$PATH" \
  mcr.microsoft.com/dotnet/sdk:9.0 \
  bash -c "dotnet restore MIMM.sln && dotnet tool install --global dotnet-ef && \
    dotnet ef database update \
    --project src/MIMM.Backend/MIMM.Backend.csproj \
    --startup-project src/MIMM.Backend/MIMM.Backend.csproj \
    --configuration Release"
```

**Co tento příkaz dělá:**
- `--env-file .env` - vloží environment proměnné z `.env` souboru
- `--network mimm-app_default` - připojí se do Docker network, kde běží postgres (KRITICKÉ!)
- `--env POSTGRES_HOST=postgres` - postgres hostname v Docker networku (NE localhost!)
- `dotnet ef database update` - aplikuje všechny pending migrations

**Pokud selže:**
```bash
# 1. Ověř, že postgres je healthy
docker compose -f docker-compose.prod.yml logs postgres

# 2. Ověř, že máš správný network name
docker network ls

# 3. Zkus se připojit z kontejneru
docker run --rm --network mimm-app_default \
  mcr.microsoft.com/dotnet/sdk:9.0 \
  ping -c 2 postgres  # ping by měl fungovat
```

5) **Spusť backend**

```bash
docker compose -f docker-compose.prod.yml up -d backend

# Čekej~30 sekund, aby se aplikace nastartovala
sleep 30

# Ověř logs
docker compose -f docker-compose.prod.yml logs backend | tail -30
# Hledej: "Application started" nebo "Now listening"
```

6) **Ověř, že backend běží**

```bash
curl http://localhost:8080/health
# Mělo by vrátit JSON: {"status":"healthy","timestamp":"..."}
```

**Pokud backend není healthy:**
```bash
docker compose -f docker-compose.prod.yml logs backend -f  # Follow logs
docker ps  # Check container status
```

---

## Fáze E: Nginx Reverse Proxy & SSL (15–20 min)

**DŮLEŽITĚ:** Backend běží na `127.0.0.1:8080` (rootless Docker), Nginx proxyuje na něj a servuje frontend.

### 1) Nginx config pro Backend API

Vytvoř `/etc/nginx/sites-available/mimm-backend`:

```bash
sudo nano /etc/nginx/sites-available/mimm-backend
```

Vlož (nahraď `api.your-domain.com` svojí doménou):

```nginx
upstream mimm_api { server 127.0.0.1:8080; }

server {
  listen 80;
  server_name api.your-domain.com;
  location /.well-known/acme-challenge/ { root /var/www/certbot; }
  location / { return 301 https://$host$request_uri; }
}

server {
  listen 443 ssl http2;
  server_name api.your-domain.com;
  ssl_certificate /etc/letsencrypt/live/api.your-domain.com/fullchain.pem;
  ssl_certificate_key /etc/letsencrypt/live/api.your-domain.com/privkey.pem;
  ssl_protocols TLSv1.2 TLSv1.3;
  add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;
  add_header X-Frame-Options "SAMEORIGIN" always;
  add_header X-Content-Type-Options "nosniff" always;
  
  location / {
    proxy_pass http://mimm_api;
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
  }
  
  location /hubs/ {
    proxy_pass http://mimm_api;
    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection "upgrade";
    proxy_set_header Host $host;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
  }
}
```

### 2) Nginx config pro Frontend

Vytvoř `/etc/nginx/sites-available/mimm-frontend`:

```bash
sudo nano /etc/nginx/sites-available/mimm-frontend
```

Vlož (nahraď `your-domain.com` svojí doménou):

```nginx
server {
  listen 80;
  server_name your-domain.com www.your-domain.com;
  location /.well-known/acme-challenge/ { root /var/www/certbot; }
  location / { return 301 https://$host$request_uri; }
}

server {
  listen 443 ssl http2;
  server_name your-domain.com www.your-domain.com;
  ssl_certificate /etc/letsencrypt/live/your-domain.com/fullchain.pem;
  ssl_certificate_key /etc/letsencrypt/live/your-domain.com/privkey.pem;
  ssl_protocols TLSv1.2 TLSv1.3;
  add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;
  
  root /home/mimm/mimm-app/src/MIMM.Frontend/wwwroot;
  
  location / {
    try_files $uri $uri/ /index.html;
    expires 1d;
  }
  
  location ~* \.(js|css|wasm)$ {
    expires 1y;
    add_header Cache-Control "public, immutable";
  }
}
```

### 3) Povolit Nginx sites

```bash
sudo ln -s /etc/nginx/sites-available/mimm-backend /etc/nginx/sites-enabled/
sudo ln -s /etc/nginx/sites-available/mimm-frontend /etc/nginx/sites-enabled/
sudo rm /etc/nginx/sites-enabled/default  # Smaž default site
sudo nginx -t  # Test konfigurace
```

### 4) Let's Encrypt SSL Certy

```bash
sudo apt install -y certbot python3-certbot-nginx

# Hlavní doména + www
sudo certbot certonly --nginx -d your-domain.com -d www.your-domain.com -e your@email.com

# API doména
sudo certbot certonly --nginx -d api.your-domain.com -e your@email.com

# Auto-renew
sudo systemctl enable certbot.timer
sudo systemctl start certbot.timer
```

### 5) Restart Nginx

```bash
sudo systemctl reload nginx
sudo systemctl status nginx
```

### 6) Ověř, že funguje

```bash
# API
curl https://api.your-domain.com/health

# Frontend (mělo by vrátit HTML Blazor app)
curl -I https://your-domain.com
```

---

## Fáze F: Smoke test (5 min)

```bash
# 1. Backend API
curl https://api.your-domain.com/health

# 2. Frontend web
curl -I https://your-domain.com  # Mělo by vrátit 200 OK

# 3. Docker containers all healthy
docker compose -f docker-compose.prod.yml ps

# 4. Database tables created
docker exec mimm-postgres psql -U mimmuser -d mimm -c "\dt"
```

---

## Fáze G: Backup (5 min)

1) Složka pro backupy

```bash
mkdir -p ~/backups
```

2) Jednorázový dump (spusť kdykoli)

```bash
docker exec mimm-postgres pg_dump -U mimmuser mimm | \
  gzip > ~/backups/mimm_db_$(date +%Y%m%d).sql.gz
```

3) Denní cron v 2:00

```bash
crontab -e
# přidej řádek (pozor na backslashy):
0 2 * * * docker exec mimm-postgres pg_dump -U mimmuser mimm | gzip > ~/backups/mimm_db_\%Y\%m\%d.sql.gz
```

---

## Troubleshooting (když se něco pokazí)

### Postgres nemůže změnit práva na adresáři
```
chmod: /var/lib/postgresql/data: Operation not permitted
```
**Řešení:** Už jsme opravili v docker-compose.prod.yml (odebrali `user: "999:999"`). Udělej `git pull origin main`.

### Backend migrations selžou (dotnet ef not found)
```
The application 'ef' does not exist.
```
**Řešení:** Použij SDK container command (viz Fáze D, krok 4). Runtime image nemá SDK.

### Nginx "connect() failed ... refused"
**Příčina:** Backend není spuštěný.
```bash
docker compose -f docker-compose.prod.yml logs backend | head -20
docker compose -f docker-compose.prod.yml restart backend
```

### SSL certificate not found
```
SSL_ERROR_RX_RECORD_TOO_LONG
```
**Řešení:** Spusť certbot znovu:
```bash
sudo certbot certonly --nginx -d your-domain.com
sudo nginx -t && sudo systemctl reload nginx
```

### Frontend vrací 404
```bash
# Ověř cestu k frontend buildu
find /home/mimm/mimm-app -name "index.html" -type f

# Uprav `root` v /etc/nginx/sites-available/mimm-frontend
# na správnou cestu
```

### Frontend vrací 500 - Permission Denied
```
stat() "/home/mimm/mimm-app/src/MIMM.Frontend/wwwroot/index.html" failed (13: Permission denied)
```
**Příčina:** Nginx (běží jako `www-data`) nemá právo traversovat `/home/mimm` adresář.

**Řešení:** Přidej execute bit pro "others" na home directory:
```bash
# Zkontroluj aktuální práva
ls -ld /home/mimm
# Mělo by být: drwxr-x--- (750) - CHYBÍ execute pro others

# Přidej execute bit (750 → 751)
sudo chmod o+x /home/mimm

# Reload nginx
sudo systemctl reload nginx

# Test
curl -I https://your-domain.com  # Mělo by vrátit 200 OK
```

**Co to dělá:** `o+x` jen povolí nginx procházet adresářem (traverse), ne čtení obsahu. Je to bezpečné - `ls /home/mimm` stále nebude fungovat pro www-data.

---

## Fáze H: Monitoring (volitelné)

---

## Go/No-Go – Ready to Deploy?

- [ ] Rootless Docker běží (`docker info | grep rootless`)
- [ ] Nginx nainstalovaný a testuje se (`sudo nginx -t`)
- [ ] SSL certy v `/etc/letsencrypt/live/`
- [ ] Postgres + Redis + Backend běží (všechny healthy)
- [ ] Migrations úspěšně aplikované
- [ ] `curl https://api.your-domain.com/health` → 200 OK
- [ ] `curl https://your-domain.com` → HTML Blazor app
- [ ] Logs jsou čisté (žádné errory)
