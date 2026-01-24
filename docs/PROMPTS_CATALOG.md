# Copilot Prompts Catalog

Přehled opakovatelných promptů pro Copilota. Prompty jsou uloženy v `.github/prompts/` a lze je používat při běžných úlohách vývoje, testování a údržby.

| Prompt | Soubor | Popis | Doporučená situace |
|--------|--------|-------|-------------------|
| **Release Notes** | `release-notes.prompt.md` | Strukturované generování poznámek k vydání dle typu (Features, Fixes, Docs, CI, apod.) | Před taggováním verze; agregace změn od posledního release |
| **E2E Tests Maintenance** | `e2e-tests-maintenance.prompt.md` | Kontrola stability integrečních testů, diagnostika pádů, refaktorování flakých testů | Při debug E2E testů; údržba test suite; před mergem PR s testy |
| **CI Fix & Optimize** | `ci-fix.prompt.md` | Diagnostika CI failů, cílení na solution file, matrix/cache, coverage, dokumentace | Když CI selže; optimalizace workflow; přidání nových OS do matrixu |
| **Feature Implementation** | `feature-implementation.prompt.md` | Strukturovaný plán implementace: Shared DTOs → Backend → Frontend → Tests → Docs | Nová feature; dodržení архитектурních vrstev; C#13 standardy |
| **Security Hardening** | `security-hardening.prompt.md` | Kontrola AuthN/AuthZ, CORS, JWT, headers, input validation, logging, dependency scan | Bezpečnostní audit; příprava na produkci; code review se zaměřením na bezpečnost |
| **EF Migrations Review** | `ef-migrations-review.prompt.md` | Validace migrací: konsistentnost s modelem, minimalizace, bezpečnost aplikace | Přezkum databázových změn; kontrola před aplicí migrace; code review DB schématu |
| **API Contract Review** | `api-contract-review.prompt.md` | Validace API surface: DTOs, endpoints, versioning, OpenAPI, error shapes | Přidání/změna endpointu; kontrola kompatibility; příprava API dokumentace |

## Jak používat

1. Zkopíruj obsah promptu (např. z `release-notes.prompt.md`).
2. Pošli ho Copilotovi spolu s kontextem (commits, kód, chybové zprávy).
3. Copilot provede kroku podle instrukcí a poskytne strukturovaný výstup.

Příklad:
```
[Copilotovi] Zde je prompt pro E2E Tests Maintenance:
[vložit obsah e2e-tests-maintenance.prompt.md]

Máme nový failure v EntriesE2ETests.Entries_CRUD_Flow_Succeeds v testu na line 94.
Jaká je diagnóza a jak to napravíme?
```

## Tipy

- Prompty si můžeš uložit jako **Favorites** v Copilot Chat pro rychlejší přístup.
- Přizpůsob je svému workflow; jsou to šablony, ne dogma.
- Při release cyclu: Release Notes → Security Hardening → EF Migrations Review → API Contract Review → CI Fix.
