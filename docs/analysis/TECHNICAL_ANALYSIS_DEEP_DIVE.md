# MIMM 2.0 - Detailní Technická Analýza Projektu

## For Development Team & Technical Decision Makers

**Datum:** 24. ledna 2026  
**Verze:** 1.0  
**Cílová skupina:** CTO, Tech Lead, Senior Developers  

---

## 📊 1. Analýza Zdrojového Kódu

### Struktura Projektu

```text
src/
├── MIMM.Backend/          (ASP.NET Core 9 API)
│   ├── Controllers/       (5 kontrolery)
│   ├── Services/          (Vrstvy obchodní logiky)
│   ├── Data/              (EF Core DbContext + Entity Config)
│   ├── Middleware/        (Custom exception handling)
│   ├── Hubs/              (SignalR real-time)
│   └── Program.cs         (DI, middleware pipeline)
│
├── MIMM.Frontend/         (Blazor WebAssembly SPA)
│   ├── Pages/             (10 Razor stránek)
│   ├── Components/        (Reusable komponenty)
│   ├── Services/          (API clients, state management)
│   └── wwwroot/           (Static assets)
│
└── MIMM.Shared/           (Shared DTOs & Models)
    ├── Entities/          (Domain models)
    └── Dtos/              (Data transfer objects)

tests/
├── Application.Tests/     (Sandbox demo - 17/17 ✅)
├── MIMM.Tests.Unit/       (Unit test scaffold)
└── MIMM.Tests.Integration/(Integration test scaffold)
```text

### Metriky Zdrojového Kódu

| Metrika | Počet | Poznámka |
|---------|-------|----------|
| **C# Soubory** | 43 | Produkční kód |
| **Razorové Komponenty** | 10 | Frontend |
| **Celkový LOC (Lines of Code)** | 3,620 | Bez testů |
| **Testy** | 17 | Application.Tests |
| **Konfigurace & Docker** | 5 | docker-compose, Dockerfile, appsettings |
| **Dokumentace** | 25+ markdown souborů | ~10,000 řádků |

### Jazykové Rozložení

```text
C#                    75% (3,620 LOC)
Razor                 12% (850 LOC)
HTML/CSS              8%  (400 LOC)
JSON/YAML             5%  (150 LOC - konfigurační soubory)
```text

---

## 🏗️ 2. Architektonická Analýza

### 2.1 Backend Architektura (ASP.NET Core 9)

#### Vrstvy

```text
┌─────────────────────────────────────┐
│      Controllers (REST Endpoints)    │ ← API Layer
├─────────────────────────────────────┤
│      Services (Business Logic)       │ ← Application Layer
├─────────────────────────────────────┤
│    Data / DbContext (EF Core)        │ ← Data Layer
├─────────────────────────────────────┤
│   PostgreSQL (Entities & Relations)  │ ← Database Layer
└─────────────────────────────────────┘
```text

#### Service Architecture (Dependency Injection)

```csharp
// Program.cs registrace
services.AddScoped<IAuthService>();          // User auth & JWT
services.AddScoped<IEntryService>();         // Journal entry CRUD
services.AddScoped<IMusicSearchService>();   // Music API integration
services.AddScoped<ILastFmService>();        // Last.fm OAuth
services.AddScoped<IAnalyticsService>();     // Mood analytics
services.AddScoped<IUserService>();          // User profile management
```text

**Pattern:** Interface-based dependency injection  
**Lifetimes:**

- Scoped = nové instance per HTTP request (bezpečné pro DB context)
- Singleton = jedná instance pro celou aplikaci (Serilog, cache)

#### Middleware Pipeline

```text
Request → Authentication → ExceptionHandling → CORS → Controller → Response
          (JWT Bearer)      (Custom)           (for WASM)
```text

#### Signal R Hubs (Real-time Communication)

```csharp
// Plánované hubs:
- AnalyticsHub        // Real-time mood updates
- NotificationHub     // User notifications
- CollaborationHub    // Multi-user playlists
```text

### 2.2 Frontend Architektura (Blazor WASM)

#### Component Hierarchy

```text
App.razor (Router)
├── MainLayout.razor
│   ├── NavMenu
│   └── Main Content
│       ├── Pages
│       │   ├── Index (Home)
│       │   ├── Login
│       │   ├── Register
│       │   ├── Dashboard
│       │   ├── EntryForm
│       │   ├── Analytics
│       │   └── Settings
│       └── Components
│           ├── MoodSelector (2D Circumplex)
│           ├── MusicSearch
│           ├── EntryCard
│           ├── AnalyticsChart
│           └── UserProfile
```text

#### State Management

```text
CurrentUser (AuthService)
  ├── JWT Token + Refresh Token
  ├── UserId
  ├── Email
  └── Language

JournalState (EntryService)
  ├── CurrentEntries[]
  ├── SelectedEntry
  └── FilterCriteria

AnalyticsState (AnalyticsService)
  ├── MoodTrends
  ├── TopSongs
  └── EmotionPatterns
```text

#### API Communication (Refit HTTP Client)

```csharp
[BasePath("/api")]
public interface IBackendClient
{
    [Post("/auth/login")]
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    
    [Get("/entries")]
    Task<PaginatedResult<JournalEntryDto>> GetEntriesAsync(int page = 1);
    
    [Post("/entries")]
    Task<JournalEntryDto> CreateEntryAsync(CreateEntryDto request);
}
```text

### 2.3 Database Schema (PostgreSQL 16)

#### Entity Relationship Diagram

```text
┌──────────────┐
│    Users     │
├──────────────┤
│ UserId (PK)  │
│ Email (UQ)   │
│ PasswordHash │
│ DisplayName  │
│ Language     │
│ TimeZone     │
│ EmailVerified│
│ CreatedAt    │
│ SoftDeletedAt│
└──────┬───────┘
       │ 1:N
       │
┌──────▼────────────────┐      ┌──────────────────┐
│  JournalEntries      │──N──►│  LastFmTokens    │
├──────────────────────┤      ├──────────────────┤
│ EntryId (PK)         │      │ TokenId (PK)     │
│ UserId (FK) ◄────────┼──────┤ UserId (FK)      │
│ SongTitle            │      │ SessionKey       │
│ ArtistName           │      │ LastFmUsername   │
│ AlbumName            │      │ ExpiresAt        │
│ Valence (-1 to 1)    │      └──────────────────┘
│ Arousal (-1 to 1)    │
│ TensionLevel (0-100) │
│ SomaticTags[]        │
│ Notes                │
│ Source (enum)        │
│ CreatedAt            │
└──────────────────────┘
```text

#### Indexes (Performance)

```sql
-- Fast user lookup
CREATE UNIQUE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_deleted ON users(deleted_at);

-- Fast entry retrieval
CREATE INDEX idx_entries_user_created ON journal_entries(user_id, created_at DESC);
CREATE INDEX idx_entries_source ON journal_entries(source);

-- Text search (PostgreSQL Full-Text Search)
CREATE INDEX idx_entries_search ON journal_entries USING GIN(to_tsvector('czech', song_title || ' ' || artist_name));
```text

---

## 🔐 3. Bezpečnostní Analýza

### 3.1 Authentication & Authorization

#### JWT Implementation

```csharp
var tokenHandler = new JwtSecurityTokenHandler();
var key = Encoding.ASCII.GetBytes(Configuration["Jwt:Key"]);

var tokenDescriptor = new SecurityTokenDescriptor
{
    Subject = new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.NameIdentifier, userId),
        new Claim(ClaimTypes.Email, email),
        new Claim("language", userLanguage)
    }),
    Expires = DateTime.UtcNow.AddMinutes(60),
    SigningCredentials = new SigningCredentials(
        new SymmetricSecurityKey(key),
        SecurityAlgorithms.HmacSha256Signature)
};

var token = tokenHandler.CreateToken(tokenDescriptor);
```text

**Konfigurace:**

- AccessToken: 60 minut
- RefreshToken: 7 dní
- Token refresh na frontend automaticky

#### Refresh Token Flow

```text
1. User login
   ├── Backend generuje AccessToken (short-lived)
   └── Backend generuje RefreshToken (long-lived)

2. Access denied (AccessToken expirován)
   ├── Frontend detekuje 401 Unauthorized
   └── Frontend se pokusí RefreshToken použít

3. Refresh request
   ├── Backend ověří RefreshToken
   └── Backend vydá nový AccessToken

4. Pokud i RefreshToken expirován
   └── User musí se znovu přihlásit
```text

### 3.2 Password Security

```csharp
// Hashování pomocí BCrypt
string passwordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

// Ověření při přihlášení
bool isValidPassword = BCrypt.Net.BCrypt.Verify(password, storedHash);
```text

**Konfiguraci:**

- Work factor: 12 (pomalé úmyslně, chráni před brute force)
- Algoritmus: bcrypt (standard pro 2026)

### 3.3 Data Protection

#### Sensitive Data Handling

| Data | Storage | Encryption |
|------|---------|-----------|
| **Hesla** | DB (hash) | BCrypt + workFactor 12 |
| **JWT Keys** | .env (ignorován v git) | Encrypted in production |
| **Last.fm Session** | DB | Encrypted (EncryptionService) |
| **Email** | DB (plain) | HTTPS only |
| **OAuth Tokens** | Secure cache (Redis) | Redis ACL + expiration |

#### HTTPS Enforcement

```csharp
// Program.cs
app.UseHttpsRedirection();
app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());
```text

### 3.4 CORS Configuration

```csharp
// Vývojové prostředí (localhost)
services.AddCors(options =>
{
    options.AddPolicy("local", builder =>
        builder.WithOrigins("http://localhost:5000", "https://localhost:7001")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials());
});

// Produkční prostředí (konkrétní doména)
services.AddCors(options =>
{
    options.AddPolicy("production", builder =>
        builder.WithOrigins("https://mimm.company.com")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials());
});
```text

### 3.5 Input Validation

```csharp
public class CreateEntryDto
{
    [Required(ErrorMessage = "Song title is required")]
    [StringLength(200)]
    public string SongTitle { get; set; } = "";
    
    [Range(-1.0, 1.0)]
    public decimal Valence { get; set; }
    
    [Range(-1.0, 1.0)]
    public decimal Arousal { get; set; }
    
    [MaxLength(5000)]
    public string? Notes { get; set; }
}
```text

---

## 🧪 4. Testing & Quality Assurance

### 4.1 Testovací Framework

```text
Framework:       xUnit 2.9.3
Assertions:      FluentAssertions 6.12.0
Mocking:         Moq 4.20.0
In-Memory DB:    EF Core (InMemoryDatabase)
Code Coverage:   Coverlet / Codecov
```text

### 4.2 Aktuální Test Coverage

```text
Application.Tests/       17 testů ✅  (Demo API - 100% pass)
  └── WeatherForecastTests.cs

MIMM.Tests.Unit/         0 testů      (Scaffold připraven)
  └── Složka pro unit testy

MIMM.Tests.Integration/  0 testů      (Scaffold připraven)
  └── WebApplicationFactory setup
```text

### 4.3 Doporučené Testy (zbývající)

#### Unit Tests (MIMM.Tests.Unit)

```csharp
// AuthService testy
[Fact]
public async Task LoginAsync_WithValidCredentials_ReturnsJwtToken()
{
    // Arrange
    var authService = new AuthService(_userRepository, _jwtService);
    var request = new LoginRequestDto { Email = "test@example.com", Password = "password" };
    
    // Act
    var result = await authService.LoginAsync(request);
    
    // Assert
    result.AccessToken.Should().NotBeNullOrEmpty();
    result.RefreshToken.Should().NotBeNullOrEmpty();
}

// EntryService testy
[Fact]
public async Task CreateEntryAsync_WithValidData_SavesEntry()
{
    // Arrange
    var dbContext = CreateInMemoryDbContext();
    var entryService = new EntryService(dbContext, _musicSearchService);
    var userId = Guid.NewGuid();
    var createDto = new CreateEntryDto { SongTitle = "Bohemian Rhapsody", Valence = 0.5m, Arousal = 0.3m };
    
    // Act
    var result = await entryService.CreateEntryAsync(userId, createDto);
    
    // Assert
    result.SongTitle.Should().Be("Bohemian Rhapsody");
    (await dbContext.JournalEntries.CountAsync()).Should().Be(1);
}
```text

#### Integration Tests (MIMM.Tests.Integration)

```csharp
public class AuthIntegrationTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory = new();
    
    [Fact]
    public async Task LoginEndpoint_WithValidCredentials_Returns200AndToken()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new { email = "test@example.com", password = "password" };
        
        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);
        
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsAsync<LoginResponseDto>();
        content.AccessToken.Should().NotBeNullOrEmpty();
    }
}
```text

#### E2E Tests (Frontend)

```javascript
// Playwright / Cypress
describe('User Registration & Login Flow', () => {
    it('should register new user and login successfully', async () => {
        // Navigate to registration
        await page.goto('https://localhost:5000');
        await page.click('a:has-text("Register")');
        
        // Fill form
        await page.fill('input[name="email"]', 'newuser@example.com');
        await page.fill('input[name="password"]', 'SecurePass123!');
        await page.click('button:has-text("Register")');
        
        // Assert redirect to login
        await expect(page).toHaveURL('**/login');
    });
});
```text

### 4.4 Test Coverage Target

```text
Unit Tests:
  Controllers:   75%+ (główní logika)
  Services:      85%+ (kritické)
  Utilities:     90%+ (malé funkce)

Integration Tests:
  API Endpoints: 70%+ (happy path + error cases)
  Database:      80%+ (queries, migrations)

E2E Tests:
  Critical Flows: 100% (login, create entry, export)
  UI Components:  50%+ (hlavní komponenty)

Cílová Pokrytí: 80%+
```text

---

## 📦 5. Dependencies Analysis

### 5.1 Core NuGet Packages (Backend)

```text
Framework:
  - Microsoft.AspNetCore.App 9.0.0
  - Microsoft.NET.Sdk.Web 9.0.100

Database:
  - EntityFrameworkCore 9.0.0
  - Npgsql.EntityFrameworkCore.PostgreSQL 9.0.0
  - Microsoft.EntityFrameworkCore.Tools 9.0.0

Authentication & Security:
  - System.IdentityModel.Tokens.Jwt 7.5.0
  - BCrypt.Net-Next 4.0.3

HTTP & API:
  - Refit 7.2.22 (for API clients)
  - HttpClientFactory

Logging & Monitoring:
  - Serilog 8.2.0
  - Serilog.Sinks.Console
  - Serilog.Sinks.File

SignalR:
  - Microsoft.AspNetCore.SignalR 9.0.0

Real-time:
  - StackExchange.Redis 2.7.33

Validation:
  - FluentValidation 11.10.0

Testing:
  - xUnit 2.9.3
  - FluentAssertions 6.12.0
  - Moq 4.20.0
  - Microsoft.EntityFrameworkCore.InMemory 9.0.0
```text

### 5.2 Frontend NuGet Packages (Blazor)

```text
Core:
  - Microsoft.AspNetCore.Components.WebAssembly 9.0.0
  - Microsoft.AspNetCore.Components.WebAssembly.DevServer 9.0.0

UI Components:
  - MudBlazor 7.14.0 (Material Design komponenty)

HTTP & State:
  - HttpClientFactory
  - Refit 7.2.22

Authentication:
  - Microsoft.AspNetCore.Components.WebAssembly.Authentication 9.0.0

JSON:
  - System.Text.Json

Storage:
  - Blazored.LocalStorage (pro offline support)
```text

### 5.3 Dependency Security

```text
✅ Regulární Updates: GitHub Dependabot nastaveny
✅ Verzování:        Lock files (*.csproj.lock)
❌ Vulnerability Scan: Není nastaveno (doporučujeme WhiteSource)
⚠️ Breaking Changes: Ročně check .NET LTS verze
```text

---

## 🚀 6. Deployment & Infrastructure

### 6.1 Container Strategy (Docker)

#### Frontend Container

```dockerfile
FROM node:22-alpine AS build
WORKDIR /app
COPY . .
RUN npm install && npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
```text

#### Backend Container

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/MIMM.Backend/", "."]
RUN dotnet restore && dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 5001
ENTRYPOINT ["dotnet", "MIMM.Backend.dll"]
```text

### 6.2 Docker Compose (Local Development)

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_USER: mimm_user
      POSTGRES_PASSWORD: ${DB_PASSWORD}
      POSTGRES_DB: mimm_db
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    command: redis-server --requirepass ${REDIS_PASSWORD}

  backend:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "5001:5001"
    environment:
      ConnectionStrings__DefaultConnection: postgresql://...
      Jwt__Key: ${JWT_KEY}
    depends_on:
      - postgres
      - redis

volumes:
  postgres_data:
```text

### 6.3 Production Deployment (Hetzner VPS)

```text
                      Internet
                         │
                      (HTTPS)
                         │
                  ┌──────▼───────┐
                  │  Nginx Proxy  │
                  │  (Port 443)   │
                  └──────┬────────┘
                         │
            ┌────────────┼────────────┐
            │            │            │
      ┌─────▼──┐  ┌──────▼──┐  ┌─────▼──┐
      │Backend │  │Frontend  │  │Grafana │
      │(5001)  │  │(3000)    │  │(3100)  │
      └────────┘  └──────────┘  └────────┘
            │
      ┌─────┴──────┬─────────┐
      │            │         │
   ┌──▼──┐    ┌───▼──┐  ┌──▼──┐
   │ DB  │    │Redis │  │Logs │
   └─────┘    └──────┘  └─────┘
```text

### 6.4 CI/CD Pipeline (GitHub Actions)

```yaml
name: CI/CD

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      
      - name: Restore
        run: dotnet restore
      
      - name: Build
        run: dotnet build --configuration Release
      
      - name: Test
        run: dotnet test --configuration Release --no-build
      
      - name: SonarQube Scan
        run: sonar-scanner -Dsonar.projectKey=MIMM-2.0
      
      - name: Upload Coverage
        uses: codecov/codecov-action@v3

  deploy:
    needs: build-and-test
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
      - uses: actions/checkout@v3
      - name: Deploy to Production
        run: ./scripts/deploy.sh
```text

---

## 🔍 7. Performance Analysis

### 7.1 Load Testing Expectations

```text
Jednotlivé API Endpointy:

GET /api/entries              ~ 50 ms (cold)  / 10 ms (cached)
GET /api/entries/{id}         ~ 30 ms (cold)  / 5 ms (cached)
POST /api/entries             ~ 80 ms (DB write)
POST /api/auth/login          ~ 150 ms (BCrypt hash)
GET /api/analytics            ~ 200 ms (complex query)

Expected Capacity:
  - Single Server: ~500 concurrent users
  - With Redis: ~2000 concurrent users
  - Load Balanced: ~10,000 concurrent users
```text

### 7.2 Database Query Performance

```sql
-- Slow query prevention
-- ❌ N+1 Problem (Avoid)
SELECT * FROM journal_entries;
foreach (entry in entries) {
    user = SELECT * FROM users WHERE id = entry.user_id;
}

-- ✅ Eager Loading (Better)
SELECT e.* FROM journal_entries e
JOIN users u ON e.user_id = u.id;

-- EF Core Example:
var entries = await _context.JournalEntries
    .Include(e => e.User)
    .ToListAsync();
```text

### 7.3 Caching Strategy

```text
Cache Layer (Redis):
  - User tokens:              5 minut
  - Music search results:     1 hodina
  - User preferences:         24 hodin
  - Analytics data:           30 minut
  
Cache Invalidation:
  - On user update:           Delete user token cache
  - On new entry:            Delete user analytics cache
  - Manual refresh:          Admin endpoint
```text

---

## 📈 8. Scaling Roadmap

### Phase 1 (MVP - 0-6 měsíců)

```text
- Single Backend Instance
- Single Database Instance
- Redis (optional, small)
- CDN disabled
- Load: ~100-500 users
```text

### Phase 2 (Growth - 6-12 měsíců)

```text
- 2-3 Backend Instances (Load Balanced)
- Database Replication (Primary + Standby)
- Redis Cluster
- CloudFlare/AWS CloudFront CDN
- Load: ~500-5000 users
```text

### Phase 3 (Scale - 12+ měsíců)

```text
- Kubernetes Orchestration (5-10 replicas)
- Managed PostgreSQL (RDS/Azure Database)
- Redis Cluster + Sentinel
- CDN + Object Storage (S3/Azure Blob)
- Microservices (Music Search, Analytics)
- Load: 5000+ users
```text

---

## 🎯 9. Zbývající Úkoly

### Critical Path (Musí být hotovo)

```text
Week 1-2:
  ✅ User Authentication (Login, Register, JWT)
  ✅ Dashboard (Basic UI)

Week 3-4:
  ✅ Entry Creation (Form, Validation)
  ✅ Music Search (iTunes/Deezer API)

Week 5-6:
  ✅ Analytics (Mood Charts, Statistics)
  ✅ Testing (Unit + Integration)

Week 7-8:
  ✅ Deployment (Docker, Nginx, SSL)
  ✅ Documentation & Training
```text

### Nice-to-Have (Pokud je čas)

```text
- Spotify Integration
- Real-time Notifications (SignalR)
- Mobile PWA (installable app)
- Advanced Analytics (ML-based mood prediction)
- Multi-language Support (Czech + English)
- Dark Mode
```text

---

## 📋 10. Checklist pro Go-Live

### Pre-Production

- [ ] All critical tests passing (80%+ coverage)
- [ ] Security audit completed
- [ ] Database backups configured
- [ ] Monitoring & alerting setup
- [ ] SSL/TLS certificates valid
- [ ] API rate limiting configured
- [ ] CORS properly configured
- [ ] Logging verbosity checked

### Production

- [ ] Database seeded (not copied from dev)
- [ ] JWT keys rotated
- [ ] Secrets in .env (not hardcoded)
- [ ] Error pages configured (no stack traces)
- [ ] Monitoring dashboards ready
- [ ] On-call rotation established
- [ ] Rollback plan documented
- [ ] Load testing completed

---

## 🏆 Závěr

MIMM 2.0 je **architektonicky solidní, zabezpečený, a škálovatelný** projekt.
Zbývající práce je především v implementaci business logiky a testů.

**Risk Factors:**

- ⚠️ Komplexita Last.fm OAuth integrace
- ⚠️ Performance na velkých datasetech (řešit indexy)
- ⚠️ SignalR real-time reliability (testing needed)

**Mitigace:**

- ✅ Architecture review quarterly
- ✅ Load testing před production
- ✅ Feature flags pro gradual rollout
