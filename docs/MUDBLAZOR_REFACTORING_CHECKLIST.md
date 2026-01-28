# MudBlazor Refactoring Checklist - All Pages

**Status:** Dashboard refactored ✅  
**Target:** All MIMM.Frontend pages use MudBlazor-first approach  
**Reference:** [docs/MUDBLAZOR_GUIDE.md](./docs/MUDBLAZOR_GUIDE.md)

---

## Pages Status

### ✅ Completed

- [x] **Dashboard.razor** - Completed 2026-01-28
  - Gradient header with MudPaper + inline Style
  - Stat cards with MudGrid/MudItem
  - Quick actions with MudStack
  - Music search with MudPaper

- [x] **Login.razor** - Already uses MudBlazor
  - Form with EditForm + MudTextField
  - Error alerts with MudAlert
  - Buttons with MudButton

- [x] **Analytics.razor** - Already uses MudBlazor
  - Summary cards with MudCard
  - Charts with MudChart
  - Grid layout with MudGrid
  - Loading states with MudSkeleton

- [x] **YearlyReport.razor** - Already uses MudBlazor
  - Stats cards with MudPaper
  - Icons with MudIcon
  - Navigation with MudButton

- [x] **Friends.razor** - Already uses MudBlazor
  - Paper containers with MudPaper
  - Text fields with MudTextField
  - Buttons with MudButton
  - List operations with MudList

- [x] **ExportImport.razor** - Already uses MudBlazor
  - Paper sections with MudPaper
  - Progress indicators with MudProgressCircular
  - Buttons with MudButton

### 🔄 Review & Verify

- [ ] **Index.razor** - Redirect page (no styling needed)
  - Status: ✅ Already minimal (just redirect)

---

## Per-Page Checklist

When reviewing/refactoring each page, verify:

### Layout Structure
- [ ] Uses `<MudContainer MaxWidth="MaxWidth.Large">`
- [ ] Uses `<MudStack Spacing="4">` for vertical rhythm
- [ ] Uses `<MudGrid Spacing="3">` for complex layouts
- [ ] No raw `<div>` tags for layout

### Components
- [ ] All text content in `<MudText>` (with proper `Typo`)
- [ ] All buttons are `<MudButton>` with `Color`, `Variant`, `Size`
- [ ] All containers are `<MudPaper>` or `<MudCard>`
- [ ] All input fields are `<MudTextField>` or `<MudSelect>`
- [ ] All alerts are `<MudAlert>` with proper `Severity`

### Styling
- [ ] Gradient/custom colors use inline `Style="..."`
- [ ] Spacing uses utility classes (`pa-6`, `mb-4`, `mx-auto`)
- [ ] Typography uses `Typo` property, not CSS
- [ ] No component-specific CSS classes in app.css
- [ ] No `!important` flags in styles

### Accessibility
- [ ] Page has `<PageTitle>`
- [ ] Page has `<LiveRegion Message="..." AriaLive="..." />`
- [ ] Forms use proper `For="@(() => model.Property"` binding
- [ ] Buttons have descriptive text or aria-labels
- [ ] Icons have labels (not standalone)

### Responsiveness
- [ ] Grid items specify `xs`, `sm`, `md`, `lg` breakpoints
- [ ] Text size changes appropriately on mobile
- [ ] No fixed widths (use `FullWidth` on buttons/inputs)
- [ ] Padding uses responsive classes (`pa-responsive`)

---

## Example: Before & After

### ❌ BEFORE (Raw HTML/CSS)

```razor
<div class="dashboard-header">
    <h3 class="dashboard-title">Welcome back</h3>
    <p class="dashboard-subtitle">Your journey continues</p>
    <div class="dashboard-actions">
        <button class="btn btn-primary">New Entry</button>
    </div>
</div>

<div class="stats-grid">
    <div class="stat-card" style="background: #0ea5e9;">
        <h4>123</h4>
        <p>Entries</p>
    </div>
</div>
```

### ✅ AFTER (MudBlazor)

```razor
<MudPaper Class="pa-8 mb-6" Elevation="2" 
          Style="background: linear-gradient(135deg, #667eea 0%, #d946ef 100%); border-radius: 16px;">
    <MudStack Spacing="3">
        <MudText Typo="Typo.h3" Class="text-white" Style="font-weight: 800;">Welcome back</MudText>
        <MudText Typo="Typo.subtitle1" Class="text-white">Your journey continues</MudText>
        
        <MudStack Row="true" Spacing="2">
            <MudButton Variant="Variant.Filled" Color="Color.Surface">
                New Entry
            </MudButton>
        </MudStack>
    </MudStack>
</MudPaper>

<MudGrid Spacing="3" Class="mb-6">
    <MudItem xs="12" sm="6" md="4">
        <MudPaper Elevation="2" Class="pa-6" 
                  Style="background: linear-gradient(135deg, #0ea5e9 0%, #06b6d4 100%); border-radius: 16px;">
            <MudText Typo="Typo.h4" Class="text-white">123</MudText>
            <MudText Typo="Typo.body2" Class="text-white">Entries</MudText>
        </MudPaper>
    </MudItem>
</MudGrid>
```

---

## Tools & Resources

1. **MudBlazor Documentation** → https://mudblazor.com/
2. **Design Tokens** → `src/MIMM.Frontend/wwwroot/css/design-tokens.css`
3. **Global Utilities** → `src/MIMM.Frontend/wwwroot/css/app.css`
4. **Animations** → `src/MIMM.Frontend/wwwroot/css/animations.css`

---

## Implementation Strategy

**For new features/refactoring:**

1. Read [docs/MUDBLAZOR_GUIDE.md](./docs/MUDBLAZOR_GUIDE.md) first
2. Find similar component in existing pages (Dashboard, Analytics, Friends)
3. Copy the MudBlazor pattern from reference page
4. Adjust colors/sizing as needed
5. Verify: No raw HTML, proper spacing, responsive
6. Test on mobile (Ctrl+Shift+M in browser)

---

## Questions?

Refer to:
- [docs/MUDBLAZOR_GUIDE.md](./docs/MUDBLAZOR_GUIDE.md) - Comprehensive reference
- [AGENTS.md](./AGENTS.md#-frontend-ui-development-mudblazor) - Agent instructions
- Existing page examples (Dashboard.razor is best practice)
- MudBlazor official docs: https://mudblazor.com/

**Bottom line:** If it can be done with a MudBlazor component, don't use raw HTML.
