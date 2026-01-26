# MIMM 2.0 – UX/UI Analýza & Návrh Budoucích Kroků

**Datum:** 26. ledna 2026  
**Agent:** UX/UI Specialist  
**Status:** Analytická fáze (bez implementace)

---

## 📊 Současný stav UI

**Technologie:**

- Blazor WebAssembly + MudBlazor 7.0
- CSS custom properties (minimal)
- Glassmorphism design pattern (dark theme)
- Responsive grid layout (MudGrid)

**Komponenty:** 7 UI komponent + 3 pages

### Přehled Razor Komponent

1. **MainLayout.razor** - Hlavní layout s AppBar
2. **Dashboard.razor** - Úvodní stránka s přehledem
3. **Login.razor** - Login/Register formulář
4. **Analytics.razor** - Analytický dashboard
5. **EntryList.razor** - Seznam mood záznamů
6. **EntryCreateDialog.razor** - Dialog pro vytvoření záznamu
7. **EntryEditDialog.razor** - Dialog pro editaci záznamu
8. **MoodSelector2D.razor** - 2D selector pro valence/arousal
9. **MusicSearchBox.razor** - Vyhledávání hudby
10. **MusicTrackCard.razor** - Zobrazení hudební skladby
11. **ConfirmDialog.razor** - Potvrzovací dialog

---

## ✅ Co Funguje Dobře (Strengths)

### 1. Konzistentní Design System

- MudBlazor poskytuje solidní základ (typography, spacing, color tokens)
- Glassmorphism efekt (backdrop-filter) je elegantní a moderní
- Dark theme s gradient background je vizuálně přitažlivý

```css
/* Současný gradient background */
background: radial-gradient(circle at 20% 20%, #1e3a8a 0, transparent 30%),
            radial-gradient(circle at 80% 0%, #7c3aed 0, transparent 35%),
            linear-gradient(135deg, #0b1120 0%, #0f172a 50%, #111827 100%);
```

### 2. Dobrá Struktura Komponent

- Komponenty jsou atomické a znovupoužitelné
- Clear separation of concerns (MusicTrackCard, MoodSelector2D)
- Event callbacks správně implementovány
- Parameter binding s `@bind-Value` je čistý

### 3. Mood Selector 2D – Inovativní

- Interaktivní 2D grid pro valence/arousal je unikátní
- Drag-and-drop UX je intuitivní
- Živé labely s hodnotami (V/A koordináty)
- Scoped CSS styles přímo v komponentě

```razor
<!-- Příklad dobré implementace -->
<div class="cursor" style="left:@CursorLeft%; top:@CursorTop%"></div>
```

### 4. Responsivní Layout

- `MudGrid` s breakpoints (xs/sm/md)
- Mobile-first approach
- Správné použití `MudContainer MaxWidth="MaxWidth.Large"`

---

## ⚠️ Identifikované UX/UI Problémy

### 🔴 **Kritické (Blokují UX)**

#### Problém 1: Chybějící Navigace v MainLayout

**Současný stav:**

```razor
<MudAppBar Elevation="0" Dense Color="Color.Transparent" Class="glass-bar">
    <MudIconButton Icon="@Icons.Material.Filled.Menu" Color="Color.Inherit" Edge="Edge.Start" />
    <MudText Typo="Typo.h6" Class="brand">MIMM 2.0</MudText>
    <MudSpacer />
    <MudIconButton Icon="@Icons.Material.Filled.Person" Color="Color.Inherit" />
</MudAppBar>
```

**Problém:**

- Žádná funkční navigace mezi stránkami (Dashboard, Analytics, Login)
- Menu ikona není napojena na drawer
- Uživatel se nemůže pohybovat v aplikaci

**Dopad:** Blokující – aplikace je nepoužitelná pro navigaci

**Návrh řešení:**

- Přidat `MudDrawer` s navigation links
- Implementovat toggle drawer na menu button
- Active state highlighting pro current page

---

#### Problém 2: Login/Register – Chybějící Vizuální Zpětná Vazba

**Současný stav:**

```razor
@if (isSubmitting)
{
    <MudProgressCircular Size="Size.Small" Indeterminate="true" Class="mr-2" />
    <span>Processing...</span>
}
```

**Problémy:**

- Žádný error message display (pouze `<ValidationSummary />`)
- Snackbar feedback chybí pro úspěch/chybu registrace
- API chyby se nezobrazují uživatelsky

**Dopad:** Uživatelé neví, proč login selhal

**Návrh řešení:**

- Přidat structured error display
- Snackbar notifications (success/error)
- Better validation messages

---

#### Problém 3: Entry List – Prázdný Stav Je Nudný

**Současný stav:**

```razor
<MudAlert Severity="Severity.Info" Variant="Variant.Outlined">
    No entries yet. Create your first mood log!
</MudAlert>
```

**Problém:**

- Chybí vizuální guidance (ilustrace, CTA button)
- První uživatelé neví, co dělat
- Žádný onboarding flow

**Dopad:** Vysoká míra opuštění aplikace nováčky

**Návrh řešení:**

- Velký ilustrovaný empty state
- CTA button "Create Your First Entry"
- Krátký vysvětlující text (jak to funguje)

---

#### Problém 4: Dashboard – Hardcoded Mock Data

**Současný stav:**

```razor
<MudText Typo="Typo.h5">Valence: +0.12</MudText>
<MudText Typo="Typo.body2">Averaged last 7 days</MudText>
<!-- ... -->
<MudText Typo="Typo.h5">Scrobbles: 0</MudText>
<MudText Typo="Typo.body2">Connect Last.fm to start</MudText>
```

**Problém:**

- Statistiky nejsou napojené na skutečná data
- Zdání nefunkčnosti
- Klamavé UX (čísla se nemění)

**Dopad:** Uživatelé nevěří, že aplikace funguje

**Návrh řešení:**

- Integrovat AnalyticsService
- Loading states pro data fetching
- Fallback pro prázdná data

---

### 🟡 **Středně Důležité (Snižují Kvalitu)**

#### Problém 5: MoodSelector2D – Chybějící Accessibility

**Současný problém:**

```razor
@onpointerdown="OnPointerDown"
@onpointermove="OnPointerMove"
@onpointerup="StopDragging"
```

**Chybí:**

- ARIA labels pro screen readery
- Keyboard navigation (pouze pointer events)
- Touch event handling pro mobile
- Focus indicator

**Dopad:** Nepoužitelné pro uživatele s handicapem, horší na mobile

**Návrh řešení:**

- ARIA attributes (`role="slider"`, `aria-valuemin/max/now`)
- Keyboard navigation (arrow keys)
- Touch events (`@ontouchstart`, `@ontouchmove`)
- Visible focus ring

---

#### Problém 6: Music Search – Debounce Chybí

**Současný stav:**

```razor
<MudTextField @bind-Value="_query"
              OnKeyDown="HandleKeyDown"
              OnAdornmentClick="SearchAsync" />
```

**Problém:**

- Search se spouští pouze na Enter nebo click
- Žádný automatický search při psaní (s debounce)
- UX je pomalejší, než by mohlo být

**Dopad:** Frustrace uživatelů (musí mačkat Enter)

**Návrh řešení:**

- Implementovat debounce timer (300ms)
- Auto-search při psaní
- Cancel předchozího requestu

---

#### Problém 7: Entry Create Dialog – Příliš Dlouhý Scroll

**Současný stav:**

- Formulář má 10+ polí v jednom dialogu:
  - Song Title, Artist, Album
  - Music Search Box
  - Mood Selector 2D
  - Tension Slider
  - Somatic Tags (12 chips)
  - Custom Tag Input
  - Notes Textarea

**Problém:**

- Není rozdělený do sekcí/kroků
- Visual hierarchy je slabá
- Cognitive overload pro uživatele

**Dopad:** Vyšší míra opuštění formuláře

**Návrh řešení:**

- Multi-step wizard (MudStepper):
  - Krok 1: Music Search + Basic Info
  - Krok 2: Mood Selection
  - Krok 3: Somatic Tags + Notes
- Progress indicator
- Back/Next/Submit buttons

---

#### Problém 8: Analytics Page – Chybějící Charty

**Současný stav:**

```razor
<MudCard>
    <MudCardContent>
        <MudText Typo="Typo.caption">Total Entries</MudText>
        <MudText Typo="Typo.h4">@_moodTrend?.TotalEntries</MudText>
    </MudCardContent>
</MudCard>
```

**Problém:**

- Pouze raw čísla v kartách (statické)
- Žádné grafy pro mood trends (line chart, heatmap)
- UX není "analytics dashboard", ale "number dump"

**Dopad:** Data jsou nudná a špatně pochopitelná

**Návrh řešení:**

- Přidat charting library (Blazor.ChartJS nebo ApexCharts.Blazor)
- Line chart pro valence/arousal over time
- Bar chart pro top artists/songs
- Donut chart pro mood distribution

---

#### Problém 9: Responsive Breakpoints – Neoptimální

**Současný stav:**

```razor
<!-- Dashboard stats cards -->
<MudItem xs="12" sm="6" md="4">
```

**Problém:**

- Na tabletu (sm) se 2 karty vedle sebe mohou zdát přeplněné
- Některé komponenty mají nekonzistentní breakpoints

**Dopad:** Suboptimální layout na různých zařízeních

**Návrh řešení:**

- Audit všech breakpoints
- Standardizovat na: `xs="12" sm="6" md="3"` pro karty
- Touch-friendly targets (min 44x44px)

---

### 🟢 **Nice-to-Have (Polishing)**

#### Problém 10: Chybějící Micro-interactions

**Co chybí:**

- Hover animations na tlačítkách
- Smooth transitions mezi stavy
- Loading states jsou základní
- Žádné ripple effects (přestože MudBlazor je má)

**Dopad:** UX působí staticky a méně profesionálně

**Návrh řešení:**

```css
/* Příklad hover animation */
.mud-button {
  transition: transform 0.2s, box-shadow 0.2s;
}
.mud-button:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
}
```

---

#### Problém 11: Typografie – Hierarchie Není Jasná

**Problém:**

- `Typo.h4`, `Typo.h5`, `Typo.h6` se používají nekonzistentně
- Chybí clear visual scale

**Návrh řešení:**

- Definovat typographic scale:
  - h1: 48px (page titles)
  - h2: 36px (section headers)
  - h3: 24px (card headers)
  - h4: 20px (subsections)
  - body1: 16px (primary text)
  - body2: 14px (secondary text)
  - caption: 12px (metadata)

---

#### Problém 12: Color System – Závislost na MudBlazor

**Problém:**

- Žádné custom CSS variables pro brand colors
- Gradient barvy jsou hardcoded v CSS
- Nedá se snadno přepnout na jiný brand palette

**Návrh řešení:**

```css
:root {
  --color-primary: #3b82f6;
  --color-secondary: #8b5cf6;
  --color-accent: #10b981;
  --color-danger: #ef4444;
  --color-warning: #f59e0b;
  
  /* Mood colors */
  --mood-happy: #22c55e;
  --mood-calm: #3b82f6;
  --mood-angry: #f97316;
  --mood-sad: #6366f1;
}
```

---

#### Problém 13: Dark Mode Je Jediná Možnost

**Problém:**

- Žádný light mode toggle
- Pro některé uživatele může být dark mode unavující

**Návrh řešení:**

- MudThemeProvider s custom themes
- Toggle button v AppBar
- Persist preference v localStorage

---

#### Problém 14: Focus States – Nedostatečné

**Problém:**

- Keyboard navigation funguje (MudBlazor default)
- Ale není vizuálně jasná

**Návrh řešení:**

```css
/* Custom focus ring */
*:focus-visible {
  outline: 2px solid var(--color-primary);
  outline-offset: 2px;
  border-radius: 4px;
}
```

---

## 🚀 Návrh Budoucích Kroků (Prioritizováno)

### **Fáze 1: Kritické UX Fixes (1-2 dny)**

#### Krok 1.1: Implementovat Funkční Navigaci

**Co:** Přidat `MudDrawer` do MainLayout.razor

**Detaily:**

- Navigation links: Dashboard, Analytics
- Conditional login/logout button
- Active state highlighting
- Responsive collapse na mobile
- Toggle drawer z menu button

**Implementační úkoly:**

1. Přidat `MudDrawer` s `@bind-Open`
2. NavMenu komponenta s `NavLink` items
3. Toggle logic v AppBar menu button
4. CSS pro active state
5. Mobile breakpoint handling

**Expected LOC:** ~80 řádků

---

#### Krok 1.2: Opravit Login/Register Feedback

**Co:** Vylepšit error handling a feedback v Login.razor

**Detaily:**

- Structured error display (ne jen ValidationSummary)
- Snackbar pro success/error notifications
- API error parsing a zobrazení
- Better loading state (disable celého formuláře)

**Implementační úkoly:**

1. Error state property (`string? _errorMessage`)
2. `MudAlert` pro API errors
3. Snackbar inject a usage
4. Try-catch v `HandleSubmit` s error parsing
5. Form disable během submission

**Expected LOC:** ~40 řádků

---

#### Krok 1.3: Vylepšit Empty States

**Co:** Redesign EntryList.razor empty state

**Detaily:**

- Ilustrace nebo velká ikona
- Velký CTA button "Create Your First Entry"
- Krátký vysvětlující text (onboarding)
- Odkaz na tutoriál/help

**Implementační úkoly:**

1. SVG ilustrace nebo Material Icon (large)
2. Custom empty state layout
3. CTA button s event callback
4. Micro-copy (friendly, encouraging)

**Expected LOC:** ~30 řádků

---

#### Krok 1.4: Napojit Dashboard na Reálná Data

**Co:** Integrace AnalyticsService do Dashboard.razor

**Detaily:**

- Nahradit hardcoded čísla API calls
- Loading skeleton pro initial load
- Error handling pro failed requests
- Refresh mechanism

**Implementační úkoly:**

1. Inject `IAnalyticsService`
2. OnInitializedAsync data fetch
3. Loading state (`bool _isLoading`)
4. Conditional rendering (loading/data/error)
5. Refresh button s reload logic

**Expected LOC:** ~60 řádků

---

### **Fáze 2: UX Enhancements (3-5 dní)**

#### Krok 2.1: Přidat Accessibility do MoodSelector2D

**Co:** ARIA labels, keyboard navigation, touch support

**Detaily:**

- `role="slider"` s `aria-valuemin/max/now`
- Arrow key navigation (←→↑↓)
- Touch events (`@ontouchstart/move/end`)
- Focus indicator

**Implementační úkoly:**

1. ARIA attributes na `.mood-plane`
2. `@onkeydown` handler pro arrow keys
3. Touch event handlers (parallel k pointer)
4. CSS focus ring
5. Update documentation

**Expected LOC:** ~100 řádků

---

#### Krok 2.2: Implementovat Music Search Debounce

**Co:** Auto-search s debounce v MusicSearchBox.razor

**Detaily:**

- Search při psaní (300ms debounce)
- Loading indicator v textboxu
- Cancel předchozího requestu
- Min. 3 znaky pro search

**Implementační úkoly:**

1. `System.Threading.Timer` pro debounce
2. `CancellationTokenSource` pro cancel
3. OnValueChanged handler místo OnKeyDown
4. Min length validation
5. Loading spinner v Adornment

**Expected LOC:** ~80 řádků

---

#### Krok 2.3: Rozdělit Entry Create Dialog na Kroky

**Co:** Multi-step wizard pomocí MudStepper

**Detaily:**

- Krok 1: Music Search + Basic Info (title, artist, album)
- Krok 2: Mood Selection (2D selector + tension)
- Krok 3: Somatic Tags + Notes
- Progress indicator
- Back/Next/Submit navigation

**Implementační úkoly:**

1. Refactor layout na `<MudStepper>`
2. 3× `<MudStep>` komponenty
3. Navigation logic (currentStep state)
4. Validation per step
5. Submit pouze na posledním kroku

**Expected LOC:** ~150 řádků (refactor existujícího)

---

#### Krok 2.4: Přidat Charty do Analytics

**Co:** Integration Blazor.ChartJS nebo ApexCharts.Blazor

**Detaily:**

- Line chart: valence/arousal over time
- Bar chart: top artists/songs
- Donut chart: mood distribution quadrants
- Interactive tooltips
- Responsive charts

**Implementační úkoly:**

1. NuGet package install (`Blazor.ChartJS`)
2. Chart configuration models
3. Data transformation (DTO → ChartData)
4. Chart components v Analytics.razor
5. Styling a responsive container

**Expected LOC:** ~200 řádků

---

### **Fáze 3: Polish & Advanced Features (5-7 dní)**

#### Krok 3.1: Vylepšit Typografickou Hierarchii

**Co:** CSS variables pro font sizes a konzistentní usage

**Detaily:**

```css
:root {
  --text-xs: 0.75rem;    /* 12px - caption */
  --text-sm: 0.875rem;   /* 14px - body2 */
  --text-base: 1rem;     /* 16px - body1 */
  --text-lg: 1.125rem;   /* 18px - subtitle */
  --text-xl: 1.25rem;    /* 20px - h6 */
  --text-2xl: 1.5rem;    /* 24px - h5 */
  --text-3xl: 1.875rem;  /* 30px - h4 */
  --text-4xl: 2.25rem;   /* 36px - h3 */
  --text-5xl: 3rem;      /* 48px - h2 */
}
```

**Implementační úkoly:**

1. Definovat CSS variables v app.css
2. Audit všech `Typo` usage
3. Nahradit inline sizes za variables
4. Dokumentovat typography scale
5. Style guide page (optional)

**Expected LOC:** ~50 řádků CSS + dokumentace

---

#### Krok 3.2: Custom Color System

**Co:** Brand color tokens v CSS variables

**Detaily:**

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
  --gradient-bg: radial-gradient(...);
}
```

**Implementační úkoly:**

1. Definovat color palette
2. Nahradit hardcoded colors
3. Theme support (dark/light variants)
4. Color documentation
5. Accessibility contrast check

**Expected LOC:** ~80 řádků CSS

---

#### Krok 3.3: Micro-interactions

**Co:** Hover animations, transitions, ripples

**Detaily:**

```css
/* Smooth transitions */
* {
  transition: color 0.2s, background-color 0.2s, border-color 0.2s;
}

/* Button hover */
.mud-button:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
}

/* Card hover */
.mud-card:hover {
  box-shadow: 0 8px 24px rgba(0,0,0,0.12);
}
```

**Implementační úkoly:**

1. Global transition rules
2. Hover states pro interactive elements
3. Focus states
4. Loading animations
5. Enter/exit transitions

**Expected LOC:** ~100 řádků CSS

---

#### Krok 3.4: Light/Dark Mode Toggle

**Co:** Theme switching s MudThemeProvider

**Detaily:**

- Custom MudTheme (dark + light)
- Toggle button v AppBar
- Persist v localStorage
- Smooth transition animation

**Implementační úkoly:**

1. Define custom MudTheme instances
2. ThemeService pro state management
3. Toggle button component
4. localStorage persist
5. CSS transition pro smooth switch

**Expected LOC:** ~120 řádků

---

#### Krok 3.5: Advanced Responsive Optimizations

**Co:** Breakpoint audit a mobile optimizations

**Detaily:**

- Mobile-first všude
- Touch-friendly targets (min 44x44px)
- Safe area insets (iOS)
- Tablet-specific layouts
- Gesture support (swipe, pinch)

**Implementační úkoly:**

1. Audit všech `MudItem` breakpoints
2. Touch target size audit
3. iOS safe area CSS
4. Tablet layouts (landscape/portrait)
5. Testing na real devices

**Expected LOC:** ~60 řádků + testing

---

## 📐 Design System Doporučení

### Spacing Scale (8px Grid)

```css
:root {
  --space-1: 0.25rem;  /* 4px */
  --space-2: 0.5rem;   /* 8px */
  --space-3: 0.75rem;  /* 12px */
  --space-4: 1rem;     /* 16px */
  --space-5: 1.25rem;  /* 20px */
  --space-6: 1.5rem;   /* 24px */
  --space-8: 2rem;     /* 32px */
  --space-10: 2.5rem;  /* 40px */
  --space-12: 3rem;    /* 48px */
  --space-16: 4rem;    /* 64px */
}
```

**Usage:**

```razor
<MudPaper Class="pa-4">  <!-- padding: var(--space-4) -->
```

---

### Border Radius System

```css
:root {
  --radius-sm: 0.375rem;  /* 6px */
  --radius-md: 0.5rem;    /* 8px */
  --radius-lg: 0.75rem;   /* 12px */
  --radius-xl: 1rem;      /* 16px */
  --radius-2xl: 1.125rem; /* 18px */
  --radius-full: 9999px;  /* pills */
}
```

---

### Shadow System

```css
:root {
  /* Elevation shadows */
  --shadow-sm: 0 1px 2px rgba(0,0,0,0.05);
  --shadow-md: 0 4px 6px rgba(0,0,0,0.1);
  --shadow-lg: 0 10px 15px rgba(0,0,0,0.1);
  --shadow-xl: 0 20px 25px rgba(0,0,0,0.15);
  --shadow-2xl: 0 25px 50px rgba(0,0,0,0.25);
  
  /* Colored shadows (for highlights) */
  --shadow-primary: 0 4px 14px rgba(59, 130, 246, 0.4);
  --shadow-success: 0 4px 14px rgba(34, 197, 94, 0.4);
}
```

---

## 🎯 UX Metrics & KPIs (Pro Testování)

### Po implementaci změn měřit

#### 1. Task Completion Rate

- **Metrika:** Kolik % uživatelů dokončí vytvoření entry?
- **Target:** >85%
- **Měření:** Analytics tracking v Entry Create Dialog

#### 2. Time to First Entry

- **Metrika:** Jak dlouho trvá nováčkovi vytvořit první záznam?
- **Target:** <2 minuty
- **Měření:** Timestamp (registration → first entry created)

#### 3. Navigation Clarity

- **Metrika:** Kolik kliků trvá dostat se z Dashboard → Analytics?
- **Target:** 1-2 clicks
- **Měření:** Click tracking

#### 4. Form Abandonment Rate

- **Metrika:** Kolik % uživatelů opustí Entry Create Dialog?
- **Target:** <20%
- **Měření:** Dialog open vs. submit events

#### 5. Mobile Usability Score

- **Metrika:** Lighthouse Mobile Score
- **Target:** >90/100
- **Měření:** Google Lighthouse CI

#### 6. Accessibility Score

- **Metrika:** WAVE errors + warnings
- **Target:** 0 errors, <5 warnings
- **Měření:** axe DevTools audit

---

## 🛠️ Doporučené Nástroje & Knihovny

### Charts & Visualizations

- **Blazor.ChartJS** - Chart.js wrapper pro Blazor
- **ApexCharts.Blazor** - Alternativa (více features)
- **Plotly.Blazor** - Pro advanced analytics

### Icons

- **Material Icons** - Už používáno (MudBlazor)
- **Heroicons** - Pro rozmanitost
- **Lucide Icons** - Modern, clean

### Animations

- **CSS Transitions** - Pro basic animations
- **Blazor.Animate** - Pro pokročilé animace
- **GSAP (via JSInterop)** - Pro complex animations

### Accessibility Testing

- **axe DevTools** - Browser extension
- **WAVE** - Web accessibility evaluation
- **Pa11y** - Automated testing CLI

### Performance

- **Lighthouse** - Google Chrome DevTools
- **WebPageTest** - Real-world testing
- **BundleAnalyzer** - Blazor bundle size analysis

### Design Tools

- **Figma** - Pro mockupy a prototypy
- **Coolors.co** - Color palette generator
- **Type Scale** - Typography scale calculator

---

## 📝 Závěr & Prioritizace

### **Nejdůležitější (Do This First):**

1. ✅ **Funkční navigace (MudDrawer)** - bez toho aplikace nepoužitelná
2. ✅ **Dashboard napojit na reálná data** - základ funkčnosti
3. ✅ **Login/Register error handling** - frustrace uživatelů

**Časový odhad:** 1-2 dny  
**Impact:** Vysoký (blokující UX issues)

---

### **Vysoká Priorita:**

1. **Debounce pro Music Search** - zlepší UX vyhledávání
2. **Analytics charts** - data musí být vizuální
3. **Entry List empty state** - first impressions matter

**Časový odhad:** 3-5 dní  
**Impact:** Střední až vysoký (quality of life)

---

### **Střední Priorita:**

1. **MoodSelector accessibility** - inclusivity
2. **Entry Dialog wizard (multi-step)** - snižuje cognitive load
3. **Custom color system** - brand consistency

**Časový odhad:** 5-7 dní  
**Impact:** Střední (polish & accessibility)

---

### **Nízká Priorita (Polish):**

1. **Light/Dark mode toggle** - user preference
2. **Micro-interactions** - professional feel
3. **Advanced responsive tweaks** - edge cases

**Časový odhad:** 3-5 dní  
**Impact:** Nízký (nice-to-have)

---

## 📊 Celkový Časový Odhad

| Fáze | Úkoly | Dny (Senior Dev) | Dny (Junior Dev) |
|------|-------|------------------|------------------|
| **Fáze 1** | Kritické UX fixes | 1-2 | 3-4 |
| **Fáze 2** | UX enhancements | 3-5 | 8-10 |
| **Fáze 3** | Polish & advanced | 5-7 | 12-15 |
| **CELKEM** | | **9-14 dní** | **23-29 dní** |

**Note:** Předpokládá se full-time práce na UX/UI. Paralelně s backend vývojem může být časová úspora.

---

## 🎨 Příklady Před/Po (Vizuální Mock-ups)

### Dashboard - Před

```
┌─────────────────────────────────┐
│ Welcome back                     │
│ [+ New Entry] [Connect Last.fm] │
├─────────────────────────────────┤
│ Valence: +0.12 (hardcoded)      │
│ Scrobbles: 0 (hardcoded)        │
├─────────────────────────────────┤
│ [Entry List]  [Music Search]    │
└─────────────────────────────────┘
```

### Dashboard - Po

```
┌─────────────────────────────────┐
│ ☰ MIMM 2.0           [Profile]  │ ← Functional nav
├─────────────────────────────────┤
│ Welcome back, John!             │ ← Personalized
│ [+ New Entry] [Connect Last.fm] │
├─────────────────────────────────┤
│ 📊 Your Mood Trends (7 days)    │
│ ┌───────┐ ┌───────┐ ┌───────┐  │
│ │Val:+.8│ │Aro:+.3│ │ 12    │  │ ← Real data
│ │ 😊    │ │ ⚡     │ │scrobl.│  │
│ └───────┘ └───────┘ └───────┘  │
├─────────────────────────────────┤
│ 📈 [Line Chart: Mood Trends]    │ ← Visual analytics
├─────────────────────────────────┤
│ [Entry List]  [Music Search]    │
└─────────────────────────────────┘
```

---

## 📚 Reference & Zdroje

### Design Inspirace

- **Spotify** - Music card layouts
- **Apple Music** - Clean typography
- **Headspace** - Mood tracking UX
- **Daylio** - Entry creation flow

### UX Best Practices

- [Nielsen Norman Group - UX Guidelines](https://www.nngroup.com/)
- [Material Design 3](https://m3.material.io/)
- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)

### Blazor Resources

- [MudBlazor Documentation](https://mudblazor.com/)
- [Blazor University](https://blazor-university.com/)
- [Microsoft Blazor Docs](https://learn.microsoft.com/en-us/aspnet/core/blazor/)

---

**Konec dokumentu**  
Vytvořeno: 26. ledna 2026  
UX/UI Specialist Agent pro MIMM 2.0
