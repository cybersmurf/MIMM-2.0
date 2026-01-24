# MIMM 2.0 – Quick Reference (Lite)

> Rychlé taháky pro běžný provoz na jednom VPS.

---

## SSH
```bash
ssh mimm@your-server-ip -p 2222
```

## Docker
```bash
# Stav a logy
docker ps -a
docker compose -f docker-compose.prod.yml logs --tail=200

# Restart služby
docker compose -f docker-compose.prod.yml restart backend

# Deploy
docker compose -f docker-compose.prod.yml build --no-cache
docker compose -f docker-compose.prod.yml up -d

# Čistka
docker image prune -f
```

## DB
```bash
# Psql
docker exec -it mimm-postgres psql -U mimmuser -d mimm

# Migrace
docker compose -f docker-compose.prod.yml run --rm backend \
  dotnet ef database update --no-build

# Backup (ruční)
docker exec mimm-postgres pg_dump -U mimmuser mimm | \
  gzip > ~/backups/mimm_db_$(date +%Y%m%d).sql.gz
```

## Nginx / SSL
```bash
sudo nginx -t && sudo nginx -s reload
sudo tail -f /var/log/nginx/error.log
sudo certbot renew --dry-run
sudo mkdir -p /var/www/certbot && sudo chown www-data:www-data /var/www/certbot
```

## Health & monitoring (light)
- Health: `curl https://api.your-domain.com/health`
- UptimeRobot check na /health
- Rychlé metriky: `docker stats`

## Performance (light)
```bash
ab -n 100 -c 5 https://api.your-domain.com/health
```

## Emergency
```bash
# Hard restart všeho
docker compose -f docker-compose.prod.yml down
docker compose -f docker-compose.prod.yml up -d

# Debug shell v backendu
docker compose -f docker-compose.prod.yml run --rm backend /bin/bash

# Rychlý 502 troubleshoot
docker ps | grep backend
docker compose -f docker-compose.prod.yml logs backend --tail=50
sudo nginx -t && sudo nginx -s reload
```

## Tahák: kam co patří
- `.env`: /home/mimm/mimm-app/.env (práva 600)
- Frontend build: /var/www/mimm-frontend (zkopíruj např. `rsync -av frontend-build/ /var/www/mimm-frontend/`)
- ACME webroot: /var/www/certbot (pro http-01 challenge)

## Co je záměrně vynecháno
- Žádné status pages, App Insights, Docker Bench/Lynis (až později)
- Redis jen pokud ho appka potřebuje
- Žádné složité load/perf testy – jen základní smoke

---

**Používej spolu s**: [DEPLOYMENT_PLAN_LITE.md](DEPLOYMENT_PLAN_LITE.md) a [DEPLOYMENT_CHECKLIST_LITE.md](DEPLOYMENT_CHECKLIST_LITE.md).
