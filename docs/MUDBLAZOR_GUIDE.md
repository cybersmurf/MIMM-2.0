# MudBlazor Integration Guide

**Version:** 1.0 | **Updated:** 2026-01-28

> **Golden Rule:** Always use MudBlazor components first. Raw HTML/CSS should only be used for global utilities and animations, not component styling.

## 📋 Table of Contents

1. [Core Principles](#core-principles)
2. [Page Layout Pattern](#page-layout-pattern)
3. [Component Styling Hierarchy](#component-styling-hierarchy)
4. [Common Patterns](#common-patterns)
5. [Dos and Don'ts](#dos-and-donts)
6. [Typography](#typography)
7. [Colors & Theming](#colors--theming)
8. [Spacing & Layout](#spacing--layout)
9. [Forms](#forms)
10. [Cards & Containers](#cards--containers)
11. [Buttons](#buttons)
12. [Alerts & Feedback](#alerts--feedback)
13. [Mobile Responsiveness](#mobile-responsiveness)

---

## Core Principles

### 1. **Component-First Approach**

Use MudBlazor components as the foundation. Avoid raw HTML tags.

```razor
<!-- ✅ GOOD -->
<MudPaper Class="pa-4">
    <MudText Typo="Typo.h4">Title</MudText>
</MudPaper>

<!-- ❌ BAD -->
<div class="paper">
    <h4>Title</h4>
</div>
```

### 2. **Inline Styling Over CSS Classes**

Use component properties and inline `Style` for component-specific styling. Reserve CSS for:
- Global utilities (spacing, visibility)
- Animations
- Design tokens
- Cross-component patterns

```razor
<!-- ✅ GOOD: Component properties + inline Style -->
<MudPaper Elevation="2" Class="pa-6" 
          Style="background: linear-gradient(135deg, #667eea 0%, #d946ef 100%); border-radius: 16px;">
    <MudText Typo="Typo.h3" Class="text-white" Style="font-weight: 800;">Welcome</MudText>
</MudPaper>

<!-- ❌ BAD: Relying on CSS classes -->
<div class="dashboard-header">
    <h3 class="dashboard-title">Welcome</h3>
</div>
<!-- with corresponding CSS file -->
```

### 3. **Leverage Built-In Properties**

MudBlazor components have rich parameters. Use them instead of CSS:

```razor
<!-- ✅ GOOD: Use component parameters -->
<MudButton Variant="Variant.Filled" Color="Color.Primary" Size="Size.Large" FullWidth />

<!-- ❌ BAD: CSS class overrides -->
<MudButton Class="custom-button-large" Style="...many overrides..." />
```

### 4. **Type Safety Over Strings**

Use C# enums and intellisense instead of string classes:

```razor
<!-- ✅ GOOD: Type-safe properties -->
<MudButton Variant="Variant.Filled" Color="Color.Secondary" Size="Size.Medium" />

<!-- ❌ BAD: String values -->
<MudButton Variant="filled" Color="secondary" Class="size-medium" />
```

---

## Page Layout Pattern

Standard page structure for all MIMM Frontend pages:

```razor
@page "/page-name"
@using MIMM.Shared.Dtos
@using MIMM.Frontend.Services
@inject IExampleService ExampleService
@inject ISnackbar Snackbar
@inject IAuthStateService AuthState
@inject NavigationManager Navigation

<PageTitle>Page Name - MIMM</PageTitle>

<LiveRegion Message="@_liveRegionMessage" AriaLive="polite" />

<MudContainer MaxWidth="MaxWidth.Large" Class="py-6">
    <MudStack Spacing="4">
        
        <!-- Page Header -->
        <MudStack Spacing="1">
            <MudText Typo="Typo.h4">Page Title</MudText>
            <MudText Typo="Typo.body2" Color="Color.Secondary">Optional subtitle or description</MudText>
        </MudStack>

        <!-- Main Content -->
        <MudGrid Spacing="3">
            <MudItem xs="12" md="8">
                <!-- Primary content here -->
            </MudItem>
            <MudItem xs="12" md="4">
                <!-- Sidebar/secondary content here -->
            </MudItem>
        </MudGrid>

    </MudStack>
</MudContainer>

@code {
    private string? _liveRegionMessage;
    // ... component logic
}
```

**Key Points:**
- Use `MudContainer` with `MaxWidth.Large` for consistent max-width
- Use `MudStack` with `Spacing="4"` for vertical rhythm
- Use `MudGrid` with responsive breakpoints (`xs`, `sm`, `md`, `lg`, `xl`)
- Always include `<PageTitle>` and `<LiveRegion>` for accessibility
- Load authentication state in `OnInitializedAsync`

---

## Component Styling Hierarchy

**Specificity order** (highest to lowest):

1. **Inline `Style` property** - Component-specific dynamic styling (gradients, custom colors)
2. **Component `Class` property** - MudBlazor utility classes and custom utility classes only
3. **Component parameters** (`Variant`, `Color`, `Size`, `Typo`, etc.)
4. **CSS from app.css** - Global utilities and animations ONLY

```razor
<!-- Example: Proper hierarchy -->
<MudPaper 
    Class="pa-6 mb-4"                  <!-- 2. Utility classes (padding, margin) -->
    Elevation="2"                      <!-- 3. Component property -->
    Style="background: linear-gradient(135deg, #667eea 0%, #d946ef 100%); border-radius: 16px;">  <!-- 1. Inline styling -->
    <MudText 
        Typo="Typo.h4"                 <!-- 3. Typography property -->
        Class="text-white"             <!-- 2. Utility class -->
        Style="font-weight: 800; letter-spacing: -1px;">  <!-- 1. Inline styling -->
        Dashboard Header
    </MudText>
</MudPaper>
```

---

## Common Patterns

### Header Section

```razor
<MudPaper Class="pa-8 mb-6" Elevation="2" 
          Style="background: linear-gradient(135deg, #667eea 0%, #d946ef 100%); border-radius: 16px;">
    <MudStack Spacing="3">
        <MudText Typo="Typo.h3" Class="text-white" Style="font-weight: 800;">Welcome back 👋</MudText>
        <MudText Typo="Typo.subtitle1" Class="text-white" Style="opacity: 0.95;">Subtitle text</MudText>
        
        <MudStack Row="true" Spacing="2" Style="flex-wrap: wrap; margin-top: 16px;">
            <MudButton Variant="Variant.Filled" Color="Color.Surface" 
                       Style="color: #667eea; background: rgba(255, 255, 255, 0.95) !important;">
                Primary Action
            </MudButton>
            <MudButton Variant="Variant.Outlined" Color="Color.Inherit" 
                       Style="color: white; border-color: rgba(255, 255, 255, 0.5);">
                Secondary Action
            </MudButton>
        </MudStack>
    </MudStack>
</MudPaper>
```

### Stat/Info Cards

```razor
<MudGrid Spacing="3" Class="mb-6">
    <MudItem xs="12" sm="6" md="4">
        <MudPaper Elevation="2" Class="pa-6" 
                  Style="background: linear-gradient(135deg, #0ea5e9 0%, #06b6d4 100%); border-radius: 16px; min-height: 180px; display: flex; flex-direction: column; justify-content: space-between;">
            <div>
                <MudText Typo="Typo.caption" Class="text-white" Style="font-weight: 700; opacity: 0.9;">Label</MudText>
                <MudText Typo="Typo.h4" Class="text-white" Style="font-weight: 800; margin-top: 8px;">123.45</MudText>
            </div>
            <MudText Typo="Typo.body2" Class="text-white" Style="opacity: 0.85;">Description</MudText>
        </MudPaper>
    </MudItem>
    
    <!-- More items... -->
</MudGrid>
```

### Panel with Actions

```razor
<MudPaper Elevation="1" Class="pa-6 mb-6" Style="border-radius: 16px;">
    <MudText Typo="Typo.h6" Class="mb-4" Style="font-weight: 700;">⚡ Quick Actions</MudText>
    <MudStack Spacing="2">
        <MudButton Variant="Variant.Filled" Color="Color.Primary" FullWidth 
                   StartIcon="@Icons.Material.Filled.Add" OnClick="HandleClick">
            Action Button
        </MudButton>
        <MudButton Variant="Variant.Outlined" Color="Color.Primary" FullWidth 
                   StartIcon="@Icons.Material.Filled.Edit">
            Secondary Action
        </MudButton>
    </MudStack>
</MudPaper>
```

---

## Dos and Don'ts

| ✅ DO | ❌ DON'T |
|--------|----------|
| Use `<MudPaper>` for containers | Use `<div class="container">` |
| Use `<MudStack>` for layouts | Use `<div class="flex">` or `<div style="display: flex">` |
| Use `<MudText>` for all text | Use `<p>`, `<span>`, `<h1>` |
| Use `<MudButton>` for buttons | Use `<button>` or `<a>` |
| Use `<MudTextField>` for inputs | Use `<input>` |
| Use `<MudSelect>` for dropdowns | Use `<select>` |
| Use `<MudCard>` for card content | Use `<div class="card">` |
| Use inline `Style` for dynamic colors | Create CSS classes for every style variation |
| Create CSS utility classes | Create CSS component-specific classes |
| Use `Class="text-white"` | Use CSS `color: white` |
| Use `Elevation="2"` | Use CSS `box-shadow` |

---

## Typography

MudBlazor typography system uses `Typo` property:

```razor
<MudText Typo="Typo.h1">Large Heading</MudText>
<MudText Typo="Typo.h2">Medium Heading</MudText>
<MudText Typo="Typo.h3">Small Heading</MudText>
<MudText Typo="Typo.h4">Subtitle Heading</MudText>
<MudText Typo="Typo.h5">Small Subtitle</MudText>
<MudText Typo="Typo.h6">Extra Small</MudText>
<MudText Typo="Typo.body1">Body text (default)</MudText>
<MudText Typo="Typo.body2">Small body text</MudText>
<MudText Typo="Typo.subtitle1">Subtitle 1</MudText>
<MudText Typo="Typo.subtitle2">Subtitle 2</MudText>
<MudText Typo="Typo.caption">Caption/label text</MudText>
<MudText Typo="Typo.button">Button text</MudText>
<MudText Typo="Typo.overline">Overline text</MudText>
```

**Custom styling:**

```razor
<!-- Inline styling with Typo -->
<MudText Typo="Typo.h3" Style="font-weight: 800; letter-spacing: -1px;">Custom Heading</MudText>

<!-- Color property -->
<MudText Typo="Typo.body2" Color="Color.Secondary">Secondary text</MudText>
<MudText Typo="Typo.body2" Color="Color.Error">Error text</MudText>
```

---

## Colors & Theming

### Predefined Colors

```razor
<!-- Using Color enum -->
<MudButton Color="Color.Primary">Primary</MudButton>
<MudButton Color="Color.Secondary">Secondary</MudButton>
<MudButton Color="Color.Tertiary">Tertiary</MudButton>
<MudButton Color="Color.Success">Success</MudButton>
<MudButton Color="Color.Warning">Warning</MudButton>
<MudButton Color="Color.Error">Error</MudButton>
<MudButton Color="Color.Info">Info</MudButton>
<MudButton Color="Color.Default">Default</MudButton>
<MudButton Color="Color.Surface">Surface</MudButton>
<MudButton Color="Color.Inherit">Inherit</MudButton>
```

### Custom Colors (Inline)

```razor
<!-- Inline hex colors -->
<MudText Style="color: #667eea;">Purple text</MudText>
<MudPaper Style="background: linear-gradient(135deg, #667eea 0%, #d946ef 100%);">
    Gradient background
</MudPaper>

<!-- CSS Variables from design-tokens.css -->
<MudText Style="color: var(--color-text-primary);">Primary text</MudText>
<MudPaper Style="background: var(--gradient-bg-primary);">
    Token-based gradient
</MudPaper>
```

---

## Spacing & Layout

### Padding & Margin

MudBlazor uses consistent spacing units (8px base):

```razor
<!-- Padding: pa-* (all), px-* (horizontal), py-* (vertical), pt-/pb-/ps-/pe-* (specific) -->
<MudPaper Class="pa-6">Padding: 24px all</MudPaper>
<MudPaper Class="px-4 py-8">Padding: 16px h, 32px v</MudPaper>

<!-- Margin: same pattern -->
<MudPaper Class="ma-6">Margin: 24px all</MudPaper>
<MudPaper Class="mx-auto mb-4">Margin: auto h, 16px bottom</MudPaper>

<!-- Spacing units: 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 16 -->
```

### Responsive Spacing

```razor
<!-- Responsive padding: pa-responsive is custom utility -->
<MudPaper Class="pa-responsive">Auto-adjusts on mobile</MudPaper>
```

See `app.css` for responsive breakpoint definitions.

### Stack Layout

```razor
<!-- Vertical stack (default) -->
<MudStack Spacing="3">
    <MudPaper>Item 1</MudPaper>
    <MudPaper>Item 2</MudPaper>
</MudStack>

<!-- Horizontal stack -->
<MudStack Row="true" Spacing="2" AlignItems="AlignItems.Center">
    <MudText>Left</MudText>
    <MudSpacer />
    <MudText>Right</MudText>
</MudStack>

<!-- Spacing: 0, 1, 2, 3, 4, 5, 6, 7, 8 -->
```

### Grid Layout

```razor
<!-- Responsive grid with xs/sm/md/lg/xl breakpoints -->
<MudGrid Spacing="3">
    <MudItem xs="12" sm="6" md="4">
        <!-- Full width on mobile, 50% on tablet, 33% on desktop -->
    </MudItem>
</MudGrid>
```

---

## Forms

### Text Input

```razor
<MudTextField @bind-Value="model.Name"
              Label="Full Name"
              Variant="Variant.Outlined"
              Margin="Margin.Normal"
              HelperText="Enter your full name"
              Clearable
              Class="mb-3" />
```

### Form with Validation

```razor
<EditForm Model="@model" OnValidSubmit="HandleSubmit">
    <DataAnnotationsValidator />
    
    <MudTextField @bind-Value="model.Email"
                  Label="Email"
                  Variant="Variant.Outlined"
                  For="@(() => model.Email)"
                  Class="mb-3" />
    
    <MudButton ButtonType="ButtonType.Submit" Variant="Variant.Filled" Color="Color.Primary">
        Submit
    </MudButton>
</EditForm>
```

### Select

```razor
<MudSelect @bind-Value="selectedValue" Label="Choose option" Variant="Variant.Outlined">
    <MudSelectItem Value="@("option1")">Option 1</MudSelectItem>
    <MudSelectItem Value="@("option2")">Option 2</MudSelectItem>
</MudSelect>
```

---

## Cards & Containers

### Card with Header

```razor
<MudCard Class="mb-4">
    <MudCardHeader>
        <CardHeaderContent>
            <MudText Typo="Typo.h6">Card Title</MudText>
        </CardHeaderContent>
        <CardHeaderActions>
            <MudIconButton Icon="@Icons.Material.Filled.Close" />
        </CardHeaderActions>
    </MudCardHeader>
    <MudCardContent>
        Card content here
    </MudCardContent>
    <MudCardActions>
        <MudButton Color="Color.Primary">Action</MudButton>
    </MudCardActions>
</MudCard>
```

### Paper (Lightweight Container)

```razor
<!-- Outlined paper -->
<MudPaper Outlined Class="pa-4">Light container</MudPaper>

<!-- Elevated paper -->
<MudPaper Elevation="2" Class="pa-4">Elevated container</MudPaper>

<!-- Custom styling -->
<MudPaper Class="pa-6" Style="background: linear-gradient(...); border-radius: 16px;">
    Styled container
</MudPaper>
```

---

## Buttons

### Button Types

```razor
<!-- Filled (default) -->
<MudButton Variant="Variant.Filled" Color="Color.Primary">Filled</MudButton>

<!-- Outlined -->
<MudButton Variant="Variant.Outlined" Color="Color.Primary">Outlined</MudButton>

<!-- Text/Ghost -->
<MudButton Variant="Variant.Text" Color="Color.Primary">Text</MudButton>

<!-- Sizes -->
<MudButton Size="Size.Small">Small</MudButton>
<MudButton Size="Size.Medium">Medium</MudButton>
<MudButton Size="Size.Large">Large</MudButton>

<!-- Full width -->
<MudButton FullWidth>Full Width</MudButton>

<!-- With icons -->
<MudButton StartIcon="@Icons.Material.Filled.Add">Add Item</MudButton>
<MudButton EndIcon="@Icons.Material.Filled.Download">Download</MudButton>

<!-- Disabled state -->
<MudButton Disabled="@isLoading">
    @if (isLoading)
    {
        <MudProgressCircular Size="Size.Small" Indeterminate Class="me-2" />
        <span>Loading...</span>
    }
    else
    {
        <span>Submit</span>
    }
</MudButton>
```

---

## Alerts & Feedback

### Snackbar (Toasts)

```razor
<!-- Inject ISnackbar -->
@inject ISnackbar Snackbar

// In code
Snackbar.Add("Success message", Severity.Success);
Snackbar.Add("Error message", Severity.Error);
Snackbar.Add("Warning message", Severity.Warning);
Snackbar.Add("Info message", Severity.Info);
```

### Alert Component

```razor
<MudAlert Severity="Severity.Success" Variant="Variant.Filled" Class="mb-4">
    Success message
</MudAlert>

<MudAlert Severity="Severity.Error" Variant="Variant.Outlined">
    Error message
</MudAlert>
```

---

## Mobile Responsiveness

### Grid Breakpoints

```razor
<MudItem xs="12" sm="6" md="4" lg="3">
    <!-- 12 cols on mobile, 6 on tablet, 4 on desktop, 3 on large -->
</MudItem>
```

Breakpoint sizes:
- `xs`: 0px (mobile)
- `sm`: 600px (tablet)
- `md`: 960px (desktop)
- `lg`: 1264px (large)
- `xl`: 1904px (extra large)

### Responsive Utilities

```razor
<!-- Hide on mobile -->
<div class="hide-sm"></div>

<!-- Show only on mobile -->
<div class="show-sm"></div>

<!-- Responsive spacing utility -->
<MudPaper Class="pa-responsive">Responsive padding</MudPaper>
```

---

## Best Practices Summary

✅ **DO:**
1. Use MudBlazor components as primary UI building blocks
2. Use inline `Style` property for dynamic/unique styling
3. Use `Class` for MudBlazor utility classes only
4. Use component parameters (`Color`, `Variant`, `Size`, `Typo`)
5. Create animations and global utilities in CSS
6. Use design tokens from `design-tokens.css`
7. Test responsiveness at all breakpoints
8. Use `MudStack` for layouts instead of raw flexbox
9. Use `MudGrid` for complex responsive layouts
10. Document custom components and styling patterns

❌ **DON'T:**
1. Create CSS classes for every component style variation
2. Use raw HTML tags (`<div>`, `<span>`, `<p>`, `<h1>`) for layout
3. Override component styles with `!important`
4. Create component-specific CSS files (use global app.css only)
5. Mix HTML and MudBlazor in the same container
6. Ignore accessibility (use `<LiveRegion>` and aria attributes)
7. Create deeply nested components without `Spacing` property
8. Hard-code colors instead of using tokens or parameters

---

## References

- [MudBlazor Official Docs](https://mudblazor.com/)
- [MIMM design tokens](../src/MIMM.Frontend/wwwroot/css/design-tokens.css)
- [MIMM animations](../src/MIMM.Frontend/wwwroot/css/animations.css)
- [Dashboard.razor example](../src/MIMM.Frontend/Pages/Dashboard.razor) - Best practice implementation

**For questions about MudBlazor components, consult:**
1. MudBlazor official documentation
2. Existing MIMM pages that use the same pattern
3. Component API reference in MudBlazor docs
