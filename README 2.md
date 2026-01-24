# Repository Starter Kit

**Verze: 1.0.0** | [Changelog](./CHANGELOG.md)

Vzorový .NET projekt demonstrující doporučenou strukturu složek a konfiguračních souborů pro optimální práci s GitHub Copilot a AI agenty.

## Účel projektu

Tento repozitář slouží jako **reference template** pro vývojáře, aby viděli:

- Jak správně strukturovat `.github` složku s instrukcemi
- Jak použít copilot-instructions.md
- Jak připravit AGENTS.md, CLAUDE.md a další soubory pro AI asistenty
- Jak organizovat dokumentaci a skripty

## Struktura projektu

```
├── .github/
│   ├── copilot-instructions.md  # Instrukce pro generování kódu
│   ├── instructions/            # Složka pro pattern-based instrukce
│   ├── agents/                  # Custom agent definice (MCP)
│   └── prompts/                 # Reusable prompty
├── docs/                        # Technická dokumentace
├── scripts/                     # Build a deployment skripty
├── src/                         # Zdrojový kód aplikace
└── tests/                       # Jednotkové testy
```

## Instrukce pro AI agenty

- [**AGENTS.md**](./AGENTS.md) - Klíčové instrukce a příkazy pro všechny AI agenty
- [**CLAUDE.md**](./CLAUDE.md) - Specifické instrukce pro Claude Code CLI
- [**GEMINI.md**](./GEMINI.md) - Specifické instrukce pro Google Gemini CLI
- [**copilot-instructions**](./.github/copilot-instructions.md) - Instrukce pro generování kódu
