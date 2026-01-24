# User Guide

Krátký průvodce pro uživatele MIMM 2.0.

## Instalace a spuštění

- Otevři MIMM 2.0 ve webovém prohlížeči po spuštění frontend a backend:
  - Backend: <https://localhost:7001>
  - Frontend: <https://localhost:5001>

## První kroky

1. Registrace účtu (email + heslo, volitelně zobrazované jméno).
2. Přihlášení – po úspěchu se zobrazí dashboard.
3. Vytváření záznamů (Entries):
   - Vyplň song (název, autor, album) a náladu (Valence/Arousal/Tension).
   - Přidej somatické tagy a poznámky.
   - Ulož – záznam se objeví v seznamu.
4. Úprava/Smazání záznamu přes dialog v seznamu.

## Hledání a filtrování

- Použij vyhledávání podle názvu písně/umělce.
- Seřazení dle data (desc) pro nejnovější záznamy.

## Tipy

- Přihlášení je na bázi JWT; relace vyprší dle `AccessTokenExpiresAt`.
- Osobní data jsou oddělena per-user (multi-tenant).
- Pro integrace (Last.fm apod.) sleduj nastavení v budoucích verzích.
