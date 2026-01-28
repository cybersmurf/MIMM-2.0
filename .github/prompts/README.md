# MIMM Prompts & Guidelines

Reusable prompts and guidelines for MIMM 2.0 development with AI agents.

## 📚 Available Prompts

### 1. **feature-implementation.prompt.md**
Implement new features with complete backend → frontend flow.
- Backend: Services, Controllers, DTOs
- Frontend: MudBlazor components, pages
- Tests: Unit + Integration
- Docs: README updates

**Contains MudBlazor-first rules** for all frontend components.

### 2. **release-notes.prompt.md**
Generate professional release notes from commits.

### 3. **e2e-tests-maintenance.prompt.md**
Maintain and debug E2E tests.

### 4. **ci-fix.prompt.md**
Fix CI/CD pipeline issues and GitHub Actions workflows.

### 5. **security-hardening.prompt.md**
Implement security improvements (JWT, encryption, validation).

### 6. **ef-migrations-review.prompt.md**
Review and apply Entity Framework Core migrations safely.

### 7. **api-contract-review.prompt.md**
Review API contracts and DTO compatibility.

### 8. **markdown-linting.prompt.md**
Lint and fix markdown documentation files.

---

## 🎨 MudBlazor Standards

**All frontend development must follow MudBlazor-first approach.**

### Quick Reference

✅ **DO:**
- Use MudBlazor components (MudPaper, MudStack, MudGrid, MudButton, etc.)
- Use inline `Style` for dynamic/unique styling
- Use `Class` for utility classes only (pa-6, mb-4, text-white)
- Use component parameters (Color, Typo, Variant, Size, Elevation)

❌ **DON'T:**
- Use raw HTML (`<div>`, `<p>`, `<h1>`, `<button>`)
- Create CSS classes for component styling
- Use `!important` in styles
- Create .razor.css scoped stylesheets

### Full Guide

See: [docs/MUDBLAZOR_GUIDE.md](../docs/MUDBLAZOR_GUIDE.md)

### Reference Implementation

Dashboard.razor is the best practice implementation with:
- Gradient header using MudPaper + inline Style
- Stat cards using MudGrid/MudItem
- Quick actions using MudStack
- Proper spacing hierarchy

---

## 📋 When to Use These Prompts

| Task | Prompt | Link |
|------|--------|------|
| Building a new feature | feature-implementation.prompt.md | [View](./feature-implementation.prompt.md) |
| Releasing a version | release-notes.prompt.md | [View](./release-notes.prompt.md) |
| Testing issues | e2e-tests-maintenance.prompt.md | [View](./e2e-tests-maintenance.prompt.md) |
| CI/CD problems | ci-fix.prompt.md | [View](./ci-fix.prompt.md) |
| Security work | security-hardening.prompt.md | [View](./security-hardening.prompt.md) |
| Database changes | ef-migrations-review.prompt.md | [View](./ef-migrations-review.prompt.md) |
| API design | api-contract-review.prompt.md | [View](./api-contract-review.prompt.md) |
| Documentation | markdown-linting.prompt.md | [View](./markdown-linting.prompt.md) |

---

## 🚀 How to Use

1. **Select appropriate prompt** based on your task
2. **Read the prompt file** for detailed instructions
3. **Provide required inputs** (feature summary, files, etc.)
4. **Follow the plan** outlined in the prompt
5. **Reference examples** from existing code

---

## 📖 Related Documentation

- [AGENTS.md](../AGENTS.md) - Main agent instructions
- [CODE_REVIEW_PLAN.md](../docs/CODE_REVIEW_PLAN.md) - Code review guidelines
- [MUDBLAZOR_GUIDE.md](../docs/MUDBLAZOR_GUIDE.md) - Complete MudBlazor reference
- [MUDBLAZOR_REFACTORING_CHECKLIST.md](../docs/MUDBLAZOR_REFACTORING_CHECKLIST.md) - Page status and checklist

---

## 💡 Tips

- **Before coding:** Read relevant prompt completely
- **During coding:** Reference existing implementations
- **After coding:** Verify against checklist in prompt
- **Questions:** Check MUDBLAZOR_GUIDE.md first
- **Stuck?** Look at similar existing code in the codebase

---

**Last Updated:** 2026-01-28  
**Version:** 1.0
