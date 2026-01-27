# Docker & Deployment Rules for MIMM 2.0

> **Pro GitHub Copilot:** Tato pravidla platí pro všechny změny v Docker konfiguraci a deployment procesu.

## 🐳 Docker Best Practices

### 1. Rootless Docker v Produkci

**VŽDY používej rootless Docker na produkčních serverech.**

```yaml
# ❌ ŠPATNĚ - explicitní user override v docker-compose.prod.yml
services:
  postgres:
    image: postgres:16-alpine
    user: "999:999"  # NIKDY nedělej tohle - brání entrypointu v chown

# ✅ SPRÁVNĚ - nech entrypoint pracovat normálně
services:
  postgres:
    image: postgres:16-alpine
    # Žádný user override - entrypoint si nastaví práva sám
```

**Proč?**

- Rootless Docker běží pod UID uživatele (např. 1000)
- Postgres entrypoint potřebuje chown na `/var/lib/postgresql/data`
- Explicitní `user: "999:999"` způsobí "Operation not permitted"

### 2. Docker Networks & Hostnames

**Backend MUSÍ používat Docker network hostnames, NE localhost.**

```bash
# ❌ ŠPATNĚ - connection string v appsettings.json
"Host=localhost;Port=5432;Database=mimm"

# ✅ SPRÁVNĚ - environment variable s Docker hostname
POSTGRES_HOST=postgres  # Docker network hostname
POSTGRES_PORT=5432
```

**Implementace v Program.cs:**

```csharp
// Funkce pro build connection stringu z env vars
static string BuildConnectionString(IConfiguration config)
{
    var host = config["POSTGRES_HOST"] ?? "localhost";
    var port = config["POSTGRES_PORT"] ?? "5432";
    var database = config["POSTGRES_DB"] ?? "mimm_dev";
    var username = config["POSTGRES_USER"] ?? "postgres";
    var password = config["POSTGRES_PASSWORD"] ?? "postgres";
    
    var connStr = $"Host={host};Port={port};Database={database};Username={username};Password={password};";
    Log.Information("Database connection: Host={Host}, Database={Database}, User={User}", host, database, username);
    return connStr;
}

// Použití
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(BuildConnectionString(builder.Configuration)));
```

### 3. EF Core Migrations na VPS

**Runtime image NEMÁ .NET SDK → použij SDK container.**

```bash
# ❌ ŠPATNĚ - dotnet ef v runtime containeru
docker exec mimm-backend dotnet ef database update
# Error: The application 'ef' does not exist

# ✅ SPRÁVNĚ - SDK container s připojením do Docker network
docker run --rm \
  --env-file .env \
  --network mimm-app_default \
  -v "$PWD":/src \
  -w /src \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e POSTGRES_HOST=postgres \
  -e POSTGRES_PORT=5432 \
  mcr.microsoft.com/dotnet/sdk:9.0 \
  bash -c "dotnet restore MIMM.sln && \
    dotnet tool install --global dotnet-ef && \
    dotnet ef database update \
    --project src/MIMM.Backend/MIMM.Backend.csproj \
    --startup-project src/MIMM.Backend/MIMM.Backend.csproj \
    --configuration Release"
```

**Kritické parametry:**

- `--network <network-name>` - MUSÍ být Docker network (zjisti: `docker network ls | grep mimm`)
- `-e POSTGRES_HOST=postgres` - Docker hostname, NE localhost
- `--env-file .env` - načte DB credentials

### 4. Bind Mounts v Rootless

**Backend v rootless módu poslouchá na `127.0.0.1:8080`, NE `0.0.0.0:8080`.**

```yaml
# docker-compose.prod.yml
services:
  backend:
    ports:
      - "127.0.0.1:8080:8080"  # ✅ SPRÁVNĚ - bind na localhost
      # NIKDY ne:
      # - "8080:8080"  # Vystavilo by port na 0.0.0.0 (všechny rozhraní)
```

**Nginx pak proxyuje:**

```nginx
upstream mimm_api { 
  server 127.0.0.1:8080;  # Backend na localhost
}
```

---

## 🚀 Deployment Rules

### 1. Environment Variables vs Secrets

**appsettings.json = výchozí hodnoty pro dev, Production = environment variables.**

```json
// appsettings.Production.json - PRÁZDNÝ (vše z env vars)
{
  "Logging": {},
  "Cors": {},
  "LastFm": {},
  "Discogs": {},
  "SendGrid": {},
  "App": {}
}
```

**Program.cs MUSÍ načítat env vars:**

```csharp
// ✅ KRITICKÉ - přidej PŘED Build()
builder.Configuration.AddEnvironmentVariables();

// JWT config s fallbackem
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secretKey = builder.Configuration["JWT_SECRET_KEY"] 
                     ?? builder.Configuration["Jwt:Key"] 
                     ?? throw new InvalidOperationException("JWT secret key not found");
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT_ISSUER"] ?? builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["JWT_AUDIENCE"] ?? builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });
```

**Required .env variables:**

```bash
# Database (KRITICKÉ)
POSTGRES_HOST=postgres          # Docker network hostname
POSTGRES_PORT=5432
POSTGRES_USER=mimmuser
POSTGRES_PASSWORD=STRONG_RANDOM_PASS
POSTGRES_DB=mimm

# Redis
REDIS_HOST=redis                # Docker network hostname
REDIS_PORT=6379
REDIS_PASSWORD=STRONG_RANDOM_PASS

# JWT (generuj: openssl rand -base64 64)
JWT_SECRET_KEY=64_CHAR_RANDOM_STRING
JWT_ISSUER=https://api.your-domain.com
JWT_AUDIENCE=mimm-frontend

# URLs
FRONTEND_URL=https://your-domain.com
BACKEND_URL=https://api.your-domain.com
```

### 2. Nginx + Blazor WASM

**Frontend path = wwwroot (ne bin/Release - to je pro compiled build).**

```nginx
# /etc/nginx/sites-available/mimm-frontend
server {
  listen 443 ssl http2;
  server_name your-domain.com www.your-domain.com;
  
  # ✅ SPRÁVNĚ - wwwroot je v Git repo
  root /home/mimm/mimm-app/src/MIMM.Frontend/wwwroot;
  
  # ❌ ŠPATNĚ - bin/Release neexistuje bez dotnet publish
  # root /home/mimm/mimm-app/src/MIMM.Frontend/bin/Release/net9.0/browser-wasm;
  
  location / {
    try_files $uri $uri/ /index.html;  # SPA routing
    expires 1d;
  }
  
  location ~* \.(js|css|wasm)$ {
    expires 1y;
    add_header Cache-Control "public, immutable";
  }
}
```

**Permission fix pro Nginx:**

```bash
# Nginx (www-data) potřebuje traverse práva na /home/mimm
sudo chmod o+x /home/mimm

# Ověř
ls -ld /home/mimm
# Mělo by být: drwxr-x--x (751) - execute pro others ✅
```

### 3. SSL/TLS Certificates

**Let's Encrypt chicken-egg problém:**

- Nginx config chce SSL certy → certy neexistují → nginx -t failuje
- Certbot potřebuje běžící nginx → nginx nemůže startovat → certbot failuje

**Řešení: Temporary HTTP-only configs**

```bash
# 1. Vytvoř temporary HTTP-only config (bez SSL)
sudo nano /etc/nginx/sites-available/mimm-frontend
```

```nginx
# Temporary config (jen HTTP + ACME challenge)
server {
  listen 80;
  server_name your-domain.com www.your-domain.com;
  
  location /.well-known/acme-challenge/ {
    root /var/www/certbot;
  }
  
  location / {
    return 200 "Waiting for SSL...";
  }
}
```

```bash
# 2. Start nginx + získej certy
sudo mkdir -p /var/www/certbot
sudo nginx -t && sudo systemctl start nginx

sudo certbot certonly --webroot \
  -w /var/www/certbot \
  -d your-domain.com -d www.your-domain.com \
  -e your@email.com

# 3. Přepni na HTTPS config s certifikáty
sudo nano /etc/nginx/sites-available/mimm-frontend
# (vlož plný HTTPS config s ssl_certificate paths)

sudo nginx -t && sudo systemctl reload nginx
```

### 4. Health Checks

**Backend MUSÍ mít /health endpoint.**

```csharp
// Program.cs
app.MapGet("/health", () => new 
{ 
    status = "healthy", 
    timestamp = DateTime.UtcNow 
});
```

**Test po deploymentu:**

```bash
# Backend
curl https://api.your-domain.com/health
# Očekávej: {"status":"healthy","timestamp":"..."}

# Frontend
curl -I https://your-domain.com
# Očekávej: HTTP/2 200

# Docker containers
docker compose -f docker-compose.prod.yml ps
# Všechny by měly být "Up" a "healthy"

# Database tables
docker exec mimm-postgres psql -U mimmuser -d mimm -c "\dt"
# Mělo by zobrazit: Users, Entries, LastFmTokens, atd.
```

---

## 🔒 Security Checklist

**Před push do production:**

- [ ] Žádná clear-text hesla v Git (use .env + .gitignore)
- [ ] JWT secret key min. 64 znaků (generuj: `openssl rand -base64 64`)
- [ ] Postgres password min. 16 znaků random
- [ ] Redis password nastaveno
- [ ] CORS AllowedOrigins jen production domény (ne `*`)
- [ ] SSL/TLS enabled s HSTS header
- [ ] Rootless Docker používán na VPS
- [ ] Backend ports bind jen na 127.0.0.1 (ne 0.0.0.0)
- [ ] UFW firewall aktivní (jen 2222/tcp, 80/tcp, 443/tcp)
- [ ] Fail2Ban běží
- [ ] SSH PasswordAuthentication=no, PermitRootLogin=no

---

## 📦 Commit Message Convention

**Pro Docker/deployment změny:**

```bash
# Docker config
git commit -m "fix(docker): remove user override for postgres entrypoint"

# Backend config
git commit -m "fix(backend): use environment variables for Production config"

# Nginx/deployment docs
git commit -m "docs(deployment): fix frontend nginx path and add permission troubleshooting"

# Migrations
git commit -m "feat(db): add LastFmTokens table migration"
```

**Typy:**

- `fix(docker)` - opravy Docker configu
- `feat(deployment)` - nové deployment features
- `docs(deployment)` - deployment dokumentace
- `chore(infra)` - infrastruktura (nginx, ssl, atd.)

---

## 🛠️ Common Troubleshooting

| Problém | Příčina | Řešení |
|---------|---------|--------|
| postgres "Operation not permitted" | `user: "999:999"` v compose | Smaž user override |
| Backend "ef not found" | Runtime image nemá SDK | Použij SDK container |
| Migrations "connection refused" | Localhost místo Docker hostname | `POSTGRES_HOST=postgres` |
| Frontend 500 Permission Denied | /home/mimm bez o+x | `sudo chmod o+x /home/mimm` |
| SSL_ERROR_RX_RECORD_TOO_LONG | Certy neexistují | Temporary HTTP config → certbot → HTTPS |
| Backend 502 Bad Gateway | Backend není spuštěný | `docker compose -f docker-compose.prod.yml logs backend` |

---

## 📝 Dokumentace Links

- [DEPLOYMENT_CHECKLIST_LITE_DETAILED.md](../docs/deployment/DEPLOYMENT_CHECKLIST_LITE_DETAILED.md) - Complete VPS deployment guide
- [AGENTS.md](../AGENTS.md) - AI agent instructions
- [README.md](../README.md) - Project overview

---

**Last Updated:** 26. ledna 2026
**Version:** 2.0.1
**Maintainer:** cybersmurf
