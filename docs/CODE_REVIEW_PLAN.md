# Code Review Implementation Plan - MIMM 2.0

**Datum:** 25. ledna 2026  
**Priorita:** Week 1 Immediate Actions (z Context7 Analysis)  
**Cíl:** Zvýšení performance, bezpečnosti a kvality kódu

---

## ⚠️ BRANCH POLICY

**All code changes MUST follow feature branch workflow:**

1. Create feature branch: `git checkout -b feature/your-feature`
2. Make changes and test locally
3. Push to feature branch: `git push origin feature/your-feature`
4. Create Pull Request on GitHub
5. Wait for code review and CI checks
6. Merge to main ONLY after approval

**NEVER commit directly to main branch.**

See [AGENTS.md § Branch Management](../AGENTS.md#-branch-management--code-safety) for detailed workflow.

---

## 📋 PLÁN IMPLEMENTACE

### Fáze 1: EF Core Query Optimization (High Impact)

#### 1.1 Add `.AsNoTracking()` to Read-Only Queries

- **Soubory:** `src/MIMM.Backend/Services/EntryService.cs`
- **Benefit:** +15-20% performance improvement na read-heavy operacích
- **Změny:**
  - `GetEntriesPagedAsync()` → Add `.AsNoTracking()`
  - `GetEntryByIdAsync()` → Add `.AsNoTracking()` (if read-only)
  - `GetEntriesByUserIdAsync()` (if exists) → Add `.AsNoTracking()`
- **Impact:** MEDIUM (affects pagination, entry list loading)
- **Risk:** LOW (only affects read operations, no write risk)

#### 1.2 Fix Soft-Delete Query Filter Warning

- **Soubor:** `src/MIMM.Backend/Data/ApplicationDbContext.cs`
- **Issue:** "Entity 'User' has global query filter but is required nav..."
- **Řešení:** Apply matching filter to JournalEntry and LastFmToken entities
- **Impact:** MEDIUM (removes EF Core warnings, improves clarity)
- **Risk:** LOW (refines filtering logic without breaking functionality)

### Fáze 2: JWT Security Hardening (Critical)

#### 2.1 Add "jti" Claim to JWT Tokens

- **Soubor:** `src/MIMM.Backend/Services/AuthService.cs`
- **Benefit:** Individual token tracking for revocation support
- **Změny:**
  - `GenerateAccessToken()` → Add `Guid.NewGuid()` as "jti" claim
  - Update `GenerateRefreshToken()` similarly
- **Impact:** HIGH (enables future token revocation mechanism)
- **Risk:** LOW (additive change, no breaking changes)

### Fáze 3: Frontend Component Refactoring - MudBlazor Best Practices

#### 3.1 Ensure All Pages Use MudBlazor Components

**IMPORTANT:** All frontend pages must follow MudBlazor-first approach.

- **Reference Guide:** [docs/MUDBLAZOR_GUIDE.md](./MUDBLAZOR_GUIDE.md)
- **Status:** ✅ Dashboard, Login, Analytics, Friends, YearlyReport, ExportImport
- **Principle:** Use MudBlazor components as primary UI building blocks
- **No Raw HTML:** Avoid `<div>`, `<span>`, `<p>`, `<h1>` for layout

**When Creating/Updating Pages:**

1. **Import MudBlazor services:**
   ```razor
   @inject ISnackbar Snackbar
   @inject IDialogService DialogService
   ```

2. **Use container hierarchy:**
   ```razor
   <MudContainer MaxWidth="MaxWidth.Large" Class="py-6">
       <MudStack Spacing="4">
           <!-- Page Header -->
           <MudStack Spacing="1">
               <MudText Typo="Typo.h4">Page Title</MudText>
               <MudText Typo="Typo.body2" Color="Color.Secondary">Subtitle</MudText>
           </MudStack>

           <!-- Main Content -->
           <MudGrid Spacing="3">
               <MudItem xs="12" md="8">Content</MudItem>
               <MudItem xs="12" md="4">Sidebar</MudItem>
           </MudGrid>
       </MudStack>
   </MudContainer>
   ```

3. **Styling hierarchy:**
   - Use **inline `Style`** for unique/dynamic styles (gradients, custom colors)
   - Use **`Class`** for MudBlazor utilities only (pa-6, mb-4, text-white)
   - Use **component parameters** (Color, Typo, Variant, Size, Elevation)
   - Use **CSS** only for global utilities and animations

4. **Examples:**
   - Header with gradient: See [Dashboard.razor](../src/MIMM.Frontend/Pages/Dashboard.razor)
   - Form handling: See [Login.razor](../src/MIMM.Frontend/Pages/Login.razor)
   - Charts and analytics: See [Analytics.razor](../src/MIMM.Frontend/Pages/Analytics.razor)

#### 3.2 Extract MusicTrackCard Component

- **Soubory:**
  - NEW: `src/MIMM.Frontend/Components/MusicTrackCard.razor`
  - MODIFY: `src/MIMM.Frontend/Components/MusicSearchBox.razor`
- **Benefit:** Code reuse, better separation of concerns
- **Změny:**
  - Create reusable MusicTrackCard.razor component using MudBlazor
  - Refactor MusicSearchBox to use MusicTrackCard
  - Add `@code` section with source label helper
- **Impact:** MEDIUM (code quality, maintainability)
- **Risk:** LOW (internal refactoring, UI identical)

### Fáze 4: Verification & Testing

#### 4.1 Build & Test Validation

- Run `dotnet build` → Verify 0 errors
- Run `dotnet test` → Verify all 43 tests pass
- Manual testing: Music search, pagination, auth flow
- Frontend: Hard refresh (Ctrl+Shift+R) and verify all pages render correctly
- **Impact:** CRITICAL (ensures no regressions)

---

## 🎯 IMPLEMENTAČNÍ POŘADÍ

**Čas:** ~2-3 hodiny za veškeré implementace

1. **15 min:** Fáze 1.1 - Add `.AsNoTracking()`
2. **20 min:** Fáze 1.2 - Fix soft-delete filter
3. **20 min:** Fáze 2.1 - Add "jti" claim
4. **25 min:** Fáze 3.1 - Verify MudBlazor compliance on all pages
5. **25 min:** Fáze 3.2 - Extract MusicTrackCard
6. **20 min:** Fáze 4.1 - Build, test, validate

---

## 📊 OČEKÁVANÝ DOPAD

### Performance

- Query optimization: **+15-20% faster pagination**
- No negative impacts expected

### Security

- Token tracking enabled for future revocation
- Compliant with JWT best practices (RFC 7519)

### Code Quality

- Reduced duplication (MusicTrackCard extraction)
- Better separation of concerns
- Cleaner EF Core configuration

### Test Coverage

- All 43 tests should continue passing
- No new test additions required (refactoring only)

---

## ✅ Definition OF DONE

- [x] Plan created and documented
- [ ] All code changes implemented
- [ ] Build passes without errors/warnings
- [ ] All tests passing (43/43)
- [ ] Git commit with detailed message
- [ ] Code review summary prepared

---

**Status:** READY FOR IMPLEMENTATION ✅
