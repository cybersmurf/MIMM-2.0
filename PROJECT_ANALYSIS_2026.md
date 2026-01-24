# 🎵 MIMM 2.0 - Komplexní Analýza Projektu

**Datum**: 24. ledna 2026  
**Stav**: Hotový scaffold, připraveno na vývoj  
**Klíčová rozhodnutí**: .NET 9, PostgreSQL, Blazor WASM, JWT auth  

---

## 📊 Part 1: Aktuální Stav Projektu

### Build & Testovací Status
- ✅ **Build**: Všech 5 MIMM projektů kompiluje bez chyb
- ✅ **Application.Tests**: 17/17 testů projde (Weather API sandbox)
- ⚠️ **MIMM.Tests**: Struktury připraveny (0 testů = placeholder)
- ✅ **Dependencies**: .NET 9.0.0 balíčky seřazeny (Refit 7.2.22, Npgsql 9.0.0, etc.)

### Architektonické Pilíře
```
Backend (ASP.NET Core 9)
├── Controllers (REST endpoints - TODO)
├── Services (IAuthService, IEntryService, etc.)
├── Data (ApplicationDbContext + 3 entities)
├── Middleware (ExceptionHandlingMiddleware - hotovo)
└── Swagger/JWT (hotovo)

Frontend (Blazor WASM 9)
├── App.razor (router)
├── MainLayout.razor (scaffold)
├── Pages/ (TODO: Login, Register, Dashboard, etc.)
└── Components/ (TODO: MoodSelector, EntryForm, etc.)

Shared (DTOs, Entities)
├── User (Email + Password + Language + Timezone)
├── JournalEntry (Song + Mood (Valence/Arousal) + Somatic)
└── LastFmToken (OAuth session)
```

### Database Design (EF Core + PostgreSQL)
**Entities** (Entity Framework Fluent API configured):
- **User**: Id, Email (unique), PasswordHash, DisplayName, Language, TimeZone, EmailVerified, CreatedAt, SoftDelete
- **JournalEntry**: UserId, SongTitle, ArtistName, AlbumName, SongId (external), CoverUrl, Source (itunes/deezer/lastfm/manual), **Valence** (-1.0 to 1.0), **Arousal** (-1.0 to 1.0), TensionLevel (0-100), SomaticTags (PostgreSQL text[]), Notes, CreatedAt
- **LastFmToken**: UserId, SessionKey, LastFmUsername, CreatedAt, ExpiresAt

**Indeksy** (pro common queries):
- Users: Email (unique), DeletedAt (soft delete filter)
- Entries: (UserId, CreatedAt), Source

---

## 🔒 Part 2: Security Analysis

### Authentication Strategy
- ✅ **JWT Bearer**: Program.cs nakonfigurován (TokenValidationParameters, Issuer/Audience check)
- ✅ **Refresh Tokens**: Config připraven (AccessTokenExpirationMinutes: 60, RefreshTokenExpirationDays: 7)
- ✅ **SignalR Support**: JwtBearerEvents s OnMessageReceived pro WebSocket auth
- ⚠️ **Password Hashing**: BCrypt.Net-Next zahrnut, ale AuthService.cs je placeholder
- ✅ **CORS**: Nastaveno pro localhost:5001 (frontend)

### Configuration & Secrets
- ✅ **User Secrets**: UserSecretsId: "mimm-backend-secrets" v csproj
- ✅ **Environment-based**: appsettings.json + appsettings.Development.json
- ✅ **.env.example**: Template pro všechny potřebné klíče
- ✅ **Serilog**: Logging configured (file + console)

### Encryption Points
- **PasswordHash**: BCrypt (standard, dobrý výběr)
- **LastFmToken.SessionKey**: "Encrypted in storage" (TODO: implementovat EncryptionService)
- **JWT Key**: Musí být 256-bit, .env izoluje prod secret

---

## 🏛️ Part 3: Architektonické Rozhodnutí & Rationale

### 1. Controller-based (ne Minimal API)
```
Zvoleno: Controllers + Services + Data layers
Důvod:
- MIMM má komplexní business logic (Last.fm, multiple music sources)
- Kontrola auth per-endpoint je jednodušší s attributes ([Authorize])
- SignalR hubs se lépe integrují
- Testovatelnost (mocking services v unit testech)

Alternativa: Vertical Slice (Feature folders)
- Byla by lepší pro micro-features (Last.fm callback, Entry creation)
- Ale scaffold pro tuto cestu není připravená

Doporučení: Počítat s migracií na Feature-based struktura v Phase 2
```

### 2. Blazor WASM (ne server-side)
```
Zvoleno: WebAssembly SPA
Důvod:
- PWA support (offline journal entries)
- Rich interactivity bez server round-trips
- Instalovatelné na mobil (iOS/Android homescreen)
- Type-safe C# frontend (sdílí Entities s backendem)

Fallback: Server-side Blazor
- Jednodušší state management
- Méně network traffic
- Ale ztratila bychom PWA a offline capability

Vedlejší efekt: Frontend client-side state musí být managed
- Blazored.LocalStorage je zahrnut (perfect)
- Potřebujeme AuthorizationStateProvider custom implementaci
```

### 3. PostgreSQL + EF Core 9
```
Zvoleno: Npgsql driver, EF Core 9 DbContext
Důvod:
- PostgreSQL: JSON/array support (text[] pro SomaticTags)
- EF Core 9: Linq queries, automatic migrations, type-safe
- SoftDelete support (HasQueryFilter na User.DeletedAt)
- Relationships: 1:N (User -> Entries), 1:1 (User -> LastFmToken)

Alternativa: Raw SQL / Dapper
- Byly bychom rychlejší, ale ztratili bychom type-safety
- Harder to test (mocking DB harder)

Consideration: N+1 query problem
- Potřebujeme .Include() v queries nebo IQueryable<> pagination
```

### 4. Refresh Token Pattern
```
Zvoleno: Dual token (AccessToken + RefreshToken)
AccessToken:
- Short-lived (60 min default)
- Stored in memory (frontend) nebo secure cookie
- JWT claims: sub (userId), email, roles

RefreshToken:
- Long-lived (7 dní default)
- HttpOnly secure cookie (ne localStorage!)
- Stored in DB (User table?) nebo cache (Redis)

Flow:
1. Login → Issue AccessToken (JWT) + RefreshToken (secure cookie)
2. API access → Use AccessToken in Authorization header
3. Access expired → Refresh endpoint → Issue new AccessToken
4. Refresh expired → Re-login required

Security wins:
- AccessToken v memory = XSS safe (can't be stolen via JS)
- RefreshToken httpOnly = CSRF mitigated
- Short AccessToken window = limited exposure if leaked
```

---

## 🛣️ Part 4: Roadmap Vývoj (12 týdnů)

### Phase 1: MVP (Týdny 1-4) - AKTUÁLNĚ ZDE
**Cíl**: Fungující auth + entry CRUD + basic UI

#### Týden 1-2: Authentication
```
Implementovat:
1. AuthService.cs
   ├── Register(email, password, displayName) → User + PasswordHash
   ├── Login(email, password) → { AccessToken, RefreshToken, User }
   ├── RefreshToken() → new AccessToken
   └── Verify/ValidateToken() → ClaimsPrincipal

2. AuthController.cs (REST endpoints)
   POST /api/auth/register
   POST /api/auth/login
   POST /api/auth/refresh
   GET /api/auth/me (current user)
   POST /api/auth/logout

3. Tests:
   - RegisterServiceTests (bcrypt hashing, duplicate email)
   - LoginServiceTests (valid/invalid credentials)
   - TokenTests (JWT creation, expiration)

4. Blazor:
   - Pages/Login.razor
   - Pages/Register.razor
   - Components/AuthorizationStateProvider.cs (custom)
   - Store tokens in LocalStorage (AccessToken) + Cookie (RefreshToken)
```

#### Týden 2-3: Entry CRUD
```
Implementovat:
1. EntryService.cs
   ├── CreateAsync(userId, request) → JournalEntry
   ├── GetAsync(userId, entryId) → JournalEntry
   ├── ListAsync(userId, pagination) → IPagedList<JournalEntry>
   ├── UpdateAsync(userId, entryId, request) → JournalEntry
   └── DeleteAsync(userId, entryId) → bool

2. EntriesController.cs
   POST /api/entries (create)
   GET /api/entries (list with pagination)
   GET /api/entries/{id} (detail)
   PUT /api/entries/{id} (update)
   DELETE /api/entries/{id}

3. Tests:
   - EntryServiceTests (CRUD operations, authorization)
   - EntriesControllerTests (API endpoints)

4. Blazor:
   - Pages/Dashboard.razor (list entries)
   - Pages/EntryDetail.razor (view/edit)
   - Components/MoodSelector.razor (2D circumplex grid)
   - Components/EntryForm.razor (create/edit form)
```

#### Týden 3-4: Last.fm Integration (v1)
```
Implementovat:
1. LastFmService.cs (placeholder)
   ├── GetAuthUrlAsync() → OAuth consent URL
   ├── ExchangeAuthTokenAsync(code) → SessionKey
   ├── SearchTracksAsync(query) → IEnumerable<Track>
   └── ScrobbleAsync(userId, track) → bool

2. LastFmHttpClient (Refit interface)
   - Configure base URL: https://ws.audioscrobbler.com
   - Auth/track.getInfo, track.search endpoints

3. Controllers/IntegrationsController.cs
   GET /api/integrations/lastfm/auth-url
   GET /api/integrations/lastfm/callback?token=...
   GET /api/integrations/lastfm/disconnect

4. Tests:
   - LastFmServiceTests (mock Refit client)

5. Blazor:
   - Components/LastFmConnect.razor
   - Store LastFm session (User.LastFmToken)
```

### Phase 2: Social & Analytics (Týdny 5-8)

#### Week 5-6: Music Search (Multi-source)
```
Implementovat:
1. IMusicSearchService implementations:
   ├── ItunesSearchClient (Refit)
   ├── DeezerSearchClient (Refit)
   ├── MusicBrainzClient (Refit)
   └── DiscogsClient (Refit, needs token)

2. SearchController.cs
   GET /api/search/tracks?q=...&source=itunes,deezer,musicbrainz

3. Blazor:
   - Components/MusicSearch.razor (search bar + results)
   - Integration with EntryForm (select song → populate form)
```

#### Week 6-7: Real-time Analytics (SignalR)
```
Implementovat:
1. AnalyticsHub.cs (SignalR hub)
   - BroadcastMoodStatistics() → avg Valence, Arousal
   - NotifyNewEntry() → real-time feed updates
   
2. AnalyticsService.cs
   ├── GetMoodDistributionAsync(userId) → { valence[], arousal[] }
   ├── GetTopArtistsAsync(userId) → IEnumerable<(Artist, Count)>
   ├── GetTimeSeriesAsync(userId, dateRange) → mood trends

3. AnalyticsController.cs
   GET /api/analytics/mood-stats
   GET /api/analytics/top-artists
   GET /api/analytics/trends

4. Blazor:
   - Pages/Analytics.razor (dashboards)
   - Components/MoodChart.razor (SignalR live mood feed)
   - Components/ArtistLeaderboard.razor
```

#### Week 7-8: Mood Search/Filters
```
Implementovat:
1. EntryService.cs extend:
   ├── SearchAsync(userId, filters) → ValenceRange, ArousalRange, DateRange, Artist
   └── GetByMoodAsync(userId, valence, arousal, threshold)

2. EntriesController.cs extend:
   GET /api/entries/search?valence=-1:1&arousal=-1:1&artist=...

3. Blazor:
   - Components/MoodFilter.razor (interactive 2D grid selector)
```

### Phase 3: Export & Mobile (Týdny 9-12)

#### Week 9: Data Export
```
Implementovat:
1. ExportService.cs
   ├── ExportAsCsvAsync(userId) → CSV bytes
   ├── ExportAsJsonAsync(userId) → JSON bytes
   ├── ExportAsPdfAsync(userId) → PDF report

2. ExportController.cs
   GET /api/export/csv
   GET /api/export/json
   GET /api/export/pdf

3. Tests: ExportServiceTests
```

#### Week 10-11: Mobile App (MAUI)
```
Considera:
- Separate VS project: MIMM.Mobile.Maui
- Share MIMM.Shared (entities, DTOs)
- HttpClient interceptor for auth tokens
- SQLite local sync with backend
- Offline support (SyncService)
```

#### Week 12: Deployment & Polish
```
- Azure App Service deployment docs
- Docker production setup
- Database backups & monitoring
- Performance tuning
- Security audit (OWASP Top 10)
```

---

## 🎯 Part 5: Prioritizace & Quick Wins

### Start Immediately (This Week)
1. **AuthService.cs implementation** (3-4h)
   - Register: Create User + hash password
   - Login: Verify + JWT generation
   - RefreshToken: Validate & issue new AccessToken
   - Test with unit tests

2. **EntryService.cs CRUD** (4-5h)
   - Interface already defined
   - Implement async methods
   - Add pagination helper

3. **First Blazor Page: Login.razor** (2-3h)
   - Form binding + validation
   - Call RegisterAsync from AuthService
   - Redirect to dashboard on success

### Follow-up (Week 2)
4. **Dashboard.razor** (list entries)
5. **MoodSelector.razor** (2D interactive grid)
6. **EntryForm.razor** (create/edit with validation)

### Dependencies to Avoid Blocking
- ❌ DON'T wait for Last.fm OAuth (can mock)
- ❌ DON'T wait for music search APIs (manual entry first)
- ✅ DO prioritize authentication (blocks everything else)

---

## ⚠️ Part 6: Known Gaps & Technical Debt

### Immediate Issues
1. **No password reset flow** → TODO: Email-based reset via SendGrid
2. **No email verification** → Config flag exists, but SMTP not implemented
3. **Soft delete** → User.DeletedAt exists, but no "undelete" endpoint
4. **No role-based access** → All users are creators (no Admin role yet)

### Database Concerns
1. **N+1 query risk** → Entries endpoint without .Include(u => u.User) will lazy-load
2. **Pagination not enforced** → No IPagedList<T> interface (implement or use PagedList.Core NuGet)
3. **No audit logging** → No CreatedBy, ModifiedBy fields for compliance

### Frontend Concerns
1. **State management** → Using LocalStorage, but no centralized Redux-like store
2. **Form validation** → DataAnnotations on DTOs, but no client-side validation rules
3. **Error handling** → Basic try-catch, no retry logic or user-friendly messages

### Security Gaps
1. **CSRF token** → Standard POST/PUT/DELETE unprotected (implement AntiForgeryToken in ASP.NET Core)
2. **LastFm session encryption** → Not implemented yet (EncryptionService needed)
3. **Rate limiting** → No anti-brute-force or DDoS protection

---

## 📌 Part 7: Best Practices & Code Review Checklist

### Before Implementing Each Feature
- [ ] Write failing unit test first (TDD)
- [ ] Implement service layer (business logic)
- [ ] Add controller endpoint (HTTP binding)
- [ ] Add Blazor component (UI)
- [ ] Run `dotnet test` (verify all pass)
- [ ] Update CHANGELOG.md with feature
- [ ] Document API in Swagger comments

### Git Workflow
```bash
# For each feature
git checkout -b feature/auth-system
# Make changes
git add .
git commit -m "feat(auth): implement register/login/refresh endpoints

- Add AuthService with bcrypt password hashing
- Create AuthController with JWT token generation
- Add RefreshToken pattern with secure cookies
- Include unit tests (8 test cases)
- Closes #42"
git push origin feature/auth-system
# Open PR on GitHub
```

### Testing Pattern
```csharp
public class AuthServiceTests
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepositoryMock;
    
    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _authService = new AuthService(_userRepositoryMock.Object);
    }
    
    [Fact]
    public async Task Register_WithValidInput_CreatesUserAndHashesPassword()
    {
        // Arrange
        var email = "test@example.com";
        var password = "SecurePassword123!";
        
        // Act
        var user = await _authService.RegisterAsync(email, password, "Test User");
        
        // Assert
        user.Should().NotBeNull();
        user.Email.Should().Be(email);
        user.PasswordHash.Should().NotBe(password); // Verify hashed
        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
    }
}
```

---

## 🚀 Part 8: Success Criteria (End of Phase 1)

✅ **Fully Functional MVP**:
- Register new user with email + password
- Login with credentials, receive AccessToken
- Refresh token when expired
- Create journal entry with mood coordinates
- List entries (paginated)
- View entry detail
- Edit & delete entries
- No Last.fm yet (manual entry only)

✅ **Testing**:
- 100+ unit tests (services + controllers)
- 20+ integration tests (end-to-end auth flow)
- Code coverage >70%

✅ **Deployment**:
- Docker build succeeds
- docker-compose up works end-to-end
- GitHub Actions CI/CD pipeline passes

✅ **Documentation**:
- README updated with quick start
- API Swagger docs complete
- Database schema documented
- Dev setup guide updated

---

## 📊 Summary Table: What's Done vs. TODO

| Component | Status | Owner | Deadline |
|-----------|--------|-------|----------|
| **Backend Scaffold** | ✅ Done | Copilot | - |
| **AuthService.cs** | ⚠️ TODO | You | Week 1 |
| **EntryService.cs** | ⚠️ TODO | You | Week 2 |
| **EntriesController.cs** | ⚠️ TODO | You | Week 2 |
| **Login.razor** | ⚠️ TODO | You | Week 2 |
| **Dashboard.razor** | ⚠️ TODO | You | Week 3 |
| **MoodSelector.razor** | ⚠️ TODO | You | Week 3 |
| **LastFm OAuth** | ⚠️ TODO | You | Week 4 |
| **Unit Tests** | ⚠️ TODO | You | All phases |
| **Database** | ✅ Ready | - | - |
| **Docker** | ✅ Ready | - | - |
| **CI/CD** | ✅ Ready | - | - |

---

## 🎓 Conclusion: Jak Pokračovat

### Strategie Vývoje
1. **Solo dev + AI pair programming** → Vstup: "Implementuj AuthService s testy" → Copilot vygeneruje
2. **Feature branches** → 1 feature = 1 branch = 1 PR = Code review (sám sobě 😄)
3. **Regular testing** → `dotnet test` před každým commitem
4. **Documentation** → README + API comments v kódu

### Next Immediate Step
```bash
cd /Users/petrsramek/AntigravityProjects/MIMM-2.0

# Vytvoř si feature branch
git checkout -b feature/auth-implementation

# Zacni s AuthService.cs
# Zkopíruj si tento template:
# - IAuthService interface (interface)
# - AuthService class (implementation)
# - AuthServiceTests class (unit tests)
# - AuthController class (HTTP endpoints)

# Po napsání kódu:
dotnet test
dotnet build

# Pak pushni do GitHub
git add .
git commit -m "feat(auth): implement register, login, refresh endpoints with tests"
git push origin feature/auth-implementation
```

### Resources Pro Inspiraci
- **Refit docs** (HTTP client): Learn.Microsoft.com/dotnet/api/refit
- **EF Core pagination** (GetPagedAsync extension): GitHub autocodes.io
- **Blazor auth** (AuthenticationStateProvider): Learn.Microsoft.com/aspnet/core/blazor/security

---

**This analysis captures the full scope, architecture, priorities, and next steps. You're ready to start coding!** 🎸

---

*Document Version*: 1.0  
*Last Updated*: 24. ledna 2026  
*AI Developer*: GitHub Copilot (MIMM-Expert-Agent)
