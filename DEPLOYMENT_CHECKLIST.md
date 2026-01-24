# MIMM 2.0 - Deployment Checklist

> **Kontrolní seznam pro nasazení** - zaškrtněte každou položku při dokončení

**Datum nasazení:** _______________  
**Provedl:** _______________  
**VPS IP:** _______________

---

## Fáze 1: Příprava VPS Serveru (Doba: ~30 min)

### Prvotní Setup

- [ ] SSH přístup na VPS jako root funguje
- [ ] System aktualizován: `apt update && apt upgrade -y`
- [ ] Základní nástroje nainstalovány (curl, wget, git, htop, nano)
- [ ] Timezone nastaven: `timedatectl set-timezone Europe/Prague`
- [ ] Hostname nastaven: `hostnamectl set-hostname mimm-production`

### Non-Root User

- [ ] User `mimm` vytvořen: `adduser mimm`
- [ ] User přidán do sudo: `usermod -aG sudo mimm`
- [ ] SSH klíče zkopírovány do `/home/mimm/.ssh/`
- [ ] Oprávnění na `.ssh` správná (700 pro složku, 600 pro klíče)
- [ ] Test přihlášení jako `mimm` úspěšný

### SSH Hardening

- [ ] `/etc/ssh/sshd_config` editován
  - [ ] `PermitRootLogin no`
  - [ ] `PasswordAuthentication no`
  - [ ] `Port 2222` (nebo jiný custom port)
  - [ ] `MaxAuthTries 3`
- [ ] SSH restart: `systemctl restart sshd`
- [ ] ⚠️ Test nového SSH spojení PŘED odpojením starého
- [ ] Staré spojení odpojeno

### Firewall Setup

- [ ] UFW nainstalován: `apt install ufw`
- [ ] Default deny incoming: `ufw default deny incoming`
- [ ] Default allow outgoing: `ufw default allow outgoing`
- [ ] SSH port povolen: `ufw allow 2222/tcp`
- [ ] HTTP povolen: `ufw allow 80/tcp`
- [ ] HTTPS povolen: `ufw allow 443/tcp`
- [ ] UFW aktivován: `ufw enable`
- [ ] Status zkontrolován: `ufw status verbose`

### Fail2Ban

- [ ] Fail2Ban nainstalován: `apt install fail2ban`
- [ ] `/etc/fail2ban/jail.local` vytvořen a nakonfigurován
- [ ] SSH jail povolen s custom portem (2222)
- [ ] Nginx jails povoleny
- [ ] Fail2Ban restart: `systemctl restart fail2ban`
- [ ] Status zkontrolován: `fail2ban-client status`

### Automatické Updates

- [ ] Unattended-upgrades nakonfigurovány
- [ ] Test konfigurace: `unattended-upgrade --dry-run`

---

## Fáze 2: Docker Instalace (Doba: ~15 min)

### Docker Engine

- [ ] Docker nainstalován pomocí get-docker.sh
- [ ] User `mimm` přidán do docker group: `usermod -aG docker mimm`
- [ ] Docker service povolen: `systemctl enable docker`
- [ ] Docker běží: `systemctl status docker`
- [ ] Test: `docker run hello-world` úspěšný
- [ ] Verze zkontrolována: `docker --version`

### Docker Compose

- [ ] Docker Compose plugin nainstalován
- [ ] Verze zkontrolována: `docker compose version`

### Docker Security

- [ ] `/etc/docker/daemon.json` vytvořen s security settings
- [ ] Log rotation nastaven (max-size: 10m, max-file: 3)
- [ ] Docker restart: `systemctl restart docker`

---

## Fáze 3: DNS & Domain Setup (Doba: ~10 min + DNS propagation)

### DNS Records

- [ ] A record pro `your-domain.com` → VPS IP
- [ ] A record pro `www.your-domain.com` → VPS IP
- [ ] A record pro `api.your-domain.com` → VPS IP
- [ ] DNS propagace zkontrolována: `dig your-domain.com`
- [ ] Ping test úspěšný: `ping api.your-domain.com`

---

## Fáze 4: Nginx Instalace & Konfigurace (Doba: ~20 min)

### Nginx Base

- [ ] Nginx nainstalován: `apt install nginx`
- [ ] Nginx běží: `systemctl status nginx`
- [ ] Default site deaktivován: `rm /etc/nginx/sites-enabled/default`

### Backend Config

- [ ] `/etc/nginx/sites-available/mimm-backend` vytvořen
- [ ] Upstream backend správně nakonfigurován (127.0.0.1:5001)
- [ ] Rate limiting nastaven
- [ ] Security headers nakonfigurovány
- [ ] Symlink vytvořen: `ln -s /etc/nginx/sites-available/mimm-backend /etc/nginx/sites-enabled/`

### Frontend Config

- [ ] `/etc/nginx/sites-available/mimm-frontend` vytvořen
- [ ] Root directory nastaven
- [ ] Compression (gzip) povolena
- [ ] Cache headers nakonfigurovány
- [ ] Symlink vytvořen: `ln -s /etc/nginx/sites-available/mimm-frontend /etc/nginx/sites-enabled/`

### Nginx Test

- [ ] Config test úspěšný: `nginx -t`
- [ ] (Nginx restart zatím NE - čekáme na SSL certifikáty)

---

## Fáze 5: SSL/TLS Certifikáty (Doba: ~10 min)

### Certbot Installation

- [ ] Certbot nainstalován: `apt install certbot python3-certbot-nginx`
- [ ] Webroot složka vytvořena: `mkdir -p /var/www/certbot`

### Certificate Acquisition

- [ ] Nginx dočasně zastaven pro standalone mode
- [ ] Certifikáty získány pro všechny domény:

  ```bash
  certbot certonly --standalone \
    -d your-domain.com \
    -d www.your-domain.com \
    -d api.your-domain.com \
    --email your-email@example.com \
    --agree-tos
  ```

- [ ] Certifikáty uloženy v `/etc/letsencrypt/live/`

### SSL Configuration

- [ ] SSL paths v Nginx configs aktualizovány
- [ ] Nginx restart s plnou konfigurací: `systemctl restart nginx`
- [ ] HTTPS test: `curl -I https://your-domain.com`
- [ ] HTTPS test backend: `curl -I https://api.your-domain.com`

### Auto-Renewal

- [ ] Certbot renewal timer aktivní: `systemctl list-timers | grep certbot`
- [ ] Dry-run test úspěšný: `certbot renew --dry-run`
- [ ] Post-renewal hook vytvořen: `/etc/letsencrypt/renewal-hooks/deploy/reload-nginx.sh`

### SSL Quality Check

- [ ] SSL Labs test spuštěn: <https://www.ssllabs.com/ssltest/>
- [ ] Rating A nebo A+ dosažen
- [ ] Security headers check: <https://securityheaders.com/>

---

## Fáze 6: Aplikace Setup (Doba: ~20 min)

### Repository Clone

- [ ] Pracovní složka vytvořena: `mkdir -p /home/mimm/mimm-app`
- [ ] Git repo klonován: `git clone https://github.com/your-org/MIMM-2.0.git`
- [ ] Nebo: kód nahrán přes SCP/rsync

### Environment Configuration

- [ ] `.env` soubor vytvořen v `/home/mimm/mimm-app/`
- [ ] Všechny proměnné vyplněny:
  - [ ] POSTGRES_USER, POSTGRES_PASSWORD, POSTGRES_DB
  - [ ] REDIS_PASSWORD
  - [ ] JWT_SECRET_KEY (min. 32 znaků)
  - [ ] JWT_ISSUER (<https://api.your-domain.com>)
  - [ ] FRONTEND_URL (<https://your-domain.com>)
  - [ ] SENDGRID_API_KEY
  - [ ] LASTFM_API_KEY a SECRET
- [ ] `.env` oprávnění: `chmod 600 .env`
- [ ] `.env` vlastník: `chown mimm:mimm .env`

### Docker Compose Config

- [ ] `docker-compose.prod.yml` vytvořen
- [ ] Produkční settings nakonfigurovány
- [ ] Security opts přidány (no-new-privileges, cap_drop)
- [ ] Health checks nakonfigurovány
- [ ] Networks správně nastaveny (frontend, backend internal)

### Application Config

- [ ] `appsettings.Production.json` aktualizován
- [ ] CORS allowed origins nastaveny
- [ ] Database connection strings používají ENV proměnné
- [ ] JWT settings produkční

### Code Updates

- [ ] `Program.cs` obsahuje ForwardedHeaders middleware
- [ ] ForwardedHeaders je PRVNÍ middleware v pipeline
- [ ] Dockerfile optimalizován (non-root user, read-only)

---

## Fáze 7: První Deployment (Doba: ~15 min)

### Docker Build

- [ ] Docker images buildnuty: `docker compose -f docker-compose.prod.yml build`
- [ ] Build úspěšný bez errors

### Database Initialization

- [ ] PostgreSQL kontejner spuštěn: `docker compose -f docker-compose.prod.yml up -d postgres`
- [ ] Health check úspěšný: `docker inspect mimm-postgres | grep Health`
- [ ] Test připojení: `docker exec -it mimm-postgres psql -U mimmuser -d mimm -c "SELECT 1;"`

### Run Migrations

- [ ] Migrace spuštěny (pokud existují):

  ```bash
  docker compose -f docker-compose.prod.yml run --rm backend \
    dotnet ef database update
  ```

- [ ] Tabulky vytvořeny: `docker exec -it mimm-postgres psql -U mimmuser -d mimm -c "\dt"`

### Start All Services

- [ ] Všechny služby spuštěny: `docker compose -f docker-compose.prod.yml up -d`
- [ ] Všechny kontejnery running: `docker ps -a`
- [ ] Health checks zelené

---

## Fáze 8: Testing & Verification (Doba: ~15 min)

### Backend API Tests

- [ ] Health endpoint: `curl https://api.your-domain.com/health`
- [ ] API root: `curl https://api.your-domain.com/api/`
- [ ] Response code 200 nebo očekávaný
- [ ] CORS headers přítomné

### Frontend Tests

- [ ] Frontend loaduje: `curl -I https://your-domain.com`
- [ ] Index.html vrací 200
- [ ] Static assets loadují (CSS, JS)
- [ ] Browser test: otevřít <https://your-domain.com>
- [ ] Browser console bez errors

### Authentication Flow

- [ ] Registrace nového uživatele funguje
- [ ] Login funguje
- [ ] JWT token je vydán
- [ ] Protected endpoint vyžaduje autentizaci

### Database Operations

- [ ] CRUD operace fungují
- [ ] Data se persistují po restartu
- [ ] Foreign keys fungují

### WebSocket/SignalR (pokud je)

- [ ] WebSocket spojení funguje
- [ ] Real-time updates fungují

---

## Fáze 9: Monitoring & Logging Setup (Doba: ~15 min)

### Log Verification

- [ ] Docker logy se zapisují: `docker compose logs`
- [ ] Nginx access log: `tail -f /var/log/nginx/mimm-backend-access.log`
- [ ] Nginx error log: `tail -f /var/log/nginx/mimm-backend-error.log`
- [ ] Aplikační logy: `ls -l /home/mimm/mimm-app/logs/`

### Monitoring Tools

- [ ] `htop` funkční
- [ ] `docker stats` ukazuje resource usage
- [ ] `df -h` ukazuje disk space

### Deployment Scripts

- [ ] `deploy.sh` vytvořen a spustitelný
- [ ] `migrate.sh` vytvořen a spustitelný
- [ ] `backup-db.sh` vytvořen a spustitelný
- [ ] `full-backup.sh` vytvořen a spustitelný
- [ ] Test deploy skriptu úspěšný

---

## Fáze 10: Backup & Recovery (Doba: ~10 min)

### Backup Scripts Test

- [ ] Backup složka existuje: `/home/mimm/backups/`
- [ ] Database backup test: `./backup-db.sh`
- [ ] Backup soubor vytvořen
- [ ] Backup soubor není prázdný: `ls -lh ~/backups/`

### Cron Jobs

- [ ] Crontab editován: `crontab -e`
- [ ] Denní DB backup nastaven (2:00 AM)
- [ ] Týdenní full backup nastaven (neděle 3:00 AM)
- [ ] Cron job test: `run-parts --test /etc/cron.daily`

### Restore Test

- [ ] Testovací restore z backupu proveden
- [ ] Data po restore correct
- [ ] Aplikace funguje po restore

---

## Fáze 11: Security Audit (Doba: ~15 min)

### Security Checklist

- [ ] Root login zakázán
- [ ] Password authentication zakázáno
- [ ] SSH pouze na custom portu
- [ ] Firewall aktivní a správně nakonfigurován
- [ ] Fail2Ban aktivní
- [ ] Docker daemon security hardened
- [ ] Kontejnery běží jako non-root
- [ ] Secrets nejsou v git repozitáři
- [ ] `.env` má správná oprávnění (600)
- [ ] HTTPS everywhere (HTTP redirects)
- [ ] HSTS header nastaven
- [ ] Security headers přítomné (X-Frame-Options, CSP, atd.)
- [ ] Rate limiting aktivní
- [ ] SSL certifikáty platné
- [ ] No exposed sensitive ports (5432, 6379)

### Security Tools Run

- [ ] Docker Bench Security spuštěn
- [ ] Lynis audit spuštěn: `lynis audit system`
- [ ] Security issues vyřešeny nebo zdokumentovány

---

## Fáze 12: Performance Testing (Doba: ~20 min)

### Load Testing

- [ ] Basic load test proveden (např. Apache Bench):

  ```bash
  ab -n 1000 -c 10 https://api.your-domain.com/health
  ```

- [ ] Response times přijatelné (< 200ms pro simple endpoints)
- [ ] No errors pod load

### Resource Usage

- [ ] CPU usage v normálu (< 50% idle)
- [ ] Memory usage přijatelná
- [ ] Disk space dostatečný (> 20% free)
- [ ] Docker stats vypadají dobře

### Database Performance

- [ ] Query performance test
- [ ] Indexes nakonfigurovány
- [ ] Connection pooling funguje

---

## Fáze 13: Documentation & Handoff (Doba: ~15 min)

### Documentation

- [ ] Production credentials uloženy v password manageru
- [ ] Emergency kontakty zdokumentovány
- [ ] Runbook vytvořen
- [ ] Known issues zdokumentovány
- [ ] Deployment process zdokumentován

### Knowledge Transfer

- [ ] Team informován o deployment
- [ ] Access credentials sdíleny (bezpečně)
- [ ] Escalation process vysvětlen

### Monitoring Setup

- [ ] Uptime monitoring nastaven (UptimeRobot, Pingdom)
- [ ] Alert notifications nakonfigurovány
- [ ] Status page vytvořena (volitelné)

---

## Fáze 14: Go Live! (Doba: ~5 min)

### Final Checks

- [ ] Všechny služby running
- [ ] Health checks green
- [ ] Frontend accessible
- [ ] Backend API responding
- [ ] User registration funguje
- [ ] User login funguje

### DNS Switch (pokud je potřeba)

- [ ] DNS A records aktualizovány
- [ ] DNS propagace zkontrolována
- [ ] Old server still up (pro fallback)

### Announcement

- [ ] Users informováni o nové platformě
- [ ] Social media update (volitelné)
- [ ] Changelog publikován

---

## Post-Deployment (První hodina)

### Monitoring

- [ ] Watch logs: `docker compose logs -f`
- [ ] Monitor errors: `tail -f /var/log/nginx/error.log`
- [ ] Check resource usage: `docker stats`
- [ ] User feedback monitoring

### Quick Fixes

- [ ] Note any issues
- [ ] Quick fixes applied if needed
- [ ] Hotfix deployment ready

---

## Post-Deployment (První den)

- [ ] Monitor uptime
- [ ] Review logs for errors
- [ ] Check backup ran successfully
- [ ] User feedback collection
- [ ] Performance metrics review

---

## Post-Deployment (První týden)

- [ ] Weekly backup verify
- [ ] SSL certificate auto-renewal test
- [ ] Security audit
- [ ] Performance optimization opportunities identified
- [ ] User satisfaction survey

---

## ✅ Deployment Complete

**Deployment Status:** [ ] Successful / [ ] Issues found  
**Downtime:** _____ minutes  
**Issues encountered:** _________________________________  
**Notes:** ______________________________________________

---

**Signoff:**

**Deployed by:** _________________ **Date:** _________  
**Verified by:** _________________ **Date:** _________  
**Approved by:** _________________ **Date:** _________

---

## 📞 Emergency Contacts

| Role | Name | Contact | Availability |
|------|------|---------|--------------|
| System Admin | __________ | __________ | __________ |
| Developer | __________ | __________ | __________ |
| DevOps | __________ | __________ | __________ |
| Hetzner Support | <support@hetzner.com> | +49 9831 505-0 | 24/7 |

---

## 🔗 Important URLs

- **Frontend:** <https://your-domain.com>
- **Backend API:** <https://api.your-domain.com>
- **Health Check:** <https://api.your-domain.com/health>
- **SSL Labs:** <https://www.ssllabs.com/ssltest/analyze.html?d=your-domain.com>
- **Security Headers:** <https://securityheaders.com/?q=your-domain.com>

---

**Document Version:** 1.0  
**Last Updated:** 24. ledna 2026
