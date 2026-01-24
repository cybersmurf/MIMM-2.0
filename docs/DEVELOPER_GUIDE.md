# Developer Guide

Tento průvodce shrnuje postupy pro vývojáře MIMM 2.0: lokální běh, testy, migrace, CI a standardy.

## Požadavky
- .NET SDK 9.0.x (ověř: `dotnet --version`)
- Docker Desktop (pro PostgreSQL + Redis)
- Git

## Lokální běh

### Databáze + Redis
```bash
docker compose up -d postgres redis
```

### Backend API
```bash
cd src/MIMM.Backend
# Použij výchozí appsettings.Development.json (gitignored)
dotnet run
# Backend běží na: https://localhost:7001
```

### Frontend (Blazor WASM)
```bash
cd src/MIMM.Frontend
dotnet run
# Frontend: https://localhost:5001
```

## EF Core migrace
```bash
# Vytvoření migrace
./scripts/ef-add-migration.sh AddFeatureX

# Aplikace migrací
cd src/MIMM.Backend
dotnet ef database update
```

## Testování
```bash
# Všechny testy (Release, minimal log)
dotnet restore MIMM.sln
dotnet build MIMM.sln --configuration Release --no-restore
dotnet test MIMM.sln --configuration Release --no-build -v minimal
```

- Integrační testy používají InMemory EF + test auth scheme (header `X-Test-User-Id`).
- E2E scénáře: registrace, login, `/api/auth/me`, Entries CRUD.

## Coverage
```bash
# CI spouští XPlat Code Coverage a ukládá artefakty
# Lokální HTML report
dotnet tool install --global dotnet-reportgenerator-globaltool
reportgenerator \
  -reports:**/TestResults/**/coverage.cobertura.xml \
  -targetdir:coverage \
  -reporttypes:Html
open coverage/index.html
```

### Codecov
- Public repo: stačí připojit repo v Codecov; badge se aktivuje po prvním uploadu.
- Private repo: přidej GitHub Secret `CODECOV_TOKEN` (viz README sekce Codecov setup).

## CI (GitHub Actions)
- Workflows: `.github/workflows/ci.yml` (OS matrix + cache + coverage), `.github/workflows/build.yml` (build + test + publish).
- Lokálně parita:
```bash
dotnet restore MIMM.sln
dotnet build MIMM.sln --configuration Release --no-restore
dotnet test MIMM.sln --configuration Release --no-build -v minimal
```

## Standardy kódu
- C# 13, `net9.0`, nullable enabled.
- DTOs jako `record`, `required` properties.
- Async metody s `Async` suffix, propagovat `CancellationToken`.
- Dodržet strukturu projektů (Backend/Frontend/Shared/Tests) a DI zvyklosti.

## Release a verzování
- Conventional commits: `feat|fix|docs|test|refactor|perf|chore(scope): message`.
- Při bumpu verze aktualizovat README, CHANGELOG, tag (např. `v1.0.0`).

## Tipy
- Logging (Serilog) v produkci.
- JWT klíče a secrets přes user-secrets/env vars.
- CORS povolené originy pro frontend.
