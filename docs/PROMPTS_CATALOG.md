# Copilot Prompts Catalog

<!-- markdownlint-disable MD013 -->
Přehled opakovatelných promptů pro Copilota. Prompty jsou uloženy v `.github/prompts/` a
lze je používat při běžných úlohách vývoje, testování a údržby.
<!-- markdownlint-enable MD013 -->

| Prompt | Soubor | Popis | Doporučená situace |
|--------|--------|-------|-------------------|
| **Release Notes** | `release-notes.prompt.md` | Strukturované generování poznámek k vydání | Před taggováním verze |
| **E2E Tests Maintenance** | `e2e-tests-maintenance.prompt.md` | Kontrola stability testů, diagnostika | Údržba test suite |
| **CI Fix & Optimize** | `ci-fix.prompt.md` | Diagnostika CI failů, optimalizace | Když CI selže |
| **Feature Implementation** | `feature-implementation.prompt.md` | Strukturovaný plán implementace | Nová feature |
| **Security Hardening** | `security-hardening.prompt.md` | Kontrola AuthN/AuthZ, CORS, JWT, input | Bezpečnostní audit |
| **EF Migrations Review** | `ef-migrations-review.prompt.md` | Validace migrací a konsistentnosti | Code review DB schématu |
| **API Contract Review** | `api-contract-review.prompt.md` | Validace API surface: DTOs, endpoints | Přidání endpointu |
| **Markdown Linting** | `markdown-linting.prompt.md` | Kontrola a oprava MD souborů (v0.40.0) | Audit dokumentace |
| **API Contract Review** | `api-contract-review.prompt.md` | Validace API surface: DTOs, endpoints | Přidání endpointu |

## Jak používat

1. Zkopíruj obsah promptu (např. z `release-notes.prompt.md`).
2. Pošli ho Copilotovi spolu s kontextem (commits, kód, chybové zprávy).
3. Copilot provede kroku podle instrukcí a poskytne strukturovaný výstup.

Příklad:

```markdown
[Copilotovi] Zde je prompt pro E2E Tests Maintenance:
[vložit obsah e2e-tests-maintenance.prompt.md]

Máme nový failure v EntriesE2ETests.Entries_CRUD_Flow_Succeeds v testu na
line 94. Jaká je diagnóza a jak to napravíme?
```

## Tipy

- Prompty si můžeš uložit jako **Favorites** v Copilot Chat pro rychlejší přístup.
- Přizpůsob je svému workflow; jsou to šablony, ne dogma.
- Při release cyclu: Release Notes → Security Hardening → EF Migrations Review → API Contract Review → CI Fix.
