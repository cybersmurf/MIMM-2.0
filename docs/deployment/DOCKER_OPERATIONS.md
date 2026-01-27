# MIMM 2.0 - Docker Operations Guide (VPS)

**Datum:** 26. ledna 2026  
**Cíl:** Post-deployment verification, monitoring, troubleshooting, a bezpečné aktualizace  
**Audience:** DevOps engineers, server administrators

---

## 📋 Obsah

1. [Post-Deployment Verification](#post-deployment-verification)
2. [Monitoring & Logging](#monitoring--logging)
3. [Troubleshooting](#troubleshooting)
4. [Application Updates](#application-updates)
5. [Backup & Recovery](#backup--recovery)

---

## Post-Deployment Verification

### Kontrola #1: Docker kontejnery běží

```bash
docker compose -f docker-compose.prod.yml ps

# Očekávaný výstup:
# NAME             STATUS         PORTS
# mimm-postgres    Up (healthy)   127.0.0.1:5432->5432/tcp
# mimm-backend     Up             (internal)
# mimm-redis       Up (healthy)   127.0.0.1:6379->6379/tcp (pokud aktivní)
```

**Co zkontrolovat:**

- ✅ Všechny kontejnery mají status `Up`
- ✅ Postgres a Redis mají `(healthy)` v závorce
- ✅ Backend běží (žádný error)

**Pokud selhá:**

```bash
# Zobrazit chyby
docker compose -f docker-compose.prod.yml logs -f postgres

# Restart
docker compose -f docker-compose.prod.yml restart postgres
```

### Kontrola #2: Backend API je dostupný

```bash
# Lokálně na serveru
curl -i http://localhost:5001/health

# Očekávaný výstup:
# HTTP/1.1 200 OK
# Content-Type: application/json
# {"status":"healthy","database":"connected"}
```

**Pokud selhá (timeout, connection refused):**

```bash
# Zkontrolovat logs backendu
docker compose -f docker-compose.prod.yml logs -f backend

# Zkontrolovat port
netstat -tuln | grep 5001

# Zkontrolovat síť
docker network ls
docker network inspect mimm-2-0_backend
```

### Kontrola #3: Datab azi je dostupná

```bash
# Z backendu (přes Docker)
docker compose -f docker-compose.prod.yml exec backend \
  dotnet ef database update --no-build

# Nebo přímo psql
docker compose -f docker-compose.prod.yml exec postgres \
  psql -U ${POSTGRES_USER} -d ${POSTGRES_DB} -c "SELECT COUNT(*) as user_count FROM \"Users\";"

# Očekávaný výstup:
# user_count
# 0 (nebo více, pokud už jsou data)
```

**Pokud selhá (connection refused):**

```bash
# Zkontrolovat health check
docker compose -f docker-compose.prod.yml logs postgres | grep -i health

# Restart Postgres
docker compose -f docker-compose.prod.yml down postgres
docker volume ls  # Ověřit, že postgres_data existuje
docker compose -f docker-compose.prod.yml up -d postgres

# Čekat 30 sekund na startup
sleep 30
docker compose -f docker-compose.prod.yml exec postgres pg_isready -U ${POSTGRES_USER}
```

### Kontrola #4: Nginx reverse proxy funguje

```bash
# Test přes HTTPS
curl -i https://api.your-domain.com/health

# Očekávaný výstup:
# HTTP/2 200
# server: nginx
# {"status":"healthy",...}

# Test SSL certifikátu
curl -v https://api.your-domain.com/health 2>&1 | grep -E "(subject=|issuer=)"
# Mělo by ukazovat správné CN a expiry
```

**Pokud selhá (SSL error, 502 Bad Gateway):**

```bash
# Zkontrolovat Nginx config
sudo nginx -t

# Zkontrolovat logs
sudo tail -f /var/log/nginx/access.log
sudo tail -f /var/log/nginx/error.log

# Zkontrolovat certifikát
sudo certbot certificates

# Teste bez SSL (pro debug)
curl -i http://api.your-domain.com/health
```

### Kontrola #5: Frontend se servuje

```bash
# Test frontend
curl -i https://your-domain.com/

# Očekávaný výstup:
# HTTP/2 200
# Content-Type: text/html
# <html>... (Blazor WASM app)</html>

# Test z prohlížeče
# https://your-domain.com/
# Měly by se načíst všechny assets
```

**Pokud selhá (404, assets nenačítají):**

```bash
# Zkontrolovat frontend Nginx config
sudo ls -la /etc/nginx/sites-enabled/

# Zkontrolovat frontend build
docker exec -it <frontend-container-id> ls -la /usr/share/nginx/html/

# Zkontrolovat Nginx logs
sudo tail -f /var/log/nginx/error.log
```

### Kontrola #6: Login funkcionalita

```bash
# Vytvoření test uživatele
curl -X POST https://api.your-domain.com/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!",
    "displayName": "Test User"
  }'

# Očekávaný výstup:
# {"userId":"...", "accessToken":"..."}

# Login
curl -X POST https://api.your-domain.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!"
  }'

# Mělo by vrátit access token
```

---

## Monitoring & Logging

### Kontrola logů v reálném čase

```bash
# Backend logs
docker compose -f docker-compose.prod.yml logs -f backend --tail=100

# Postgres logs
docker compose -f docker-compose.prod.yml logs -f postgres --tail=50

# Redis logs (pokud aktivní)
docker compose -f docker-compose.prod.yml logs -f redis --tail=50

# Všechny kontejnery
docker compose -f docker-compose.prod.yml logs -f --tail=100
```

### Centralizované logování (optional - ELK Stack)

Pokud chceš logs centralizovat (doporučuji pro produkci):

```yaml
# Přidat do docker-compose.prod.yml
  logstash:
    image: docker.elastic.co/logstash/logstash:8.0.0
    volumes:
      - ./logstash.conf:/usr/share/logstash/pipeline/logstash.conf
    networks:
      - backend

  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.0.0
    environment:
      discovery.type: single-node
      xpack.security.enabled: "false"
    volumes:
      - elasticsearch_data:/usr/share/elasticsearch/data
    networks:
      - backend

  kibana:
    image: docker.elastic.co/kibana/kibana:8.0.0
    ports:
      - "127.0.0.1:5601:5601"
    networks:
      - backend
```

### CPU/Memory monitoring

```bash
# Živý monitor kontejnerů
docker stats --no-stream

# Výstup:
# CONTAINER         CPU %    MEM USAGE / LIMIT
# mimm-backend      2.5%     256MiB / 1GiB
# mimm-postgres     1.2%     512MiB / 2GiB
# mimm-redis        0.8%     64MiB / 256MiB

# Pokud backend zbíhá na paměť:
docker compose -f docker-compose.prod.yml update backend -e DOTNET_GCHeapHardLimit=1073741824
# (1GB = 1073741824 bytů)
```

### Disk usage

```bash
# Kolik místa Docker zabírá
docker system df

# Výstup:
# TYPE            TOTAL    ACTIVE   SIZE     RECLAIMABLE
# Images          3        2        1.2GB    500MB
# Containers      5        3        2.3GB    1.1GB
# Local Volumes   2        2        45GB     5GB (postgres_data, redis_data)

# Vyčistit nepoužívané veci
docker system prune -a
# ⚠️ Vymaže images/containers/networks které se nepoužívají
```

### Disk space pro Postgres backup

```bash
# Zkontrolovat volné místo
df -h /home/mimm/backups

# Pokud je místo kritické (<5% volného):
# - Smaž staré backupy
# - Přidej disk
# - Nebo přidej external backup storage

# Nastavit quota
mkdir -p /home/mimm/backups
chmod 700 /home/mimm/backups

# Přidat cron job na cleanup starých backupů
crontab -e
# 0 2 * * * find /home/mimm/backups -name "*.sql" -mtime +30 -delete
```

---

## Troubleshooting

### ❌ Backend kontejner neběží

```bash
# Zkontrolovat status
docker compose -f docker-compose.prod.yml ps backend

# Zobrazit error
docker compose -f docker-compose.prod.yml logs backend --tail=50

# Běžné chyby:
# 1. "Database connection refused"
#    → Postgres ještě nestartl, čekej 30 sekund a restartuj backend
docker compose -f docker-compose.prod.yml restart backend

# 2. "Address already in use :5001"
#    → Port 5001 je zablokaný
lsof -i :5001
kill -9 <PID>

# 3. "dotnet: command not found"
#    → .NET Runtime chybí v image (rebuild Docker image)
docker compose -f docker-compose.prod.yml build --no-cache backend
docker compose -f docker-compose.prod.yml up -d
```

### ❌ Postgres neodpovídá

```bash
# Status
docker compose -f docker-compose.prod.yml logs postgres | tail -20

# Health check
docker compose -f docker-compose.prod.yml exec postgres pg_isready

# Restart
docker compose -f docker-compose.prod.yml down postgres
docker volume inspect mimm-2-0_postgres_data  # Ověřit, že data zůstala
docker compose -f docker-compose.prod.yml up -d postgres

# Pokud data jsou poškozená:
# ⚠️ Toto vymaže veškerá data!
docker compose -f docker-compose.prod.yml down
docker volume rm mimm-2-0_postgres_data
docker compose -f docker-compose.prod.yml up -d postgres
# Pak obnoví z backupu:
docker compose -f docker-compose.prod.yml exec postgres psql -U ${POSTGRES_USER} -d ${POSTGRES_DB} < backup.sql
```

### ❌ Nginx vrací 502 Bad Gateway

```bash
# Nginx logs
sudo tail -f /var/log/nginx/error.log

# Běžné příčiny:
# 1. Backend není dostupný
curl http://127.0.0.1:5001/health  # Zkontrolovat, zda je up

# 2. Proxy settings v Nginx
sudo nano /etc/nginx/sites-available/api.your-domain.com
# Zkontrolovat: proxy_pass http://127.0.0.1:5001;

# 3. Firewall blokuje
sudo ufw status
sudo ufw allow 5001

# Test s netcat
nc -zv 127.0.0.1 5001
# Should output: Connection to 127.0.0.1 5001 port [tcp/*] succeeded!
```

### ❌ SSL/TLS certificate chyby

```bash
# Zkontrolovat certifikát
sudo certbot certificates

# Pokud expired:
sudo certbot renew --dry-run  # Test renew
sudo certbot renew             # Opravdu renew

# Zkontrolovat Nginx SSL config
sudo grep -r "ssl_certificate" /etc/nginx/

# Restart Nginx
sudo systemctl restart nginx
sudo nginx -t  # Ověřit syntax

# Manuální test
echo | openssl s_client -servername api.your-domain.com -connect api.your-domain.com:443
# Mělo by zobrazit certifikát
```

### ❌ Frontend nenačítá assets (404 errors)

```bash
# Zkontrolovat frontend files
sudo ls -la /var/www/mimm/

# Kontrola Nginx config
sudo cat /etc/nginx/sites-available/your-domain.com | grep -E "root|location"

# Rebuild frontend
cd /home/mimm/MIMM-2.0/src/MIMM.Frontend
dotnet publish -c Release -o dist
sudo cp -r dist/wwwroot/* /var/www/mimm/

# Restart Nginx
sudo systemctl restart nginx
```

---

## Application Updates

### ✅ Bezpečná aktualizace bez downtime

#### Scénář: Update kódu, databáze OK, jen backend restart

```bash
cd /home/mimm/MIMM-2.0

# 1. Pull nejnovější kód
git fetch origin
git checkout main
git reset --hard origin/main

# 2. Build nový image
docker compose -f docker-compose.prod.yml build backend

# 3. Start nový backend (stará instance vypne postupně)
docker compose -f docker-compose.prod.yml up -d backend

# 4. Ověřit, že nový backend běží
sleep 5
docker compose -f docker-compose.prod.yml logs backend | tail -20

# 5. Test
curl https://api.your-domain.com/health
```

#### Scénář: Update s databázovými změnami (migrations)

```bash
# ⚠️ Toto má DOWNTIME (5-10 sekund)

# 1. Pull kód
git fetch origin && git checkout main && git reset --hard origin/main

# 2. Build backend
docker compose -f docker-compose.prod.yml build backend

# 3. Zastavit backend (uživatelé dostanou 503 - optional)
docker compose -f docker-compose.prod.yml stop backend

# 4. Spustit migrations
docker compose -f docker-compose.prod.yml run --rm backend \
  dotnet ef database update

# 5. Start nový backend
docker compose -f docker-compose.prod.yml up -d backend

# 6. Ověřit
sleep 10
curl https://api.your-domain.com/health
```

#### Scénář: Rollback na starší verzi

```bash
# Pokud se nová verze pokazí

# 1. Zjistit poslední dobrou verzi
git log --oneline | head -10

# 2. Checkout na starší commit
git checkout <commit-hash>

# 3. Rebuild a restart
docker compose -f docker-compose.prod.yml build --no-cache backend
docker compose -f docker-compose.prod.yml up -d backend

# 4. Ověřit
curl https://api.your-domain.com/health

# Pokud databáze je broken, obnovit z backupu
docker compose -f docker-compose.prod.yml down
docker volume rm mimm-2-0_postgres_data
docker compose -f docker-compose.prod.yml up -d postgres
docker compose -f docker-compose.prod.yml exec postgres psql -U ${POSTGRES_USER} -d ${POSTGRES_DB} < /home/mimm/backups/backup-20260126.sql
```

### Scheduled updates (cron job)

```bash
# Vytvořit update script
cat > /home/mimm/update-mimm.sh << 'EOF'
#!/bin/bash
set -e

cd /home/mimm/MIMM-2.0
git fetch origin
git checkout main
git reset --hard origin/main

docker compose -f docker-compose.prod.yml build backend
docker compose -f docker-compose.prod.yml up -d backend

# Health check
sleep 5
if ! curl -f http://localhost:5001/health > /dev/null; then
  echo "Health check failed, rolling back..."
  git checkout HEAD~1
  docker compose -f docker-compose.prod.yml build backend
  docker compose -f docker-compose.prod.yml up -d backend
  exit 1
fi

echo "Update successful at $(date)" >> /home/mimm/update.log
EOF

chmod +x /home/mimm/update-mimm.sh

# Přidat do cronu (update v 2 AM)
crontab -e
# 0 2 * * * /home/mimm/update-mimm.sh >> /home/mimm/update.log 2>&1
```

---

## Backup & Recovery

### Daily automated backups

```bash
# Vytvořit backup script
mkdir -p /home/mimm/backups
cat > /home/mimm/backup-db.sh << 'EOF'
#!/bin/bash
BACKUP_DIR="/home/mimm/backups"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="$BACKUP_DIR/mimm_db_$TIMESTAMP.sql"

docker compose -f docker-compose.prod.yml exec -T postgres \
  pg_dump -U ${POSTGRES_USER} -d ${POSTGRES_DB} > "$BACKUP_FILE"

# Komprimuj
gzip "$BACKUP_FILE"

# Smaž backupy starší než 30 dní
find "$BACKUP_DIR" -name "*.sql.gz" -mtime +30 -delete

echo "Backup created: $BACKUP_FILE.gz" >> /home/mimm/backup.log
EOF

chmod +x /home/mimm/backup-db.sh

# Test
/home/mimm/backup-db.sh
ls -lh /home/mimm/backups/

# Přidat do cronu (daily v 1 AM)
crontab -e
# 0 1 * * * /home/mimm/backup-db.sh
```

### Restore from backup

```bash
# Pokud je třeba obnovit data
cd /home/mimm/backups

# Najdi správný backup
ls -lh *.sql.gz

# Obnoví z backupu
docker compose -f docker-compose.prod.yml down
docker volume rm mimm-2-0_postgres_data
docker compose -f docker-compose.prod.yml up -d postgres

# Čekat na startup
sleep 30

# Restore
gunzip -c mimm_db_20260126_010000.sql.gz | \
  docker compose -f docker-compose.prod.yml exec -T postgres \
  psql -U ${POSTGRES_USER} -d ${POSTGRES_DB}

# Ověřit
docker compose -f docker-compose.prod.yml exec postgres \
  psql -U ${POSTGRES_USER} -d ${POSTGRES_DB} -c "SELECT COUNT(*) FROM \"Users\";"
```

### Offline backup na vlastní NAS/storage

```bash
# Stáhnout poslední backup lokálně
mkdir -p ~/MIMM-backups
scp -P 2222 mimm@your-vps-ip:/home/mimm/backups/mimm_db_*.sql.gz ~/MIMM-backups/

# Nebo pomocí rsync (lepší pro velké files)
rsync -avz -e "ssh -p 2222" mimm@your-vps-ip:/home/mimm/backups/ ~/MIMM-backups/
```

---

## Checklist pro produkční readiness

- [ ] Všechny 6 kontrol-pointů projdou (docker ps, health, db, nginx, frontend, login)
- [ ] Nginx logs neobsahují errory
- [ ] Backend logs neobsahují warnings
- [ ] SSL certifikát je platný (`certbot certificates`)
- [ ] Backup script běží správně (`./backup-db.sh`)
- [ ] Firewall UFW je aktivní (`ufw status`)
- [ ] SSH klíč je nastavený (login bez hesla funguje)
- [ ] Fail2Ban je aktivní (`fail2ban-client status`)
- [ ] DNS je správně nakonfigurován (`nslookup your-domain.com`)
- [ ] HTTPS redirect funguje (HTTP -> HTTPS)

---

**Created:** 26. ledna 2026  
**Status:** Production Ready  
**Last Updated:** 26. ledna 2026
