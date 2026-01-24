# MIMM 2.0 - Feature Status & Development Roadmap

## What's Done, What's In Progress, and What's Coming

**Datum:** 24. ledna 2026  
**Verze:** 1.0  

---

## 🎯 Feature Status Overview

### Co je MIMM 2.0 (Music In My Mind)?

Aplikace pro sledování hudby co ti **zní v hlavě** (kterou si zpívám,
představuji bez zvuku) a jak to ovlivňuje mou náladu a fyzické pocity.

### Legend

- ✅ **Complete** – Plně implementováno a testováno
- 🔄 **In Progress** – Aktivně se pracuje
- 📋 **Planned** – Na roadmapě, čeká na zahájení
- ❌ **Not Started** – Zatím nezahájeno
- 🚀 **Phase** – V které fázi vývoje

---

## 📊 Feature Matrix

### Core Features (MVP - Critical Path)

| Feature | Status | Difficulty | Est. Hours | Owner | Notes |
|---------|--------|-----------|-----------|-------|-------|
| **User Registration** | ❌ | Medium | 24 | Backend | Email validation, password strength |
| **User Login** | ❌ | Medium | 16 | Backend | JWT token generation, refresh tokens |
| **Password Reset** | ❌ | Medium | 12 | Backend | Email-based token flow |
| **User Profile** | 📋 | Low | 12 | Frontend | Edit name, language, timezone |
| **Entry Creation Form** | 📋 | Medium | 20 | Frontend | Song input, mood selector, notes |
| **Entry CRUD** | ❌ | Medium | 28 | Backend | Create, Read, Update, Delete operations |
| **Mood Selector** | 📋 | High | 24 | Frontend | Russell's 2D Circumplex UI |
| **Music Search** | ❌ | High | 40 | Backend | iTunes + Deezer API integration |
| **Entry Analytics** | ❌ | High | 32 | Backend+Frontend | Mood trends, statistics, charts |
| **Data Export** | ❌ | Medium | 16 | Backend | CSV, JSON, PDF export |

**Subtotal Core:** ~224 hours

### Enhanced Features (Phase 2)

| Feature | Status | Difficulty | Est. Hours | Phase |
|---------|--------|-----------|-----------|-------|
| **Last.fm Integration** | 📋 | High | 48 | 6-9 měsíců |
| **Spotify Integration** | 📋 | High | 40 | 9-12 měsíců |
| **Real-time Notifications** | 📋 | High | 32 | 9-12 měsíců |
| **Collaborative Playlists** | 📋 | High | 36 | 12+ měsíců |
| **Advanced Analytics** | 📋 | High | 48 | 12+ měsíců |
| **Mobile PWA** | 📋 | Medium | 28 | 12+ měsíců |
| **Dark Mode** | 📋 | Low | 8 | 6-9 měsíců |
| **Multi-language** | 📋 | Low | 16 | 6-9 měsíců |

**Subtotal Enhanced:** ~256 hours

### Technical Features (Infrastructure)

| Feature | Status | Difficulty | Est. Hours | Owner |
|---------|--------|-----------|-----------|-------|
| **Docker Setup** | ✅ | Low | 12 | DevOps | Complete |
| **PostgreSQL DB** | ✅ | Medium | 20 | Backend | Configured, migrations ready |
| **Redis Cache** | ✅ | Low | 8 | Backend | Optional, not required yet |
| **JWT Auth** | 🔄 | Medium | 16 | Backend | In configuration |
| **Logging (Serilog)** | ✅ | Low | 12 | Backend | Configured |
| **Exception Handling** | ✅ | Low | 8 | Backend | Middleware ready |
| **CORS** | ✅ | Low | 4 | Backend | Configured |
| **GitHub Actions CI/CD** | 📋 | High | 24 | DevOps | Automated deploy pipeline |
| **Monitoring & Alerting** | 📋 | High | 40 | DevOps | Prometheus, Grafana, Slack |
| **Performance Testing** | ❌ | High | 32 | QA | Load testing, bottleneck analysis |
| **Security Audit** | ❌ | High | 24 | Security | Penetration testing |

**Subtotal Technical:** ~200 hours

---

## 🔄 Current Sprint (Week 1-2 of Development)

### Focus: Authentication & Login

```text
┌─────────────────────────────────────────┐
│ Priority 1: User Auth System            │
└─────────────────────────────────────────┘

Backend Tasks (32 hours):
  □ Implement AuthService.cs
  │ ├── RegisterAsync(email, password)
  │ ├── LoginAsync(email, password)
  │ ├── RefreshTokenAsync(token)
  │ ├── ValidateTokenAsync(token)
  │ └── LogoutAsync(userId)
  │
  □ Create Auth Controller
  │ ├── POST /api/auth/register
  │ ├── POST /api/auth/login
  │ ├── POST /api/auth/refresh
  │ └── POST /api/auth/logout
  │
  □ Add email verification
  │ ├── SendVerificationEmail()
  │ └── VerifyEmailToken()
  │
  └─ Unit Tests
     └── 8 tests for auth flow

Frontend Tasks (24 hours):
  □ Create RegisterPage.razor
  │ ├── Email input with validation
  │ ├── Password strength indicator
  │ ├── Confirm password field
  │ └── Error messages
  │
  □ Create LoginPage.razor
  │ ├── Email & password form
  │ ├── Remember me option
  │ ├── Forgot password link
  │ └── Error handling
  │
  □ Create AuthService.cs (Frontend)
  │ ├── Call backend login endpoint
  │ ├── Store JWT token in localStorage
  │ ├── Setup token refresh flow
  │ └── Provide getCurrentUser()
  │
  □ Add auth guards
  │ ├── AuthorizeRouteView
  │ ├── Redirect to login if not authenticated
  │ └── Auto-refresh token on expiration
  │
  └─ Integration Tests
     └── 6 tests for UI flow

Testing (16 hours):
  □ Manual testing flow
  □ Password reset flow testing
  □ Token expiration handling
  □ Edge cases (SQL injection, etc.)

Total: 72 hours
Timeline: Week 1 (ideally)
```text

### Success Criteria

- ✓ User can register with email
- ✓ Email verification works
- ✓ User can login and receive JWT token
- ✓ Token refreshes automatically
- ✓ Protected routes redirect to login
- ✓ 80%+ test coverage for auth code
- ✓ Zero security vulnerabilities

---

## 📅 8-Week Development Timeline

### Week 1-2: Authentication & Setup

```text
┌──────────────────────────────────────┐
│ ✅ Backend Framework & DB             │
│ 🔄 User Authentication System        │
│ 📋 Login/Register Frontend Pages     │
│ 📋 Testing Infrastructure            │
│ Status: 35% Complete                 │
└──────────────────────────────────────┘

Deliverables:
  - Secure login/registration
  - JWT token management
  - User profile creation
  
Time: 72 hours
Blockers: None
```text

### Week 3-4: Entry Management

```text
┌──────────────────────────────────────┐
│ 📋 Entry CRUD Operations            │
│ 📋 Music Search Integration         │
│ 📋 Mood Selector Component          │
│ 📋 Entry Form & Validation          │
│ Status: 0% Complete                 │
└──────────────────────────────────────┘

Deliverables:
  - Users can create journal entries
  - Music search from iTunes/Deezer
  - Mood selector (2D interface)
  
Time: 88 hours
Blockers: Music API keys needed
```text

### Week 5-6: Analytics & Visualization

```text
┌──────────────────────────────────────┐
│ 📋 Analytics Backend                │
│ 📋 Chart Components (MudBlazor)     │
│ 📋 Statistics Dashboard             │
│ 📋 Trend Analysis                   │
│ Status: 0% Complete                 │
└──────────────────────────────────────┘

Deliverables:
  - Mood trends over time
  - Top songs & artists
  - Emotion distribution
  
Time: 64 hours
Blockers: None
```text

### Week 7-8: Testing & Deployment

```text
┌──────────────────────────────────────┐
│ 📋 Comprehensive Testing            │
│ 📋 Performance Optimization         │
│ 📋 Security Audit                   │
│ 📋 Production Deployment            │
│ Status: 0% Complete                 │
└──────────────────────────────────────┘

Deliverables:
  - 80%+ test coverage
  - Live production environment
  - Monitoring & alerting
  - Documentation
  
Time: 96 hours
Blockers: Server access needed
```text

---

## 📈 Feature Detailed Breakdown

### Module 1: Authentication (3 features)

#### 1.1 User Registration

```text
Acceptance Criteria:
  ✓ User provides email & password
  ✓ Password strength validated (min 8 chars, 1 number, 1 special)
  ✓ Email verified before account activation
  ✓ User cannot register twice with same email
  ✓ Password hashed with BCrypt (workFactor 12)
  ✓ Confirmation email sent
  ✓ Token expires in 24 hours

Error Handling:
  ✗ Invalid email format → error message
  ✗ Weak password → specific feedback
  ✗ Email already exists → suggest login
  ✗ Server error → retry button

API Endpoint:
  POST /api/auth/register
  {
    "email": "user@example.com",
    "password": "SecurePass123!",
    "displayName": "John Doe"
  }
  
  Response 201:
  {
    "userId": "uuid",
    "email": "user@example.com",
    "message": "Verification email sent"
  }
```text

#### 1.2 User Login

```text
Acceptance Criteria:
  ✓ User provides email & password
  ✓ Password verified against hash
  ✓ JWT access token issued (60 min expiry)
  ✓ Refresh token issued (7 day expiry)
  ✓ Both tokens stored securely
  ✓ Login attempt logged
  ✓ Account locked after 5 failed attempts

Error Handling:
  ✗ Email not found → generic error (security)
  ✗ Password incorrect → generic error
  ✗ Email not verified → prompt verification
  ✗ Account locked → unlock email option

API Endpoint:
  POST /api/auth/login
  {
    "email": "user@example.com",
    "password": "SecurePass123!"
  }
  
  Response 200:
  {
    "accessToken": "eyJ...",
    "refreshToken": "rf_...",
    "expiresIn": 3600,
    "user": {
      "id": "uuid",
      "email": "user@example.com",
      "displayName": "John Doe"
    }
  }
```text

#### 1.3 Password Reset

```text
Acceptance Criteria:
  ✓ User requests password reset
  ✓ Reset token sent to email
  ✓ Token valid for 1 hour
  ✓ Token can only be used once
  ✓ New password must be different from old
  ✓ Password immediately invalidates old refresh tokens

API Endpoints:
  POST /api/auth/forgot-password
  POST /api/auth/reset-password
```text

### Module 2: Entry Management (4 features)

#### 2.1 Create Entry

```text
Form Fields:
  - Song Title (required, autocomplete from search)
  - Artist Name (required, autocomplete)
  - Album Name (optional)
  - Mood (Valence -1 to 1, Arousal -1 to 1)
  - Tension Level (0-100 slider)
  - Physical Tags (multi-select)
  - Notes (optional, max 5000 chars)
  - Date & Time (defaults to now)

Validation:
  ✓ All required fields filled
  ✓ Mood values in range
  ✓ Notes under character limit
  ✓ User owns entry (authorization)

API Endpoint:
  POST /api/entries
  {
    "songTitle": "Bohemian Rhapsody",
    "artistName": "Queen",
    "valence": 0.6,
    "arousal": 0.8,
    "tensionLevel": 45,
    "somaticTags": ["headache", "fatigue"],
    "notes": "Amazing song!",
    "createdAt": "2026-01-24T10:30:00Z"
  }
```text

#### 2.2 Edit Entry

```text
Features:
  - Edit all fields
  - Change date/time retroactively
  - Update mood retrospectively
  - Soft-delete (not permanent)

Authorization:
  - Only entry owner can edit
  - Cannot edit if deleted
  - Admin override possible

API Endpoint:
  PUT /api/entries/{id}
```text

#### 2.3 View Entries

```text
Pagination:
  - 20 entries per page
  - Latest entries first
  - Filter by date range
  - Filter by mood (valence/arousal bounds)
  - Search by song/artist

Performance:
  - Cache user's last 100 entries
  - Lazy load older entries

API Endpoints:
  GET /api/entries?page=1&limit=20
  GET /api/entries?from=2026-01-01&to=2026-01-31
```text

#### 2.4 Delete Entry

```text
Options:
  - Soft delete (recoverable)
  - Hard delete (permanent)
  - Restore deleted entries

Retention:
  - Soft deleted entries kept 30 days
  - After 30 days: hard deleted
  - Admin can force permanent delete

API Endpoints:
  DELETE /api/entries/{id}
  POST /api/entries/{id}/restore
```text

### Module 3: Music Search (3 sources)

#### 3.1 iTunes Search

```text
Integration:
  - Search API endpoint
  - Parse JSON response
  - Extract: title, artist, album, artwork

Caching:
  - Cache results for 1 hour
  - Invalidate on new search

Error Handling:
  - No results found
  - API timeout (5 sec)
  - Rate limit exceeded
```text

#### 3.2 Deezer Search

```text
Similar to iTunes
- Need API key
- Different response format
- Artist images available
```text

#### 3.3 Manual Entry

```text
Allow user to:
  - Enter song manually
  - No auto-search
  - Manual metadata entry
```text

### Module 4: Analytics (5 views)

#### 4.1 Mood Timeline

```text
Chart Type: Time Series
  - X-axis: Date/Time
  - Y-axis: Mood (Valence)
  - Color: Arousal level
  
Features:
  - Zoom in/out
  - Hover for details
  - Export as image
  
Time Range:
  - Last 7 days (default)
  - Last 30 days
  - Last year
  - Custom range
```text

#### 4.2 Mood Heatmap

```text
Chart Type: Russell's Circumplex
  - X-axis: Valence (-1 to 1)
  - Y-axis: Arousal (-1 to 1)
  - Points: Individual entries
  - Density: Color intensity
  
Insights:
  - Which quadrant is most common
  - Outliers detection
  - Trend arrows
```text

#### 4.3 Top Songs

```text
Ranking by:
  - Most listened (count)
  - Average mood (positive/negative)
  - Frequency over time

Display:
  - Top 10 list
  - Album art thumbnails
  - Mood associated with each
```text

#### 4.4 Artist Trends

```text
Show:
  - Most frequent artists
  - Mood correlation with artist
  - Genre analysis (if available)
```text

#### 4.5 Statistics Dashboard

```text
KPIs:
  - Total entries: 42
  - Entries this month: 12
  - Average mood: Positive
  - Top emotion: Happy (28%)
  - Most active: Tuesday evenings
  - Favorite time of day: 20:00-22:00
```text

---

## 🚀 Phase 2 Roadmap (Months 6-12)

### Priority 1: External API Integration (120 hours)

```text
Last.fm OAuth Login (48 hours)
├── OAuth2 flow implementation
├── Scrobble sync (historical data)
├── Real-time scrobbling
└── Token refresh handling

Spotify Integration (40 hours)
├── Search & playback preview
├── Playlist creation in MIMM
├── Share to Spotify
└── Now playing integration

MusicBrainz Metadata (32 hours)
├── Album art fetching
├── Genre tags
├── Release date accuracy
└── Artist disambiguation

Total: ~120 hours (2-3 senior devs, 3 weeks)
```text

### Priority 2: Real-time Features (80 hours)

```text
SignalR Hubs:
├── AnalyticsHub (live updates)
├── NotificationHub (new entries)
└── CollaborationHub (shared playlists)

Features:
├── Real-time mood updates for other users
├── Instant notifications
├── Live collaboration features
└── Broadcasting statistics

Total: ~80 hours (1 senior dev, 2 weeks)
```text

### Priority 3: Advanced Analytics (88 hours)

```text
ML-based Predictions:
├── Mood forecasting
├── Song recommendations
├── Optimal listening time
└── Emotion-genre mapping

Data Analysis:
├── Correlation: music ↔ mood
├── Circadian patterns
├── Weekly trends
└── Seasonal variations

Reporting:
├── Monthly mood report (PDF)
├── Yearly insights
├── Custom date range analysis
└── Comparison with other users (anonymized)

Total: ~88 hours (2 senior devs, 3 weeks)
```text

---

## 🛑 Blockers & Risks

### External Dependencies

```text
❌ API Keys Required:
  - [ ] iTunes API (free, registration needed)
  - [ ] Deezer API (free tier, rate limited)
  - [ ] Last.fm OAuth (free, setup needed)
  - [ ] Spotify API (free, rate limited)

Timeline Impact: +1 week to get all keys
Risk Level: Low (all free tier available)
Mitigation: Request keys immediately
```text

### Technical Risks

```text
⚠️ Performance (Medium Risk)
  - Symptom: Slow analytics queries
  - Mitigation: Add indexes, caching
  - Timeline impact: +1 week load testing

⚠️ SignalR Scalability (Medium Risk)
  - Symptom: Dropped connections > 1000 users
  - Mitigation: Connection pooling, backplane
  - Timeline impact: +2 weeks for phase 2

⚠️ Database Growth (Low Risk)
  - Symptom: Disk space issues after 1M entries
  - Mitigation: Archive old data, partitioning
  - Timeline impact: +3 months (acceptable)
```text

### Team Risks

```text
⚠️ Junior Developer Learning Curve (High)
  - Impact: Slower initial velocity
  - Mitigation: Pair programming, code reviews
  - Timeline impact: +30% to junior estimates

⚠️ Scope Creep (High)
  - Impact: Missing MVP deadline
  - Mitigation: Strict prioritization, no new features during sprint
  - Timeline impact: +2-4 weeks if not managed

❌ Key Person Dependency (Medium)
  - Impact: Project halts if senior unavailable
  - Mitigation: Documentation, knowledge sharing
  - Timeline impact: +3 weeks if person leaves
```text

---

## 📊 Effort Estimation Summary

### Backend Development

```text
Authentication:           24 hours    ✅ High Priority
Entry Management:        40 hours    ✅ High Priority
Music Search:            48 hours    ✅ High Priority
Analytics:              48 hours    ✅ High Priority
API Integration:        40 hours    📋 Phase 2
Real-time (SignalR):    32 hours    📋 Phase 2
Total Backend:         232 hours
```text

### Frontend Development

```text
Login/Register Pages:    20 hours    ✅ High Priority
Entry Form:             24 hours    ✅ High Priority
Entry List & Details:   16 hours    ✅ High Priority
Mood Selector (2D):     24 hours    ✅ High Priority
Analytics Dashboard:    40 hours    ✅ High Priority
Settings & Profile:     12 hours    ✅ High Priority
Mobile PWA:             28 hours    📋 Phase 2
Dark Mode:              8 hours     📋 Phase 2
Total Frontend:        172 hours
```text

### Testing & QA

```text
Unit Tests:             40 hours
Integration Tests:      32 hours
E2E Tests:             24 hours
Performance Testing:    16 hours
Security Testing:       12 hours
Total Testing:         124 hours
```text

### DevOps & Deployment

```text
CI/CD Pipeline:        24 hours
Monitoring Setup:      16 hours
Backup Strategy:       12 hours
Documentation:         20 hours
Server Configuration:  12 hours
Total DevOps:          84 hours
```text

### **Grand Total: 612 hours (~3-4 senior devs, 4-6 weeks)**

---

## 📋 Feature Dependency Graph

```text
User Registration ──┐
                    ├──► JWT Auth ──┐
User Login ─────────┤              ├──► Protected Routes
Password Reset ─────┘              │
                                   ├──► Entry Management
                    Create Entry ──┤
                                   ├──► Music Search
                    View Entry ────┤
                                   ├──► Analytics
                    Edit Entry ────┘

Last.fm OAuth ─────┐
                   ├──► Scrobbling
Real-time Sync ───┘

Analytics Data ────┐
                   ├──► ML Predictions
User Preferences ──┘
```text

---

## ✅ Success Metrics

### MVP Launch Criteria

- [ ] 80%+ test coverage
- [ ] All critical features working
- [ ] Zero P0 (Critical) bugs
- [ ] <10 P1 (High) bugs
- [ ] Performance: <200ms API response
- [ ] Uptime: 99%+ in staging
- [ ] Documentation complete
- [ ] Security audit passed

### Post-Launch KPIs

- **User Acquisition:** 100 users in first month
- **Feature Usage:** 80%+ of users create at least 1 entry
- **Retention:** 60%+ monthly active users
- **Bug Reports:** <2 critical bugs/month
- **Performance:** 99.5% uptime

---
