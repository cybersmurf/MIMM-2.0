# MIMM 2.0 – UX/UI Action Plan

**Datum:** 26. ledna 2026  
**Agent:** UX/UI Specialist  
**Status:** Akční plán (bez implementace)

## 🎯 Cíl

Rychle zlepšit použitelnost a vizuální kvalitu frontendů (Blazor + MudBlazor) se zaměřením na navigaci, zpětnou vazbu formulářů, prázdné stavy a vizualizaci dat.

## 🧭 Principy

- Udržet konzistentní design system (typografie, barvy, spacing, stíny)
- Preferovat MudBlazor komponenty, minimalizovat JS interop
- Respektovat WCAG (focus states, klávesnice, aria)
- Mobile-first, responzivní breakpoints xs/sm/md

## 🚀 Fáze 1 – Kritické UX opravy (1–2 dny)

1) **Navigace**
   - Přidat MudDrawer do MainLayout, nav odkazy: Dashboard, Analytics, Logout/Login.
   - Toggle z AppBar menu button, aktivní stav linků.
2) **Dashboard data**
   - Nahradit mockované čísla reálným API (AnalyticsService).
   - Přidat loading skeleton a error state.
3) **Login/Register feedback**
   - Snackbar pro success/error, strukturované API chyby.
   - Disable form během submitu, zobrazit spinner v tlačítku.
4) **Empty state EntryList**
   - Ilustrace/ikona + CTA „Create your first entry“.
   - Krátký onboarding text.

## ✨ Fáze 2 – UX vylepšení (3–5 dní)

5) **Music Search debounce**
   - Auto-search při psaní (300 ms), min 3 znaky.
   - Adornment spinner, cancel předchozího requestu.
2) **MoodSelector2D A11y**
   - role="slider", aria-* hodnoty, focus ring.
   - Klávesnice (šipky), touch events.
3) **EntryCreateDialog → wizard**
   - 3 kroky: (1) Info/Search, (2) Mood/Tension, (3) Tags/Notes.
   - Stepper s Back/Next/Submit, validace per krok.
4) **Analytics grafy**
   - Line chart (valence/arousal), bar chart (top artists/songs), donut (mood distribution).
   - Vybrat knihovnu (Blazor.ChartJS / ApexCharts.Blazor).

## 🎨 Fáze 3 – Polish & systém (5–7 dní)

9) **Design tokens**
   - CSS variables pro typografii, barvy, spacing, stíny.
   - Nahradit hardcoded barvy/velikosti.
2) **Micro-interactions**
    - Hover/focus animace, smooth transitions, ripple použití.
3) **Light/Dark toggle**
    - MudThemeProvider, two themes, persist v localStorage.
4) **Responsive audit**
    - Breakpointy pro karty/layout, touch targets ≥ 44px, safe-area pro iOS.

## 🧪 Metriky úspěchu

- Time to first entry < 2 min
- Task completion rate (create entry) > 85 %
- Form abandonment < 20 %
- Navigation clarity: Dashboard → Analytics ≤ 2 kliky
- Lighthouse Mobile > 90, Accessibility audit (axe/WAVE) 0 errors

## 🔗 Odkazy na klíčové soubory

- Layout: src/MIMM.Frontend/MainLayout.razor
- Dashboard: src/MIMM.Frontend/Pages/Dashboard.razor
- Login: src/MIMM.Frontend/Pages/Login.razor
- Entry list/dialogy: src/MIMM.Frontend/Components/
- Styles: src/MIMM.Frontend/wwwroot/css/app.css

## ✅ Doporučené pořadí

1. Navigace + Dashboard data + Login feedback + Empty state
2. Debounce search + MoodSelector A11y + Wizard dialog + Grafy
3. Design tokens + Micro-interactions + Theme toggle + Responsive audit
