# MIMM 2.0 🎵

**Music & Mood Journal** – Enterprise-ready web application for tracking how music affects emotions and physical sensations.

[![Build](https://github.com/cybersmurf/MIMM-2.0/actions/workflows/build.yml/badge.svg)](https://github.com/cybersmurf/MIMM-2.0/actions)
[![CI](https://github.com/cybersmurf/MIMM-2.0/actions/workflows/ci.yml/badge.svg)](https://github.com/cybersmurf/MIMM-2.0/actions/workflows/ci.yml)
[![Coverage](https://codecov.io/gh/cybersmurf/MIMM-2.0/branch/main/graph/badge.svg)](https://codecov.io/gh/cybersmurf/MIMM-2.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/)

---

## 🌟 Features

- ✅ **User Authentication** – Secure JWT-based login with email verification
- ✅ **Multi-tenant Architecture** – Each user has isolated journal data
- ✅ **Russell's Circumplex Model** – 2D mood tracking (Valence × Arousal)
- ✅ **Multi-source Music Search** – iTunes, Deezer, MusicBrainz, Discogs
- ✅ **Last.fm Integration** – OAuth login + automatic scrobbling
- ✅ **Real-time Analytics** – SignalR-powered live updates
- ✅ **PWA Support** – Installable on mobile devices
- ✅ **Bilingual** – Czech 🇨🇿 + English 🇬🇧
- ✅ **Type-safe** – Full C# stack (backend + Blazor frontend)

---

## 🏗️ Architecture

### Stack

- **Backend**: ASP.NET Core 9 (REST API + SignalR)
- **Frontend**: Blazor WebAssembly (C# SPA)
- **Database**: PostgreSQL 16 + Entity Framework Core
- **Caching**: Redis (optional)
- **Authentication**: JWT + Refresh Tokens
- **Testing**: xUnit + FluentAssertions
- **CI/CD**: GitHub Actions

### Project Structure

```
MIMM-2.0/
├── src/
│   ├── MIMM.Backend/           # ASP.NET Core API
│   │   ├── Controllers/        # REST endpoints
│   │   ├── Services/           # Business logic
│   │   ├── Data/               # EF Core DbContext + migrations
│   │   ├── Hubs/               # SignalR real-time hubs
│   │   └── Middleware/         # Custom middleware
│   ├── MIMM.Frontend/          # Blazor WASM
│   │   ├── Pages/              # Razor pages
│   │   ├── Components/         # Reusable components
│   │   └── Services/           # API clients
│   └── MIMM.Shared/            # Shared DTOs & models
├── tests/
│   ├── MIMM.Tests.Unit/        # Unit tests
│   └── MIMM.Tests.Integration/ # Integration tests
├── docker-compose.yml
├── Dockerfile
└── MIMM.sln
```

---

## 🚀 Quick Start

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)

### 1. Clone Repository

```bash
git clone https://github.com/cybersmurf/MIMM-2.0.git
cd MIMM-2.0
```

### 2. Start Database

```bash
docker-compose up -d postgres redis
```

### 3. Configure Environment

```bash
cp .env.example .env
# Edit .env with your settings (JWT key, Last.fm credentials, etc.)
```

### 4. Run Migrations

```bash
cd src/MIMM.Backend
dotnet ef database update
```

### 5. Start Backend

```bash
dotnet run
# Backend running at: https://localhost:7001
# Swagger UI: https://localhost:7001/swagger
```

### 6. Start Frontend

```bash
cd src/MIMM.Frontend
dotnet run
# Frontend running at: https://localhost:5001
```

### 7. Open in Browser

Navigate to https://localhost:5001 and register your first account.

---

## 📚 Documentation

### Getting Started & Development
- [📍 **TODAY'S ACTION PLAN**](TODAY_ACTION_PLAN.md) – 🔥 **START HERE** - Database setup + E2E test (2-3h)
- [✅ Action 1 Completion](ACTION_1_COMPLETION.md) – Database setup done ✅ 
- [🧪 Action 2: E2E Test](ACTION_2_E2E_TEST.md) – Test auth flow (30 min)

### Planning & Strategic
- [📊 Strategic Action Plan](STRATEGIC_ACTION_PLAN_2026.md) – Comprehensive 3-week MVP roadmap
- [🗓️ Sprint Timeline](SPRINT_TIMELINE.md) – Visual timeline & progress tracker
- [🔬 Project Analysis](PROJECT_ANALYSIS_2026.md) – Technical deep dive & architecture

### Migration & APIs
- [🔄 Migration Guide](MIGRATION_GUIDE.md) – Migrate data from MIMM 1.0
- [📖 Setup Guide](SETUP_GUIDE.md) – Complete installation & configuration
- [🔗 API Documentation](https://localhost:7001/swagger) – Interactive Swagger (run backend first)

### AI Agents & Instructions
- [AGENTS.md](AGENTS.md) – Klíčové instrukce a příkazy pro AI agenty
- [CLAUDE.md](CLAUDE.md) – Instrukce pro Claude Code
- [GEMINI.md](GEMINI.md) – Instrukce pro Google Gemini
- [.github/copilot-instructions.md](.github/copilot-instructions.md) – Pravidla generování kódu
- `.github/agents/` – definice custom agentů (MCP)

---

## 🧪 Testing

```bash
# Unit tests
dotnet test tests/MIMM.Tests.Unit

# Integration tests
dotnet test tests/MIMM.Tests.Integration

# All tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Run CI locally

Pro lokální ověření toho, co běží v CI, spusť:

```bash
# Restore + Build (Release)
dotnet restore MIMM.sln
dotnet build MIMM.sln --configuration Release --no-restore

# Testy (Release)
dotnet test MIMM.sln --configuration Release --no-build -v minimal
```

Volitelné: spouštění GitHub Actions lokálně pomocí `act` (pokud ho používáš):

```bash
# Nainstaluj act dle dokumentace: https://github.com/nektos/act
# Spusť CI workflow lokálně
act -W .github/workflows/ci.yml -j build-and-test
```

### CI artifacts & coverage
- Výstup coverage je ukládán jako artefakt `coverage-reports` v GitHub Actions runu.
- Obsahuje soubory `coverage.cobertura.xml` a `coverage.json` pod `**/TestResults/**`.
- Pro stažení otevři konkrétní run v Actions → sekce Artifacts → `coverage-reports`.
- Pro lokální prohlížení můžeš použít libovolný Cobertura viewer, nebo VS Code pluginy pro coverage.

### Codecov setup (coverage badge)
- Public repo: běžně nevyžaduje token; stačí připojit repo v Codecov a první CI upload.
- Private repo: vytvoř GitHub Secret `CODECOV_TOKEN` s hodnotou tokenu z Codecov (Repo → Settings → General → Upload Token).
- Přidání Secret: GitHub → Repo → Settings → Secrets and variables → Actions → New repository secret → `CODECOV_TOKEN`.
- Badge v README se aktivuje po prvním úspěšném uploadu a zpracování reportu.

### CI detaily
- OS matrix: CI běží na `ubuntu`, `windows` a `macos` pro širší kompatibilitu.
- NuGet cache: CI ukládá balíčky do `~/.nuget/packages` (Linux/macOS) a `C:\Users\runneradmin\.nuget\packages` (Windows) pro rychlejší běhy.

### Copilot prompty
- Opakovatelné prompty jsou v `.github/prompts/`:
  - **[📋 Prompts Catalog](docs/PROMPTS_CATALOG.md)** – přehledná tabulka promptů
  - release-notes.prompt.md
  - e2e-tests-maintenance.prompt.md
  - ci-fix.prompt.md
  - feature-implementation.prompt.md
  - security-hardening.prompt.md
  - ef-migrations-review.prompt.md
  - api-contract-review.prompt.md
- Pro agenty jsou odkazy také v [AGENTS.md](AGENTS.md).

---

## 🐳 Docker Deployment

```bash
# Build image
docker build -t mimm-backend:latest .

# Run full stack
docker-compose up -d

# View logs
docker-compose logs -f backend
```

---

## 🔐 Environment Variables

| Variable | Description | Required | Default |
|----------|-------------|----------|---------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection | ✅ Yes | – |
| `Jwt__Key` | JWT signing key (256-bit) | ✅ Yes | – |
| `Jwt__Issuer` | JWT issuer | ✅ Yes | `https://localhost:7001` |
| `Jwt__Audience` | JWT audience | ✅ Yes | `mimm-frontend` |
| `LastFm__ApiKey` | Last.fm API key | ⚠️ Optional | – |
| `LastFm__SharedSecret` | Last.fm shared secret | ⚠️ Optional | – |
| `Discogs__Token` | Discogs API token | ⚠️ Optional | – |
| `SendGrid__ApiKey` | SendGrid email API key | ⚠️ Optional | – |
| `ConnectionStrings__Redis` | Redis connection | ⚠️ Optional | – |

See .env.example for the complete list.

---

## 🛠️ Development Tools

### Recommended IDEs

- Visual Studio 2025 (Windows/Mac)
- VS Code + C# Dev Kit
- Rider (JetBrains)

### Useful Commands

```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run with hot reload
dotnet watch run

# Create EF Core migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Format code
dotnet format
```

---

## 🌍 Deployment

### Azure App Service

```bash
# Login to Azure
az login

# Create resource group
az group create --name mimm-rg --location westeurope

# Create App Service
az webapp create --resource-group mimm-rg --plan mimm-plan --name mimm-app --runtime "DOTNETCORE:9.0"

# Deploy
az webapp deployment source config-zip --resource-group mimm-rg --name mimm-app --src publish.zip
```

See DEPLOYMENT.md for detailed instructions.

---

## 🤝 Contributing

Contributions are welcome! Please read CONTRIBUTING.md first.

### Development Workflow

1. Fork the repository
2. Create feature branch: `git checkout -b feature/amazing-feature`
3. Commit changes: `git commit -m "Add amazing feature"`
4. Push to branch: `git push origin feature/amazing-feature`
5. Open Pull Request

---

## 📜 License

This project is licensed under the MIT License – see LICENSE.

---

## 🙏 Acknowledgments

- MIMM 1.0 – Original localStorage-based MVP
- Last.fm API – Music metadata & scrobbling
- Russell's Circumplex Model – Mood coordinate system
- MudBlazor – Blazor UI components (optional)

---

## 📞 Support

- Issues: https://github.com/cybersmurf/MIMM-2.0/issues
- Discussions: https://github.com/cybersmurf/MIMM-2.0/discussions
- Email: support@mimm.example.com

---

## 📈 Roadmap

### Phase 1: MVP (Current)
- [x] User authentication
- [x] Entry CRUD
- [x] Music search (multi-source)
- [x] Basic analytics
- [ ] Last.fm OAuth
- [ ] Last.fm scrobbling

### Phase 2: Social Features
- [ ] Public user profiles
- [ ] Artist leaderboards
- [ ] Share entries
- [ ] Friend system

### Phase 3: Premium
- [ ] Subscription tiers
- [ ] Export data (PDF, CSV)
- [ ] Advanced analytics
- [ ] Mobile apps (Maui)

---

**Built with love using C# and .NET**

---

## Screenshots

### Login Page
![Login](docs/screenshots/login.png)

### Dashboard
![Dashboard](docs/screenshots/dashboard.png)

### Mood Selector
![Mood Selector](docs/screenshots/mood-selector.png)

### Analytics
![Analytics](docs/screenshots/analytics.png)

---

Version: 1.0.0
Last Updated: 24. ledna 2026
