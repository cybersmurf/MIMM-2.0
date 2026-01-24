# AGENTS.md

[![CI](https://github.com/cybersmurf/MIMM-2.0/actions/workflows/ci.yml/badge.svg)](https://github.com/cybersmurf/MIMM-2.0/actions/workflows/ci.yml)

- zde jsou klíčové instrukce pro všechny AI agenty
- **níže jsou uvedeny další instrukční soubory, které je nutné respektovat**

- [README.md](./README.md) - základní informace o projektu
- [copilot-instructions](./.github/copilot-instructions.md) - pravidla generování kódu
- `.github/prompts/` - opakovatelné prompty pro Copilot (release notes, E2E maintenance, CI fix, feature pattern)

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
