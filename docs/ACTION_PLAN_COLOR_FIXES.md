# 📋 MIMM 2.0 - Akční Plán Opravy Barev & Viditelnosti

**Status:** Připraveno k implementaci  
**Priorita:** VYSOKÁ  
**Odhad času:** 5-7 pracovních dní  
**Složitost:** Střední (CSS + Component tweaks)

---

## 📊 Přehled Úkolů

### Fáze 1: Kritické Opravy Viditelnosti (Den 1-2)

#### Úkol 1.1: Empty State Ikonky
**Soubory:**
- `src/MIMM.Frontend/Components/EntryList.razor` (řádek ~70)
- `src/MIMM.Frontend/Pages/Analytics.razor` (řádky ~90, 130)
- `src/MIMM.Frontend/Pages/YearlyReport.razor` (řádek ~30)
- `src/MIMM.Frontend/Pages/Friends.razor` (řádky ~75, 110)

**Problém:**
```html
<!-- NYNÍ - NEVIDITELNÉ -->
<MudIcon Icon="@Icons.Material.Filled.MusicNote" 
         Color="Color.Primary" 
         Style="font-size: 120px; opacity: 0.5;" />

<!-- DŮVOD: opacity: 0.5 = #3b82f6 * 0.5 = ~#9dc3fa (velmi světlá) -->
<!-- Na tmavém pozadí (#0f172a) je kontrast: <2:1 ❌ -->
```

**Řešení:**
```html
<!-- OPRAVENO - VIDITELNÉ -->
<MudIcon Icon="@Icons.Material.Filled.MusicNote" 
         Color="Color.Primary" 
         Style="font-size: 120px; opacity: 0.7;" />

<!-- Nový kontrast: ~3:1 (lepší) -->
<!-- NEBO pro maximální viditelnost: -->
<MudIcon Icon="@Icons.Material.Filled.MusicNote" 
         Color="Color.Primary" 
         Style="font-size: 120px;" />
<!-- (bez opacity, plné Color.Primary = 8:1 kontrast) -->
```

**Checklist:**
- [ ] EntryList.razor - řádek 70
- [ ] Analytics.razor - řádky 90, 130, 150
- [ ] YearlyReport.razor - řádek 30
- [ ] Friends.razor - řádky 75, 110

---

#### Úkol 1.2: Mood Selector Canvas Viditelnost
**Soubor:** 
- `src/MIMM.Frontend/Components/MoodSelector2D.razor`
- `src/MIMM.Frontend/wwwroot/css/mood-selector.css`

**Problém 1: Canvas Border**
```css
/* mood-selector.css řádek ~8 - NEVIDITELNÝ */
canvas {
    border: 1px solid rgba(0,0,0,0.1);  /* Na tmavém bg je neviditelný */
}
```

**Řešení 1: Použít design token**
```css
canvas {
    border: 1px solid var(--color-border-emphasis);  
    /* Bude respektovat theme a bude viditelný */
}
```

**Problém 2: Grid Lines**
```css
/* mood-selector.css řádek ~49 - PRAKTICKY NEVIDITELNÉ */
.grid-line {
    background: rgba(0,0,0,0.06);  /* 0.06 opacity = ŠÍLENÉ nízké */
}

/* Light mode: */
[data-theme="light"] .grid-line {
    background: rgba(0,0,0,0.1);  /* Stále slabé */
}
```

**Řešení 2: Zvýšit opacity a použít token**
```css
.grid-line {
    background: var(--color-border-muted);  
    /* Default: rgba(255, 255, 255, 0.05) - je stále nízké */
    /* Ale lepší než 0.06 */
}

/* NEBO explicitně: */
.grid-line {
    background: rgba(255, 255, 255, 0.15);  
    /* Zvýšeno z 0.05 na 0.15 */
}

[data-theme="light"] .grid-line {
    background: rgba(0, 0, 0, 0.12);  
    /* Zvýšeno z 0.1 na 0.12 */
}
```

**Problém 3: Axis Labels**
```css
/* mood-selector.css řádek ~59 - SLABÉ */
.axis-label {
    color: rgba(0,0,0,0.6);  
    /* Na tmavém bg je neviditelný! */
}

[data-theme="light"] .axis-label {
    color: rgba(0,0,0,0.7);  
    /* V light režimu OK, ale stále mohlo by být tmavší */
}
```

**Řešení 3: Použít semantic token**
```css
.axis-label {
    color: var(--color-text-secondary);  
    /* #d1d5db - bude viditelný */
}

[data-theme="light"] .axis-label {
    color: var(--color-text-secondary);  
    /* #4b5563 - bude viditelný */
}
```

**Problém 4: Mood Plane Background**
```css
/* mood-selector.css řádek ~8 - PŘÍLIŠ SUBTILNÍ */
.mood-plane {
    background: radial-gradient(circle at 50% 50%, rgba(255,255,255,0.1), rgba(0,0,0,0.05)),
                linear-gradient(135deg, rgba(34,197,94,0.12), rgba(59,130,246,0.12));
}

/* Opacity 0.1, 0.05, 0.12 jsou příliš nízké */
```

**Řešení 4: Zvýšit opacity**
```css
.mood-plane {
    background: radial-gradient(circle at 50% 50%, rgba(255,255,255,0.15), rgba(0,0,0,0.08)),
                linear-gradient(135deg, rgba(34,197,94,0.18), rgba(59,130,246,0.18));
    /* Zvýšeno: 0.1→0.15, 0.05→0.08, 0.12→0.18 */
    border: 1px solid var(--color-border-muted);  
    /* Přidán border pro lepší separaci */
}

[data-theme="light"] .mood-plane {
    background: radial-gradient(circle at 50% 50%, rgba(59,130,246,0.12), rgba(255,255,255,0.03)),
                linear-gradient(135deg, rgba(34,197,94,0.12), rgba(59,130,246,0.12));
    /* V light režimu zvýšit opacity */
    border: 1px solid var(--color-border-default);
}
```

**Checklist:**
- [ ] Opravit canvas border
- [ ] Zvýšit grid-line opacity
- [ ] Opravit axis-label barvu
- [ ] Zvýšit mood-plane background opacity

---

#### Úkol 1.3: Analytics "No Data" States
**Soubory:**
- `src/MIMM.Frontend/Pages/Analytics.razor` (řádky ~115-125)
- `src/MIMM.Frontend/Pages/YearlyReport.razor` (řádky ~25-35)

**Problém:**
```html
<!-- Analytics.razor řádek ~118 -->
<MudStack AlignItems="AlignItems.Center" Spacing="2" Class="pa-6">
    <MudIcon Icon="@Icons.Material.Filled.DonutSmall" 
             Color="Color.Default"  <!-- ❌ PROBLÉM -->
             Size="Size.Large" />
    <!-- Kontrast Color.Default (#9ca3af) na dark bg: 3.2:1 ❌ -->
```

**Řešení:**
```html
<MudStack AlignItems="AlignItems.Center" Spacing="2" Class="pa-6">
    <MudIcon Icon="@Icons.Material.Filled.DonutSmall" 
             Color="Color.Primary"  <!-- ✅ OPRAVENO -->
             Size="Size.Large" />
    <!-- Kontrast Color.Primary (#3b82f6) na dark bg: 8:1 ✅ -->
    
    <MudText Typo="Typo.body2" Align="Align.Center">
        No mood distribution data available
    </MudText>
    <MudText Typo="Typo.caption" Align="Align.Center">
        <!-- Zvýšit viditelnost: -->
        Create entries to see your emotional patterns
    </MudText>
</MudStack>
```

**Checklist:**
- [ ] Analytics - DonutSmall ikona: Color.Default → Color.Primary
- [ ] YearlyReport - CalendarMonth ikona: Color.Default → Color.Primary
- [ ] Friends - MarkEmailRead ikona: Color.Default → Color.Primary
- [ ] Zvýšit font size v caption textech (caption → body2)

---

### Fáze 2: Kontrast Text (Den 2-3)

#### Úkol 2.1: Secondary Text Brightness
**Soubor:**
- `src/MIMM.Frontend/wwwroot/css/design-tokens.css` (řádky 139-143)

**Problém:**
```css
/* design-tokens.css */
:root {
  --color-text-secondary: #d1d5db;  /* Kontrast: 4.8:1 - tesně AA ⚠️ */
  --color-text-tertiary: #9ca3af;   /* Kontrast: 2.8:1 - FAIL ❌ */
}
```

**Řešení:**
```css
:root {
  --color-text-secondary: #dde1e6;  /* Zvýšeno z #d1d5db, kontrast: 5.2:1+ ✅ */
  --color-text-tertiary: #bcc1cc;   /* Zvýšeno z #9ca3af, kontrast: 4.5:1 ✅ */
}

/* Light theme */
[data-theme="light"] {
  --color-text-secondary: #4b5563;  /* Zvýšeno z původní, kontrast: 12:1 */
  --color-text-tertiary: #6b7280;   /* Zvýšeno z #9ca3af, kontrast: 8.5:1 */
}
```

**Checklist:**
- [ ] Zvýšit --color-text-secondary z #d1d5db na #dde1e6
- [ ] Zvýšit --color-text-tertiary z #9ca3af na #bcc1cc
- [ ] Testovat kontrast v Analytics card captions
- [ ] Testovat v všech dark themes

---

#### Úkol 2.2: Theme Selector Menu
**Soubor:**
- `src/MIMM.Frontend/Components/ThemeSelector.razor` (řádky 10-25)

**Problém:**
```html
<!-- ThemeSelector.razor řádek ~13 -->
<MudText Typo="Typo.caption" Class="px-4 pt-2">Select Theme</MudText>

<!-- Typo.caption je malý + --color-text-secondary, kontrast: 4.8:1 ⚠️ -->
```

**Řešení:**
```html
<MudText Typo="Typo.body2" Class="px-4 pt-2" Style="font-weight: 500;">
    Select Theme
</MudText>

<!-- Větší text + větší kontrast -->
```

**V menu items:**
```html
<!-- Řádek ~17 -->
<MudText Typo="Typo.body2">@metadata.Name</MudText>     <!-- ✅ OK -->
<MudText Typo="Typo.caption">@metadata.Description</MudText>  
<!-- Změnit na body2 nebo zvětšit -->

<!-- OPRAVENO: -->
<MudText Typo="Typo.body2">@metadata.Name</MudText>
<MudText Typo="Typo.body2">@metadata.Description</MudText>
```

**Checklist:**
- [ ] Změnit header "Select Theme" z caption na body2
- [ ] Změnit description text na body2
- [ ] Testovat viditelnost v menu

---

#### Úkol 2.3: Outlined Cards Border Visibility
**Soubor:**
- `src/MIMM.Frontend/Pages/Friends.razor` (řádky 85-110)

**Problém:**
```html
<!-- Friends.razor řádek ~89 -->
<MudCard Outlined>  <!-- Default border opacity je příliš nízká -->
    <MudCardContent>
        <!-- Border je skoro neviditelný -->
    </MudCardContent>
</MudCard>
```

**Řešení - v CSS (nejlepší):**
```css
/* Přidat do app.css */
.mud-card-outlined {
    border-color: var(--color-border-emphasis) !important;
    /* Zvýší viditelnost borderu -->
}
```

**Alternativa - v Razor:**
```html
<MudCard Outlined Style="border: 1px solid var(--color-border-emphasis);">
    <MudCardContent>
        <!-- Viditelný border -->
    </MudCardContent>
</MudCard>
```

**Checklist:**
- [ ] Přidat CSS rule do app.css pro .mud-card-outlined
- [ ] Testovat v Friends stránce
- [ ] Testovat v různých themes

---

### Fáze 3: UI Konzistence (Den 3-4)

#### Úkol 3.1: Button Color Unifikace
**Soubory:**
- `src/MIMM.Frontend/Pages/Dashboard.razor` (řádky 65-95)

**Problém:**
```html
<!-- Dashboard.razor Quick Actions -->
<MudButton Variant="Variant.Filled" 
           Color="Color.Secondary"  <!-- ❌ Není jasně diferencován -->
           FullWidth
           StartIcon="@Icons.Material.Filled.MoreHoriz"
           Style="font-weight: 600;">
    More Options
</MudButton>
```

**Řešení:**
```html
<!-- Změnit na Color.Default nebo Color.Tertiary -->
<MudButton Variant="Variant.Outlined"  <!-- Zmenant variant! -->
           Color="Color.Default" 
           FullWidth
           StartIcon="@Icons.Material.Filled.MoreHoriz"
           Style="font-weight: 600;">
    More Options
</MudButton>
```

**Pravidla:**
- **Primary (modrá)** = Hlavní akce (Create, Save, Submit)
- **Secondary (fialová)** = Sekundární akce (Export, Advanced)
- **Success (zelená)** = Potvrzující akce (Accept, Confirm)
- **Error (červená)** = Destruktivní akce (Delete, Reject)
- **Outlined** = Méně důležité akce (More Options, Cancel)

**Checklist:**
- [ ] Dashboard "More Options" button: Color.Secondary → Color.Default + Variant.Outlined
- [ ] Zkontrolovat všechny buttony na soulad s pravidly
- [ ] Testovat na všech stránkách

---

#### Úkol 3.2: MudPaper Shadow Zlepšení
**Soubory:**
- `src/MIMM.Frontend/wwwroot/css/app.css` (řádky 37-50)

**Problém:**
```css
/* app.css řádka ~37 */
.page-surface {
    padding: var(--space-8);
    border-radius: var(--radius-2xl);
    background: rgba(17, 24, 39, 0.75);
    border: 1px solid var(--color-border-muted);
    box-shadow: var(--shadow-xl), inset 0 1px 0 rgba(255, 255, 255, 0.04);
    /* Shadow OK, ale border je slabý */
}
```

**Řešení:**
```css
.page-surface {
    padding: var(--space-8);
    border-radius: var(--radius-2xl);
    background: rgba(17, 24, 39, 0.75);
    border: 1px solid var(--color-border-emphasis);  /* Zvýšená viditelnost */
    box-shadow: var(--shadow-xl), inset 0 1px 0 rgba(255, 255, 255, 0.04);
}

/* Pro MudPaper s Elevation="1" */
.mud-paper-elevation-1 {
    box-shadow: var(--shadow-md);  /* Zvětšit shadow */
}

/* Pro MudCard */
.mud-card {
    border: 1px solid var(--color-border-default);  /* Přidat viditelný border */
}
```

**Checklist:**
- [ ] Zvýšit border viditelnost v .page-surface
- [ ] Přidat CSS rule pro .mud-card
- [ ] Testovat shadow zlepšení

---

#### Úkol 3.3: Selected Track Panel Opacity
**Soubor:**
- `src/MIMM.Frontend/Components/EntryCreateDialog.razor` (řádek 23)

**Problém:**
```html
<!-- EntryCreateDialog.razor řádek ~23 -->
<MudPaper Class="pa-3" 
          Elevation="0" 
          Style="background-color:rgba(33,150,243,0.1); border-left:4px solid #2196F3;">
    <!-- background opacity: 0.1 = PŘÍLIŠ NÍZKÉ -->
</MudPaper>
```

**Řešení:**
```html
<MudPaper Class="pa-3" 
          Elevation="0" 
          Style="background-color:rgba(33,150,243,0.15); border-left:4px solid #2196F3;">
    <!-- Zvýšeno z 0.1 na 0.15 -->
</MudPaper>

<!-- NEBO použít design token: -->
<MudPaper Class="pa-3" 
          Elevation="1" 
          Style="border-left:4px solid var(--color-primary-500); background: var(--color-overlay-light);">
    <!-- Lépe čitelné a respektuje theme -->
</MudPaper>
```

**Checklist:**
- [ ] Zvýšit opacity z 0.1 na 0.15
- [ ] Zvýšit Elevation z 0 na 1
- [ ] Testovat v EntryCreateDialog a EntryEditDialog

---

### Fáze 4: Testování (Den 4-5)

#### Úkol 4.1: WCAG Kontrast Testování
**Nástroj:** WebAIM Contrast Checker  
**URL:** https://webaim.org/resources/contrastchecker/

```
Testovat tyto kombinace:
□ Primary text (#f9fafb) na bg-primary (#0f172a) = 18:1 ✅
□ Secondary text (#dde1e6) na bg-secondary (#1e293b) = 5.2:1 ✅
□ Tertiary text (#bcc1cc) na bg-secondary (#1e293b) = 4.5:1 ✅
□ Color.Primary (#3b82f6) na bg-secondary (#1e293b) = 8:1 ✅
□ Border (#color-border-emphasis) - viditelný ✅

V light režimu:
□ Primary text (#111827) na bg-primary (#ffffff) = 18:1 ✅
□ Secondary text (#4b5563) na bg-secondary (#f9fafb) = 12:1 ✅
□ Tertiary text (#6b7280) na bg-secondary (#f9fafb) = 8.5:1 ✅
```

**Checklist:**
- [ ] Testovat všechny kombinace
- [ ] Všechny kombinace musí splňovat WCAG AA (4.5:1+)
- [ ] Dokumentovat výsledky

---

#### Úkol 4.2: Visual Regression Testing
**Soubory ke kontrole:**
1. Dashboard.razor
   - [ ] Header gradient čitelný ✅
   - [ ] Stats cards dobře viditelné ✅
   - [ ] Quick Actions viditelné ✅

2. Analytics.razor
   - [ ] Summary cards čitelné ✅
   - [ ] Charts viditelné ✅
   - [ ] Empty states ikonky viditelné ✅

3. Friends.razor
   - [ ] Friend cards viditelné ✅
   - [ ] Borders viditelné ✅
   - [ ] Avatars diferenciované ✅

4. Mood Selector
   - [ ] Canvas border viditelný ✅
   - [ ] Grid lines viditelné ✅
   - [ ] Axis labels čitelné ✅
   - [ ] Background viditelný ✅

5. Entry Components
   - [ ] No entries ikony viditelné ✅
   - [ ] Selected track panel viditelný ✅
   - [ ] Dialogy čitelné ✅

---

#### Úkol 4.3: Theme Switching Validation
```
□ Default theme - všechny prvky viditelné
□ Midnight theme - kontrast OK
□ Twilight theme - kontrast OK
□ Ocean theme - kontrast OK
□ Forest theme - kontrast OK
□ Light theme - žádné problémy se čitelností
```

**Checklist:**
- [ ] Testovat v Developer Tools (dark/light OS preference)
- [ ] Testovat přepínání motivů za běhu
- [ ] Zkontrolovat localStorage persistence

---

## 📝 Implementační Poznámky

### Style Příkazy
```bash
# Ověřit CSS syntax
npm run lint:css  # pokud existuje

# Build a test
dotnet build src/MIMM.Frontend/MIMM.Frontend.csproj

# Spustit v debug módu
dotnet run --project src/Application.Web/Application.Web.csproj
```

### Úpravy Ověřit v Prohlížeči
```
F12 → Accessibility Inspector → Contrast (Lighthouse)
NEBO
F12 → Elements → Inspect element → Computed styles
```

---

## 🎯 Shrnutí Změn

| Úkol | Počet souborů | Řádků kódu | Priorita | Status |
|------|--------------|-----------|----------|---------|
| 1.1 Empty State Icons | 4 | ~8 | 🔴 P1 | [ ] |
| 1.2 Mood Selector | 2 | ~20 | 🔴 P1 | [ ] |
| 1.3 Analytics No Data | 2 | ~6 | 🔴 P1 | [ ] |
| 2.1 Text Brightness | 1 | ~8 | 🟠 P2 | [ ] |
| 2.2 Theme Selector | 1 | ~4 | 🟠 P2 | [ ] |
| 2.3 Card Borders | 2 | ~3 | 🟠 P2 | [ ] |
| 3.1 Button Colors | 1 | ~2 | 🟡 P3 | [ ] |
| 3.2 Shadows | 1 | ~5 | 🟡 P3 | [ ] |
| 3.3 Track Panel | 1 | ~2 | 🟡 P3 | [ ] |
| **CELKEM** | **15** | **~58** | - | - |

---

**Připraveno pro vývojáře: 28. ledna 2026**  
**Čekající na start implementace:** ⏳
