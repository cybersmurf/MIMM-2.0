# Prompt: MIMM Daily Context

Goal: Provide a ready-to-use context snapshot for daily MIMM tasks.

Context Snapshot:
- Repo: MIMM 2.0 main @ 5e1b40b (origin/main is synced).
- Stack: ASP.NET Core 9 backend (EF Core + PostgreSQL + JWT + Serilog + SignalR-ready); Blazor WASM + MudBlazor frontend; shared DTOs/entities.
- Services: AuthService, UserService, EntryService, AnalyticsService, LastFmService, MusicSearchService, EmailService; Refit client for Last.fm; CORS policy AllowFrontend; dev auto-migrate DB.
- Env: ApiBaseUrl default http://localhost:5001; backend https://localhost:7001 dev; required env vars ConnectionStrings__DefaultConnection, Jwt__Key/Issuer/Audience; BCrypt for passwords.
- Tests: Application.Tests (17 passing) are active; MIMM unit/integration are scaffolded.
- Docs: README, Developer Guide, User Guide, analysis pack (ANALYSIS_SUMMARY, EXECUTIVE_SUMMARY, TECHNICAL_DEEP_DIVE, FEATURE_STATUS_AND_ROADMAP), deployment checklists.

Roadmap Focus:
- Week 1–2: ship registration/login end-to-end; entry creation form; mood selector (Russell 2D); validate DB schema.
- Week 3–4: entry list/edit/delete; music search integration; raise test coverage to 50%.
- Week 5–8: mood analytics + charts; Last.fm integration; production deploy + security hardening.

Standards:
- C# 13, net9.0, nullable enabled; DTOs as record with required props; Async suffix, propagate CancellationToken; use collection expressions and primary constructors when sensible; no hardcoded secrets; conventional commits.

Usage:
- Paste this prompt before coding to keep context loaded and aligned with current plan.
