# Prompt: Implement Feature (MIMM Pattern)

## ⚠️ CRITICAL: Branch Policy

**NEVER commit directly to `main` branch.**

All changes MUST follow this workflow:

```bash
# 1. Create feature branch
git checkout -b feature/your-feature-name

# 2. Make changes locally
# ... edit files ...

# 3. Test thoroughly
dotnet build MIMM.sln --configuration Release
dotnet test MIMM.sln --configuration Release

# 4. Commit to feature branch (NOT main)
git add .
git commit -m "type(scope): description"

# 5. Push to feature branch
git push origin feature/your-feature-name

# 6. Create Pull Request on GitHub
# Wait for code review before merging to main
```

**Branch naming:** `feature/descriptive-name` (not `feature/wip` or `feature/test`)

## Input

- Feature summary (one paragraph)
- Affected layers (Backend/Frontend/Shared)
- API endpoints + DTOs

## Plan

1. Update Shared DTOs (records, required).
2. Backend: Service + Controller + DI wiring.
3. Frontend: Service client + Pages/Components (MudBlazor-first).
4. Tests: Unit + Integration for core flow.
5. Docs: Update README sections.

## Rules

### Backend & Shared
- Use C# 13, net9.0, nullable enabled.
- Keep changes minimal and idiomatic.
- Propagate CancellationToken.
- Validate DTOs with `required` modifier.
- Follow clean architecture (Services → Controllers).

### Frontend Components (MudBlazor)

**Golden Rule:** ALWAYS use MudBlazor components, NOT raw HTML.

- Use `<MudContainer>`, `<MudStack>`, `<MudGrid>` for layouts.
- Use `<MudText>` for text, `<MudButton>` for buttons.
- Style hierarchy:
  1. **Inline `Style`** - Dynamic/unique styling (gradients, custom colors)
  2. **`Class`** - MudBlazor utilities only (`pa-6`, `mb-4`, `text-white`)
  3. **Component params** - `Color`, `Typo`, `Variant`, `Size`, `Elevation`
  4. **CSS** - Global utilities and animations ONLY

- **Reference:** [docs/MUDBLAZOR_GUIDE.md](../../docs/MUDBLAZOR_GUIDE.md)
- **Examples:** Dashboard.razor, Login.razor, Analytics.razor

### Page Template

```razor
@page "/feature-name"
@using MIMM.Shared.Dtos
@using MIMM.Frontend.Services
@inject IFeatureService FeatureService
@inject ISnackbar Snackbar

<PageTitle>Feature Name - MIMM</PageTitle>
<LiveRegion Message="@_liveRegionMessage" AriaLive="polite" />

<MudContainer MaxWidth="MaxWidth.Large" Class="py-6">
    <MudStack Spacing="4">
        <!-- Page Header -->
        <MudStack Spacing="1">
            <MudText Typo="Typo.h4">Feature Title</MudText>
            <MudText Typo="Typo.body2" Color="Color.Secondary">Description</MudText>
        </MudStack>

        <!-- Main Content -->
        <MudGrid Spacing="3">
            <MudItem xs="12" md="8">Primary content</MudItem>
            <MudItem xs="12" md="4">Sidebar</MudItem>
        </MudGrid>
    </MudStack>
</MudContainer>

@code {
    private string? _liveRegionMessage;
    
    protected override async Task OnInitializedAsync()
    {
        _liveRegionMessage = "Loading...";
        // Load data
    }
}
```

Input:

- Feature summary (one paragraph)
- Affected layers (Backend/Frontend/Shared)
- API endpoints + DTOs

Plan:

1. Update Shared DTOs (records, required).
2. Backend: Service + Controller + DI wiring.
3. Frontend: Service client + Pages/Components.
4. Tests: Unit + Integration for core flow.
5. Docs: Update README sections.

Rules:

- Use C# 13, net9.0, nullable enabled.
- Keep changes minimal and idiomatic.
- Propagate CancellationToken.
