# 🗓️ MIMM 2.0 - Sprint Timeline Visualization

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    MVP DEVELOPMENT: 3 WEEKS (21 DAYS)                    │
│                    Start: 24.1.2026  →  Launch: 14.2.2026                │
└─────────────────────────────────────────────────────────────────────────┘

SPRINT 1: DATABASE + E2E + ENTRY CRUD UI (24.1 - 28.1)
════════════════════════════════════════════════════════
Week:  [████████████████████████████████████████] 100%
Status: 🎯 READY TO START

Monday 24.1     ├─ Action 1: Database Setup (2h)
                ├─ Action 2: E2E Auth Test (30min)
                └─ ✅ CRITICAL PATH COMPLETE

Tuesday 25.1    ├─ Action 3.1: EntryApiService (2h)
                └─ Action 3.2: EntryList.razor (2h)

Wednesday 26.1  ├─ Action 3.3: EntryCreate.razor (2h)
                └─ Action 3.4: EntryEdit.razor (2h)

Thursday 27.1   ├─ Action 3.5: Integration (2h)
                └─ Action 4: MoodSelector (4h)

Friday 28.1     └─ ✅ Sprint Review + Testing

Deliverables:
  ✅ PostgreSQL running with migrations
  ✅ E2E auth flow tested
  ✅ Entry CRUD UI complete (list, create, edit, delete)
  ✅ MoodSelector 2D grid component


SPRINT 2: TESTING + ERROR HANDLING + MUSIC SEARCH (29.1 - 4.2)
═══════════════════════════════════════════════════════════════
Week:  [░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░] 0%
Status: 📅 SCHEDULED

Monday 29.1     └─ Action 5.1-5.2: Integration Tests - Auth (4h)

Tuesday 30.1    └─ Action 5.3: Integration Tests - Entries (3h)

Wednesday 31.1  └─ Action 6: Error Handling + Loading States (4h)

Thursday 1.2    └─ Action 7.1-7.3: Music Search Backend (5h)

Friday 2.2      └─ Action 7.4-7.5: Music Search Frontend (5h)

Deliverables:
  ✅ 20+ integration tests (Auth + Entries)
  ✅ Error boundaries + loading spinners
  ✅ Music search (iTunes + Deezer)
  ✅ MusicSearch autocomplete component


SPRINT 3: LASTFM + ANALYTICS + DEPLOYMENT (5.2 - 14.2)
══════════════════════════════════════════════════════
Week:  [░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░] 0%
Status: 📅 SCHEDULED

Monday 5.2      └─ Action 8.1-8.2: Last.fm OAuth Backend (4h)

Tuesday 6.2     └─ Action 8.3-8.4: Last.fm OAuth Frontend (3h)

Wednesday 7.2   └─ Action 9.1-9.2: Analytics Service (4h)

Thursday 8.2    └─ Action 9.3-9.4: Analytics Charts (4h)

Friday 9.2      └─ 🛠️ BUFFER DAY (bug fixes, polish)

Monday 10.2     └─ Action 10.1: Docker Production Build (2h)

Tuesday 11.2    └─ Action 10.2: Azure Setup (3h)

Wednesday 12.2  └─ Action 10.3: CI/CD Pipeline (2h)

Thursday 13.2   └─ Action 10.4: Security Hardening (2h)

Friday 14.2     └─ 🚀 MVP LAUNCH v1.0.0

Deliverables:
  ✅ Last.fm OAuth integration
  ✅ Mood analytics dashboard
  ✅ Deployed to Azure App Service
  ✅ CI/CD pipeline operational
  🎉 PRODUCTION READY MVP


═══════════════════════════════════════════════════════════════════════════
                            PROGRESS TRACKER
═══════════════════════════════════════════════════════════════════════════

Current Status (24.1.2026):

Backend API                [████████████████████] 100%  ✅ COMPLETE
  ├─ AuthController         ✅ 6 endpoints
  ├─ EntriesController      ✅ 7 endpoints
  └─ Swagger/OpenAPI        ✅ Configured

Business Logic             [████████████████████] 100%  ✅ COMPLETE
  ├─ AuthService            ✅ 17 unit tests
  ├─ EntryService           ✅ 18 unit tests
  └─ All tests passing      ✅ 35/35

Frontend Auth UI           [████████████████░░░░] 80%   🚧 IN PROGRESS
  ├─ Login.razor            ✅ Complete
  ├─ Dashboard.razor        ⚠️  Placeholder only
  └─ MudBlazor              ✅ Integrated

Frontend Entry UI          [░░░░░░░░░░░░░░░░░░░░] 0%    ⏳ TODO
  ├─ EntryList.razor        ⏳ Not started
  ├─ EntryCreate.razor      ⏳ Not started
  ├─ EntryEdit.razor        ⏳ Not started
  └─ MoodSelector.razor     ⏳ Not started

External Integrations      [░░░░░░░░░░░░░░░░░░░░] 0%    ⏳ TODO
  ├─ Last.fm OAuth          ⏳ Not started
  ├─ Music Search           ⏳ Not started
  └─ Album Art Fetch        ⏳ Not started

Database                   [░░░░░░░░░░░░░░░░░░░░] 0%    🎯 TODAY
  ├─ PostgreSQL Running     ⏳ Run docker-compose
  ├─ First Migration        ⏳ dotnet ef migrations add
  └─ Tables Created         ⏳ dotnet ef database update

Testing                    [████░░░░░░░░░░░░░░░░] 20%   🚧 PARTIAL
  ├─ Unit Tests             ✅ 35 tests passing
  ├─ Integration Tests      ⏳ 0 tests (Sprint 2)
  └─ E2E Tests              ⏳ Manual only (Sprint 1)

Deployment                 [░░░░░░░░░░░░░░░░░░░░] 0%    ⏳ SPRINT 3
  ├─ Docker Production      ⏳ Week 4
  ├─ Azure Setup            ⏳ Week 4
  └─ CI/CD Pipeline         ⏳ Week 4

─────────────────────────────────────────────────────────────────────────
OVERALL MVP COMPLETION:     [████████████░░░░░░░░] 60%   🚀 ON TRACK
─────────────────────────────────────────────────────────────────────────


═══════════════════════════════════════════════════════════════════════════
                              CRITICAL PATH
═══════════════════════════════════════════════════════════════════════════

🔴 BLOCKER TASKS (Must complete before anything else):

  1. [TODAY]     Database Setup
                 └─ Without DB, cannot test real auth flow
  
  2. [TODAY]     E2E Auth Test
                 └─ Verify frontend + backend work together
  
  3. [Week 1]    Entry CRUD UI
                 └─ Dashboard is empty without this

🟡 HIGH PRIORITY (Can work in parallel):

  4. [Week 2]    Integration Tests
                 └─ Ensures API layer works correctly
  
  5. [Week 2]    Music Search
                 └─ Better UX than manual entry
  
  6. [Week 3]    Last.fm OAuth
                 └─ Social feature, optional but cool

🟢 NICE-TO-HAVE (Can be deferred to Phase 2):

  7. [Week 3]    Advanced Analytics
                 └─ MVP can launch with basic stats
  
  8. [Post-MVP]  Real-time (SignalR)
                 └─ Not critical for v1.0


═══════════════════════════════════════════════════════════════════════════
                          VELOCITY FORECAST
═══════════════════════════════════════════════════════════════════════════

Based on current progress (60% in ~5 days):

Estimated completion velocity: ~8-10% per day
Remaining work: 40%
Days needed: 4-5 days (realistic)

However, Entry CRUD UI is larger scope (40h estimated).
Adjusted forecast:

Sprint 1 (Week 1):     60% → 85%  (+25%)  ✅ Achievable
Sprint 2 (Week 2):     85% → 95%  (+10%)  ✅ Achievable  
Sprint 3 (Week 3-4):   95% → 100% (+5%)   ✅ Achievable

Conclusion: MVP launch on 14.2.2026 is FEASIBLE with buffer days.


═══════════════════════════════════════════════════════════════════════════
                           TEAM CAPACITY
═══════════════════════════════════════════════════════════════════════════

Solo Developer: 1 person (you + GitHub Copilot)

Daily capacity:     4-6 hours coding
Weekly capacity:    20-30 hours
Sprint capacity:    20-30 hours per week × 3 weeks = 60-90 hours

Estimated work remaining:
  - Entry CRUD UI:        10h
  - Integration Tests:     8h
  - Error Handling:        4h
  - Music Search:         12h
  - Last.fm OAuth:        10h
  - Analytics:             8h
  - Deployment:            6h
  ────────────────────────────
  TOTAL:                  58h

Available capacity: 60-90h
Required work:      58h

Buffer:             2-32h (3-35% contingency) ✅ HEALTHY BUFFER


═══════════════════════════════════════════════════════════════════════════
                            MILESTONE DATES
═══════════════════════════════════════════════════════════════════════════

✅ 20.1.2026   Project scaffold created
✅ 22.1.2026   AuthService + EntryService complete (35 tests passing)
✅ 23.1.2026   Controllers + Blazor auth UI complete
🎯 24.1.2026   Database setup + E2E test (TODAY)
📅 28.1.2026   Entry CRUD UI complete (Sprint 1 end)
📅 4.2.2026    Testing + Music search complete (Sprint 2 end)
📅 14.2.2026   🚀 MVP LAUNCH v1.0.0 (Sprint 3 end)


═══════════════════════════════════════════════════════════════════════════
                          SUCCESS CRITERIA
═══════════════════════════════════════════════════════════════════════════

MVP Launch Checklist (Must-Have):

Core Features:
  [ ] User registration + login
  [ ] Create journal entry (song + mood)
  [ ] View entry list (paginated)
  [ ] Edit entry
  [ ] Delete entry
  [ ] Music search (iTunes + Deezer)
  [ ] Last.fm OAuth (optional)
  [ ] Mood analytics dashboard

Technical:
  [ ] 50+ unit tests
  [ ] 20+ integration tests
  [ ] CI/CD pipeline passes
  [ ] Deployed to Azure
  [ ] HTTPS enabled
  [ ] Zero P0 bugs

Documentation:
  [ ] README updated
  [ ] CHANGELOG v1.0.0 entry
  [ ] API docs (Swagger)
  [ ] Setup guide tested

Performance:
  [ ] Page load < 2s
  [ ] API response < 500ms
  [ ] Mobile responsive
  [ ] Lighthouse score > 80


═══════════════════════════════════════════════════════════════════════════
                         COMMUNICATION PLAN
═══════════════════════════════════════════════════════════════════════════

Daily (Solo Dev Standup):
  - What did I complete yesterday?
  - What will I work on today?
  - Any blockers?
  - Update GitHub Project board

Weekly (Friday Review):
  - Sprint goals completed?
  - Update CHANGELOG.md
  - Git tag: v0.X.0-alpha
  - Retrospective: What went well? What to improve?

Launch Day (14.2.2026):
  - Deploy to production
  - Create GitHub Release v1.0.0
  - Announce on LinkedIn, X (Twitter)
  - Write blog post (dev.to, Medium)
  - Share in .NET community


═══════════════════════════════════════════════════════════════════════════
                           RISK REGISTER
═══════════════════════════════════════════════════════════════════════════

HIGH RISK:
  🔴 R1: Database migration fails
     └─ Mitigation: Test on local PostgreSQL first
     └─ Contingency: Use in-memory DB for MVP

  🔴 R2: CORS blocks frontend calls
     └─ Mitigation: Verify Program.cs CORS config
     └─ Contingency: Add wildcard CORS (dev only)

MEDIUM RISK:
  🟡 R6: MoodSelector UX confusing
     └─ Mitigation: Add tooltips + help text
     └─ Contingency: Fallback to dropdown

  🟡 R7: Music search returns no results
     └─ Mitigation: Test with common queries
     └─ Contingency: Allow manual entry always

LOW RISK:
  🟢 R9: Frontend bundle size too large
     └─ Mitigation: Use lazy loading
     └─ Contingency: Remove MudBlazor (extreme)


═══════════════════════════════════════════════════════════════════════════

Legend:
  ✅ Complete    🚧 In Progress    ⏳ Scheduled    ❌ Blocked
  🎯 Critical    🔴 High Priority  🟡 Medium       🟢 Low
  ████ Done      ░░░░ Pending

Document Version: 1.0
Created: 24. ledna 2026
Next Update: 28. ledna 2026 (Sprint 1 Review)
