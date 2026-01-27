# MIMM 2.0 - Quick Start Guide for Admins

**Last Updated:** 26 January 2026  
**Time to Setup:** ~30 minutes (local dev) or ~2 hours (Azure production)

---

## 🚀 Choose Your Path

### Path 1: Local Development (30 min)

You want to develop locally and test features.

```bash
# 1. Clone repository
git clone https://github.com/cybersmurf/MIMM-2.0.git
cd MIMM-2.0

# 2. Start databases (Docker required)
docker-compose up -d postgres redis

# 3. Restore & build
dotnet restore
dotnet build MIMM.sln --configuration Release

# 4. Apply database migrations
dotnet ef database update -p src/MIMM.Backend

# 5. Start backend (Terminal 1)
dotnet run --project src/MIMM.Backend/MIMM.Backend.csproj

# 6. Start frontend (Terminal 2)
cd src/MIMM.Frontend && dotnet watch

# 7. Open browser
# Frontend: http://localhost:5000
# Backend: https://localhost:7001
```

### Path 2: Production on Azure (2 hours)

You want to deploy to production with enterprise features.

**See:** [Azure Deployment Guide](AZURE_DEPLOYMENT_GUIDE.md) – Complete instructions

Quick summary:

```bash
# 1. Create Azure resources
az group create --name mimm-rg --location eastus
az postgres flexible-server create --name mimm-db ...
az keyvault create --name mimm-kv ...
az webapp create --name mimm-app ...

# 2. Configure & deploy
# Follow guide for environment variables, SSL, monitoring

# 3. Verify
curl https://mimm.app/health
```

### Path 3: Docker Compose (Production) (1 hour)

You want to self-host on your own server.

```bash
# Update docker-compose.prod.yml with your settings
docker-compose -f docker-compose.prod.yml up -d

# Verify
curl http://localhost/health
```

---

## 📋 Verify Installation

### Quick Health Check

```bash
# Backend health
curl -i http://localhost:5001/health
# Expected: 200 OK

# Database connection
curl -i http://localhost:5001/api/health/db
# Expected: 200 OK with database status

# Frontend loading
curl -i http://localhost:5000
# Expected: 200 OK with HTML
```

### Run Tests

```bash
# All tests (should pass)
dotnet test MIMM.sln

# Specific test
dotnet test MIMM.sln -k "LoginService"
```

---

## 🔑 First Admin Account

### Create via Registration (UI)

1. Open http://localhost:5000/register
2. Enter email: `admin@example.com`
3. Password: `Admin123!` (at least 8 chars, uppercase, number, special char)
4. Click "Create Account"

### Create via API

```bash
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "Admin123!",
    "displayName": "Admin User"
  }'
```

### Verify in Database

```bash
psql -h localhost -U postgres -d mimm \
  -c "SELECT id, email, display_name FROM users WHERE email='admin@example.com';"
```

---

## 📊 Key Commands Reference

### Build & Deploy

```bash
# Build Release version
dotnet build MIMM.sln --configuration Release

# Publish for deployment
dotnet publish src/MIMM.Backend -c Release -o ./publish

# Run tests
dotnet test MIMM.sln --configuration Release
```

### Database

```bash
# Apply migrations
dotnet ef database update -p src/MIMM.Backend

# Create migration
dotnet ef migrations add MigrationName -p src/MIMM.Backend

# Backup production database
pg_dump -h prod-db.postgres.database.azure.com \
        -U admin@prod-db \
        -d mimm > backup.sql
```

### Docker

```bash
# Start services
docker-compose up -d

# View logs
docker-compose logs -f postgres

# Stop everything
docker-compose down
```

### Azure Deployment

```bash
# Check app status
az webapp show -g mimm-rg -n mimm-app --query state

# View logs
az webapp log tail -g mimm-rg -n mimm-app

# Configure settings
az webapp config appsettings set \
  --resource-group mimm-rg \
  --name mimm-app \
  --settings KEY="VALUE"
```

---

## ⚠️ Common Issues

| Issue | Solution |
|-------|----------|
| Port 5001 already in use | `lsof -i :5001` → kill process or use different port |
| Database connection refused | `docker-compose ps postgres` → ensure running |
| Tests failing | `dotnet test -v normal` for detailed output |
| Frontend not loading | Check backend CORS config, browser console for errors |
| Slow queries | Check `docker-compose logs postgres` for warnings |

---

## 📚 Next Steps

1. **Read:** [Admin Onboarding Guide](ADMIN_ONBOARDING_GUIDE.md) – Detailed operations manual
2. **Test:** [E2E Test Guide](testing/E2E_TEST_GUIDE.md) – Run test suite
3. **Deploy:** [Azure Deployment Guide](deployment/AZURE_DEPLOYMENT_GUIDE.md) – Production setup
4. **Monitor:** Set up Application Insights for production monitoring

---

## 🆘 Need Help?

- **Docs:** See [docs/](../docs/) folder
- **Issues:** https://github.com/cybersmurf/MIMM-2.0/issues
- **GitHub:** https://github.com/cybersmurf/MIMM-2.0

---

**Version:** 2.0.1  
**Status:** ✅ Production Ready (97% Complete)

Next review: 2 February 2026 (Staging Deployment)
