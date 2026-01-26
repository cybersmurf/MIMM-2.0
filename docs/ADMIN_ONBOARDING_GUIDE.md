# MIMM 2.0 - Administrator Onboarding Guide

**Version:** 1.0  
**Date:** 26 January 2026  
**Audience:** System administrators, DevOps engineers, site reliability engineers

---

## 📋 Table of Contents

1. [Before You Start](#before-you-start)
2. [Local Development Setup](#local-development-setup)
3. [Production Deployment](#production-deployment)
4. [Operational Tasks](#operational-tasks)
5. [Monitoring & Alerting](#monitoring--alerting)
6. [Troubleshooting](#troubleshooting)
7. [Security Hardening](#security-hardening)

---

## Before You Start

### Required Knowledge

- **Linux/macOS/Windows:** Command line basics
- **.NET 9:** ASP.NET Core framework concepts
- **PostgreSQL 16:** SQL basics, connection strings
- **Docker/Docker Compose:** Container management
- **Git:** Version control fundamentals
- **GitHub Actions:** CI/CD pipeline understanding

### Required Software

```bash
# Check installed versions
dotnet --version          # Should be 9.0.x
node --version           # Should be 20.x or later
docker --version         # Should be 24.x or later
docker-compose --version # Should be 2.x or later
git --version           # Any recent version
```

### Repository Access

```bash
# Clone the repository
git clone https://github.com/cybersmurf/MIMM-2.0.git
cd MIMM-2.0

# Verify you have main branch
git branch -a
# Should show: main, develop, feature branches

# Check current version
cat README.md | grep Version
# Should show: Version 2.0.1 or later
```

---

## Local Development Setup

### Step 1: Clone and Restore

```bash
# Clone repository
git clone https://github.com/cybersmurf/MIMM-2.0.git
cd MIMM-2.0

# Restore NuGet packages
dotnet restore MIMM.sln

# Verify .NET SDK version
dotnet --version  # Should be 9.0.100 or compatible
cat global.json   # Verify rollForward policy
```

### Step 2: Database Setup

```bash
# Start PostgreSQL container
docker-compose up -d postgres redis

# Verify containers are running
docker-compose ps
# Expected output: postgres (healthy), redis (healthy)

# Check PostgreSQL is accessible
psql -h localhost -p 5432 -U postgres -c "SELECT version();"
# You should see PostgreSQL 16.x running

# Create application database (if not exists)
psql -h localhost -U postgres -c "CREATE DATABASE mimm;"
```

### Step 3: Entity Framework Migrations

```bash
# Apply database migrations
dotnet ef database update \
  -p src/MIMM.Backend/MIMM.Backend.csproj \
  -c ApplicationDbContext

# Verify tables were created
psql -h localhost -U postgres -d mimm -c "\dt"
# Should list tables: users, journal_entries, last_fm_tokens, etc.

# Check migration history
psql -h localhost -U postgres -d mimm -c "SELECT * FROM __EFMigrationsHistory;"
```

### Step 4: Create Test User

```bash
# Register a test user via API
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "AdminTest123!",
    "displayName": "Admin User"
  }'

# Expected response: { "userId": "...", "accessToken": "..." }
```

### Step 5: Build and Run

```bash
# Build entire solution
dotnet build MIMM.sln --configuration Release

# Run backend API
dotnet run --project src/MIMM.Backend/MIMM.Backend.csproj

# Backend should start on https://localhost:7001
# Health check: curl https://localhost:7001/health

# In another terminal, run frontend
cd src/MIMM.Frontend && dotnet watch

# Frontend should start on http://localhost:5000
```

---

## Production Deployment

### Deployment Options

#### Option 1: Azure App Service (Recommended)

See [Azure Deployment Guide](./AZURE_DEPLOYMENT_GUIDE.md) for detailed steps.

```bash
# Quick summary:
# 1. Create resource group & app service
# 2. Set up PostgreSQL Database for Azure
# 3. Configure Key Vault for secrets
# 4. Set up managed identity
# 5. Deploy via GitHub Actions
```

#### Option 2: Docker Compose (Self-Hosted)

```bash
# Update docker-compose.yml for production
# 1. Change PostgreSQL password
# 2. Configure environment variables
# 3. Set up reverse proxy (Nginx/Traefik)
# 4. Configure SSL certificates

docker-compose -f docker-compose.prod.yml up -d

# Verify deployment
curl https://yourdomain.com/health
```

#### Option 3: Kubernetes (Advanced)

```bash
# Create Helm chart or Kustomize manifests
# Deploy to AKS, EKS, or GKE cluster
# Example: kubectl apply -f k8s/mimm-deployment.yaml
```

---

## Operational Tasks

### Daily Operations

#### Health Monitoring

```bash
# Check API health
curl -i https://mimm.app/health

# Check database connection
curl -i https://mimm.app/api/health/db

# View recent logs
az webapp log tail --name mimm-app --resource-group mimm-rg

# Or with Docker:
docker-compose logs -f mimm-backend
```

#### Database Maintenance

```bash
# Backup production database
pg_dump -h mimm-db.postgres.database.azure.com \
        -U mimmadmin@mimm-db \
        -d mimm > backup-$(date +%Y%m%d).sql

# Verify backup
gunzip -c backup-20260126.sql | head -20

# Restore from backup (if needed)
psql -h mimm-db.postgres.database.azure.com \
     -U mimmadmin@mimm-db \
     -d mimm < backup-20260126.sql
```

### Weekly Operations

#### Security Updates

```bash
# Check for NuGet package updates
dotnet list package --outdated

# Update vulnerable packages
dotnet package update --target-framework net9.0

# Rebuild and test
dotnet build MIMM.sln --configuration Release
dotnet test MIMM.sln
```

#### Performance Review

```bash
# Query Application Insights for top errors
# Azure Portal > Application Insights > Failures

# Check slow queries
# PostgreSQL > Query Performance > Slow Queries

# Review rate limiting events
curl https://mimm.app/api/metrics/rate-limit
```

### Monthly Operations

#### Feature Releases

```bash
# Create release branch
git checkout -b release/v2.1.0

# Update version numbers
# - README.md
# - CHANGELOG.md
# - AGENTS.md
# - src/**/csproj files (if needed)

git commit -m "chore(release): bump version to v2.1.0"
git push origin release/v2.1.0

# Create Pull Request on GitHub
# After approval and merge, tag release:
git tag -a v2.1.0 -m "Release v2.1.0 - New features"
git push origin v2.1.0

# GitHub Actions will automatically deploy to production
```

#### Database Schema Changes

```bash
# Create new EF migration
dotnet ef migrations add AddNewFeature \
  -p src/MIMM.Backend/MIMM.Backend.csproj

# Review generated migration file
# src/MIMM.Backend/Data/Migrations/*_AddNewFeature.cs

# Test locally
dotnet ef database update -p src/MIMM.Backend

# Run tests to ensure no breaking changes
dotnet test MIMM.sln

# Commit and push
git add src/MIMM.Backend/Data/Migrations/
git commit -m "feat(db): add migration for new feature"
git push origin feature-branch
```

---

## Monitoring & Alerting

### Application Insights Setup

```bash
# Create Application Insights resource
az monitor app-insights create \
  --resource-group mimm-rg \
  --application mimm-insights \
  --location eastus

# Get instrumentation key
INSTRUMENTATION_KEY=$(az monitor app-insights component show \
  -g mimm-rg -a mimm-insights \
  --query instrumentationKey -o tsv)

# Add to App Service configuration
az webapp config appsettings set \
  --resource-group mimm-rg \
  --name mimm-app \
  --settings APPINSIGHTS_INSTRUMENTATIONKEY="$INSTRUMENTATION_KEY"
```

### Key Metrics to Monitor

| Metric | Threshold | Alert |
|--------|-----------|-------|
| HTTP 5xx errors | > 10 per hour | WARNING |
| API response time (p95) | > 5 seconds | NOTICE |
| Database query time (p95) | > 1 second | NOTICE |
| CPU utilization | > 80% | WARNING |
| Memory usage | > 85% | WARNING |
| Disk usage | > 90% | CRITICAL |
| Rate limit violations | > 100 per minute | INFO |

### Kusto Queries (Application Insights)

```kusto
// Top 10 slowest API endpoints
requests
| where duration > 1000
| summarize SlowCalls=count(), AvgDuration=avg(duration) by name
| order by AvgDuration desc
| limit 10

// Authentication failures by email
customEvents
| where name == "LoginFailure"
| summarize count() by tostring(customDimensions.email)

// Rate limit violations
customEvents
| where name == "RateLimitExceeded"
| summarize count() by client_IP, bin(timestamp, 1h)

// Database connection pool exhaustion
customEvents
| where name == "DbConnectionPoolExhausted"
| summarize count() by bin(timestamp, 5m)
```

---

## Troubleshooting

### Problem: Backend Won't Start

```bash
# Check port is available
lsof -i :5001 | grep -v COMMAND | awk '{print $2}' | xargs kill -9

# Check database connection string
echo $DefaultConnection

# Verify PostgreSQL is running
docker-compose ps postgres

# Check logs
docker-compose logs postgres

# Restart backend with verbose logging
dotnet run --project src/MIMM.Backend -- --verbosity debug
```

### Problem: High Database Latency

```bash
# Check running queries
psql -h localhost -U postgres -d mimm \
  -c "SELECT * FROM pg_stat_statements ORDER BY mean_time DESC LIMIT 10;"

# Check connection pool stats
psql -h localhost -U postgres -d mimm \
  -c "SELECT * FROM pg_stat_database WHERE datname='mimm';"

# Restart database if needed
docker-compose restart postgres
```

### Problem: Users Can't Login

```bash
# Check user exists in database
psql -h localhost -U postgres -d mimm \
  -c "SELECT id, email, is_email_verified FROM users WHERE email='test@example.com';"

# Check rate limiting isn't blocking login
curl -X GET http://localhost:5001/api/metrics/rate-limit \
  -H "Authorization: Bearer <admin-token>"

# Check JWT secret is configured
cat src/MIMM.Backend/appsettings.json | grep -A 3 "Jwt"
```

### Problem: Tests Failing in CI

```bash
# Run tests locally to reproduce
dotnet test MIMM.sln --configuration Release -v normal

# Check test logs
cat logs/*.txt

# Run specific failing test
dotnet test MIMM.sln -k "TestName"

# Enable debug logging
export DOTNET_CLI_VERBOSITY=diagnostic
dotnet test MIMM.sln
```

---

## Security Hardening

### Pre-Deployment Checklist

- [ ] JWT secret is strong (256-bit minimum)
- [ ] Database password is random and complex
- [ ] HTTPS is enabled (TLS 1.3+)
- [ ] CORS is configured for your domain only
- [ ] Rate limiting is enabled
- [ ] Security headers are set (X-Frame-Options, CSP, HSTS)
- [ ] SQL injection prevention (EF Core parameterized queries)
- [ ] XSS protection (output encoding in Blazor)
- [ ] CSRF tokens enabled
- [ ] Secrets stored in Key Vault (not in code)

### Post-Deployment Security Tasks

```bash
# Enable HTTPS only
az webapp update \
  --resource-group mimm-rg \
  --name mimm-app \
  --set httpsOnly=true

# Configure security headers via middleware
# See: src/MIMM.Backend/Middleware/SecurityHeadersMiddleware.cs

# Set up WAF rules
az network front-door create \
  --resource-group mimm-rg \
  --name mimm-waf

# Enable logging and monitoring
az monitor diagnostic-settings create \
  --name mimm-diagnostics \
  --resource /subscriptions/{sub-id}/resourceGroups/mimm-rg/providers/Microsoft.Web/sites/mimm-app \
  --logs '[{"category":"AppServiceHTTPLogs","enabled":true}]'
```

### Incident Response

```bash
# If data breach suspected:
# 1. Revoke all JWT tokens
az webapp config appsettings set \
  --resource-group mimm-rg \
  --name mimm-app \
  --settings Jwt__Key="<new-strong-key>"

# 2. Check access logs
az webapp log download \
  --resource-group mimm-rg \
  --name mimm-app

# 3. Notify affected users
# Email template in: docs/templates/security-breach-notification.html

# 4. Perform database audit
psql -h localhost -U postgres -d mimm \
  -c "SELECT * FROM audit_log WHERE created_at > NOW() - INTERVAL '1 hour' ORDER BY created_at DESC;"

# 5. Enable MFA for all admin accounts
```

---

## Useful Commands Reference

```bash
# 🏗️ Build & Deploy
dotnet restore MIMM.sln
dotnet build MIMM.sln --configuration Release
dotnet publish src/MIMM.Backend -c Release -o ./publish

# 🧪 Testing
dotnet test MIMM.sln --configuration Release
dotnet test MIMM.sln -k "TestName" --verbosity normal

# 🗄️ Database
dotnet ef migrations add MigrationName -p src/MIMM.Backend
dotnet ef database update -p src/MIMM.Backend
pg_dump -h localhost -U postgres -d mimm > backup.sql

# 🐳 Docker
docker-compose up -d
docker-compose logs -f postgres
docker-compose down -v

# 🚀 Git & Releases
git checkout -b release/v2.1.0
git tag -a v2.1.0 -m "Release v2.1.0"
git push origin v2.1.0

# ☁️ Azure
az group create --name mimm-rg --location eastus
az webapp create --resource-group mimm-rg --plan mimm-plan --name mimm-app
az webapp config appsettings set --resource-group mimm-rg --name mimm-app --settings KEY="VALUE"

# 📊 Monitoring
curl https://mimm.app/health
curl -i https://mimm.app/api/health/db
az webapp log tail --name mimm-app --resource-group mimm-rg
```

---

## Next Steps

1. **Complete Local Setup:** Follow [Local Development Setup](#local-development-setup)
2. **Deploy to Staging:** Use [Azure Deployment Guide](./AZURE_DEPLOYMENT_GUIDE.md)
3. **Configure Monitoring:** Set up [Monitoring & Alerting](#monitoring--alerting)
4. **Run E2E Tests:** See [E2E Test Guide](./E2E_TEST_GUIDE.md)
5. **Security Audit:** Complete [Security Hardening](#security-hardening) checklist

---

## Support

- **Documentation:** See [docs/](../docs/) folder
- **Issues:** Report bugs on GitHub Issues
- **Security:** Report vulnerabilities to security@mimm.app
- **Questions:** Open a Discussion on GitHub

---

**Created:** 26 January 2026  
**Version:** 1.0  
**Status:** Production Ready
