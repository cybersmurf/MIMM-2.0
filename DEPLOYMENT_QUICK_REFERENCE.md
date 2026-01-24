# MIMM 2.0 - Quick Reference Guide

> Rychlá referenční příručka pro běžné operace na produkčním serveru

---

## 🔐 Přístup na server

```bash
# SSH připojení (s custom portem)
ssh mimm@your-server-ip -p 2222

# První připojení s root (pouze na začátku)
ssh root@your-server-ip
```

---

## 🐳 Docker Operace

### Základní příkazy

```bash
# Status všech kontejnerů
docker ps -a

# Logy všech služeb
docker compose -f docker-compose.prod.yml logs -f

# Logy konkrétní služby
docker compose -f docker-compose.prod.yml logs -f backend
docker compose -f docker-compose.prod.yml logs -f postgres

# Restart služby
docker compose -f docker-compose.prod.yml restart backend

# Restart všeho
docker compose -f docker-compose.prod.yml restart

# Zastavení všech služeb
docker compose -f docker-compose.prod.yml down

# Start všech služeb
docker compose -f docker-compose.prod.yml up -d

# Status zdrojů
docker stats
```

### Build a Deploy

```bash
# Build nových images
docker compose -f docker-compose.prod.yml build --no-cache

# Pull aktuálních images
docker compose -f docker-compose.prod.yml pull

# Deploy (down + up)
docker compose -f docker-compose.prod.yml down
docker compose -f docker-compose.prod.yml up -d

# Nebo použij deployment skript
./deploy.sh
```

### Cleanup

```bash
# Odstranění nepoužívaných images
docker image prune -f

# Odstranění všeho nepoužívaného
docker system prune -a -f --volumes  # ⚠️ POZOR: smaže i volumes!

# Bezpečný prune (bez volumes)
docker system prune -a -f
```

---

## 🗃️ Database Operace

### Přímý přístup do DB

```bash
# Připojení do PostgreSQL
docker exec -it mimm-postgres psql -U mimmuser -d mimm

# Běžné SQL příkazy v psql:
\l              # Seznam databází
\c mimm         # Připojení k databázi
\dt             # Seznam tabulek
\d Entries      # Popis tabulky
\q              # Quit
```

### Migrace

```bash
# Spuštění migrací
./migrate.sh

# Nebo manuálně:
docker compose -f docker-compose.prod.yml run --rm backend \
  dotnet ef database update --no-build
```

### Backup a Restore

```bash
# Vytvoření backupu
./backup-db.sh

# Manuální backup
docker exec mimm-postgres pg_dump -U mimmuser mimm | gzip > backup_$(date +%Y%m%d).sql.gz

# Restore z backupu
gunzip -c backup_20260124.sql.gz | docker exec -i mimm-postgres psql -U mimmuser -d mimm

# Seznam backupů
ls -lh ~/backups/
```

---

## 🌐 Nginx Operace

### Základní příkazy

```bash
# Test konfigurace
sudo nginx -t

# Reload konfigurace (bez downtime)
sudo nginx -s reload

# Restart
sudo systemctl restart nginx

# Status
sudo systemctl status nginx

# Start/Stop
sudo systemctl start nginx
sudo systemctl stop nginx
```

### Logy

```bash
# Access log (backend)
sudo tail -f /var/log/nginx/mimm-backend-access.log

# Error log (backend)
sudo tail -f /var/log/nginx/mimm-backend-error.log

# Frontend logy
sudo tail -f /var/log/nginx/mimm-frontend-access.log

# Všechny logy současně
sudo tail -f /var/log/nginx/*.log
```

### Editace konfigurace

```bash
# Backend config
sudo nano /etc/nginx/sites-available/mimm-backend

# Frontend config
sudo nano /etc/nginx/sites-available/mimm-frontend

# Po změnách VŽDY:
sudo nginx -t && sudo nginx -s reload
```

---

## 🔒 SSL/HTTPS Operace

### Certbot příkazy

```bash
# Status certifikátů
sudo certbot certificates

# Manuální obnova
sudo certbot renew

# Test obnovy (dry-run)
sudo certbot renew --dry-run

# Kontrola expirace
echo | openssl s_client -connect api.your-domain.com:443 2>/dev/null | openssl x509 -noout -dates
```

### Troubleshooting SSL

```bash
# Test SSL konfigurace online
# https://www.ssllabs.com/ssltest/analyze.html?d=your-domain.com

# Local test
curl -vI https://api.your-domain.com
```

---

## 🔥 Firewall (UFW)

```bash
# Status
sudo ufw status verbose

# Povolení portu
sudo ufw allow 8080/tcp comment 'Custom port'

# Blokování portu
sudo ufw deny 8080/tcp

# Odstranění pravidla
sudo ufw status numbered
sudo ufw delete [číslo]

# Reset (⚠️ POZOR!)
# sudo ufw reset
```

---

## 🚫 Fail2Ban

```bash
# Status
sudo fail2ban-client status

# Status konkrétního jailu
sudo fail2ban-client status sshd
sudo fail2ban-client status nginx-http-auth

# Unban IP adresy
sudo fail2ban-client set sshd unbanip 192.168.1.100

# Restart
sudo systemctl restart fail2ban
```

---

## 📊 Monitoring

### System Resources

```bash
# Interaktivní monitoring
htop

# Disk usage
df -h
du -sh /home/mimm/*

# Memory
free -h

# CPU info
lscpu
top
```

### Network

```bash
# Otevřené porty
sudo netstat -tulpn

# Nebo s ss:
sudo ss -tulpn

# Aktivní spojení
sudo netstat -an | grep ESTABLISHED

# Konkrétní port
sudo lsof -i :443
```

### Docker specifics

```bash
# Resource usage
docker stats --no-stream

# Disk usage
docker system df

# Network inspect
docker network ls
docker network inspect mimm-app_frontend
```

---

## 🐛 Troubleshooting

### Backend nefunguje

```bash
# 1. Kontrola statusu
docker ps -a

# 2. Logy
docker compose -f docker-compose.prod.yml logs backend --tail=100

# 3. Health check
curl http://localhost:5001/health

# 4. Restart
docker compose -f docker-compose.prod.yml restart backend

# 5. Exec do containeru
docker exec -it mimm-backend /bin/bash
```

### Database connection issues

```bash
# 1. Kontrola běží-li PostgreSQL
docker ps | grep postgres

# 2. Test spojení
docker exec -it mimm-backend ping postgres

# 3. Kontrola credentials v .env
cat ~/mimm-app/.env | grep POSTGRES

# 4. PostgreSQL logy
docker compose -f docker-compose.prod.yml logs postgres

# 5. Přímé připojení
docker exec -it mimm-postgres psql -U mimmuser -d mimm -c "SELECT 1;"
```

### High CPU/Memory

```bash
# 1. Identifikace problému
docker stats --no-stream

# 2. Aplikační logy
docker compose -f docker-compose.prod.yml logs backend --tail=200

# 3. System logy
sudo journalctl -xe

# 4. Process list
ps aux | grep dotnet

# 5. Restart problematické služby
docker compose -f docker-compose.prod.yml restart backend
```

### Nginx 502 Bad Gateway

```bash
# 1. Backend běží?
docker ps | grep backend
curl http://localhost:5001/health

# 2. Nginx error log
sudo tail -100 /var/log/nginx/mimm-backend-error.log

# 3. Test Nginx konfigurace
sudo nginx -t

# 4. Restart chain
docker compose -f docker-compose.prod.yml restart backend
sudo systemctl reload nginx
```

---

## 📝 Rychlé Edity

### Environment Variables

```bash
# Editace .env
nano ~/mimm-app/.env

# Po změnách: restart
docker compose -f docker-compose.prod.yml down
docker compose -f docker-compose.prod.yml up -d
```

### Application Settings

```bash
# Production settings (vyžaduje rebuild)
nano ~/mimm-app/src/MIMM.Backend/appsettings.Production.json

# Rebuild a deploy
cd ~/mimm-app
docker compose -f docker-compose.prod.yml build backend
docker compose -f docker-compose.prod.yml up -d backend
```

---

## 🔄 Běžné Workflow

### Deploy nové verze

```bash
cd ~/mimm-app

# 1. Pull změn
git pull origin main

# 2. Backup (volitelné)
./backup-db.sh

# 3. Build nových images
docker compose -f docker-compose.prod.yml build --no-cache

# 4. Migrace (pokud jsou potřeba)
./migrate.sh

# 5. Deploy
docker compose -f docker-compose.prod.yml down
docker compose -f docker-compose.prod.yml up -d

# 6. Verify
curl https://api.your-domain.com/health
curl https://your-domain.com

# 7. Cleanup
docker image prune -f
```

### Database migrace workflow

```bash
# 1. Backup před migrací
./backup-db.sh

# 2. Spuštění migrací
./migrate.sh

# 3. Ověření
docker exec -it mimm-postgres psql -U mimmuser -d mimm -c "\dt"

# 4. Restart backend (pokud je potřeba)
docker compose -f docker-compose.prod.yml restart backend
```

### Rolling restart (bez downtime)

```bash
# Postupný restart služeb
docker compose -f docker-compose.prod.yml restart redis
sleep 5
docker compose -f docker-compose.prod.yml restart postgres
sleep 10
docker compose -f docker-compose.prod.yml restart backend
sleep 5
docker compose -f docker-compose.prod.yml restart frontend
```

---

## 📈 Performance Tuning

### PostgreSQL

```bash
# Vstup do DB
docker exec -it mimm-postgres psql -U mimmuser -d mimm

# Sledování slow queries
SELECT query, calls, total_time, mean_time
FROM pg_stat_statements
ORDER BY mean_time DESC
LIMIT 10;

# Velikost databáze
SELECT pg_size_pretty(pg_database_size('mimm'));

# Index usage
SELECT schemaname, tablename, indexname, idx_scan
FROM pg_stat_user_indexes
ORDER BY idx_scan;
```

### Redis

```bash
# Redis CLI
docker exec -it mimm-redis redis-cli -a ${REDIS_PASSWORD}

# Stats
INFO
INFO STATS

# Memory
INFO MEMORY

# Keys count
DBSIZE
```

---

## 🆘 Emergency Procedures

### Úplná havárie

```bash
# 1. Restart všeho
sudo reboot

# 2. Po restartu: kontrola služeb
sudo systemctl status nginx
sudo systemctl status docker
docker ps -a

# 3. Restart Docker services
cd ~/mimm-app
docker compose -f docker-compose.prod.yml up -d
```

### Restore z backupu

```bash
# 1. Zastavení aplikace
docker compose -f docker-compose.prod.yml down

# 2. Vyčištění existující DB (⚠️ POZOR!)
docker volume rm mimm-app_postgres_data

# 3. Start pouze DB
docker compose -f docker-compose.prod.yml up -d postgres

# 4. Čekání na DB ready
sleep 10

# 5. Restore
gunzip -c ~/backups/mimm_db_YYYYMMDD_HHMMSS.sql.gz | \
  docker exec -i mimm-postgres psql -U mimmuser -d mimm

# 6. Start zbylých služeb
docker compose -f docker-compose.prod.yml up -d
```

---

## 📞 Kontakty a Reference

### Důležité soubory

- **Docker Compose:** `~/mimm-app/docker-compose.prod.yml`
- **Environment:** `~/mimm-app/.env`
- **Deploy Script:** `~/mimm-app/deploy.sh`
- **Nginx Backend:** `/etc/nginx/sites-available/mimm-backend`
- **Nginx Frontend:** `/etc/nginx/sites-available/mimm-frontend`
- **Backups:** `~/backups/`

### Online nástroje

- **SSL Test:** <https://www.ssllabs.com/ssltest/>
- **Security Headers:** <https://securityheaders.com/>
- **Uptime Monitoring:** <https://uptimerobot.com/>

### Dokumentace

- Detailní deployment plán: `DEPLOYMENT_PLAN.md`
- README: `README.md`
- Agent instrukce: `AGENTS.md`

---

**💡 Tip:** Vytvoř si alias v `~/.bashrc` pro častá použití:

```bash
echo "alias dps='docker ps -a'" >> ~/.bashrc
echo "alias dlogs='docker compose -f ~/mimm-app/docker-compose.prod.yml logs -f'" >> ~/.bashrc
echo "alias dstats='docker stats --no-stream'" >> ~/.bashrc
echo "alias mimm='cd ~/mimm-app'" >> ~/.bashrc
source ~/.bashrc
```

---

**Poslední aktualizace:** 24. ledna 2026
