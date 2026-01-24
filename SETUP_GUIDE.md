# MIMM 2.0 - Complete Setup Guide

**Version**: 1.0  
**Date**: 24. ledna 2026  
**For**: Developers setting up MIMM 2.0 for the first time

---

## Prerequisites

### Required Software

- **.NET 9 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop)
- **PostgreSQL 16** - (via Docker or local install)
- **Git** - [Download](https://git-scm.com/)
- **Visual Studio 2025** or **VS Code** with C# Dev Kit

### Optional Software

- **Azure Data Studio** or **pgAdmin** (PostgreSQL GUI)
- **Postman** or **Insomnia** (API testing)
- **Redis** (optional caching layer)

### Check Prerequisites

```bash
# Check .NET version
dotnet --version
# Expected: 9.0.x

# Check Docker
docker --version
# Expected: Docker version 24.x+

# Check Git
git --version
```

---

## Part 1: Clone Repository

```bash
# Clone the repository
git clone https://github.com/YOUR_USERNAME/MIMM-2.0.git
cd MIMM-2.0

# Verify structure
ls -la
# Expected: MIMM.sln, src/, tests/, docker-compose.yml, etc.
```

---

## Part 2: Database Setup (Docker)

### Option A: Use Docker Compose (Recommended)

```bash
# Start PostgreSQL + Redis containers
docker-compose up -d postgres redis

# Verify containers are running
docker ps
# Expected: mimm-postgres (port 5432), mimm-redis (port 6379)

# Check PostgreSQL logs
docker logs mimm-postgres

# Test connection
docker exec -it mimm-postgres psql -U postgres -d mimm_dev -c "SELECT version();"
```

### Option B: Local PostgreSQL Installation

```bash
# macOS (via Homebrew)
brew install postgresql@16
brew services start postgresql@16

# Create database
createdb mimm_dev

# Verify
psql mimm_dev -c "SELECT version();"
```

---

## Part 3: Configuration

### Step 1: Copy Environment Template

```bash
cp .env.example .env
```

### Step 2: Edit `.env` File

Open `.env` in your editor and set:

```bash
# Database (if using Docker, this is already correct)
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=mimm_dev;Username=postgres;Password=postgres

# JWT Secret (generate with: openssl rand -base64 32)
Jwt__Key=YOUR_GENERATED_SECRET_KEY_HERE

# CORS (frontend URL)
Cors__AllowedOrigins__0=https://localhost:5001

# Last.fm (get from https://www.last.fm/api/account/create)
LastFm__ApiKey=YOUR_LASTFM_API_KEY
LastFm__SharedSecret=YOUR_LASTFM_SHARED_SECRET

# Optional: Discogs, SendGrid, Redis
```

### Step 3: User Secrets (Development)

For added security, use .NET User Secrets for sensitive data:

```bash
cd src/MIMM.Backend

# Initialize user secrets
dotnet user-secrets init

# Set JWT key
dotnet user-secrets set "Jwt:Key" "your-secret-key-here"

# Set database connection
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=mimm_dev;Username=postgres;Password=postgres"

# Set Last.fm credentials
dotnet user-secrets set "LastFm:ApiKey" "your-api-key"
dotnet user-secrets set "LastFm:SharedSecret" "your-shared-secret"

# List all secrets
dotnet user-secrets list
```

---

## Part 4: Build & Restore

### Step 1: Restore NuGet Packages

```bash
# From solution root
dotnet restore

# Expected output: "Restore succeeded."
```

### Step 2: Build Solution

```bash
# Build all projects
dotnet build

# Expected output: "Build succeeded. 0 Warning(s). 0 Error(s)."
```

### Step 3: Verify Build

```bash
# Check build artifacts
ls -la src/MIMM.Backend/bin/Debug/net9.0/
# Expected: MIMM.Backend.dll, appsettings.json, etc.
```

---

## Part 5: Database Migrations

### Step 1: Install EF Core Tools

```bash
# Install globally
dotnet tool install --global dotnet-ef

# Verify
dotnet ef --version
# Expected: Entity Framework Core .NET Command-line Tools 9.0.x
```

### Step 2: Create Initial Migration

```bash
cd src/MIMM.Backend

# Create migration
dotnet ef migrations add InitialCreate

# Expected output: "Done. To undo... dotnet ef migrations remove"
```

### Step 3: Apply Migration to Database

```bash
# Update database
dotnet ef database update

# Expected output: "Applying migration '20260124_InitialCreate'... Done."
```

### Step 4: Verify Database Schema

```bash
# Connect to PostgreSQL
docker exec -it mimm-postgres psql -U postgres -d mimm_dev

# List tables
\dt

# Expected output:
# public | Users
# public | Entries
# public | LastFmTokens
# public | __EFMigrationsHistory

# Exit
\q
```

---

## Part 6: Run Backend

### Step 1: Start Backend API

```bash
cd src/MIMM.Backend

# Run in development mode
dotnet run

# Expected output:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: https://localhost:7001
#       Now listening on: http://localhost:7000
```

### Step 2: Verify API is Running

Open browser or use curl:

```bash
# Health check
curl https://localhost:7001/health
# Expected: {"status":"healthy","timestamp":"2026-01-24T..."}

# Swagger UI
open https://localhost:7001/swagger
```

### Step 3: Test API Endpoints

In Swagger UI, try:

1. **POST /api/auth/register**

   ```json
   {
     "email": "test@example.com",
     "password": "Test123!",
     "displayName": "Test User"
   }
   ```

   Expected: `200 OK`

2. **POST /api/auth/login**

   ```json
   {
     "email": "test@example.com",
     "password": "Test123!"
   }
   ```

   Expected: `{ "accessToken": "eyJ...", "user": {...} }`

---

## Part 7: Run Frontend (Blazor WASM)

### Step 1: Configure Frontend

Edit `src/MIMM.Frontend/wwwroot/appsettings.json`:

```json
{
  "ApiBaseUrl": "https://localhost:7001"
}
```

### Step 2: Start Frontend

```bash
cd src/MIMM.Frontend

# Run in development mode
dotnet run

# Expected output:
# Now listening on: https://localhost:5001
# Now listening on: http://localhost:5000
```

### Step 3: Open in Browser

```bash
open https://localhost:5001
```

Expected: MIMM 2.0 login page

---

## Part 8: Run Tests

### Unit Tests

```bash
cd tests/MIMM.Tests.Unit
dotnet test

# Expected: "Passed! - Failed: 0, Passed: X, Skipped: 0"
```

### Integration Tests

```bash
cd tests/MIMM.Tests.Integration
dotnet test

# Expected: "Passed! - Failed: 0, Passed: X, Skipped: 0"
```

### Test with Coverage

```bash
# Install ReportGenerator
dotnet tool install --global dotnet-reportgenerator-globaltool

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate HTML report
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage-report

# Open report
open coverage-report/index.html
```

---

## Part 9: Docker Setup (Full Stack)

### Build Docker Image

```bash
# From solution root
docker build -t mimm-backend:latest .

# Verify image
docker images | grep mimm
```

### Run Full Stack with Docker Compose

```bash
# Start all services (postgres, redis, backend)
docker-compose up -d

# Check logs
docker-compose logs -f backend

# Stop all services
docker-compose down
```

### Access Services

- **Backend API**: <https://localhost:7001>
- **Swagger**: <https://localhost:7001/swagger>
- **PostgreSQL**: localhost:5432
- **Redis**: localhost:6379

---

## Part 10: IDE Setup

### Visual Studio 2025

1. Open `MIMM.sln`
2. Set `MIMM.Backend` as startup project
3. Press F5 to run
4. Breakpoints work automatically

### VS Code

1. Open `MIMM-2.0` folder
2. Install extensions:
   - C# Dev Kit
   - C# Extensions
   - Docker
3. Open Command Palette (Cmd+Shift+P)
4. Select: "C#: Generate Assets for Build and Debug"
5. Press F5 to run

---

## Part 11: Troubleshooting

### Problem: "dotnet: command not found"

**Solution**: Install .NET SDK or add to PATH

```bash
# macOS - add to ~/.zshrc or ~/.bash_profile
export PATH="$PATH:/usr/local/share/dotnet"
```

### Problem: PostgreSQL connection fails

**Solution**: Check Docker container status

```bash
docker ps
docker logs mimm-postgres

# Restart if needed
docker-compose restart postgres
```

### Problem: Port 5432 already in use

**Solution**: Stop local PostgreSQL or change port

```bash
# Stop local PostgreSQL
brew services stop postgresql@16

# OR change Docker port in docker-compose.yml
ports:
  - "5433:5432"  # Use 5433 externally
```

### Problem: Migrations fail with "column already exists"

**Solution**: Reset database

```bash
# Drop database
docker exec -it mimm-postgres psql -U postgres -c "DROP DATABASE mimm_dev;"

# Recreate
docker exec -it mimm-postgres psql -U postgres -c "CREATE DATABASE mimm_dev;"

# Reapply migrations
cd src/MIMM.Backend
dotnet ef database update
```

### Problem: HTTPS certificate error

**Solution**: Trust development certificate

```bash
dotnet dev-certs https --trust
```

### Problem: Swagger not loading

**Solution**: Check appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

---

## Part 12: Next Steps

### Add Your First Feature

1. **Create Controller** in `src/MIMM.Backend/Controllers/`
2. **Add Service** in `src/MIMM.Backend/Services/`
3. **Update DbContext** if adding entities
4. **Create Migration**: `dotnet ef migrations add FeatureName`
5. **Write Tests** in `tests/`

### Deploy to Production

See `DEPLOYMENT.md` for:

- Azure App Service deployment
- Docker production setup
- Environment variable configuration
- SSL/HTTPS setup
- Monitoring & logging

### Connect Last.fm

1. Get API key: <https://www.last.fm/api/account/create>
2. Add to `.env`:

   ```
   LastFm__ApiKey=YOUR_KEY
   LastFm__SharedSecret=YOUR_SECRET
   LastFm__CallbackUrl=https://localhost:7001/api/integrations/lastfm/callback
   ```

3. Implement OAuth flow in `LastFmService.cs`

---

## Part 13: Useful Commands Cheat Sheet

```bash
# === BUILD ===
dotnet restore                 # Restore NuGet packages
dotnet build                   # Build solution
dotnet clean                   # Clean build artifacts

# === RUN ===
dotnet run --project src/MIMM.Backend     # Run backend
dotnet watch run                          # Run with hot reload

# === TEST ===
dotnet test                              # Run all tests
dotnet test --filter Category=Unit       # Run unit tests only
dotnet test --logger "console;verbosity=detailed"  # Verbose output

# === MIGRATIONS ===
dotnet ef migrations add MigrationName   # Create migration
dotnet ef database update                # Apply migrations
dotnet ef migrations remove              # Remove last migration
dotnet ef database drop                  # Drop database

# === DOCKER ===
docker-compose up -d              # Start services
docker-compose down               # Stop services
docker-compose logs -f backend    # View backend logs
docker-compose ps                 # List running services

# === USER SECRETS ===
dotnet user-secrets init                    # Initialize
dotnet user-secrets set "Key" "Value"       # Set secret
dotnet user-secrets list                    # List all secrets
dotnet user-secrets clear                   # Clear all secrets

# === PUBLISH ===
dotnet publish -c Release -o ./publish     # Publish for deployment
```

---

## Support & Resources

- **GitHub Issues**: <https://github.com/YOUR_USERNAME/MIMM-2.0/issues>
- **.NET Documentation**: <https://learn.microsoft.com/dotnet/>
- **Entity Framework Core**: <https://learn.microsoft.com/ef/core/>
- **Blazor**: <https://learn.microsoft.com/aspnet/core/blazor/>

---

**Document Version**: 1.0  
**Last Updated**: 24. ledna 2026  
**Author**: MIMM Team
