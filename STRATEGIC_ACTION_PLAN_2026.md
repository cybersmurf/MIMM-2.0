# 🎯 MIMM 2.0 - Strategický Akční Plán 2026

**Datum analýzy**: 24. ledna 2026  
**Aktuální verze**: 1.0.0-alpha  
**Stav projektu**: Phase 1 MVP - Backend API + Frontend Auth ✅ 60% Complete  
**Cílové datum MVP**: 14. února 2026 (3 týdny)

---

## 📊 Executive Summary

### Co Je Hotovo (✅ 60%)

**Backend API - REST Endpoints (100% complete)**:
- ✅ AuthController (6 endpoints: register, login, refresh, verify, /me, logout)
- ✅ EntriesController (7 endpoints: CRUD + search + statistics)
- ✅ JWT authentication s refresh token pattern
- ✅ Swagger/OpenAPI dokumentace
- ✅ Exception handling middleware
- ✅ CORS konfigurace pro localhost frontend

**Business Logic - Services (100% complete)**:
- ✅ AuthService (17 unit testů, všechny passing)
- ✅ EntryService (18 unit testů, všechny passing)
- ✅ 35/35 testů prochází (100% success rate)
- ✅ EF Core ApplicationDbContext s 3 entitami (User, JournalEntry, LastFmToken)

**Frontend - Authentication UI (80% complete)**:
- ✅ Login.razor (dual login/register mode s MudBlazor)
- ✅ Dashboard.razor (protected page s placeholder UI)
- ✅ AuthApiService (HTTP API wrapper)
- ✅ AuthStateService (localStorage token management)
- ✅ AuthorizationMessageHandler (auto JWT injection)
- ✅ MudBlazor 7.0.0 integrovaný
- ⚠️ Index.razor (jen redirect na login)

**Infrastruktura (100% complete)**:
- ✅ Docker Compose (PostgreSQL + Redis)
- ✅ EF Core migrations připraveny (0 migrations aktuálně)
- ✅ Build pipeline (dotnet build uspěšný, 0 errors)
- ✅ launchSettings.json (backend na port 7001)

### Co Chybí (⏳ 40%)

**Frontend - Entry Management UI (0%)**:
- ⏳ EntryList.razor (zobrazení seznamu záznamů)
- ⏳ EntryCreate.razor (formulář pro nový záznam)
- ⏳ EntryEdit.razor (editace existujícího záznamu)
- ⏳ EntryApiService (HTTP wrapper pro entries endpoints)
- ⏳ MoodSelector.razor (2D Valence-Arousal grid komponenta)

**External Integrations (0%)**:
- ⏳ Last.fm OAuth flow (login + token exchange)
- ⏳ Last.fm scrobbling service
- ⏳ Music search (iTunes, Deezer, MusicBrainz, Discogs)
- ⏳ Album art fetching

**Database (0%)**:
- ⏳ První EF Core migration (InitialCreate)
- ⏳ Seed data (test users)
- ⏳ PostgreSQL running v Docker

**Testing (20%)**:
- ✅ Backend unit tests (35 testů)
- ⏳ Integration tests (0 testů)
- ⏳ E2E tests (0 testů)
- ⏳ Frontend component tests (0 testů)

---

## 🏗️ Architektonická Analýza

### 1. Aktuální Stav Kódové Báze

**Statistiky**:
- 60 souborů (.cs + .razor)
- ~2,879 řádků C# kódu (bez tests)
- 5 projektů (.csproj)
- 2 controllery (Auth + Entries)
- 2 services (Auth + Entry) + 5 placeholders
- 3 Razor stránky (Login, Dashboard, Index)
- 0 Razor komponenty (MoodSelector chybí)

**Silné Stránky**:
1. **Clean Architecture**: Controllers → Services → Data layer separation
2. **Type Safety**: C# 13 s nullable reference types enabled
3. **Modern Stack**: .NET 9, EF Core 9, Blazor WASM
4. **Testing Coverage**: 35 unit testů pro kritické služby (Auth + Entry)
5. **Security**: JWT Bearer + refresh token, password hashing (BCrypt)

**Slabá Místa**:
1. **Zero integračních testů**: Backend API netestován end-to-end
2. **Database chybí**: Žádná migrace neproběhla, žádný PostgreSQL
3. **Frontend neúplný**: Dashboard má jen placeholders, chybí entry CRUD UI
4. **External API integrce**: Last.fm, iTunes, Deezer, MusicBrainz, Discogs - nic neimplementováno
5. **No real-time**: SignalR konfigurace existuje, ale žádné huby
6. **No pagination**: IPagedList<T> není implementován v EntryService

### 2. Technický Dluh

**Vysoká Priorita** (blokuje MVP):
1. **Database Migration**: Bez DB nelze testovat registraci/login
2. **Entry CRUD UI**: Dashboard je prázdný bez entry list
3. **MoodSelector Component**: Kritický pro vytváření záznamu

**Střední Priorita** (zlepšuje UX):
4. **Error Boundaries**: Blazor nemá error handling pro unhandled exceptions
5. **Loading States**: API calls nemají loading indikátory (kromě login button)
6. **Form Validation**: Client-side validace chybí, jen server-side
7. **Pagination**: EntryService vrací IEnumerable, ne IPagedList

**Nízká Priorita** (pro Phase 2):
8. **Last.fm OAuth**: MVP může mít manual entry bez Last.fm
9. **Music Search**: Autocomplete nice-to-have, ne must-have
10. **Real-time Updates**: SignalR pro live analytics můžeme odložit

### 3. Rizika & Mitigation

| Riziko | Pravděpodobnost | Dopad | Mitigation |
|--------|------------------|--------|------------|
| **Frontend-Backend Integration Failure** | Vysoká | Kritická | End-to-end test ASAP (run backend + frontend together) |
| **CORS Issues** | Střední | Vysoká | Verify CORS config in Program.cs includes localhost:5000 |
| **JWT Token Expiration Bug** | Střední | Střední | Integration test: refresh token flow |
| **PostgreSQL Connection Issues** | Nízká | Vysoká | Docker Compose test before DB migration |
| **Last.fm API Rate Limits** | Nízká | Nízká | Implement retry logic + caching |
| **MudBlazor Component Breaking Changes** | Velmi nízká | Střední | Pin version to 7.0.0 in .csproj |

---

## 🎯 Prioritized Action Plan

### ⚡ KRITICKÉ (Start Immediately)

#### Action 1: Database Setup & First Migration
**Why**: Bez DB nemůžeme testovat real auth flow  
**Estimate**: 1-2h  
**Owner**: Dev  
**Deadline**: 24. ledna 2026 (dnes)

```bash
# 1. Start PostgreSQL in Docker
docker-compose up -d postgres

# 2. Verify connection string in appsettings.Development.json
# Should be: "Host=localhost;Database=mimm;Username=mimmuser;Password=mimmpass"

# 3. Create first migration
cd src/MIMM.Backend
dotnet ef migrations add InitialCreate --output-dir Data/Migrations

# 4. Apply migration
dotnet ef database update

# 5. Verify tables created
docker exec -it mimm-postgres psql -U mimmuser -d mimm -c "\dt"

# Expected tables: Users, JournalEntries, LastFmTokens, __EFMigrationsHistory
```

**Success Criteria**:
- ✅ 3 tables created in PostgreSQL
- ✅ Seed data možný (optional: create test user)
- ✅ Backend startup bez errors

---

#### Action 2: End-to-End Authentication Test
**Why**: Ověřit že frontend + backend fungují spolu  
**Estimate**: 30min  
**Owner**: Dev  
**Deadline**: 24. ledna 2026 (dnes po Action 1)

```bash
# Terminal 1: Backend
cd src/MIMM.Backend
dotnet run
# Expected: https://localhost:7001 (Swagger UI launch)

# Terminal 2: Frontend
cd src/MIMM.Frontend
dotnet run
# Expected: https://localhost:5001 (Blazor WASM)

# Browser: https://localhost:5001
# 1. Fill register form (email: test@example.com, password: Test123!)
# 2. Click "Register"
# 3. Verify redirect to /dashboard
# 4. Verify dashboard shows "Welcome, test@example.com"
# 5. Click "Logout"
# 6. Verify redirect to /login
# 7. Fill login form with same credentials
# 8. Click "Login"
# 9. Verify redirect to /dashboard

# Check backend logs for:
# - POST /api/auth/register → 200 OK
# - POST /api/auth/login → 200 OK
# - GET /api/auth/me → 200 OK (with Authorization header)
```

**Success Criteria**:
- ✅ Registrace vytvoří uživatele v DB
- ✅ Login vrací JWT token
- ✅ Dashboard page načte user data
- ✅ Logout smaže token z localStorage
- ✅ Protected page redirectuje na /login při missing token

**Fix If Fails**:
- CORS error → Check Program.cs CORS policy includes http://localhost:5000 AND https://localhost:5001
- 401 Unauthorized → Check JWT token v Authorization header (DevTools Network tab)
- Redirect loop → Check AuthStateService.IsAuthenticatedAsync() logic

---

### 🚀 HIGH PRIORITY (Week 1)

#### Action 3: Entry CRUD UI Implementation
**Why**: Dashboard je prázdný bez entry list - hlavní feature MVP  
**Estimate**: 8-10h  
**Owner**: Dev  
**Deadline**: 27. ledna 2026

**Step 3.1: EntryApiService (HTTP Wrapper)**

```bash
# Create file: src/MIMM.Frontend/Services/EntryApiService.cs
```

**Interface & Implementation**:
```csharp
public interface IEntryApiService
{
    Task<PagedResult<JournalEntryDto>?> GetEntriesAsync(int page = 1, int pageSize = 10, string sortBy = "created", string direction = "desc");
    Task<JournalEntryDto?> GetEntryByIdAsync(int id);
    Task<JournalEntryDto?> CreateEntryAsync(CreateEntryRequest request);
    Task<JournalEntryDto?> UpdateEntryAsync(int id, UpdateEntryRequest request);
    Task<bool> DeleteEntryAsync(int id);
    Task<PagedResult<JournalEntryDto>?> SearchEntriesAsync(SearchEntriesRequest request);
    Task<EntryStatisticsDto?> GetStatisticsAsync();
}

// HTTP wrapper volá EntriesController endpoints
// POST /api/entries, GET /api/entries, GET /api/entries/{id}, etc.
```

**Step 3.2: EntryList.razor (Main Dashboard)**

```bash
# Create file: src/MIMM.Frontend/Pages/Entries/EntryList.razor
```

**Komponenta Features**:
- MudTable nebo MudDataGrid s paginací
- Columns: Song Title, Artist, Album, Mood (Valence/Arousal), Created Date
- Actions: Edit button, Delete button (s confirm dialog)
- Filter bar: Search text, date range picker
- "New Entry" button → naviguje na /entries/create

**Step 3.3: EntryCreate.razor (Create New Entry)**

```bash
# Create file: src/MIMM.Frontend/Pages/Entries/EntryCreate.razor
```

**Form Fields**:
- Song Title (MudTextField, required)
- Artist Name (MudTextField, required)
- Album Name (MudTextField, optional)
- Source (MudSelect: itunes, deezer, lastfm, manual)
- Cover URL (MudTextField, optional)
- **MoodSelector component** (2D grid - see Action 4)
- Tension Level (MudSlider: 0-100)
- Somatic Tags (MudChipSet s MudChip: "headache", "nausea", "butterflies", custom)
- Notes (MudTextField multiline)

**Step 3.4: EntryEdit.razor (Edit Existing)**

```bash
# Create file: src/MIMM.Frontend/Pages/Entries/EntryEdit.razor
```

- Load entry by ID (from route parameter)
- Pre-populate form s existing values
- Same form fields jako EntryCreate
- "Save" button → PUT /api/entries/{id}
- "Cancel" button → navigate back to list

**Step 3.5: Integration & Routing**

Update `src/MIMM.Frontend/Program.cs`:
```csharp
builder.Services.AddScoped<IEntryApiService, EntryApiService>();
```

Update `src/MIMM.Frontend/App.razor` (add routes):
```razor
<Route Path="/entries" Component="@typeof(EntryList)" />
<Route Path="/entries/create" Component="@typeof(EntryCreate)" />
<Route Path="/entries/edit/{id:int}" Component="@typeof(EntryEdit)" />
```

Update `Dashboard.razor`:
- Replace placeholder cards s <EntryList /> component

**Success Criteria**:
- ✅ Dashboard zobrazuje seznam entries (prázdný pokud žádné)
- ✅ Klik "New Entry" → naviguje na /entries/create
- ✅ Vyplnění formuláře + Submit → vytvoří entry v DB
- ✅ Entry list se refreshne s novým záznamem
- ✅ Klik "Edit" → naviguje na /entries/edit/{id} s pre-populated form
- ✅ Klik "Delete" → zobrazí confirm dialog → smaže entry

---

#### Action 4: MoodSelector Component (2D Circumplex Grid)
**Why**: Hlavní differentiátor MIMM - Russell's Circumplex Model  
**Estimate**: 4-5h  
**Owner**: Dev  
**Deadline**: 28. ledna 2026

```bash
# Create file: src/MIMM.Frontend/Components/MoodSelector.razor
```

**UI Design**:
```
         High Arousal (+1.0)
              ^
              |
    Excited   |   Tense
              |
Negative -----+-----> Positive  (Valence)
  (-1.0)      |        (+1.0)
    Sad       |   Calm
              |
              v
         Low Arousal (-1.0)
```

**Features**:
- SVG canvas 400x400px
- 2 ose: Valence (X), Arousal (Y)
- Clickable: klik na grid nastaví Valence + Arousal
- Visual feedback: colored dot na pozici
- Color gradient: Negative (red) → Neutral (yellow) → Positive (green)
- Labels: "Happy", "Sad", "Angry", "Calm" v 4 kvadrantech
- Output: `@bind-Valence` a `@bind-Arousal` parametry (double, -1.0 to 1.0)

**Integration**:
- Use in `EntryCreate.razor` a `EntryEdit.razor`
- Replace placeholder input fields pro Valence/Arousal

**Success Criteria**:
- ✅ Klik na grid aktualizuje Valence + Arousal values
- ✅ Visual dot indicator zobrazuje current selection
- ✅ Two-way binding funguje (change in form updates grid, change in grid updates form)

---

### 📈 MEDIUM PRIORITY (Week 2)

#### Action 5: Integration Tests (Backend API)
**Why**: Unit testy nemají pokrytí API layer + DB interactions  
**Estimate**: 6-8h  
**Owner**: Dev  
**Deadline**: 31. ledna 2026

**Step 5.1: Setup WebApplicationFactory**

Create `tests/MIMM.Tests.Integration/CustomWebApplicationFactory.cs`:
```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove real DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null) services.Remove(descriptor);
            
            // Add in-memory database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
        });
    }
}
```

**Step 5.2: Auth Integration Tests**

Create `tests/MIMM.Tests.Integration/Controllers/AuthControllerTests.cs`:
```csharp
public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Register_WithValidData_Returns200AndJwtToken() { }
    
    [Fact]
    public async Task Login_WithValidCredentials_Returns200AndJwtToken() { }
    
    [Fact]
    public async Task RefreshToken_WithValidRefreshToken_Returns200AndNewAccessToken() { }
    
    [Fact]
    public async Task GetMe_WithValidToken_Returns200AndUserDto() { }
    
    [Fact]
    public async Task GetMe_WithoutToken_Returns401Unauthorized() { }
}
```

**Step 5.3: Entries Integration Tests**

Create `tests/MIMM.Tests.Integration/Controllers/EntriesControllerTests.cs`:
```csharp
public class EntriesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task CreateEntry_WithValidData_Returns201Created() { }
    
    [Fact]
    public async Task GetEntries_ReturnsPagedList() { }
    
    [Fact]
    public async Task UpdateEntry_ByOwner_Returns200() { }
    
    [Fact]
    public async Task DeleteEntry_ByOwner_Returns204NoContent() { }
    
    [Fact]
    public async Task GetEntry_ByNonOwner_Returns404NotFound() { }
}
```

**Success Criteria**:
- ✅ 10+ integration tests (Auth + Entries)
- ✅ Všechny projdou (`dotnet test MIMM.sln`)
- ✅ Tests používají in-memory DB (ne real PostgreSQL)

---

#### Action 6: Frontend Error Handling & Loading States
**Why**: Lepší UX při API call failures  
**Estimate**: 3-4h  
**Owner**: Dev  
**Deadline**: 1. února 2026

**Step 6.1: Error Boundary Component**

Create `src/MIMM.Frontend/Components/ErrorBoundary.razor`:
```razor
<ErrorBoundary @ref="errorBoundary">
    <ChildContent>
        @ChildContent
    </ChildContent>
    <ErrorContent>
        <MudAlert Severity="Severity.Error">
            Something went wrong. Please refresh the page.
        </MudAlert>
    </ErrorContent>
</ErrorBoundary>
```

Wrap `App.razor` RouteView v ErrorBoundary.

**Step 6.2: Global Loading Indicator**

Create `src/MIMM.Frontend/Components/LoadingIndicator.razor`:
```razor
@if (IsLoading)
{
    <MudOverlay Visible="true" ZIndex="9999">
        <MudProgressCircular Indeterminate="true" Size="Size.Large" />
    </MudOverlay>
}
```

**Step 6.3: API Call Wrapper with Try-Catch**

Update all `*ApiService.cs` methods:
```csharp
public async Task<JournalEntryDto?> GetEntryByIdAsync(int id)
{
    try
    {
        var response = await _httpClient.GetAsync($"/api/entries/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JournalEntryDto>();
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "Failed to fetch entry {Id}", id);
        // Show snackbar error
        _snackbar.Add($"Failed to load entry: {ex.Message}", Severity.Error);
        return null;
    }
}
```

**Success Criteria**:
- ✅ Unhandled exceptions zobrazí error boundary, ne white screen
- ✅ API failures zobrazí snackbar error message
- ✅ Loading spinner během API calls

---

### 🎨 LOWER PRIORITY (Week 3)

#### Action 7: Music Search Integration (Multi-source)
**Why**: Better UX než manual entry, ale MVP funguje i bez  
**Estimate**: 10-12h  
**Owner**: Dev  
**Deadline**: 7. února 2026

**Step 7.1: Refit Clients**

Install Refit NuGet package:
```bash
cd src/MIMM.Backend
dotnet add package Refit --version 7.2.22
```

Create interfaces:
```csharp
// Services/ExternalApis/IItunesApi.cs
public interface IItunesApi
{
    [Get("/search?term={query}&media=music")]
    Task<ItunesSearchResponse> SearchAsync(string query);
}

// Services/ExternalApis/IDeezerApi.cs
public interface IDeezerApi
{
    [Get("/search/track?q={query}")]
    Task<DeezerSearchResponse> SearchAsync(string query);
}
```

Register in Program.cs:
```csharp
builder.Services.AddRefitClient<IItunesApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://itunes.apple.com"));

builder.Services.AddRefitClient<IDeezerApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.deezer.com"));
```

**Step 7.2: MusicSearchService**

Implement `Services/MusicSearchService.cs`:
```csharp
public class MusicSearchService : IMusicSearchService
{
    public async Task<IEnumerable<TrackSearchResult>> SearchAsync(string query, string[] sources)
    {
        var tasks = new List<Task<IEnumerable<TrackSearchResult>>>();
        
        if (sources.Contains("itunes"))
            tasks.Add(SearchItunesAsync(query));
        
        if (sources.Contains("deezer"))
            tasks.Add(SearchDeezerAsync(query));
        
        var results = await Task.WhenAll(tasks);
        return results.SelectMany(r => r).Take(20);
    }
}
```

**Step 7.3: SearchController**

Create `Controllers/SearchController.cs`:
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SearchController : ControllerBase
{
    [HttpGet("tracks")]
    public async Task<IActionResult> SearchTracks(
        [FromQuery] string q,
        [FromQuery] string sources = "itunes,deezer")
    {
        var results = await _musicSearchService.SearchAsync(q, sources.Split(','));
        return Ok(results);
    }
}
```

**Step 7.4: Frontend Music Search Component**

Create `src/MIMM.Frontend/Components/MusicSearch.razor`:
```razor
<MudAutocomplete T="TrackSearchResult"
                 Label="Search Song"
                 SearchFunc="SearchTracks"
                 ToStringFunc="@(t => $"{t.Title} - {t.Artist}")"
                 ValueChanged="OnTrackSelected">
    <ItemTemplate>
        <MudListItem>
            <img src="@context.CoverUrl" width="50" />
            <div>
                <strong>@context.Title</strong><br />
                @context.Artist - @context.Album
            </div>
        </MudListItem>
    </ItemTemplate>
</MudAutocomplete>
```

**Step 7.5: Integration with EntryCreate**

Update `EntryCreate.razor`:
- Replace manual Song Title/Artist/Album inputs s `<MusicSearch />`
- OnTrackSelected → pre-populate form fields

**Success Criteria**:
- ✅ MusicSearch autocomplete funguje s >3 characters
- ✅ Results zobrazují album art + song title + artist
- ✅ Klik na result → populate EntryCreate form
- ✅ User může stále zvolit manual entry (fallback)

---

#### Action 8: Last.fm OAuth Integration
**Why**: Social feature, optional pro MVP  
**Estimate**: 8-10h  
**Owner**: Dev  
**Deadline**: 10. února 2026

**Prerequisites**:
1. Zaregistrovat MIMM 2.0 na Last.fm API website
2. Získat API Key + Shared Secret
3. Set callback URL: https://localhost:7001/api/integrations/lastfm/callback

**Step 8.1: LastFmHttpClient (Refit)**

```csharp
public interface ILastFmHttpClient
{
    [Get("/2.0/?method=auth.getToken&api_key={apiKey}&format=json")]
    Task<LastFmTokenResponse> GetRequestTokenAsync(string apiKey);
    
    [Get("/2.0/?method=auth.getSession&api_key={apiKey}&token={token}&api_sig={signature}&format=json")]
    Task<LastFmSessionResponse> GetSessionAsync(string apiKey, string token, string signature);
    
    [Get("/2.0/?method=track.search&api_key={apiKey}&track={query}&format=json")]
    Task<LastFmSearchResponse> SearchTracksAsync(string apiKey, string query);
}
```

**Step 8.2: LastFmService Implementation**

```csharp
public class LastFmService : ILastFmService
{
    public async Task<string> GetAuthUrlAsync()
    {
        var token = await _lastFmClient.GetRequestTokenAsync(_config.ApiKey);
        return $"https://www.last.fm/api/auth/?api_key={_config.ApiKey}&token={token.Token}";
    }
    
    public async Task<LastFmToken> ExchangeAuthTokenAsync(int userId, string token)
    {
        // Generate API signature (MD5 hash)
        var signature = GenerateSignature(token);
        var session = await _lastFmClient.GetSessionAsync(_config.ApiKey, token, signature);
        
        // Store session in database
        var lastFmToken = new LastFmToken
        {
            UserId = userId,
            SessionKey = session.SessionKey,
            LastFmUsername = session.Username,
            CreatedAt = DateTime.UtcNow
        };
        
        await _dbContext.LastFmTokens.AddAsync(lastFmToken);
        await _dbContext.SaveChangesAsync();
        
        return lastFmToken;
    }
}
```

**Step 8.3: IntegrationsController**

```csharp
[ApiController]
[Route("api/integrations/lastfm")]
[Authorize]
public class IntegrationsController : ControllerBase
{
    [HttpGet("auth-url")]
    public async Task<IActionResult> GetLastFmAuthUrl()
    {
        var url = await _lastFmService.GetAuthUrlAsync();
        return Ok(new { authUrl = url });
    }
    
    [HttpGet("callback")]
    public async Task<IActionResult> LastFmCallback([FromQuery] string token)
    {
        var userId = GetCurrentUserId();
        await _lastFmService.ExchangeAuthTokenAsync(userId, token);
        return Redirect("https://localhost:5001/settings");
    }
}
```

**Step 8.4: Frontend Last.fm Connect**

Create `src/MIMM.Frontend/Components/LastFmConnect.razor`:
```razor
@if (IsConnected)
{
    <MudAlert Severity="Severity.Success">
        Connected to Last.fm as @Username
    </MudAlert>
    <MudButton OnClick="Disconnect">Disconnect</MudButton>
}
else
{
    <MudButton OnClick="ConnectLastFm" Color="Color.Error">
        Connect Last.fm
    </MudButton>
}
```

**Success Criteria**:
- ✅ Klik "Connect Last.fm" → otevře Last.fm OAuth consent page
- ✅ Po approve → redirect zpět na MIMM settings page
- ✅ LastFmToken uložen v DB
- ✅ Dashboard zobrazuje "Connected to Last.fm as {username}"

---

#### Action 9: Advanced Dashboard Analytics
**Why**: Mood patterns visualization - cool feature, ne critical  
**Estimate**: 6-8h  
**Owner**: Dev  
**Deadline**: 12. února 2026

**Step 9.1: AnalyticsService Implementation**

```csharp
public class AnalyticsService : IAnalyticsService
{
    public async Task<MoodDistributionDto> GetMoodDistributionAsync(int userId)
    {
        var entries = await _dbContext.JournalEntries
            .Where(e => e.UserId == userId)
            .Select(e => new { e.Valence, e.Arousal })
            .ToListAsync();
        
        return new MoodDistributionDto
        {
            AverageValence = entries.Average(e => e.Valence),
            AverageArousal = entries.Average(e => e.Arousal),
            // ... more stats
        };
    }
}
```

**Step 9.2: AnalyticsController**

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    [HttpGet("mood-stats")]
    public async Task<IActionResult> GetMoodStatistics()
    {
        var userId = GetCurrentUserId();
        var stats = await _analyticsService.GetMoodDistributionAsync(userId);
        return Ok(stats);
    }
    
    [HttpGet("top-artists")]
    public async Task<IActionResult> GetTopArtists()
    {
        var userId = GetCurrentUserId();
        var artists = await _analyticsService.GetTopArtistsAsync(userId);
        return Ok(artists);
    }
}
```

**Step 9.3: Frontend Analytics Charts**

Install Blazor charting library:
```bash
cd src/MIMM.Frontend
dotnet add package Blazor.Extensions.Canvas
```

Create `src/MIMM.Frontend/Components/MoodChart.razor`:
- Scatter plot: X = Valence, Y = Arousal
- Each point = journal entry
- Color by date (gradient: old → new)
- Hover shows song title + artist

Create `src/MIMM.Frontend/Components/TopArtistsChart.razor`:
- Horizontal bar chart
- Top 10 artists by entry count

**Step 9.4: Update Dashboard**

Replace placeholder cards v `Dashboard.razor` s:
- `<MoodChart />`
- `<TopArtistsChart />`
- `<RecentEntriesList />` (last 5 entries)

**Success Criteria**:
- ✅ Dashboard zobrazuje mood scatter plot
- ✅ Dashboard zobrazuje top artists bar chart
- ✅ Charts se updatují po vytvoření nového entry

---

### 📦 DEPLOYMENT (Week 4)

#### Action 10: Production Readiness Checklist
**Why**: MVP musí být deploynutelný na cloud  
**Estimate**: 4-6h  
**Owner**: Dev  
**Deadline**: 14. února 2026

**Step 10.1: Docker Production Build**

Create `Dockerfile.production`:
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/MIMM.Backend/MIMM.Backend.csproj", "MIMM.Backend/"]
COPY ["src/MIMM.Shared/MIMM.Shared.csproj", "MIMM.Shared/"]
RUN dotnet restore "MIMM.Backend/MIMM.Backend.csproj"
COPY src/ .
RUN dotnet publish "MIMM.Backend/MIMM.Backend.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MIMM.Backend.dll"]
```

Test Docker build:
```bash
docker build -f Dockerfile.production -t mimm-backend:1.0.0 .
docker run -p 7001:8080 mimm-backend:1.0.0
```

**Step 10.2: Azure App Service Setup**

Azure CLI commands:
```bash
# Login
az login

# Create resource group
az group create --name mimm-rg --location westeurope

# Create PostgreSQL Flexible Server
az postgres flexible-server create \
  --resource-group mimm-rg \
  --name mimm-postgres \
  --location westeurope \
  --admin-user mimmadmin \
  --admin-password <STRONG_PASSWORD> \
  --sku-name Standard_B1ms \
  --tier Burstable

# Create App Service Plan
az appservice plan create \
  --resource-group mimm-rg \
  --name mimm-plan \
  --sku B1 \
  --is-linux

# Create Web App
az webapp create \
  --resource-group mimm-rg \
  --plan mimm-plan \
  --name mimm-app \
  --runtime "DOTNETCORE:9.0"

# Configure connection string
az webapp config connection-string set \
  --resource-group mimm-rg \
  --name mimm-app \
  --connection-string-type PostgreSQL \
  --settings DefaultConnection="Host=mimm-postgres.postgres.database.azure.com;Database=mimm;Username=mimmadmin;Password=<PASSWORD>"
```

**Step 10.3: GitHub Actions CI/CD**

Create `.github/workflows/deploy.yml`:
```yaml
name: Deploy to Azure

on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      
      - name: Restore dependencies
        run: dotnet restore MIMM.sln
      
      - name: Run tests
        run: dotnet test MIMM.sln --no-restore --verbosity normal
      
      - name: Publish
        run: dotnet publish src/MIMM.Backend/MIMM.Backend.csproj -c Release -o ${{env.DOTNET_ROOT}}/publish
      
      - name: Deploy to Azure Web App
        uses: azure/webapps-deploy@v2
        with:
          app-name: 'mimm-app'
          slot-name: 'Production'
          publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
          package: ${{env.DOTNET_ROOT}}/publish
```

**Step 10.4: Security Hardening**

- [ ] HTTPS only (remove HTTP endpoints)
- [ ] Add rate limiting (AspNetCoreRateLimit NuGet)
- [ ] Enable CORS only for production frontend URL
- [ ] Rotate JWT secret (use Azure Key Vault)
- [ ] Enable Application Insights logging
- [ ] Setup Azure Monitor alerts

**Success Criteria**:
- ✅ Docker image builds successfully
- ✅ App deploynut na Azure App Service
- ✅ PostgreSQL Flexible Server running
- ✅ CI/CD pipeline projde (tests + deploy)
- ✅ HTTPS funguje s valid certificate
- ✅ Frontend může volat backend API (CORS OK)

---

## 📅 Sprint Schedule (3 Weeks)

### Sprint 1 (24. - 28. ledna)
**Theme**: Database + E2E Testing + Entry CRUD UI

**Mon 24.1**:
- ✅ Action 1: Database setup (1-2h)
- ✅ Action 2: E2E auth test (30min)

**Tue 25.1**:
- 🚧 Action 3.1-3.2: EntryApiService + EntryList.razor (4h)

**Wed 26.1**:
- 🚧 Action 3.3-3.4: EntryCreate + EntryEdit (4h)

**Thu 27.1**:
- 🚧 Action 3.5: Integration & routing (2h)
- 🚧 Action 4: MoodSelector component (4h)

**Fri 28.1**:
- ✅ Sprint review
- ✅ Test all entry CRUD flows

---

### Sprint 2 (29. ledna - 4. února)
**Theme**: Testing + Error Handling + Music Search

**Mon 29.1**:
- 🚧 Action 5.1-5.2: Integration tests setup + Auth tests (4h)

**Tue 30.1**:
- 🚧 Action 5.3: Entries integration tests (3h)

**Wed 31.1**:
- 🚧 Action 6: Error handling + loading states (4h)

**Thu 1.2**:
- 🚧 Action 7.1-7.3: Music search backend (Refit + Service + Controller) (5h)

**Fri 2.2**:
- 🚧 Action 7.4-7.5: Music search frontend (MusicSearch component + integration) (5h)

---

### Sprint 3 (5. - 14. února)
**Theme**: Last.fm + Analytics + Deployment

**Mon 5.2**:
- 🚧 Action 8.1-8.2: Last.fm OAuth backend (4h)

**Tue 6.2**:
- 🚧 Action 8.3-8.4: Last.fm OAuth frontend (3h)

**Wed 7.2**:
- 🚧 Action 9.1-9.2: Analytics service + controller (4h)

**Thu 8.2**:
- 🚧 Action 9.3-9.4: Analytics charts + dashboard update (4h)

**Fri 9.2** (Buffer Day):
- ✅ Bug fixes
- ✅ Polish UI
- ✅ Write deployment docs

**Mon 10.2**:
- 🚧 Action 10.1: Docker production build (2h)

**Tue 11.2**:
- 🚧 Action 10.2: Azure setup (3h)

**Wed 12.2**:
- 🚧 Action 10.3: CI/CD pipeline (2h)

**Thu 13.2**:
- 🚧 Action 10.4: Security hardening (2h)

**Fri 14.2** (**MVP LAUNCH**):
- ✅ Final testing
- ✅ Deploy to production
- 🎉 **MVP Release 1.0.0**

---

## 📊 Success Metrics (MVP Launch)

### Functionality Checklist

- [ ] **User can register** with email + password
- [ ] **User can login** and receive JWT token
- [ ] **User can create journal entry** with song + mood + notes
- [ ] **User can view entry list** (paginated, sortable)
- [ ] **User can edit entry**
- [ ] **User can delete entry**
- [ ] **User can search music** (iTunes + Deezer)
- [ ] **User can connect Last.fm** account
- [ ] **User can view mood analytics** (scatter plot, top artists)
- [ ] **App works on mobile** (responsive UI)
- [ ] **App is deployed** on Azure App Service (production URL)

### Technical Metrics

- [ ] **100% CI/CD success rate** (last 10 deployments)
- [ ] **50+ unit tests** (Auth + Entry + Analytics services)
- [ ] **20+ integration tests** (Controllers + DB)
- [ ] **Zero critical bugs** (P0 blockers)
- [ ] **< 2s page load time** (Lighthouse performance score > 80)
- [ ] **95%+ API uptime** (Azure Monitor)

### Code Quality

- [ ] **Zero compiler errors**
- [ ] **< 10 compiler warnings**
- [ ] **OWASP Top 10 compliant** (security audit pass)
- [ ] **README.md up-to-date** (quick start instructions)
- [ ] **CHANGELOG.md updated** (version 1.0.0 entry)

---

## 🚨 Risk Register & Contingency Plans

### High-Severity Risks

| Risk ID | Description | Impact | Mitigation | Contingency |
|---------|-------------|--------|------------|-------------|
| R1 | **Database migration fails** | Cannot test auth | Test migration on local PostgreSQL first | Use in-memory DB for MVP (downgrade) |
| R2 | **CORS blocks frontend calls** | Frontend cannot reach backend | Verify CORS config in Program.cs | Add wildcard CORS (dev only) |
| R3 | **JWT token refresh loop** | Infinite redirects | Integration test refresh flow | Remove refresh token, use long-lived access token (30 days) |
| R4 | **Last.fm API unavailable** | OAuth fails | Implement retry logic + fallback | Skip Last.fm for MVP, manual entry only |
| R5 | **Azure deployment fails** | Cannot launch MVP | Test Docker build locally first | Deploy to DigitalOcean/Railway.app instead |

### Medium-Severity Risks

| Risk ID | Description | Impact | Mitigation | Contingency |
|---------|-------------|--------|------------|-------------|
| R6 | **MoodSelector UX confusing** | Users don't understand 2D grid | Add tooltips + help text | Fallback to dropdown (Happy/Sad/Angry/Calm) |
| R7 | **Music search returns no results** | Bad autocomplete UX | Test with common queries | Allow manual entry always |
| R8 | **Integration tests slow** | CI/CD pipeline > 10 min | Use in-memory DB, not PostgreSQL | Skip integration tests in PR checks, only main |
| R9 | **Frontend bundle size too large** | Slow page load on mobile | Use lazy loading for pages | Remove MudBlazor, use plain HTML/CSS |

---

## 📚 Appendix: Reference Documentation

### Internal Docs

- [PROJECT_ANALYSIS_2026.md](./PROJECT_ANALYSIS_2026.md) - Kompletní technická analýza
- [README.md](./README.md) - Quick start guide
- [SETUP_GUIDE.md](./SETUP_GUIDE.md) - Step-by-step instalace
- [MIGRATION_GUIDE.md](./MIGRATION_GUIDE.md) - MIMM 1.0 → 2.0 migrace
- [AGENTS.md](./AGENTS.md) - Instrukce pro AI agenty
- [CHANGELOG.md](./CHANGELOG.md) - Version history

### External APIs

- **Last.fm API**: https://www.last.fm/api
- **iTunes Search API**: https://developer.apple.com/library/archive/documentation/AudioVideo/Conceptual/iTuneSearchAPI/
- **Deezer API**: https://developers.deezer.com/api
- **MusicBrainz API**: https://musicbrainz.org/doc/MusicBrainz_API
- **Discogs API**: https://www.discogs.com/developers

### .NET Resources

- **.NET 9 Docs**: https://learn.microsoft.com/dotnet/
- **EF Core 9**: https://learn.microsoft.com/ef/core/
- **Blazor WASM**: https://learn.microsoft.com/aspnet/core/blazor/
- **MudBlazor**: https://mudblazor.com/
- **Refit**: https://github.com/reactiveui/refit
- **xUnit**: https://xunit.net/

---

## 🎯 Conclusion & Next Steps

### Immediate Actions (Today - 24. ledna)

1. **Start Docker PostgreSQL**:
   ```bash
   docker-compose up -d postgres
   ```

2. **Create first EF Core migration**:
   ```bash
   cd src/MIMM.Backend
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

3. **Run E2E test** (backend + frontend together):
   ```bash
   # Terminal 1
   cd src/MIMM.Backend && dotnet run
   
   # Terminal 2
   cd src/MIMM.Frontend && dotnet run
   
   # Browser: https://localhost:5001
   # Test: Register → Login → Dashboard
   ```

4. **Verify all tests pass**:
   ```bash
   dotnet test MIMM.sln
   # Expected: 35/35 tests passing
   ```

### Communication Plan

- **Daily standup** (solo dev = self-reflection):
  - What did I complete yesterday?
  - What will I work on today?
  - Any blockers?

- **Weekly review** (každý pátek):
  - Sprint goals completed?
  - Update CHANGELOG.md
  - Git tag: `v0.X.0` (alpha versions)

- **MVP Launch** (14. února 2026):
  - Deploy to production
  - Announce on social media (LinkedIn, X)
  - Create GitHub Release (v1.0.0)
  - Write blog post (dev.to, Medium)

---

**This strategic action plan transforms the current 60% complete MIMM 2.0 into a production-ready MVP in exactly 3 weeks (21 days).** 🚀

---

**Document Version**: 1.0  
**Created**: 24. ledna 2026  
**AI Developer**: GitHub Copilot (MIMM-Expert-Agent)  
**Next Review**: 28. ledna 2026 (Sprint 1 retrospective)
