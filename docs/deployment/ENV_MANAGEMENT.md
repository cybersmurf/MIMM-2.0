# .env Management Guide - MIMM 2.0

**⚠️ CRITICAL:** `.env` files contain production secrets. They are **NEVER** committed to git.

---

## What is .env?

`.env` is a local configuration file containing sensitive data:
- Database credentials (username, password)
- JWT signing key
- API keys (Last.fm, SendGrid, etc.)
- Application URLs
- Environment mode (Development vs Production)

---

## Local Development Setup

### Step 1: Create `.env` in Project Root

```bash
cd /Users/petrsramek/AntigravityProjects/MIMM-2.0
cat > .env << 'EOF'
# PostgreSQL Configuration
POSTGRES_USER=mimm
POSTGRES_PASSWORD=YOUR_DEV_PASSWORD_HERE
POSTGRES_DB=mimm

# JWT Authentication
JWT_SECRET_KEY=HaBTAZysXEcWLMaXMeelHiHHcJu/8+fwhm81kD3aKGc=
JWT_ISSUER=https://api.musicinmymind.app
JWT_AUDIENCE=mimm-frontend

# Application URLs
FRONTEND_URL=https://musicinmymind.app
BACKEND_URL=https://api.musicinmymind.app

# Environment Mode
ASPNETCORE_ENVIRONMENT=Development
EOF
```

### Step 2: Verify .env is Ignored

```bash
# Check .gitignore
grep ".env" .gitignore

# Should output:
# .env
# .env.local
# .env.*.local

# Verify .env not tracked
git status | grep .env

# Should show: nothing (not tracked)
```

---

## Production VPS Setup

### Step 1: Securely Generate Production Secrets

```bash
# On your local machine (or VPS), generate strong JWT key:
openssl rand -base64 64

# Generate strong password (PostgreSQL):
# Use a password manager or: openssl rand -hex 32
```

### Step 2: Create `.env` on VPS

```bash
# SSH to VPS
ssh user@musicinmymind.app

# Navigate to deployment folder
cd /opt/mimm  # or your deployment path

# Create .env with production values
# Use generated secrets from Step 1
cat > .env << 'EOF'
POSTGRES_USER=mimm_prod
POSTGRES_PASSWORD=YOUR_SUPER_SECURE_PASSWORD_FROM_STEP_1
POSTGRES_DB=mimm_production

JWT_SECRET_KEY=YOUR_GENERATED_KEY_FROM_STEP_1_openssl_rand_-base64_64
JWT_ISSUER=https://api.musicinmymind.app
JWT_AUDIENCE=mimm-frontend

FRONTEND_URL=https://musicinmymind.app
BACKEND_URL=https://api.musicinmymind.app

ASPNETCORE_ENVIRONMENT=Production
EOF

# Restrict permissions (only owner can read)
chmod 600 .env

# Verify
ls -la .env  # Should show: -rw------- (600 permissions)
```

### Step 3: Start Services with Production .env

```bash
# From the deployment directory on VPS
cd /opt/mimm

# Build and start
docker compose build
docker compose up -d

# Verify all running
docker compose ps

# Check logs
docker compose logs backend | tail -20
```

---

## Updating .env

### Local Development

```bash
# Edit .env file directly
nano .env  # or vim, or your editor

# Restart services
docker compose down
docker compose up -d

# Verify changes took effect
docker compose logs backend | grep "Database connection"
```

### Production VPS

```bash
# SSH to server
ssh user@musicinmymind.app
cd /opt/mimm

# Edit .env (example: update JWT key)
nano .env

# Stop services gracefully
docker compose down

# Start with new configuration
docker compose up -d

# Verify and monitor
docker compose logs -f backend --tail 50
```

---

## .env Variables Reference

| Variable | Required | Example | Notes |
|---|---|---|---|
| `POSTGRES_USER` | ✅ | `mimm` | Database username |
| `POSTGRES_PASSWORD` | ✅ | `very_secure_password` | Database password - use `openssl rand -hex 32` |
| `POSTGRES_DB` | ✅ | `mimm` | Database name |
| `JWT_SECRET_KEY` | ✅ | `base64_string` | Use `openssl rand -base64 64` - min 32 chars |
| `JWT_ISSUER` | ✅ | `https://api.musicinmymind.app` | Must match API domain |
| `JWT_AUDIENCE` | ✅ | `mimm-frontend` | Token audience identifier |
| `FRONTEND_URL` | ✅ | `https://musicinmymind.app` | SPA frontend domain (for CORS) |
| `BACKEND_URL` | ✅ | `https://api.musicinmymind.app` | API backend domain |
| `ASPNETCORE_ENVIRONMENT` | ✅ | `Development` or `Production` | Development auto-migrates DB |
| `LASTFM_API_KEY` | ❌ | `abc123xyz` | Optional - Last.fm integration |
| `LASTFM_SHARED_SECRET` | ❌ | `secret123` | Optional - Last.fm integration |
| `SENDGRID_API_KEY` | ❌ | `sg_live_...` | Optional - Email notifications |

---

## Security Best Practices

### ✅ DO:
- ✅ Generate strong passwords: `openssl rand -hex 32` → 64 hex chars
- ✅ Generate strong JWT keys: `openssl rand -base64 64` → 88 base64 chars
- ✅ Use unique passwords for each environment (dev, staging, prod)
- ✅ Store production `.env` in secure location (not git, use VCS like 1Password)
- ✅ Restrict file permissions: `chmod 600 .env`
- ✅ Backup `.env` separately from code repository
- ✅ Rotate JWT keys periodically (quarterly)
- ✅ Use secrets manager (AWS Secrets Manager, Azure Key Vault, etc.) for production

### ❌ DON'T:
- ❌ Commit `.env` to Git (even accidentally)
- ❌ Share `.env` via Slack, email, or chat
- ❌ Use same password across environments
- ❌ Use weak passwords like `postgres` or `password123`
- ❌ Keep `.env` in `/tmp` or world-readable locations
- ❌ Log `.env` values in container logs (security risk)
- ❌ Use placeholder values in production

---

## Troubleshooting

### Problem: "cannot find .env"

```bash
# Check if file exists
ls -la .env

# If missing, create it:
cat > .env << 'EOF'
# Your variables here
EOF
```

### Problem: Variables not taking effect

**Solution:** Docker caches environment. Rebuild and restart:
```bash
docker compose down -v  # Remove volumes
docker compose build --no-cache backend
docker compose up -d
```

### Problem: "permission denied" on .env

```bash
# Fix permissions
chmod 600 .env

# Verify
ls -l .env  # Should show: -rw------- 1 user user
```

### Problem: Accidentally committed .env to Git

```bash
# IMMEDIATELY ROTATE ALL SECRETS IN .env

# Remove from git history:
git rm --cached .env
git commit -m "remove .env from tracking (secrets rotated)"
git push origin main

# Reset sensitive values on all systems
```

---

## Development vs Production

### Development (.env)
```
POSTGRES_PASSWORD=dev_password (can be simple)
JWT_SECRET_KEY=dev_key_at_least_32_characters
ASPNETCORE_ENVIRONMENT=Development  # Auto-migrates DB
```

### Production (.env)
```
POSTGRES_PASSWORD=VERY_SECURE_GENERATED_PASSWORD_32_CHARS
JWT_SECRET_KEY=VERY_SECURE_GENERATED_KEY_64_CHARS
ASPNETCORE_ENVIRONMENT=Production  # Requires manual migration
FRONTEND_URL=https://musicinmymind.app  # Real domain, not localhost
```

---

## Backup Strategy

### Local Backup
```bash
# Encrypt and backup .env locally
gpg -c .env  # Creates .env.gpg (password-protected)

# Or use Mac's built-in encryption:
openssl enc -aes-256-cbc -in .env -out .env.enc
```

### VPS Backup (Manual)
```bash
# On VPS, separate from code directory
cp /opt/mimm/.env /secure/backup/.env.backup.$(date +%Y%m%d)

# Secure the backup location
chmod 700 /secure/backup/
chmod 600 /secure/backup/.env.*
```

### Never Backup To:
- ❌ Public git repositories
- ❌ Unencrypted cloud storage
- ❌ Shared server locations
- ❌ Email or Slack

---

## Audit & Monitoring

```bash
# Check when .env was last modified
stat .env

# Verify .env permissions are correct
ls -l .env  # Must be -rw------- (600)

# Check if .env ever got committed (it shouldn't be)
git log --all -- .env  # Should be empty

# Monitor access to .env
lastlog | grep .env  # Show when file was accessed
```

---

**Last Updated:** 27. ledna 2026 | **Version:** 1.0
