# MIMM 2.0 🎵

**Music In My Mind** – Personal journal application for tracking music that plays in your head (mental music) and how it affects your mood and physical sensations.

[![Build](https://github.com/cybersmurf/MIMM-2.0/actions/workflows/build.yml/badge.svg)](https://github.com/cybersmurf/MIMM-2.0/actions)
[![CI](https://github.com/cybersmurf/MIMM-2.0/actions/workflows/ci.yml/badge.svg)](https://github.com/cybersmurf/MIMM-2.0/actions/workflows/ci.yml)
[![Coverage](https://codecov.io/gh/cybersmurf/MIMM-2.0/branch/main/graph/badge.svg)](https://codecov.io/gh/cybersmurf/MIMM-2.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/)

**Project Status:** 60% Complete – Foundation & Infrastructure Ready | 40% Remaining – Features & Testing

---

## 🎯 What is MIMM?

MIMM stands for **"Music In My Mind"** – It's the music that plays internally (that you hum, sing, or imagine in your head) without external playback. The MIMM app lets you:

- 📝 **Track mental music** – Record songs that play in your mind
- 😊 **Log emotions** – How does that mental music affect your mood?
- 📊 **Discover patterns** – Which songs (in your head) influence your emotions the most?
- 🧠 **Understand yourself** – See correlations between imagined music and feelings

**Key differentiator:** Unlike music streaming apps, MIMM focuses on the **emotional and psychological impact** of music you imagine, without needing external playback. First-mover advantage in mental music + mood tracking space.

---

## 📊 Project Status

### ✅ What's Done

- **Backend Infrastructure** (90% complete)
  - ASP.NET Core 9 REST API with Controllers
  - Entity Framework Core with PostgreSQL database
  - JWT authentication (login, refresh tokens, password reset)
  - Custom exception handling middleware
  - SignalR setup for real-time features
  - Serilog logging configured
  - Docker containerization ready
  
- **Frontend Foundation** (30% complete)
  - Blazor WebAssembly project scaffold
  - MudBlazor UI components integrated
  - Refit HTTP client for API communication
  - Basic project structure & routing

- **Data Layer** (100% complete)
  - PostgreSQL database configured
  - Entity Framework Core migrations ready
  - User, JournalEntry, LastFmToken entities defined
  - Seed data scripts prepared

- **Testing Framework** (10% complete)
  - xUnit test project scaffold
  - 17 tests passing (Weather API demo)
  - Test utilities and mocking setup ready

- **Documentation** (100% complete)
  - Comprehensive analysis documents created
  - Architecture & technical deep dive
  - Feature roadmap & status matrix
  - Management & stakeholder reports
  - Setup guides & deployment checklists

### 🚧 What's In Progress

- **Authentication & Login** (Started, not tested end-to-end)
  - JWT token generation framework ready
  - Refresh token logic scaffolded
  - Password hashing (BCrypt) configured

- **Core Features** (Planned, not started)
  - Entry creation form (need to implement UI)
  - Mood selector (Russell's 2D Circumplex UI)
  - Music search API integration
  - Entry CRUD operations (backend controllers exist, not tested)

### ❌ What's NOT Started Yet

- **User Features**
  - User registration & validation
  - User profile management
  - Email verification
  - Password reset flow
  - User dashboard

- **Journal Features**
  - Entry list & filtering
  - Entry editing & deletion
  - Note attachments
  - Entry search

- **Analytics**
  - Mood trend analysis
  - Music impact statistics
  - Charts & visualizations
  - Export data (CSV, PDF, JSON)

- **Integrations**
  - Last.fm OAuth integration
  - Last.fm scrobbling
  - Music search (iTunes, Deezer, MusicBrainz, Discogs)
  - Spotify integration

- **Advanced Features**
  - Real-time notifications
  - Collaborative features
  - Mobile PWA
  - Dark mode
  - Multi-language support

- **Testing & Quality**
  - Unit tests (10% coverage)
  - Integration tests (not started)
  - E2E tests (not started)
  - Security audit
  - Performance testing

---

## 📈 Development Roadmap (Next 8 Weeks)

### Week 1-2: MVP Foundation
- [ ] Complete user registration & login (E2E test)
- [ ] Basic entry creation form
- [ ] Mood selector UI
- [ ] Database schema validation

### Week 3-4: Core Features
- [ ] Entry list & display
- [ ] Edit/delete entries
- [ ] Music search integration
- [ ] 50+ test coverage

### Week 5-6: Analytics & Polish
- [ ] Basic mood analytics
- [ ] Chart visualizations
- [ ] UI/UX refinement
- [ ] Performance optimization

### Week 7-8: Integration & Deployment
- [ ] Last.fm integration
- [ ] Production deployment setup
- [ ] Security hardening
- [ ] Documentation completion

---

## 🏗️ Architecture

### Stack

- **Backend**: ASP.NET Core 9 (REST API + SignalR)
- **Frontend**: Blazor WebAssembly (C# SPA)
- **Database**: PostgreSQL 16 + Entity Framework Core 9
- **Caching**: Redis (optional)
- **Authentication**: JWT + Refresh Tokens
- **Password Security**: BCrypt (workFactor: 12)
- **Testing**: xUnit + FluentAssertions + Moq
- **CI/CD**: GitHub Actions
- **Deployment**: Docker + Nginx

### Project Structure

```text
MIMM-2.0/
├── src/
│   ├── MIMM.Backend/              # ASP.NET Core 9 API (3620 lines)
│   │   ├── Controllers/           # REST endpoints (scaffolded)
│   │   ├── Services/              # Business logic (incomplete)
│   │   ├── Data/                  # EF Core DbContext + migrations
│   │   ├── Hubs/                  # SignalR real-time hubs
│   │   ├── Middleware/            # Exception handling, logging
│   │   └── Program.cs             # DI configuration, middleware pipeline
│   ├── MIMM.Frontend/             # Blazor WASM (850 lines, 30% done)
│   │   ├── Pages/                 # Razor pages (scaffold only)
│   │   ├── Components/            # Reusable MudBlazor components
│   │   ├── Services/              # Refit HTTP clients
│   │   └── Program.cs             # Client DI & authentication
│   └── MIMM.Shared/               # Shared DTOs & entities (complete)
│       ├── Dtos/
│       └── Entities/
├── tests/
│   ├── Application.Tests/         # Weather API demo (17 tests passing)
│   ├── MIMM.Tests.Unit/           # Backend unit tests (scaffold)
│   └── MIMM.Tests.Integration/    # Integration tests (scaffold)
├── docs/
│   ├── DEVELOPER_GUIDE.md         # Development workflow
│   ├── USER_GUIDE.md              # End-user documentation
│   └── PROMPTS_CATALOG.md         # Copilot prompts
├── .github/
│   ├── workflows/                 # CI/CD pipelines
│   ├── prompts/                   # Reusable Copilot prompts
│   └── copilot-instructions.md   # Code generation rules
├── docker-compose.yml             # PostgreSQL + Redis stack
├── Dockerfile                      # Production image
└── MIMM.sln
```

---

## 🚀 Quick Start

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)
- PostgreSQL 16 (or Docker)

### 1. Clone & Setup

```bash
git clone https://github.com/cybersmurf/MIMM-2.0.git
cd MIMM-2.0
dotnet restore
```

### 2. Start Database

```bash
docker-compose up -d postgres redis
```

### 3. Configure Environment

```bash
# Copy example
cp src/MIMM.Backend/appsettings.Development.json.example \
   src/MIMM.Backend/appsettings.Development.json

# Edit with your settings
dotnet user-secrets set "Jwt:Key" "your-256-bit-key-here"
```

### 4. Run Migrations

```bash
cd src/MIMM.Backend
dotnet ef database update
```

### 5. Start Backend

```bash
dotnet run
# Backend: https://localhost:7001
# Swagger: https://localhost:7001/swagger
```

### 6. Start Frontend

```bash
cd src/MIMM.Frontend
dotnet run
# Frontend: https://localhost:5001
```

### 7. Open Browser

Navigate to <https://localhost:5001> and test the application.

---

## 📚 Documentation

### 🔥 Analysis & Planning (NEW)

- **[00_DOCUMENTATION_INDEX.md](00_DOCUMENTATION_INDEX.md)** – Master index for all analysis
- **[EXECUTIVE_SUMMARY_MANAGEMENT_REPORT.md](EXECUTIVE_SUMMARY_MANAGEMENT_REPORT.md)** – For business decision-makers (30 min read)
- **[MANAGEMENT_QUICK_START_CZ.md](MANAGEMENT_QUICK_START_CZ.md)** – Non-technical overview (Czech)
- **[QUICK_REFERENCE_CARD.md](QUICK_REFERENCE_CARD.md)** – One-page cheat sheet
- **[TECHNICAL_ANALYSIS_DEEP_DIVE.md](TECHNICAL_ANALYSIS_DEEP_DIVE.md)** – For technical leads (90 min read)
- **[FEATURE_STATUS_AND_ROADMAP.md](FEATURE_STATUS_AND_ROADMAP.md)** – Feature matrix & timelines
- **[ANALYSIS_SUMMARY.md](ANALYSIS_SUMMARY.md)** – Financial projections & risk analysis

### 🎯 Development

- **[AGENTS.md](AGENTS.md)** – Instructions for AI agents (Copilot, Claude, Gemini)
- **[TODAY_ACTION_PLAN.md](TODAY_ACTION_PLAN.md)** – Current sprint priorities
- **[SETUP_GUIDE.md](SETUP_GUIDE.md)** – Complete installation guide
- **[DEVELOPER_GUIDE.md](docs/DEVELOPER_GUIDE.md)** – Development workflow & standards

### 📋 Prompts & Instructions

- **[.github/prompts/](./github/prompts/)** – Reusable Copilot prompts
- **[.github/copilot-instructions.md](.github/copilot-instructions.md)** – Code generation rules
- **[docs/PROMPTS_CATALOG.md](docs/PROMPTS_CATALOG.md)** – Prompt reference table

### 🚀 Deployment

- **[DEPLOYMENT_CHECKLIST.md](DEPLOYMENT_CHECKLIST.md)** – Full deployment guide
- **[DEPLOYMENT_PLAN.md](DEPLOYMENT_PLAN.md)** – Strategy & timeline
- **[MIGRATION_GUIDE.md](MIGRATION_GUIDE.md)** – Migrate from MIMM 1.0

---

## 🧪 Testing

### Run Tests

```bash
# All tests
dotnet test MIMM.sln

# Specific project
dotnet test tests/Application.Tests/

# With coverage
dotnet test MIMM.sln --collect:"XPlat Code Coverage"
```

### Run CI Locally

```bash
# Build (Release)
dotnet build MIMM.sln --configuration Release

# Test (Release)
dotnet test MIMM.sln --configuration Release --no-build -v minimal
```

---

## 🔐 Environment Variables

| Variable | Description | Required |
|----------|-------------|----------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL | ✅ Yes |
| `Jwt__Key` | JWT signing key (256-bit) | ✅ Yes |
| `Jwt__Issuer` | JWT issuer | ✅ Yes |
| `Jwt__Audience` | JWT audience | ✅ Yes |
| `LastFm__ApiKey` | Last.fm API key | ⚠️ Optional |
| `LastFm__SharedSecret` | Last.fm secret | ⚠️ Optional |

See `appsettings.json` for the complete list.

---

## 🛠️ Useful Commands

```bash
# Build
dotnet build MIMM.sln

# Restore packages
dotnet restore

# Run backend
dotnet run -p src/MIMM.Backend

# Create migration
dotnet ef migrations add MigrationName -p src/MIMM.Backend

# Update database
dotnet ef database update -p src/MIMM.Backend

# Format code
dotnet format

# Clean build
dotnet clean && dotnet build
```

---

## 🌍 Deployment

### Local Docker

```bash
docker-compose up -d
# Backend: http://localhost:80
```

### Azure App Service

See [DEPLOYMENT_CHECKLIST.md](DEPLOYMENT_CHECKLIST.md) for detailed steps.

---

## 🤝 Contributing

Read [CONTRIBUTING.md](CONTRIBUTING.md) before submitting PRs.

### Code Standards

- C# 13 (modern syntax: collection expressions, primary constructors)
- `#nullable enable` in all files
- Conventional commits: `feat:`, `fix:`, `docs:`, `test:`, `refactor:`
- 80%+ test coverage target

---

## 📊 Metrics

| Metric | Value |
|--------|-------|
| **Total Lines (Backend)** | 3,620 |
| **Total Lines (Frontend)** | 850 |
| **Source Files** | 43 |
| **Test Files** | 3 |
| **Test Coverage** | ~10% |
| **Project Completion** | 60% |
| **Est. Remaining Hours** | 170-440 (Senior-Junior) |

---

## 📜 License

MIT License – see [LICENSE](LICENSE).

---

## 📞 Support

- **Issues:** <https://github.com/cybersmurf/MIMM-2.0/issues>
- **Discussions:** <https://github.com/cybersmurf/MIMM-2.0/discussions>

---

**Built with ❤️ using .NET 9 & Blazor**

---

Version: 2.0.0
Last Updated: 24. ledna 2026
