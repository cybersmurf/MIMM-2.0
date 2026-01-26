# MIMM 2.0 - Deployment Guide (Azure)

**Datum:** 26. ledna 2026  
**Target:** Production release na Azure App Service  
**Audience:** DevOps engineers, system administrators

---

## 📋 Deployment Architecture

```
┌─────────────────────────────────────────────────────────┐
│                  Azure Subscription                     │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ┌────────────────────┐      ┌───────────────────────┐ │
│  │  App Service Plan  │      │  Azure Key Vault      │ │
│  │  (Windows/Linux)   │      │  (Secrets Management) │ │
│  └────────────────────┘      └───────────────────────┘ │
│           │                           │                │
│           ▼                           ▼                │
│  ┌────────────────────┐      ┌───────────────────────┐ │
│  │  MIMM Backend API  │◄─────│  Managed Identities   │ │
│  │  (ASP.NET Core 9)  │      │  (for Key Vault)      │ │
│  └────────────────────┘      └───────────────────────┘ │
│           │                                             │
│           ▼                                             │
│  ┌────────────────────┐                               │
│  │  MIMM Frontend     │                               │
│  │  (Blazor WASM)     │                               │
│  └────────────────────┘                               │
│           │                                             │
└───────────┼─────────────────────────────────────────────┘
            │
            ▼
    ┌─────────────────────────────────────┐
    │  Database                           │
    │  (Azure Database for PostgreSQL)    │
    │  - Version: 16                      │
    │  - SSL/TLS Required                 │
    │  - Firewall Rules                   │
    └─────────────────────────────────────┘
```

---

## 🚀 Step-by-Step Deployment

### Phase 1: Infrastructure Setup (Azure Portal)

#### 1.1 Resource Group

```bash
az group create \
  --name mimm-rg \
  --location eastus
```

#### 1.2 Azure Database for PostgreSQL (Flexible Server)

```bash
az postgres flexible-server create \
  --resource-group mimm-rg \
  --name mimm-db \
  --admin-user mimmadmin \
  --admin-password <SecurePassword123!> \
  --sku-name Standard_B1ms \
  --tier Burstable \
  --storage-size 32 \
  --version 16 \
  --high-availability Disabled \
  --public-access Enabled \
  --allow-public-access true
```

**Important:** Configure firewall rules to allow Azure App Service + your IP for setup.

```bash
az postgres flexible-server firewall-rule create \
  --resource-group mimm-rg \
  --name mimm-db \
  --rule-name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

#### 1.3 Create Initial Database

```bash
psql --host=mimm-db.postgres.database.azure.com \
     --port=5432 \
     --username=mimmadmin@mimm-db \
     --dbname=postgres

CREATE DATABASE mimm;
\c mimm
\q
```

#### 1.4 Key Vault (Secrets Management)

```bash
az keyvault create \
  --resource-group mimm-rg \
  --name mimm-kv \
  --enable-soft-delete true

# Store secrets
az keyvault secret set \
  --vault-name mimm-kv \
  --name JwtKey \
  --value "<generate-256-bit-key>"

az keyvault secret set \
  --vault-name mimm-kv \
  --name DbConnectionString \
  --value "Host=mimm-db.postgres.database.azure.com;Port=5432;Database=mimm;Username=mimmadmin@mimm-db;Password=<password>;SslMode=Require;"

az keyvault secret set \
  --vault-name mimm-kv \
  --name LastFmApiKey \
  --value "<last-fm-api-key>"
```

#### 1.5 App Service Plan & Web App

```bash
az appservice plan create \
  --name mimm-plan \
  --resource-group mimm-rg \
  --sku B1 \
  --is-linux

az webapp create \
  --resource-group mimm-rg \
  --plan mimm-plan \
  --name mimm-app \
  --runtime "DOTNETCORE|9.0"
```

---

### Phase 2: Application Configuration

#### 2.1 Web App Settings (App Configuration)

```bash
az webapp config appsettings set \
  --resource-group mimm-rg \
  --name mimm-app \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    Jwt__Issuer=https://mimm.app \
    Jwt__Audience=https://mimm.app \
    Cors__AllowedOrigins__0=https://mimm.app \
    Cors__AllowedOrigins__1=https://www.mimm.app

# Link Key Vault references
az webapp config appsettings set \
  --resource-group mimm-rg \
  --name mimm-app \
  --settings \
    Jwt__Key="@Microsoft.KeyVault(VaultName=mimm-kv,SecretName=JwtKey)" \
    ConnectionStrings__DefaultConnection="@Microsoft.KeyVault(VaultName=mimm-kv,SecretName=DbConnectionString)"
```

#### 2.2 HTTPS Only

```bash
az webapp update \
  --resource-group mimm-rg \
  --name mimm-app \
  --set httpsOnly=true
```

#### 2.3 Managed Identity for Key Vault Access

```bash
# Enable system-assigned identity
az webapp identity assign \
  --resource-group mimm-rg \
  --name mimm-app

# Grant permissions
az keyvault set-policy \
  --name mimm-kv \
  --object-id <managed-identity-object-id> \
  --secret-permissions get list
```

---

### Phase 3: Database Migration & Deployment

#### 3.1 EF Core Migration

```bash
# Locally or in CI/CD:
cd src/MIMM.Backend
dotnet ef migrations add InitialCreate \
  --context ApplicationDbContext

dotnet ef database update \
  --context ApplicationDbContext \
  --connection "Host=mimm-db.postgres.database.azure.com;Port=5432;Database=mimm;Username=mimmadmin@mimm-db;Password=<password>;SslMode=Require;"
```

#### 3.2 Build & Deploy

**Option A: GitHub Actions (Recommended)**

Create `.github/workflows/deploy.yml`:

```yaml
name: Deploy to Azure

on:
  push:
    tags:
      - 'v*'

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      
      - name: Build
        run: dotnet build MIMM.sln --configuration Release
      
      - name: Publish
        run: dotnet publish src/MIMM.Backend -c Release -o ./publish
      
      - name: Azure Login
        uses: azure/login@v1
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}
      
      - name: Deploy
        uses: azure/webapps-deploy@v2
        with:
          app-name: mimm-app
          package: ./publish
```

**Option B: Manual Deployment**

```bash
# Build locally
dotnet publish src/MIMM.Backend -c Release -o ./publish

# ZIP and deploy
cd publish
zip -r ../mimm-backend.zip .

# Upload to Azure
az webapp up --name mimm-app --resource-group mimm-rg
```

---

### Phase 4: Post-Deployment Validation

#### 4.1 Health Check

```bash
curl -I https://mimm-app.azurewebsites.net/health
# Expected: 200 OK
```

#### 4.2 Database Connection

```bash
# App Service console
psql -h mimm-db.postgres.database.azure.com -U mimmadmin@mimm-db -d mimm -c "SELECT COUNT(*) FROM \"Users\";"
```

#### 4.3 Application Insights

```bash
az monitor app-insights create \
  --resource-group mimm-rg \
  --application mimm-insights \
  --location eastus
```

Link to App Service:

```bash
az webapp config appsettings set \
  --resource-group mimm-rg \
  --name mimm-app \
  --settings APPINSIGHTS_INSTRUMENTATIONKEY="<key>"
```

---

## 🔐 Security Hardening

### SSL/TLS Certificate

```bash
# Azure-managed certificate (free)
az appservice plan create \
  --resource-group mimm-rg \
  --name mimm-plan \
  --sku B1 \
  --is-linux \
  --reserved

# Or bring your own certificate
az webapp config ssl bind \
  --certificate-thumbprint <thumbprint> \
  --ssl-type SNI \
  --resource-group mimm-rg \
  --name mimm-app
```

### WAF (Web Application Firewall)

```bash
az network front-door create \
  --resource-group mimm-rg \
  --name mimm-waf \
  --backend-address mimm-app.azurewebsites.net \
  --backend-host-header mimm-app.azurewebsites.net
```

### Database Firewall

```bash
# Allow only Azure App Service
az postgres flexible-server firewall-rule update \
  --resource-group mimm-rg \
  --name mimm-db \
  --rule-name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Disable public access (after setup)
az postgres flexible-server update \
  --resource-group mimm-rg \
  --name mimm-db \
  --public-access false
```

---

## 📊 Monitoring & Logging

### Application Insights Queries

```kusto
// Track authentication failures
customEvents
| where name == "LoginFailure"
| summarize count() by user_Id, bin(timestamp, 1h)

// Monitor API response times
requests
| where name startswith "POST /api"
| summarize avg(duration) by name, resultCode
| order by avg_duration desc

// Rate limit violations
customEvents
| where name == "RateLimitExceeded"
| summarize count() by client_IP, bin(timestamp, 1m)
```

### Backup & Disaster Recovery

```bash
# Automated backups (7 days retention)
az postgres flexible-server parameter set \
  --resource-group mimm-rg \
  --server-name mimm-db \
  --name backup_retention_days \
  --value 7

# Manual backup
pg_dump -h mimm-db.postgres.database.azure.com \
        -U mimmadmin@mimm-db \
        -d mimm > backup.sql
```

---

## 🔄 Continuous Deployment

### GitHub Actions Secrets Setup

```bash
az ad sp create-for-rbac \
  --name mimm-ci \
  --role Contributor \
  --scopes /subscriptions/<subscription-id>/resourceGroups/mimm-rg

# Add to GitHub Secrets:
# AZURE_CREDENTIALS = <json-output>
```

### Deployment Workflow

```
Push to main
    ↓
CI: Build & Test (45/45 ✅)
    ↓
Markdown Lint (0 errors ✅)
    ↓
[Optional] Manual approval
    ↓
Deploy to Staging (mimm-staging.azurewebsites.net)
    ↓
Smoke tests (register, login, create entry)
    ↓
Deploy to Production (mimm-app.azurewebsites.net)
    ↓
Notify team (Slack/Teams)
```

---

## 📋 Pre-Deployment Checklist

- [ ] PostgreSQL database created & accessible
- [ ] Key Vault secrets configured
- [ ] App Service HTTPS enabled
- [ ] Environment variables set
- [ ] EF Core migrations applied
- [ ] Database backup policy configured
- [ ] Application Insights enabled
- [ ] Health check endpoint verified
- [ ] CORS origins configured
- [ ] Rate limiting configured (5 registrations/hour, 10 logins/5 min)
- [ ] WAF rules configured
- [ ] Disaster recovery plan documented
- [ ] Security headers verified (X-Frame-Options, CSP, etc.)
- [ ] JWT secret is strong (256-bit minimum)
- [ ] Database credentials NOT in code (using Key Vault)
- [ ] Logging configured for audit trail
- [ ] Monitoring alerts set up

---

## 🚨 Rollback Strategy

If deployment fails:

```bash
# Revert to previous slot
az webapp slot swap \
  --resource-group mimm-rg \
  --name mimm-app \
  --slot staging

# Or restore from backup
psql -h mimm-db.postgres.database.azure.com \
     -U mimmadmin@mimm-db \
     -d mimm \
     -f backup.sql
```

---

## 📞 Support & Troubleshooting

- **App Service Logs:** `az webapp log tail --name mimm-app --resource-group mimm-rg`
- **Database Logs:** Azure Portal > PostgreSQL > Server Logs
- **Metrics:** Azure Portal > App Service > Metrics
- **Alerts:** Set up notification rules for CPU, memory, HTTP 5xx errors

---

**Created:** 26. ledna 2026  
**Version:** 1.0  
**Status:** Ready for production deployment
