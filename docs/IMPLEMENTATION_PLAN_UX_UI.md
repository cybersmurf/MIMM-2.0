# MIMM 2.0 – UX/UI Implementation Plan

**Datum:** 26. ledna 2026  
**Verze:** 1.0  
**Status:** Ready for Implementation  
**Zpracoval:** MIMM-Expert-Agent + UX/UI Specialist

---

## 📋 Executive Summary

Tento dokument poskytuje **konkrétní implementační plán** pro opravy a vylepšení UX/UI identifikované v [UX_UI_ANALYSIS_AND_ROADMAP.md](./UX_UI_ANALYSIS_AND_ROADMAP.md).

**Klíčové poznatky z analýzy:**

- ✅ **Strengths:** MudBlazor základ, glassmorphism design, 2D Mood Selector
- 🔴 **Kritické problémy:** Chybějící navigace, Dashboard mock data, slabý error feedback
- 🟡 **Střední problémy:** Accessibility gaps, chybějící debounce, nepřehledný Entry Dialog
- 🟢 **Polish:** Micro-interactions, typografie, color system, light/dark mode

**Časový odhad (senior dev):** 9-14 dní full-time  
**Priorita:** Fáze 1 (kritické fixes) → Fáze 2 (UX enhancements) → Fáze 3 (polish)

---

## 🎯 Cíle Implementace

### Primární cíle

1. **Odstranit blokující UX problémy** – Navigace, data integration, error handling
2. **Zvýšit task completion rate** na 85%+ (zejména vytváření entry)
3. **Zlepšit accessibility** – WCAG 2.1 compliance, keyboard nav, screen reader support
4. **Profesionalizovat vizuální prezentaci** – Micro-interactions, typography, polish

### Metriky úspěchu

| Metrika | Současný stav | Cílový stav | Měření |
|---------|--------------|-------------|--------|
| Task Completion Rate | ~60% (odhad) | >85% | Analytics tracking |
| Time to First Entry | ~5 min | <2 min | Timestamp rozdíl |
| Form Abandonment Rate | ~35% (odhad) | <20% | Dialog open vs submit |
| Mobile Usability Score | ~75/100 (odhad) | >90/100 | Lighthouse CI |
| Accessibility Score | 10+ errors | 0 errors | axe DevTools |
| Navigation Clarity | N/A | 1-2 clicks | Click tracking |

---

## 🚀 Fáze 1: Kritické UX Fixes (1-2 dny)

**Priorita:** 🔴 Nejvyšší  
**Impact:** Blokující → nepoužitelná aplikace bez těchto fixes  
**Časový odhad:** 1-2 dny (senior dev)

---

### Task 1.1: Implementovat Funkční Navigaci

**Problém:** MainLayout nemá funkční drawer/menu → uživatelé se nemohou pohybovat mezi stránkami.

**Řešení:** Přidat `MudDrawer` s navigation links, toggle button, active state highlighting.

#### Implementační kroky

1. **Upravit MainLayout.razor**
   - Přidat `MudDrawer` s `@bind-Open="_drawerOpen"`
   - Toggle logic v menu button: `@onclick="ToggleDrawer"`
   - NavMenu komponenta uvnitř drawer

2. **Vytvořit NavMenu.razor komponentu**
   - Navigation items: Dashboard, Analytics, Logout
   - `NavLink` s `Match="NavLinkMatch.All"` pro active state
   - Ikony: `@Icons.Material.Filled.Dashboard`, `@Icons.Material.Filled.Analytics`

3. **CSS styling pro active state**
   - `.nav-link-active` třída s highlighting

#### Code Reference

**MainLayout.razor (upravené):**

```razor
@inherits LayoutComponentBase

<MudLayout Class="app-shell">
    <MudDrawer @bind-Open="_drawerOpen" Elevation="2" Variant="DrawerVariant.Temporary" Anchor="Anchor.Start">
        <NavMenu />
    </MudDrawer>
    
    <MudAppBar Elevation="0" Dense Color="Color.Transparent" Class="glass-bar">
        <MudIconButton Icon="@Icons.Material.Filled.Menu" 
                       Color="Color.Inherit" 
                       Edge="Edge.Start" 
                       OnClick="ToggleDrawer" />
        <MudText Typo="Typo.h6" Class="brand">MIMM 2.0</MudText>
        <MudSpacer />
        <MudIconButton Icon="@Icons.Material.Filled.Person" Color="Color.Inherit" />
    </MudAppBar>
    
    <MudMainContent>
        <MudContainer MaxWidth="MaxWidth.Medium" Class="page-container">
            <MudPaper Elevation="3" Class="page-surface">
                @Body
            </MudPaper>
        </MudContainer>
    </MudMainContent>
</MudLayout>

@code {
    private bool _drawerOpen = false;

    private void ToggleDrawer()
    {
        _drawerOpen = !_drawerOpen;
    }
}
```

**NavMenu.razor (nová komponenta):**

```razor
@inject IAuthStateService AuthState
@inject NavigationManager Navigation

<MudNavMenu>
    <MudNavLink Href="/dashboard" Icon="@Icons.Material.Filled.Dashboard" Match="NavLinkMatch.All">
        Dashboard
    </MudNavLink>
    <MudNavLink Href="/analytics" Icon="@Icons.Material.Filled.Analytics" Match="NavLinkMatch.All">
        Analytics
    </MudNavLink>
    <MudDivider Class="my-2" />
    <MudNavLink OnClick="HandleLogout" Icon="@Icons.Material.Filled.Logout">
        Logout
    </MudNavLink>
</MudNavMenu>

@code {
    private async Task HandleLogout()
    {
        await AuthState.LogoutAsync();
        Navigation.NavigateTo("/login");
    }
}
```

#### Expected LOC

- MainLayout.razor: +15 řádků
- NavMenu.razor: ~40 řádků (nový soubor)
- **Celkem:** ~55 řádků

#### Testing

- [ ] Drawer opens/closes na menu button
- [ ] Navigation links fungují
- [ ] Active state highlighting je viditelný
- [ ] Logout přesměruje na `/login`
- [ ] Mobile responsive (drawer temporary variant)

---

### Task 1.2: Opravit Login/Register Error Feedback

**Problém:** API chyby nejsou uživatelsky zobrazeny → uživatel neví, proč login selhal.

**Řešení:** Structured error display, Snackbar notifications, better validation messages.

#### Implementační kroky

1. **Přidat error state do Login.razor**
   - `string? _errorMessage = null;`
   - `MudAlert` pro zobrazení API errors

2. **Implementovat Snackbar feedback**
   - Success: "Registration successful! Redirecting..."
   - Error: Parse API response a zobrazit

3. **Try-catch v HandleSubmit**
   - Error parsing: `ProblemDetails` → user-friendly message
   - Form disable během submission: `disabled="@isSubmitting"`

#### Code Reference

**Login.razor (upravené části):**

```razor
@inject ISnackbar Snackbar

<!-- Po ValidationSummary přidat -->
@if (!string.IsNullOrWhiteSpace(_errorMessage))
{
    <MudAlert Severity="Severity.Error" Variant="Variant.Outlined" Class="mb-3">
        @_errorMessage
    </MudAlert>
}

<!-- V @code: -->
@code {
    private string? _errorMessage = null;

    private async Task HandleSubmit()
    {
        isSubmitting = true;
        _errorMessage = null; // Reset error
        StateHasChanged();

        try
        {
            if (isLoginMode)
            {
                var response = await AuthApi.LoginAsync(new LoginRequest
                {
                    Email = model.Email,
                    Password = model.Password
                });
                
                await AuthState.SetTokenAsync(response.AccessToken);
                Snackbar.Add("Login successful!", Severity.Success);
                Navigation.NavigateTo("/dashboard");
            }
            else
            {
                var response = await AuthApi.RegisterAsync(new RegisterRequest
                {
                    Email = model.Email,
                    Password = model.Password,
                    DisplayName = model.DisplayName,
                    Language = model.Language
                });
                
                await AuthState.SetTokenAsync(response.AccessToken);
                Snackbar.Add("Registration successful! Welcome!", Severity.Success);
                Navigation.NavigateTo("/dashboard");
            }
        }
        catch (HttpRequestException ex)
        {
            // Parse API error response
            _errorMessage = ParseApiError(ex.Message);
            Snackbar.Add(_errorMessage, Severity.Error);
        }
        catch (Exception ex)
        {
            _errorMessage = "An unexpected error occurred. Please try again.";
            Snackbar.Add(_errorMessage, Severity.Error);
        }
        finally
        {
            isSubmitting = false;
            StateHasChanged();
        }
    }

    private string ParseApiError(string rawError)
    {
        // Simple parsing – enhance based on API error format
        if (rawError.Contains("email", StringComparison.OrdinalIgnoreCase))
            return "Email is already registered or invalid.";
        if (rawError.Contains("password", StringComparison.OrdinalIgnoreCase))
            return "Password does not meet requirements.";
        if (rawError.Contains("401"))
            return "Invalid email or password.";
        
        return "Login failed. Please check your credentials.";
    }
}
```

#### Expected LOC

- Login.razor: +50 řádků (error handling, parsing, Snackbar)
- **Celkem:** ~50 řádků

#### Testing

- [ ] API error zobrazí user-friendly message
- [ ] Snackbar notifikace fungují (success/error)
- [ ] Form se disable během submission
- [ ] Validation messages jsou jasné

---

### Task 1.3: Vylepšit Empty States (EntryList)

**Problém:** Prázdný stav EntryList je nudný → žádná vizuální guidance nebo CTA.

**Řešení:** Ilustrace, velký CTA button, encouraging text.

#### Implementační kroky

1. **Redesign empty state v EntryList.razor**
   - Velká ikona nebo SVG ilustrace
   - CTA button "Create Your First Entry"
   - Krátký vysvětlující text

2. **Event callback na Dashboard**
   - Button volá `OnCreateFirstEntry` callback
   - Dashboard otevře `EntryCreateDialog`

#### Code Reference

**EntryList.razor (empty state):**

```razor
else if (Entries.Count == 0)
{
    <MudStack AlignItems="AlignItems.Center" Spacing="3" Class="pa-8">
        <MudIcon Icon="@Icons.Material.Filled.MusicNote" 
                 Color="Color.Primary" 
                 Style="font-size: 120px; opacity: 0.5;" />
        
        <MudText Typo="Typo.h5" Align="Align.Center">
            No entries yet
        </MudText>
        
        <MudText Typo="Typo.body1" Align="Align.Center" Color="Color.Secondary">
            Start tracking your mood and music journey.<br />
            Your first entry is just a click away!
        </MudText>
        
        <MudButton Variant="Variant.Filled" 
                   Color="Color.Primary" 
                   Size="Size.Large"
                   StartIcon="@Icons.Material.Filled.AddCircle"
                   OnClick="@(() => OnCreateFirstEntry.InvokeAsync())">
            Create Your First Entry
        </MudButton>
    </MudStack>
}

@code {
    [Parameter]
    public EventCallback OnCreateFirstEntry { get; set; }
    
    // ... existing code
}
```

**Dashboard.razor (callback handling):**

```razor
<EntryList @ref="_entryList" 
           OnEditEntry="OpenEditDialog" 
           OnEntryChanged="RefreshData"
           OnCreateFirstEntry="OpenCreateDialog" />

@code {
    // OpenCreateDialog už existuje, jen přidat callback
}
```

#### Expected LOC

- EntryList.razor: +25 řádků (empty state redesign)
- Dashboard.razor: +1 řádek (callback)
- **Celkem:** ~26 řádků

#### Testing

- [ ] Empty state je vizuálně přitažlivý
- [ ] CTA button funguje (otevře dialog)
- [ ] Text je encouraging a jasný

---

### Task 1.4: Napojit Dashboard na Reálná Data

**Problém:** Dashboard statistiky jsou hardcoded → klamavé UX.

**Řešení:** Integrace `IAnalyticsService`, loading states, error handling.

#### Implementační kroky

1. **Inject analytics service do Dashboard.razor**
   - `@inject IAnalyticsApiService AnalyticsApi`

2. **Fetch data v OnInitializedAsync**
   - `GetMoodTrends(daysLookback: 7)`
   - `GetMusicStatistics()`

3. **Loading skeleton**
   - `bool _isLoadingStats = true;`
   - `MudSkeleton` pro stats cards

4. **Error handling**
   - Fallback pro chybné requesty
   - Retry button

#### Code Reference

**Dashboard.razor (data integration):**

```razor
@inject IAnalyticsApiService AnalyticsApi

<!-- Stats cards (updated) -->
<MudGrid Class="mb-4" Spacing="2">
    @if (_isLoadingStats)
    {
        <MudItem xs="12" sm="6" md="4">
            <MudSkeleton SkeletonType="SkeletonType.Rectangle" Height="120px" />
        </MudItem>
        <MudItem xs="12" sm="6" md="4">
            <MudSkeleton SkeletonType="SkeletonType.Rectangle" Height="120px" />
        </MudItem>
        <MudItem xs="12" sm="6" md="4">
            <MudSkeleton SkeletonType="SkeletonType.Rectangle" Height="120px" />
        </MudItem>
    }
    else if (_moodTrend != null)
    {
        <MudItem xs="12" sm="6" md="4">
            <MudPaper Elevation="3" Class="pa-4" Style="background: linear-gradient(135deg,#0ea5e9,#38bdf8); color: #0b1120;">
                <MudText Typo="Typo.caption" Class="mb-1">Mood balance</MudText>
                <MudText Typo="Typo.h5">Valence: @_moodTrend.AverageValence.ToString("F2")</MudText>
                <MudText Typo="Typo.body2">Averaged last 7 days</MudText>
            </MudPaper>
        </MudItem>
        <MudItem xs="12" sm="6" md="4">
            <MudPaper Elevation="3" Class="pa-4" Style="background: linear-gradient(135deg,#a855f7,#c084fc); color: #0b1120;">
                <MudText Typo="Typo.caption" Class="mb-1">Energy trend</MudText>
                <MudText Typo="Typo.h5">Arousal: @_moodTrend.AverageArousal.ToString("F2")</MudText>
                <MudText Typo="Typo.body2">Last @_moodTrend.TotalEntries sessions</MudText>
            </MudPaper>
        </MudItem>
        <MudItem xs="12" sm="6" md="4">
            <MudPaper Elevation="3" Class="pa-4" Style="background: linear-gradient(135deg,#22c55e,#86efac); color: #0b1120;">
                <MudText Typo="Typo.caption" Class="mb-1">Scrobbles</MudText>
                <MudText Typo="Typo.h5">Synced: @(_musicStats?.TotalScrobbles ?? 0)</MudText>
                <MudText Typo="Typo.body2">@(_musicStats != null ? "Last 30 days" : "Connect Last.fm to start")</MudText>
            </MudPaper>
        </MudItem>
    }
</MudGrid>

@code {
    private bool _isLoadingStats = true;
    private MoodTrendDto? _moodTrend;
    private MusicStatisticsDto? _musicStats;

    protected override async Task OnInitializedAsync()
    {
        var isAuthenticated = await AuthState.IsAuthenticatedAsync();
        if (!isAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        await LoadStatsAsync();
        // ... existing Last.fm callback handling
    }

    private async Task LoadStatsAsync()
    {
        _isLoadingStats = true;
        StateHasChanged();

        try
        {
            var moodTask = AnalyticsApi.GetMoodTrendsAsync(daysLookback: 7);
            var musicTask = AnalyticsApi.GetMusicStatisticsAsync();

            await Task.WhenAll(moodTask, musicTask);

            _moodTrend = await moodTask;
            _musicStats = await musicTask;
        }
        catch (Exception ex)
        {
            Snackbar.Add("Failed to load statistics. Please try again.", Severity.Warning);
            // Fallback: show empty state or retry button
        }
        finally
        {
            _isLoadingStats = false;
            StateHasChanged();
        }
    }

    private async Task RefreshData()
    {
        await LoadStatsAsync();
        await _entryList.LoadEntriesAsync();
    }
}
```

**IAnalyticsApiService.cs (frontend service interface):**

```csharp
public interface IAnalyticsApiService
{
    Task<MoodTrendDto> GetMoodTrendsAsync(int daysLookback = 30, CancellationToken cancellationToken = default);
    Task<MusicStatisticsDto> GetMusicStatisticsAsync(CancellationToken cancellationToken = default);
}

public class AnalyticsApiService(HttpClient httpClient) : IAnalyticsApiService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<MoodTrendDto> GetMoodTrendsAsync(int daysLookback = 30, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/analytics/mood-trends?daysLookback={daysLookback}", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MoodTrendDto>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("Failed to deserialize mood trends");
    }

    public async Task<MusicStatisticsDto> GetMusicStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("api/analytics/music-stats", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MusicStatisticsDto>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("Failed to deserialize music statistics");
    }
}
```

**Program.cs (registrace service):**

```csharp
builder.Services.AddScoped<IAnalyticsApiService, AnalyticsApiService>();
```

#### Expected LOC

- Dashboard.razor: +60 řádků (data fetching, loading states)
- AnalyticsApiService.cs: ~30 řádků (nový soubor)
- Program.cs: +1 řádek (DI registrace)
- **Celkem:** ~91 řádků

#### Testing

- [ ] Stats cards zobrazují reálná data
- [ ] Loading skeleton funguje
- [ ] Error handling pro failed requests
- [ ] Refresh data po vytvoření entry

---

## 🎨 Fáze 2: UX Enhancements (3-5 dní)

**Priorita:** 🟡 Vysoká  
**Impact:** Quality of life improvements  
**Časový odhad:** 3-5 dní (senior dev)

---

### Task 2.1: Accessibility v MoodSelector2D

**Problém:** Pouze pointer events → nepoužitelné pro keyboard navigation, screen readers, touch devices.

**Řešení:** ARIA attributes, keyboard navigation, touch events, focus indicator.

#### Implementační kroky

1. **ARIA attributes**
   - `role="slider"` na `.mood-plane`
   - `aria-valuemin="-1"`, `aria-valuemax="1"`
   - `aria-valuenow="@Valence"` (pro valence), samostatný pro arousal
   - `aria-label="Mood selector: valence and arousal"`

2. **Keyboard navigation**
   - `@onkeydown="HandleKeyDown"`
   - Arrow keys: ←→ pro valence, ↑↓ pro arousal
   - Step: 0.1 na jednu klávesu

3. **Touch events**
   - `@ontouchstart="OnTouchStart"`
   - `@ontouchmove="OnTouchMove"`
   - `@ontouchend="OnTouchEnd"`

4. **Focus indicator**
   - CSS `:focus-visible` ring
   - `tabindex="0"` na `.mood-plane`

#### Code Reference

**MoodSelector2D.razor (accessibility enhancements):**

```razor
<MudPaper Class="mood-plane" 
          Elevation="2" 
          Style=$"width:{Size}px;height:{Size}px;"
          role="slider"
          aria-label="Mood selector: valence and arousal"
          aria-valuemin="-1"
          aria-valuemax="1"
          aria-valuenow="@Valence"
          tabindex="0"
          @onpointerdown="OnPointerDown"
          @onpointermove="OnPointerMove"
          @onpointerup="StopDragging"
          @onpointerleave="StopDragging"
          @ontouchstart="OnTouchStart"
          @ontouchmove="OnTouchMove"
          @ontouchend="OnTouchEnd"
          @onkeydown="HandleKeyDown">
    <!-- ... existing content ... -->
</MudPaper>

@code {
    // Touch event handlers
    private async Task OnTouchStart(TouchEventArgs e)
    {
        if (e.Touches.Length > 0)
        {
            _isDragging = true;
            await UpdateFromTouchAsync(e.Touches[0]);
        }
    }

    private async Task OnTouchMove(TouchEventArgs e)
    {
        if (!_isDragging || e.Touches.Length == 0)
            return;
        
        await UpdateFromTouchAsync(e.Touches[0]);
    }

    private void OnTouchEnd(TouchEventArgs e)
    {
        _isDragging = false;
    }

    private async Task UpdateFromTouchAsync(TouchPoint touch)
    {
        // Calculate relative position within the plane
        var x = touch.ClientX - touch.OffsetX; // Adjust for component offset
        var y = touch.ClientY - touch.OffsetY;
        
        var valence = Math.Clamp((x / Size) * 2 - 1, -1, 1);
        var arousal = Math.Clamp(1 - (y / Size) * 2, -1, 1);
        
        await SetValuesAsync(valence, arousal);
    }

    // Keyboard navigation
    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        const double step = 0.1;
        var newValence = Valence;
        var newArousal = Arousal;

        switch (e.Key)
        {
            case "ArrowLeft":
                newValence = Math.Clamp(Valence - step, -1, 1);
                break;
            case "ArrowRight":
                newValence = Math.Clamp(Valence + step, -1, 1);
                break;
            case "ArrowUp":
                newArousal = Math.Clamp(Arousal + step, -1, 1);
                break;
            case "ArrowDown":
                newArousal = Math.Clamp(Arousal - step, -1, 1);
                break;
            default:
                return; // No action for other keys
        }

        await SetValuesAsync(newValence, newArousal);
        e.PreventDefault(); // Prevent page scroll
    }
}
```

**CSS (focus indicator):**

```css
.mood-plane:focus-visible {
    outline: 3px solid var(--mud-palette-primary);
    outline-offset: 2px;
}
```

#### Expected LOC

- MoodSelector2D.razor: +100 řádků (touch + keyboard + ARIA)
- CSS: +4 řádky
- **Celkem:** ~104 řádků

#### Testing

- [ ] Keyboard navigation funguje (arrow keys)
- [ ] Touch events fungují na mobile/tablet
- [ ] ARIA attributes jsou správně nastavené (axe DevTools)
- [ ] Focus indicator je viditelný

---

### Task 2.2: Music Search Debounce

**Problém:** Search pouze na Enter → pomalé UX.

**Řešení:** Auto-search s 300ms debounce, cancel předchozího requestu.

#### Implementační kroky

1. **Debounce timer**
   - `System.Threading.Timer` nebo `Task.Delay` pattern
   - 300ms delay

2. **Cancel previous request**
   - `CancellationTokenSource` pro každý search
   - Cancel při novém keystroke

3. **Min. 3 znaky validation**
   - Nespouštět search pro kratší queries

4. **Loading indicator v textboxu**
   - `Adornment` s `MudProgressCircular`

#### Code Reference

**MusicSearchBox.razor (debounce implementation):**

```razor
@using System.Threading

<MudTextField @bind-Value="_query"
              Label="Search music"
              Variant="Variant.Outlined"
              Margin="Margin.Dense"
              Adornment="Adornment.End"
              AdornmentIcon="@(_isSearching ? null : Icons.Material.Filled.Search)"
              OnAdornmentClick="SearchAsync"
              Immediate="true"
              OnBlur="OnQueryChanged"
              OnKeyDown="HandleKeyDown">
    @if (_isSearching)
    {
        <MudProgressCircular Size="Size.Small" Indeterminate />
    }
</MudTextField>

@code {
    private string _query = string.Empty;
    private bool _isSearching;
    private CancellationTokenSource? _searchCts;
    private Timer? _debounceTimer;

    private void OnQueryChanged()
    {
        // Cancel existing timer
        _debounceTimer?.Dispose();
        
        // Cancel previous search
        _searchCts?.Cancel();
        
        if (string.IsNullOrWhiteSpace(_query) || _query.Length < 3)
        {
            // Clear results if query too short
            Results.Clear();
            StateHasChanged();
            return;
        }

        // Start new debounce timer
        _debounceTimer = new Timer(_ =>
        {
            InvokeAsync(async () => await SearchAsync());
        }, null, 300, Timeout.Infinite);
    }

    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(_query) || _query.Length < 3)
            return;

        // Cancel previous search
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();

        _isSearching = true;
        StateHasChanged();

        try
        {
            Results = await MusicSearchApi.SearchAsync(_query, _searchCts.Token);
        }
        catch (OperationCanceledException)
        {
            // Search was cancelled (new search started)
        }
        catch (Exception ex)
        {
            Snackbar.Add("Search failed. Please try again.", Severity.Error);
        }
        finally
        {
            _isSearching = false;
            StateHasChanged();
        }
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            _debounceTimer?.Dispose(); // Cancel debounce
            await SearchAsync();
        }
    }

    public void Dispose()
    {
        _debounceTimer?.Dispose();
        _searchCts?.Cancel();
        _searchCts?.Dispose();
    }
}
```

#### Expected LOC

- MusicSearchBox.razor: +80 řádků (debounce logic, cancellation)
- **Celkem:** ~80 řádků

#### Testing

- [ ] Search spouští automaticky po 300ms
- [ ] Previous requests jsou cancelled
- [ ] Min. 3 znaky validation funguje
- [ ] Loading indicator se zobrazuje

---

### Task 2.3: Multi-Step Entry Create Dialog

**Problém:** Formulář je příliš dlouhý → cognitive overload.

**Řešení:** Rozdělit na 3 kroky pomocí `MudStepper`.

#### Implementační kroky

1. **Refactor na MudStepper**
   - `<MudStepper @ref="_stepper">`
   - 3× `<MudStep>`

2. **Krok 1: Music Search**
   - MusicSearchBox
   - Title, Artist, Album fields

3. **Krok 2: Mood Selection**
   - MoodSelector2D
   - Tension slider

4. **Krok 3: Tags & Notes**
   - Somatic tags
   - Custom tag input
   - Notes textarea

5. **Navigation buttons**
   - Back (krok 2-3)
   - Next (krok 1-2)
   - Submit (krok 3)

#### Code Reference

**EntryCreateDialog.razor (refactored):**

```razor
<MudDialog>
    <DialogContent>
        <MudStepper @ref="_stepper" Linear="true">
            <!-- Step 1: Music Search -->
            <MudStep Title="Select Music" Icon="@Icons.Material.Filled.MusicNote">
                <MudStack Spacing="2">
                    <MusicSearchBox @bind-SelectedTrack="_selectedTrack" />
                    
                    @if (_selectedTrack != null)
                    {
                        <MudTextField @bind-Value="_model.SongTitle" Label="Song Title" Variant="Variant.Outlined" />
                        <MudTextField @bind-Value="_model.ArtistName" Label="Artist" Variant="Variant.Outlined" />
                        <MudTextField @bind-Value="_model.AlbumName" Label="Album (optional)" Variant="Variant.Outlined" />
                    }
                </MudStack>
            </MudStep>

            <!-- Step 2: Mood Selection -->
            <MudStep Title="Set Mood" Icon="@Icons.Material.Filled.Mood">
                <MudStack Spacing="3">
                    <MoodSelector2D @bind-Valence="_model.Valence" 
                                    @bind-Arousal="_model.Arousal" />
                    
                    <MudSlider @bind-Value="_model.Tension" 
                               Min="-1" 
                               Max="1" 
                               Step="0.1" 
                               Color="Color.Primary">
                        Tension: @_model.Tension.ToString("F1")
                    </MudSlider>
                </MudStack>
            </MudStep>

            <!-- Step 3: Tags & Notes -->
            <MudStep Title="Add Details" Icon="@Icons.Material.Filled.Note">
                <MudStack Spacing="2">
                    <MudText Typo="Typo.subtitle2">Somatic sensations</MudText>
                    <MudChipSet @bind-SelectedValues="_selectedSomaticTags" MultiSelection Filter>
                        @foreach (var tag in _somaticTags)
                        {
                            <MudChip T="string" Value="@tag">@tag</MudChip>
                        }
                    </MudChipSet>
                    
                    <MudTextField @bind-Value="_customTag" 
                                  Label="Add custom tag" 
                                  Variant="Variant.Outlined"
                                  Adornment="Adornment.End"
                                  AdornmentIcon="@Icons.Material.Filled.Add"
                                  OnAdornmentClick="AddCustomTag" />
                    
                    <MudTextField @bind-Value="_model.Notes" 
                                  Label="Notes (optional)" 
                                  Variant="Variant.Outlined" 
                                  Lines="4" />
                </MudStack>
            </MudStep>
        </MudStepper>
    </DialogContent>

    <DialogActions>
        <MudButton OnClick="Cancel">Cancel</MudButton>
        
        @if (_stepper.ActiveIndex > 0)
        {
            <MudButton OnClick="() => _stepper.Previous()">Back</MudButton>
        }
        
        @if (_stepper.ActiveIndex < 2)
        {
            <MudButton Color="Color.Primary" OnClick="() => _stepper.Next()">Next</MudButton>
        }
        else
        {
            <MudButton Color="Color.Primary" 
                       Variant="Variant.Filled" 
                       OnClick="HandleSubmit"
                       Disabled="@_isSubmitting">
                @if (_isSubmitting)
                {
                    <MudProgressCircular Size="Size.Small" Indeterminate Class="mr-2" />
                }
                Create Entry
            </MudButton>
        }
    </DialogActions>
</MudDialog>

@code {
    private MudStepper _stepper = null!;
    
    // ... existing code
}
```

#### Expected LOC

- EntryCreateDialog.razor: +150 řádků (refactor + stepper logic)
- **Celkem:** ~150 řádků

#### Testing

- [ ] Stepper navigation funguje (Next/Back)
- [ ] Data persist mezi kroky
- [ ] Submit pouze na posledním kroku
- [ ] Validation per step

---

### Task 2.4: Analytics Charts Integration

**Problém:** Pouze raw čísla → data nejsou vizuální.

**Řešení:** Přidat `Blazor.ChartJS` pro line chart, bar chart, donut chart.

#### Implementační kroky

1. **NuGet package install**
   - `PSC.Blazor.Components.Chartjs` (nebo `ApexCharts.Blazor`)

2. **Line chart: Mood trends over time**
   - X axis: dates
   - Y axis dual: valence (blue), arousal (purple)

3. **Bar chart: Top artists/songs**
   - Horizontal bar chart

4. **Donut chart: Mood quadrants distribution**
   - 4 segments: Happy, Calm, Tense, Down

#### Code Reference

**Analytics.razor (charts integration):**

```razor
@using PSC.Blazor.Components.Chartjs
@using PSC.Blazor.Components.Chartjs.Models.Line

@inject IAnalyticsApiService AnalyticsApi

<MudContainer MaxWidth="MaxWidth.Large" Class="mt-6">
    <MudText Typo="Typo.h4" Class="mb-4">Analytics Dashboard</MudText>

    @if (_isLoading)
    {
        <MudProgressCircular Indeterminate />
    }
    else if (_moodTrend != null)
    {
        <MudGrid Spacing="3">
            <!-- Line Chart: Mood Trends -->
            <MudItem xs="12" md="8">
                <MudPaper Elevation="2" Class="pa-4">
                    <MudText Typo="Typo.h6" Class="mb-3">Mood Trends (Last 30 Days)</MudText>
                    <Chart Config="_lineChartConfig" @ref="_lineChart" />
                </MudPaper>
            </MudItem>

            <!-- Donut Chart: Mood Distribution -->
            <MudItem xs="12" md="4">
                <MudPaper Elevation="2" Class="pa-4">
                    <MudText Typo="Typo.h6" Class="mb-3">Mood Distribution</MudText>
                    <Chart Config="_donutChartConfig" @ref="_donutChart" />
                </MudPaper>
            </MudItem>

            <!-- Bar Chart: Top Artists -->
            <MudItem xs="12">
                <MudPaper Elevation="2" Class="pa-4">
                    <MudText Typo="Typo.h6" Class="mb-3">Top Artists</MudText>
                    <Chart Config="_barChartConfig" @ref="_barChart" />
                </MudPaper>
            </MudItem>
        </MudGrid>
    }
</MudContainer>

@code {
    private bool _isLoading = true;
    private MoodTrendDto? _moodTrend;
    private MusicStatisticsDto? _musicStats;

    private Chart _lineChart = null!;
    private Chart _donutChart = null!;
    private Chart _barChart = null!;

    private LineChartConfig _lineChartConfig = null!;
    private DoughnutChartConfig _donutChartConfig = null!;
    private BarChartConfig _barChartConfig = null!;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
        BuildCharts();
    }

    private async Task LoadDataAsync()
    {
        _isLoading = true;
        try
        {
            _moodTrend = await AnalyticsApi.GetMoodTrendsAsync(daysLookback: 30);
            _musicStats = await AnalyticsApi.GetMusicStatisticsAsync();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void BuildCharts()
    {
        if (_moodTrend == null) return;

        // Line Chart: Mood Trends
        _lineChartConfig = new LineChartConfig
        {
            Options = new LineChartOptions
            {
                Responsive = true,
                Scales = new Scales
                {
                    Y = new LinearAxis { BeginAtZero = false, Min = -1, Max = 1 }
                }
            }
        };

        var valenceLine = new LineDataset
        {
            Label = "Valence",
            Data = _moodTrend.DailyAverages.Select(d => d.Valence).ToList(),
            BorderColor = "#3b82f6",
            BackgroundColor = "rgba(59, 130, 246, 0.1)"
        };

        var arousalLine = new LineDataset
        {
            Label = "Arousal",
            Data = _moodTrend.DailyAverages.Select(d => d.Arousal).ToList(),
            BorderColor = "#a855f7",
            BackgroundColor = "rgba(168, 85, 247, 0.1)"
        };

        _lineChartConfig.Data.Labels = _moodTrend.DailyAverages.Select(d => d.Date.ToString("MMM d")).ToList();
        _lineChartConfig.Data.Datasets.Add(valenceLine);
        _lineChartConfig.Data.Datasets.Add(arousalLine);

        // Donut Chart: Mood Distribution
        _donutChartConfig = new DoughnutChartConfig
        {
            Options = new DoughnutChartOptions { Responsive = true }
        };

        var donutDataset = new DoughnutDataset
        {
            Data = [
                _moodTrend.Distribution.Happy,
                _moodTrend.Distribution.Calm,
                _moodTrend.Distribution.Tense,
                _moodTrend.Distribution.Down
            ],
            BackgroundColor = ["#22c55e", "#3b82f6", "#f97316", "#6366f1"]
        };

        _donutChartConfig.Data.Labels = ["Happy", "Calm", "Tense", "Down"];
        _donutChartConfig.Data.Datasets.Add(donutDataset);

        // Bar Chart: Top Artists (if music stats available)
        if (_musicStats?.TopArtists != null)
        {
            _barChartConfig = new BarChartConfig
            {
                Options = new BarChartOptions { Responsive = true, IndexAxis = "y" }
            };

            var barDataset = new BarDataset
            {
                Label = "Play Count",
                Data = _musicStats.TopArtists.Select(a => (double)a.PlayCount).ToList(),
                BackgroundColor = "#8b5cf6"
            };

            _barChartConfig.Data.Labels = _musicStats.TopArtists.Select(a => a.Name).ToList();
            _barChartConfig.Data.Datasets.Add(barDataset);
        }
    }
}
```

**MIMM.Frontend.csproj (package reference):**

```xml
<PackageReference Include="PSC.Blazor.Components.Chartjs" Version="7.0.0" />
```

#### Expected LOC

- Analytics.razor: +200 řádků (charts setup + data transformation)
- MIMM.Frontend.csproj: +1 řádek (package)
- **Celkem:** ~201 řádků

#### Testing

- [ ] Line chart zobrazuje mood trends
- [ ] Donut chart ukazuje distribuci
- [ ] Bar chart zobrazuje top artists
- [ ] Charts jsou responsive

---

## 🎨 Fáze 3: Polish & Advanced Features (5-7 dní)

**Priorita:** 🟢 Střední  
**Impact:** Professional polish  
**Časový odhad:** 5-7 dní (senior dev)

---

### Task 3.1: Typografická Hierarchie

**Cíl:** CSS variables pro font sizes, konzistentní usage.

#### Code Reference

**wwwroot/css/app.css:**

```css
:root {
  /* Typography scale */
  --text-xs: 0.75rem;    /* 12px - caption */
  --text-sm: 0.875rem;   /* 14px - body2 */
  --text-base: 1rem;     /* 16px - body1 */
  --text-lg: 1.125rem;   /* 18px - subtitle */
  --text-xl: 1.25rem;    /* 20px - h6 */
  --text-2xl: 1.5rem;    /* 24px - h5 */
  --text-3xl: 1.875rem;  /* 30px - h4 */
  --text-4xl: 2.25rem;   /* 36px - h3 */
  --text-5xl: 3rem;      /* 48px - h2 */

  /* Line heights */
  --leading-tight: 1.25;
  --leading-normal: 1.5;
  --leading-relaxed: 1.75;
}

/* Typography utility classes */
.text-xs { font-size: var(--text-xs); }
.text-sm { font-size: var(--text-sm); }
.text-base { font-size: var(--text-base); }
.text-lg { font-size: var(--text-lg); }
.text-xl { font-size: var(--text-xl); }
.text-2xl { font-size: var(--text-2xl); }
.text-3xl { font-size: var(--text-3xl); }
.text-4xl { font-size: var(--text-4xl); }
.text-5xl { font-size: var(--text-5xl); }
```

---

### Task 3.2: Custom Color System

**Cíl:** Brand color tokens v CSS variables.

#### Code Reference

```css
:root {
  /* Brand colors */
  --color-primary: #3b82f6;
  --color-secondary: #8b5cf6;
  --color-accent: #10b981;
  --color-danger: #ef4444;
  --color-warning: #f59e0b;
  
  /* Mood-specific colors */
  --mood-happy: #22c55e;
  --mood-calm: #3b82f6;
  --mood-angry: #f97316;
  --mood-sad: #6366f1;
  
  /* Gradients */
  --gradient-bg: radial-gradient(circle at 20% 20%, #1e3a8a 0, transparent 30%),
                 radial-gradient(circle at 80% 0%, #7c3aed 0, transparent 35%),
                 linear-gradient(135deg, #0b1120 0%, #0f172a 50%, #111827 100%);
}
```

---

### Task 3.3: Micro-interactions

**Cíl:** Hover animations, transitions, ripples.

#### Code Reference

```css
/* Smooth transitions */
* {
  transition: color 0.2s, background-color 0.2s, border-color 0.2s;
}

/* Button hover */
.mud-button:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
  transition: transform 0.2s, box-shadow 0.2s;
}

/* Card hover */
.mud-card:hover {
  box-shadow: 0 8px 24px rgba(0,0,0,0.12);
  transition: box-shadow 0.3s;
}

/* Focus states */
*:focus-visible {
  outline: 2px solid var(--color-primary);
  outline-offset: 2px;
  border-radius: 4px;
}
```

---

### Task 3.4: Light/Dark Mode Toggle

**Cíl:** Theme switching s MudThemeProvider.

#### Implementační kroky

1. **Define custom MudTheme instances**
   - Dark theme (current)
   - Light theme (new)

2. **ThemeService pro state management**
   - Toggle method
   - Persist v localStorage

3. **Toggle button v AppBar**
   - `MudIconButton` s `@Icons.Material.Filled.LightMode` / `DarkMode`

#### Code Reference

**App.razor:**

```razor
<MudThemeProvider @ref="_themeProvider" Theme="_currentTheme" />

@code {
    private MudThemeProvider _themeProvider = null!;
    private MudTheme _currentTheme = new MudTheme(); // Default dark

    private MudTheme _darkTheme = new MudTheme
    {
        Palette = new PaletteLight
        {
            Primary = "#3b82f6",
            Secondary = "#8b5cf6",
            Background = "#0f172a",
            Surface = "#1e293b"
        }
    };

    private MudTheme _lightTheme = new MudTheme
    {
        Palette = new PaletteLight
        {
            Primary = "#3b82f6",
            Secondary = "#8b5cf6",
            Background = "#ffffff",
            Surface = "#f8fafc"
        }
    };

    protected override async Task OnInitializedAsync()
    {
        // Load theme preference from localStorage
        var isDark = await JS.InvokeAsync<bool>("localStorage.getItem", "isDarkMode");
        _currentTheme = isDark ? _darkTheme : _lightTheme;
    }
}
```

---

### Task 3.5: Advanced Responsive Optimizations

**Cíl:** Touch-friendly targets, iOS safe area, tablet layouts.

#### Code Reference

```css
/* Touch-friendly targets */
.mud-button,
.mud-icon-button {
  min-width: 44px;
  min-height: 44px;
}

/* iOS safe area */
@supports (padding: max(0px)) {
  .page-container {
    padding-left: max(16px, env(safe-area-inset-left));
    padding-right: max(16px, env(safe-area-inset-right));
    padding-bottom: max(16px, env(safe-area-inset-bottom));
  }
}

/* Tablet landscape optimizations */
@media (min-width: 768px) and (orientation: landscape) {
  .mud-drawer {
    width: 280px;
  }
  
  .mood-plane {
    max-width: 400px;
  }
}
```

---

## 📊 Testing & Validation Checklist

### Fáze 1 (Kritické)

- [ ] **Navigation:** Drawer funguje, active state highlighting
- [ ] **Login/Register:** Error messages zobrazeny, Snackbar notifikace
- [ ] **Empty States:** CTA button funkční, vizuálně přitažlivé
- [ ] **Dashboard Data:** Stats zobrazují reálná data, loading states

### Fáze 2 (UX Enhancements)

- [ ] **Accessibility:** Keyboard nav, ARIA labels, touch events
- [ ] **Debounce:** Auto-search funguje, cancel previous requests
- [ ] **Multi-Step Dialog:** Stepper navigation, data persist
- [ ] **Analytics Charts:** Všechny 3 charts zobrazují data

### Fáze 3 (Polish)

- [ ] **Typography:** Font scale konzistentní
- [ ] **Colors:** Brand tokens použité všude
- [ ] **Micro-interactions:** Hover animations smooth
- [ ] **Theme Toggle:** Light/Dark mode switch funguje
- [ ] **Responsive:** Touch targets 44x44px, iOS safe area

### Accessibility Audit

- [ ] **axe DevTools:** 0 errors, <5 warnings
- [ ] **WAVE:** No errors
- [ ] **Keyboard Navigation:** Všechny interaktivní elementy dostupné
- [ ] **Screen Reader:** ARIA labels správně nastavené

### Performance

- [ ] **Lighthouse Mobile Score:** >90/100
- [ ] **Bundle Size:** <500KB (gzip)
- [ ] **Time to Interactive:** <3s

---

## 🚢 Deployment & Rollout

### Pre-Release Checklist

1. **Code Review:**
   - [ ] Všechny PR mají minimálně 1 approval
   - [ ] CI/CD pipeline prošla (testy + lint)

2. **Testing:**
   - [ ] Manuální testing na 3 zařízeních (desktop, tablet, mobile)
   - [ ] Cross-browser testing (Chrome, Firefox, Safari, Edge)
   - [ ] E2E tests updated (Playwright/Cypress)

3. **Documentation:**
   - [ ] CHANGELOG.md updated
   - [ ] User Guide updated (nové features)
   - [ ] Developer Guide updated (nové komponenty)

4. **Version Bump:**
   - [ ] README.md version badge
   - [ ] AGENTS.md (pokud architektua změny)
   - [ ] Git tag: `v2.1.0` (příklad)

### Rollout Fáze

**Week 1:** Fáze 1 (kritické fixes) → staging deploy  
**Week 2:** Fáze 2 (UX enhancements) → internal beta  
**Week 3-4:** Fáze 3 (polish) → production deploy

---

## 📚 Appendix: Design System Reference

### Spacing Scale (8px Grid)

```css
--space-1: 0.25rem;  /* 4px */
--space-2: 0.5rem;   /* 8px */
--space-3: 0.75rem;  /* 12px */
--space-4: 1rem;     /* 16px */
--space-6: 1.5rem;   /* 24px */
--space-8: 2rem;     /* 32px */
--space-12: 3rem;    /* 48px */
```

### Border Radius

```css
--radius-sm: 0.375rem;  /* 6px */
--radius-md: 0.5rem;    /* 8px */
--radius-lg: 0.75rem;   /* 12px */
--radius-xl: 1rem;      /* 16px */
```

### Shadow System

```css
--shadow-sm: 0 1px 2px rgba(0,0,0,0.05);
--shadow-md: 0 4px 6px rgba(0,0,0,0.1);
--shadow-lg: 0 10px 15px rgba(0,0,0,0.1);
--shadow-xl: 0 20px 25px rgba(0,0,0,0.15);
```

---

## 🎯 Konec Plánu

**Next Steps:**

1. Začít implementací **Fáze 1** (kritické fixes) – 1-2 dny
2. Code review každého tasku před merge
3. Update tohoto dokumentu s progress notes
4. Po dokončení Fáze 1 → přejít na Fáze 2

**Contact:** Pro otázky nebo clarifikace, ping @MIMM-Expert-Agent nebo @MIMM-UXUI-Specialist-Agent.

---

**Dokument vytvořil:** MIMM-Expert-Agent  
**Datum:** 26. ledna 2026  
**Verze:** 1.0  
**Status:** ✅ Ready for Implementation
