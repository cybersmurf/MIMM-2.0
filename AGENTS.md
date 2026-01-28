# AGENTS.md

[![CI](https://github.com/cybersmurf/MIMM-2.0/actions/workflows/ci.yml/badge.svg)](https://github.com/cybersmurf/MIMM-2.0/actions/workflows/ci.yml)
**Version:** 2.0.1 | **Status:** Code Review & Optimization Complete ✅

- zde jsou klíčové instrukce pro všechny AI agenty
- **níže jsou uvedeny další instrukční soubory, které je nutné respektovat**

- [README.md](./README.md) - základní informace o projektu
- [CODE_REVIEW_PLAN.md](./docs/CODE_REVIEW_PLAN.md) - detailní plán refaktoringu a optimizací
- [DOCKER_DEPLOYMENT_RULES.md](./.github/DOCKER_DEPLOYMENT_RULES.md) - pravidla pro Docker a deployment
- `.github/prompts/` - opakovatelné prompty pro Copilot (release notes, E2E maintenance, CI fix, feature pattern)
  - release-notes.prompt.md
  - e2e-tests-maintenance.prompt.md
  - ci-fix.prompt.md
  - feature-implementation.prompt.md
  - security-hardening.prompt.md
  - ef-migrations-review.prompt.md
  - api-contract-review.prompt.md
  - markdown-linting.prompt.md
- `.markdownlint.json` - konfigurace pro markdownlint v0.40.0

## Markdown Linting (Documentation Quality)

- Tool: `markdownlint-cli` (v0.40.0)
- Konfigurace: `.markdownlint.json` s pravidly pro projekt
- Běžné chyby: hard tabs, chybějící blank lines, dlouhé řádky, chybějící language tagy
- Oprava: `markdownlint --fix "**/*.md"` (auto-oprava) nebo ručně
- Ověření: `markdownlint "README.md" "CHANGELOG.md" "AGENTS.md" "docs/*.md"`

## CI & Coverage pro agenty

- CI stav: viz badge nahoře a detailní běhy v GitHub Actions.
- Lokální CI parita:

```bash
# Restore + Build (Release)
dotnet restore MIMM.sln
dotnet build MIMM.sln --configuration Release --no-restore

# Testy s minimálním logem
dotnet test MIMM.sln --configuration Release --no-build -v minimal
```

- Coverage artefakty: v CI se nahrávají jako `coverage-reports`. Lokálně můžeš vygenerovat HTML report:

```bash

## Konfigurační soubory

- `global.json` - Verze .NET SDK (9.0.100) s rollForward policy
- `.editorconfig` - Pravidla pro formátování a styl kódu

## Požadavky

- .NET 9.0 SDK (verze 9.0.100 nebo kompatibilní díky rollForward)
- Ověření instalace: `dotnet --version`

## Struktura projektu

- Codecov: nastavení tokenu a badge je popsáno v README (sekce „Codecov setup“).

Toto je ASP.NET Core Minimal API projekt s následující strukturou:

- `src/Application.Web/` - Hlavní webová API aplikace
- `src/Application.Lib/` - Business logika a služby
- `tests/Application.Tests/` - xUnit testovací projekt

## Klíčové balíčky

- Microsoft.AspNetCore.OpenApi 10.0.0
- xUnit 2.9.3 (testovací framework)
- Microsoft.NET.Test.Sdk 17.14.1

## Běžné příkazy

### Build & Restore

```bash
dotnet restore          # Obnovení závislostí (z rootu repozitáře)
dotnet build           # Build celého solution (z rootu repozitáře)
dotnet clean           # Vyčištění build artefaktů
```

## 📐 Frontend UI Development (MudBlazor)

**Golden Rule:** Use MudBlazor components first. Raw HTML/CSS only for global utilities and animations.

### Key Files

- **Guide:** [docs/MUDBLAZOR_GUIDE.md](./docs/MUDBLAZOR_GUIDE.md) - Complete MudBlazor integration patterns
- **Components:** `src/MIMM.Frontend/Pages/*.razor` - Reference implementations
- **Styling:** 
  - `src/MIMM.Frontend/wwwroot/css/app.css` - Global utilities only
  - `src/MIMM.Frontend/wwwroot/css/design-tokens.css` - CSS variables
  - `src/MIMM.Frontend/wwwroot/css/animations.css` - Animation definitions

### Component Structure Pattern

All Razor pages follow this structure:

```razor
@page "/page-name"
@using MIMM.Shared.Dtos
@using MIMM.Frontend.Services
@inject IExampleService ExampleService
@inject ISnackbar Snackbar

<PageTitle>Page Name - MIMM</PageTitle>
<LiveRegion Message="@_liveRegionMessage" AriaLive="polite" />

<MudContainer MaxWidth="MaxWidth.Large" Class="py-6">
    <MudStack Spacing="4">
        <!-- Content here using MudBlazor components -->
    </MudStack>
</MudContainer>

@code { /* Logic here */ }
```

### Styling Hierarchy (Highest → Lowest Specificity)

1. **Inline `Style` property** - Dynamic/unique styling (gradients, custom colors)
2. **Component `Class`** - MudBlazor utility classes only (`pa-6`, `mb-4`, `text-white`)
3. **Component parameters** - `Variant`, `Color`, `Size`, `Typo`, `Elevation`
4. **CSS from app.css** - Global utilities and animations ONLY

### Best Practices

✅ **DO:**
- Use `<MudPaper>` instead of `<div>`
- Use `<MudStack>` instead of flexbox divs
- Use `<MudText>` for all text content
- Use inline `Style` for dynamic styling
- Use design tokens from CSS variables
- Import docs/MUDBLAZOR_GUIDE.md before starting any UI work

❌ **DON'T:**
- Create CSS classes for component-specific styling
- Use raw HTML tags for layout
- Mix HTML and MudBlazor in same container
- Create .razor.css files (use app.css global styles instead)
- Use `!important` flag in styles

### Common Components

- **Layout:** `MudContainer`, `MudStack`, `MudGrid`, `MudItem`, `MudPaper`
- **Text:** `MudText` (with `Typo` property)
- **Forms:** `MudTextField`, `MudSelect`, `EditForm`, `MudButton`
- **Feedback:** `MudAlert`, `MudSnackbar` (via `ISnackbar`)
- **Cards:** `MudCard`, `MudCardHeader`, `MudCardContent`, `MudCardActions`
- **Icons:** `MudIcon` + `Icons.Material.Filled.*`

### Reference Pages

- **Dashboard.razor** - Header with gradient, stat cards, grid layout
- **Login.razor** - Form handling, validation, alerts
- **Analytics.razor** - Charts, complex layouts, loading states
- **Friends.razor** - List operations, dialogs, filtering

---

## Běžné příkazy

### Build & Restore

```bash
dotnet restore          # Obnovení závislostí (z rootu repozitáře)
dotnet build           # Build celého solution (z rootu repozitáře)
dotnet clean           # Vyčištění build artefaktů
```

### Spuštění aplikace

```bash
dotnet run --project src/Application.Web/Application.Web.csproj   # Z rootu repozitáře
cd src/Application.Web && dotnet run                               # Ze složky Web projektu
```

### Testování

```bash
dotnet test            # Spuštění všech testů (z rootu repozitáře)
dotnet test --no-build # Spuštění testů bez rebuildu
```

### Vývoj

- Aplikace běží na: `http://localhost:5150` (výchozí)
- OpenAPI endpoint (pouze dev): `/openapi/v1.json`
- Weather API endpoint: `/api/weatherforecast`

## Ladění (Debugging)

### VS Code

Repozitář obsahuje `.vscode/launch.json` konfiguraci pro ladění:

- **Launch Web API** - Spustí Application.Web s připojeným debuggerem
- Breakpointy lze nastavit v jakémkoliv `.cs` souboru
- Použij F5 pro spuštění ladění z VS Code

### CLI Debugging

```bash
# Spuštění s podporou debuggeru
dotnet run --project src/Application.Web/Application.Web.csproj --debug

# Připojení k běžícímu procesu
dotnet attach <process-id>
```
