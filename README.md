# MIMM 2.0 🎵

**Music & Mood Journal** - Enterprise-ready web application for tracking how music affects your emotions and physical sensations.

[![Build](https://github.com/YOUR_USERNAME/MIMM-2.0/workflows/Build%20and%20Test/badge.svg)](https://github.com/YOUR_USERNAME/MIMM-2.0/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/)

---

## 🌟 Features

- ✅ **User Authentication** - Secure JWT-based login with email verification
- ✅ **Multi-tenant Architecture** - Each user has isolated journal data
- ✅ **Russell's Circumplex Model** - 2D mood tracking (Valence × Arousal)
- ✅ **Multi-source Music Search** - iTunes, Deezer, MusicBrainz, Discogs
- ✅ **Last.fm Integration** - OAuth login + automatic scrobbling
- ✅ **Real-time Analytics** - SignalR-powered live updates
- ✅ **PWA Support** - Installable on mobile devices
- ✅ **Bilingual** - Czech 🇨🇿 + English 🇬🇧
- ✅ **Type-safe** - Full C# stack (backend + Blazor frontend)

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
│   ├── MIMM.Backend/          # ASP.NET Core API
│   │   ├── Controllers/       # REST endpoints
│   │   ├── Services/          # Business logic
│   │   ├── Data/              # EF Core DbContext + migrations
│   │   ├── Hubs/              # SignalR real-time hubs
│   │   └── Middleware/        # Custom middleware
│   ├── MIMM.Frontend/         # Blazor WASM
│   │   ├── Pages/             # Razor pages
│   │   ├── Components/        # Reusable components
│   │   └── Services/          # API clients
│   └── MIMM.Shared/           # Shared DTOs & models
├── tests/
│   ├── MIMM.Tests.Unit/       # Unit tests
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
git clone https://github.com/YOUR_USERNAME/MIMM-2.0.git
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

Navigate to **https://localhost:5001** and register your first account!

---

## 📚 Documentation

- **[Setup Guide](SETUP_GUIDE.md)** - Complete installation & configuration
- **[Migration Guide](MIGRATION_GUIDE.md)** - Migrate data from MIMM 1.0
- **[Architecture Spec](MIMM_2.0_SPECIFICATION_DOTNET.md)** - Technical deep dive
- **[API Documentation](https://localhost:7001/swagger)** - Interactive API docs

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
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection | ✅ Yes | - |
| `Jwt__Key` | JWT signing key (256-bit) | ✅ Yes | - |
| `Jwt__Issuer` | JWT issuer | ✅ Yes | `https://localhost:7001` |
| `Jwt__Audience` | JWT audience | ✅ Yes | `mimm-frontend` |
| `LastFm__ApiKey` | Last.fm API key | ⚠️ Optional | - |
| `LastFm__SharedSecret` | Last.fm shared secret | ⚠️ Optional | - |
| `Discogs__Token` | Discogs API token | ⚠️ Optional | - |
| `SendGrid__ApiKey` | SendGrid email API key | ⚠️ Optional | - |
| `ConnectionStrings__Redis` | Redis connection | ⚠️ Optional | - |

See [.env.example](.env.example) for complete list.

---

## 🛠️ Development Tools

### Recommended IDEs

- **Visual Studio 2025** (Windows/Mac) - Best experience
- **VS Code** + C# Dev Kit extension
- **Rider** (JetBrains)

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

See [DEPLOYMENT.md](DEPLOYMENT.md) for detailed instructions.

---

## 🤝 Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) first.

### Development Workflow

1. Fork the repository
2. Create feature branch: `git checkout -b feature/amazing-feature`
3. Commit changes: `git commit -m 'Add amazing feature'`
4. Push to branch: `git push origin feature/amazing-feature`
5. Open Pull Request

---

## 📜 License

This project is licensed under the MIT License - see [LICENSE](LICENSE) file.

---

## 🙏 Acknowledgments

- **MIMM 1.0** - Original localStorage-based MVP
- **Last.fm API** - Music metadata & scrobbling
- **Russell's Circumplex Model** - Mood coordinate system
- **MudBlazor** - Blazor UI components (optional)

---

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/YOUR_USERNAME/MIMM-2.0/issues)
- **Discussions**: [GitHub Discussions](https://github.com/YOUR_USERNAME/MIMM-2.0/discussions)
- **Email**: support@mimm.example.com

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

**Built with ❤️ using C# and .NET**

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

**Version**: 1.0.0  
**Last Updated**: 24. ledna 2026
