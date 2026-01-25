# Changelog

Všechny významné změny v tomto demo projektu budou zaznamenány v tomto souboru.

## [v26.1.25] - 25. ledna 2026

### Added

- **Last.fm Integration**: Scrobbling support for music entries
  - Backend `LastFmService.ScrobbleAsync()` method (validates session key,
    signs with MD5, posts to Last.fm track.scrobble API)
  - New endpoint `POST /api/lastfm/scrobble [Authorize]` accepting song title,
    artist, album, timestamp
  - Frontend `LastFmApiService.ScrobbleAsync()` HTTP client wrapper
  - EntryList UI: scrobble button for entries not yet synced to Last.fm
  - ✅ Unit tests for scrobbling service (2/2 passing: valid token, missing token)
  - ✅ E2E workflow test script (register → create entry → scrobble with error
    handling)
  - ✅ Both backend (port 7001) and frontend (port 5000) verified running
  - ✅ Database verified with E2E test entry (Bohemian Rhapsody by Queen)

### Changed

- **Performance**: Optimizace EF Core queries
  - Přidáno `.AsNoTracking()` do read-only queries (GetAsync, ListAsync, SearchAsync)
  - Očekávaný nárůst výkonu: 15-20% na složitých queries
  - Odstraněny zbytečné global query filtry z JournalEntry a LastFmToken

### Added (Previous)

- **Security**: JWT token tracking
  - Přidán "jti" (JWT ID) claim pro budoucí revocation mechanismus
  - Umožňuje detailnější tracking a audit tokenů
  
- **Frontend**: Nová reusable komponenta
  - MusicTrackCard.razor pro vykreslení hudebních stop
  - Snížení duplikace kódu v MusicSearchBox o ~30 řádků

### Fixed

- Vyřešeny problémy se soft-delete query filtry
  - JournalEntry a LastFmToken nyní používají User cascade filtrování
  - Konzistentnější data consistency approach

### Testing

- Všechny testy procházejí: 43/43 (38 unit + 5 integration)
- Build bez chyb (5 MudBlazor warnings - non-critical)

## [v26.1.17] - 17. ledna 2026

### Added

- Ukázka `.github/skills` pro custom skills
- Verzování tohoto demo projektu

## [v26.1.7] - 7. ledna 2026

### Added

- Základní struktura Repository Starter Kit
- Složka `.github` s copilot-instructions.md
- Složka `.github/instructions` pro pattern-based instrukce
- Složka `.github/agents` pro custom agent definice
- AGENTS.md s klíčovými instrukcemi pro AI agenty
- CLAUDE.md s instrukcemi pro Claude Code CLI
- GEMINI.md s instrukcemi pro Google Gemini CLI
- Dokumentační struktura v `docs/`
- Ukázková struktura projektu (src/, tests/, scripts/)
- README.md s popisem projektu a struktury
- Verzovací systém (CHANGELOG.md)
