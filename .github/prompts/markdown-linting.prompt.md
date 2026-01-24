# Prompt: Markdown Linting & Documentation Quality

## Cíl

Ověřit, opravit a optimalizovat Markdown dokumenty podle `markdownlint` v0.40.0 pravidel.

## Kontext

Projekt MIMM-2.0 používá VS Code rozšíření `markdownlint` a engine `markdownlint-cli2` pro kontrolu kvality dokumentace.
Konfigurace globs je v `.markdownlint-cli2.jsonc` a pravidla v `.markdownlint.json`:

- **Line length**: 120 znaků (místo výchozích 80)
- **Tables**: vyloučeny z kontroly délky řádků
- **MD036** (emphasis as heading): vypnuto (povoluje dekorativní text)
- **MD041** (first-line heading): vypnuto
- **MD045** (no-alt-text): vypnuto

## Běžné problémy a řešení

### MD007 / MD010 - Unordered list indentation + Hard tabs

**Problém**: Seznamy používají tabulátory místo mezer nebo špatnou indentaci.

```markdown
# Špatně:
- Item 1
 - Nested (hard tab - MD010)
    - Another (3 mezery - MD007)

# Správně:
- Item 1
  - Nested (2 mezery)
  - Another
```

**Řešení**: `markdownlint --fix` nebo ruční oprava na 2 mezery.

### MD022, MD031, MD032 - Blank lines kolem headings, lisů, code bloků

**Problém**: Chybějí prázdné řádky.

```markdown
# Špatně:
## Heading
- Item 1
text bez blank line

```text
code
```

## Správně

- Item 1

```text
code
```

```markdown

**Řešení**: `markdownlint --fix` přidá automaticky.

### MD013 - Line length

**Problém**: Dlouhé řádky (>120 znaků).

**Řešení**: Rozdělit na více řádků nebo vyloučit pravidlo v konfiguraci.

```markdown
<!-- markdownlint-disable MD013 -->
Dlouhý řádek bez mezer sem sem sem sem sem
<!-- markdownlint-enable MD013 -->
```

**Řešení**: Rozdělit na více řádků nebo vyloučit pravidlo.

### MD040 - Missing language in code block

**Problém**: Code block bez specifikace jazyka.

```text
# Špatně:

```text
dotnet build
```

## Správně (correct)

```bash
dotnet build
```

```text

**Řešení**: Přidat jazyk (`bash`, `csharp`, `markdown`, `text`, apod.) nebo `text`.

### MD034 - Bare URLs

**Problém**: URL bez angle brackets.

```markdown
# Špatně:
Viz https://example.com

# Správně:
Viz <https://example.com>
```

**Řešení**: Obalit v `<...>` nebo vytvořit link `[text](url)`.

## Pracovní postup

1. **Spusť kontrolu v editoru (VS Code)**:

   - Command Palette: „Markdownlint: Lint Workspace“
   - Formát dokumentu: „Format Document“ nebo „Fix All: markdownlint“

   Alternativně lokálně přes CLI2 (pokud je nainstalováno):

   ```bash
   npx markdownlint-cli2 "**/*.md"
   ```

2. **Automatické opravy (CLI)**:

   - Jednotlivé soubory:

     ```bash
     markdownlint --fix "README.md" "CHANGELOG.md" "AGENTS.md" "docs/*.md"
     ```

   - V editoru (doporučeno): „Fix All: markdownlint“ nebo „Format Document“

   - V CI: viz workflow [.github/workflows/markdownlint.yml](.github/workflows/markdownlint.yml)

3. **Ručně ověř zbývající chyby**:
   - MD013 v tabulkách (nelze automaticky)
   - MD036 (dekorativní text - není chyba)

4. **Commitni opravy**:

   ```bash
   git add README.md CHANGELOG.md AGENTS.md docs/
   git commit -m "style: markdown linting fixes (md013, md010, md022, etc.)"
   git push origin <branch>
   ```

## Tipy

- **Suchý běh (CLI2)**: `npx markdownlint-cli2 "**/*.md"`
- **Editor auto-fix**: Zapni „Format on Save“ a „source.fixAll.markdownlint“ ve VS Code nastavení
- **Globs**: Uprav [.markdownlint-cli2.jsonc](../../.markdownlint-cli2.jsonc) pro cílení souborů
- **Konkrétní soubor**: `markdownlint docs/DEVELOPER_GUIDE.md`
- **Všechny MD soubory**: `markdownlint "**/*.md"`
- **Ignorace chyby v souboru**: `<!-- markdownlint-disable MDXXX -->`...`<!-- markdownlint-enable MDXXX -->`

## Relevantní soubory

- Konfigurace: `.markdownlint.json`
- Dokumenty: `README.md`, `CHANGELOG.md`, `AGENTS.md`, `docs/*.md`
- Referenční pravidla: [markdownlint Rules (v0.40.0)](https://github.com/DavidAnson/markdownlint/blob/v0.40.0/doc/Rules.md)
