# MIMM 2.0 – Phase 4: Advanced Features Roadmap

**Version:** 1.0  
**Status:** Planning & Architecture Design  
**Date:** January 26, 2026

---

## Executive Summary

Phase 4 introduces 7 advanced features to transform MIMM from a personal journal into a social, real-time, analytics-rich, and community-driven platform. All features build on solid Phase 1-3 foundations (authentication, core entities, responsive UI, accessibility).

**Phase 4 Timeline:** Feb-March 2026 (8 weeks)  
**Delivery:** 2-week sprints with feature toggles for gradual rollout

---

## Table of Contents

1. [Feature Overview & Priority](#feature-overview--priority)
2. [Database Schema Additions](#database-schema-additions)
3. [Backend Architecture](#backend-architecture)
4. [Frontend Architecture](#frontend-architecture)
5. [Implementation Timeline](#implementation-timeline)
6. [Risk Assessment](#risk-assessment)

---

## Feature Overview & Priority

| Priority | Feature | Effort | Risk | Benefit | Sprint |
|----------|---------|--------|------|---------|--------|
| 🔴 P0 | Real-time Notifications (SignalR) | 5d | Low | Critical UX | 4.1 |
| 🔴 P1 | Advanced Analytics | 8d | Low | High insights | 4.1-4.2 |
| 🟡 P2 | Offline Support (PWA) | 6d | Medium | Mobile-first | 4.2 |
| 🟡 P2 | Export/Import (CSV/JSON) | 4d | Low | Data portability | 4.2 |
| 🟡 P2 | Dark Mode Improvements | 3d | Low | UX polish | 4.1 |
| 🟠 P3 | Social Features | 10d | Medium | Community | 4.3 |
| 🟠 P3 | Admin Panel | 7d | Medium | Ops | 4.3 |

**Effort Scale:** 1d = 6-8 hours, 5d = 1 week, 10d = 2 weeks

---

## Database Schema Additions

### New Tables (EF Core Entities)

#### 1. **Notifications** (P0)
```csharp
public class Notification
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; } // Who receives it
    public User? User { get; set; }
    
    public string Type { get; set; } // "entry_created", "friend_request", "scrobble"
    public string Title { get; set; }
    public string Message { get; set; }
    public string? Link { get; set; } // Navigation URL
    
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}
```

#### 2. **UserFriendships** (P3)
```csharp
public class UserFriendship
{
    public Guid Id { get; set; }
    
    public Guid RequesterId { get; set; }
    public User? Requester { get; set; }
    
    public Guid ReceiverId { get; set; }
    public User? Receiver { get; set; }
    
    public FriendshipStatus Status { get; set; } // Pending, Accepted, Blocked
    public DateTime CreatedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
}

public enum FriendshipStatus
{
    Pending = 0,
    Accepted = 1,
    Blocked = 2
}
```

#### 3. **SharedEntries** (P3)
```csharp
public class SharedEntry
{
    public Guid Id { get; set; }
    
    public Guid EntryId { get; set; }
    public JournalEntry? Entry { get; set; }
    
    public Guid SharedWithUserId { get; set; }
    public User? SharedWithUser { get; set; }
    
    public SharePermission Permission { get; set; } // View, Comment
    public DateTime CreatedAt { get; set; }
}

public enum SharePermission
{
    View = 0,
    Comment = 1
}
```

#### 4. **AnalyticsSnapshot** (P1)
```csharp
public class AnalyticsSnapshot
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    public User? User { get; set; }
    
    public int Year { get; set; }
    public int Month { get; set; } // 1-12
    
    // Aggregated metrics
    public int TotalEntries { get; set; }
    public decimal AverageMoodValence { get; set; } // -1 to 1
    public decimal AverageMoodArousal { get; set; } // -1 to 1
    public string[] TopArtists { get; set; } // JSON array
    public string[] TopSongs { get; set; } // JSON array
    
    public DateTime GeneratedAt { get; set; }
}
```

#### 5. **UserSettings** (P2-P4)
```csharp
public class UserSettings
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    public User? User { get; set; }
    
    // Feature toggles & preferences
    public bool NotificationsEnabled { get; set; } = true;
    public bool RealTimeUpdatesEnabled { get; set; } = true;
    public bool PublicProfile { get; set; } = false;
    public bool ShareMusicWithFriends { get; set; } = false;
    
    public string ThemePreference { get; set; } = "light"; // light, dark, auto
    public string LanguagePreference { get; set; } = "en";
    
    // Data export settings
    public DateTime? LastExportedAt { get; set; }
    public string? ExportFormat { get; set; } // json, csv
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

#### 6. **AdminAuditLog** (P3)
```csharp
public class AdminAuditLog
{
    public Guid Id { get; set; }
    
    public Guid AdminUserId { get; set; }
    public User? AdminUser { get; set; }
    
    public string Action { get; set; } // "user_deleted", "reported_content_reviewed"
    public Guid? TargetUserId { get; set; }
    public Guid? TargetEntryId { get; set; }
    
    public string Details { get; set; } // JSON log
    public DateTime CreatedAt { get; set; }
}
```

---

## Backend Architecture

### New Controllers

#### 1. **NotificationController** (P0)
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    // GET /api/notification/list - Get user's notifications
    [HttpGet("list")]
    public async Task<IActionResult> GetNotifications(int page = 1, int pageSize = 20)
    
    // GET /api/notification/{id} - Get single notification
    [HttpGet("{id}")]
    public async Task<IActionResult> GetNotification(Guid id)
    
    // POST /api/notification/{id}/read - Mark as read
    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    
    // POST /api/notification/read-all - Mark all as read
    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    
    // DELETE /api/notification/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(Guid id)
}
```

#### 2. **AnalyticsController** (P1)
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    // GET /api/analytics/yearly/{year} - Yearly report
    [HttpGet("yearly/{year}")]
    public async Task<IActionResult> GetYearlyReport(int year)
    
    // GET /api/analytics/trends - Mood trends (6 months)
    [HttpGet("trends")]
    public async Task<IActionResult> GetMoodTrends(int months = 6)
    
    // GET /api/analytics/top-artists - Top artists (cached)
    [HttpGet("top-artists")]
    public async Task<IActionResult> GetTopArtists(int limit = 10)
    
    // GET /api/analytics/mood-by-artist - Mood correlation with artists
    [HttpGet("mood-by-artist")]
    public async Task<IActionResult> GetMoodByArtist()
    
    // GET /api/analytics/summary - Quick summary (dashboard)
    [HttpGet("summary")]
    public async Task<IActionResult> GetAnalyticsSummary()
}
```

#### 3. **SocialController** (P3)
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SocialController : ControllerBase
{
    // POST /api/social/friend-request - Send friend request
    [HttpPost("friend-request")]
    public async Task<IActionResult> SendFriendRequest(FriendRequestDto request)
    
    // POST /api/social/friend-request/{id}/accept
    [HttpPost("friend-request/{id}/accept")]
    public async Task<IActionResult> AcceptFriendRequest(Guid id)
    
    // DELETE /api/social/friendship/{id} - Remove friend
    [HttpDelete("friendship/{id}")]
    public async Task<IActionResult> RemoveFriend(Guid id)
    
    // GET /api/social/friends - List friends
    [HttpGet("friends")]
    public async Task<IActionResult> GetFriends()
    
    // GET /api/social/friend-requests - Pending requests
    [HttpGet("friend-requests")]
    public async Task<IActionResult> GetFriendRequests()
    
    // POST /api/social/share-entry - Share entry with friend
    [HttpPost("share-entry")]
    public async Task<IActionResult> ShareEntry(ShareEntryDto dto)
    
    // GET /api/social/shared-with-me - Entries shared with me
    [HttpGet("shared-with-me")]
    public async Task<IActionResult> GetSharedWithMe()
}
```

#### 4. **ExportController** (P2)
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExportController : ControllerBase
{
    // POST /api/export/request - Request data export
    [HttpPost("request")]
    public async Task<IActionResult> RequestExport(ExportRequestDto dto)
    
    // GET /api/export/status/{requestId} - Check export status
    [HttpGet("status/{requestId}")]
    public async Task<IActionResult> GetExportStatus(Guid requestId)
    
    // GET /api/export/download/{requestId} - Download export file
    [HttpGet("download/{requestId}")]
    public async Task<IActionResult> DownloadExport(Guid requestId)
    
    // POST /api/export/import - Import from JSON/CSV
    [HttpPost("import")]
    public async Task<IActionResult> ImportData(IFormFile file)
}
```

#### 5. **AdminController** (P3)
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    // GET /api/admin/users - List all users (paginated)
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(int page = 1, int pageSize = 50)
    
    // POST /api/admin/users/{id}/ban - Ban user
    [HttpPost("users/{id}/ban")]
    public async Task<IActionResult> BanUser(Guid id, [FromBody] string reason)
    
    // DELETE /api/admin/entries/{id} - Delete inappropriate entry
    [HttpDelete("entries/{id}")]
    public async Task<IActionResult> DeleteEntry(Guid id)
    
    // GET /api/admin/audit-logs - Audit trail
    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs(int page = 1)
    
    // POST /api/admin/audit-logs - Log admin action
    [HttpPost("audit-logs")]
    public async Task<IActionResult> CreateAuditLog(AdminAuditLogDto dto)
}
```

### New Services

#### 1. **NotificationService** (P0)
```csharp
public interface INotificationService
{
    Task CreateNotificationAsync(Guid userId, NotificationDto notification);
    Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, int page, int pageSize);
    Task MarkAsReadAsync(Guid notificationId);
    Task MarkAllAsReadAsync(Guid userId);
    Task DeleteNotificationAsync(Guid notificationId);
    
    // SignalR helpers
    Task NotifyUserAsync(Guid userId, string message);
    Task BroadcastAsync(string message);
}
```

#### 2. **AnalyticsService** (P1) - Extend existing
```csharp
public interface IAnalyticsService
{
    // Existing methods...
    
    // NEW: Advanced analytics
    Task<YearlyReportDto> GetYearlyReportAsync(Guid userId, int year);
    Task<MoodTrendsDto> GetMoodTrendsAsync(Guid userId, int months = 6);
    Task<List<ArtistStatsDto>> GetTopArtistsAsync(Guid userId, int limit = 10);
    Task<MoodCorrelationDto> GetMoodByArtistAsync(Guid userId);
    Task GenerateMonthlySnapshotAsync(Guid userId, int year, int month);
}
```

#### 3. **SocialService** (P3)
```csharp
public interface ISocialService
{
    // Friend management
    Task SendFriendRequestAsync(Guid requesterId, Guid receiverId);
    Task AcceptFriendRequestAsync(Guid requestId);
    Task RemoveFriendAsync(Guid friendshipId);
    Task<List<UserDto>> GetFriendsAsync(Guid userId);
    Task<List<FriendRequestDto>> GetPendingRequestsAsync(Guid userId);
    
    // Entry sharing
    Task ShareEntryAsync(Guid entryId, Guid userId, SharePermission permission);
    Task<List<SharedEntryDto>> GetSharedWithMeAsync(Guid userId);
    Task UnshareEntryAsync(Guid sharedEntryId);
    
    // Privacy
    Task<bool> IsPublicProfileAsync(Guid userId);
    Task SetPublicProfileAsync(Guid userId, bool isPublic);
}
```

#### 4. **ExportService** (P2)
```csharp
public interface IExportService
{
    Task<string> ExportAsJsonAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<string> ExportAsCsvAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<bool> ImportFromJsonAsync(Guid userId, string jsonContent);
    Task<bool> ImportFromCsvAsync(Guid userId, string csvContent);
    Task<ExportStatusDto> GetExportStatusAsync(Guid requestId);
}
```

#### 5. **AdminService** (P3)
```csharp
public interface IAdminService
{
    Task<List<UserDto>> GetAllUsersAsync(int page, int pageSize);
    Task BanUserAsync(Guid userId, string reason);
    Task UnbanUserAsync(Guid userId);
    Task DeleteEntryAsync(Guid entryId, string reason);
    Task<List<AdminAuditLogDto>> GetAuditLogsAsync(int page);
    Task LogActionAsync(Guid adminId, string action, Guid? targetUserId, Guid? targetEntryId, string details);
}
```

### SignalR Hub

#### **NotificationHub** (P0)
```csharp
public class NotificationHub : Hub
{
    private readonly INotificationService _notificationService;
    private readonly IUserService _userService;
    
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("sub")?.Value;
        if (userId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }
        await base.OnConnectedAsync();
    }
    
    // Called by server: broadcast notification to user
    public async Task SendNotification(string userId, NotificationDto notification)
    {
        await Clients.Group($"user-{userId}")
            .SendAsync("ReceiveNotification", notification);
    }
    
    // Called by server: new entry created by friend
    public async Task BroadcastEntryCreated(EntryDto entry, List<Guid> friendIds)
    {
        foreach (var friendId in friendIds)
        {
            await Clients.Group($"user-{friendId}")
                .SendAsync("FriendEntryCreated", entry);
        }
    }
    
    // Called by client: user is viewing dashboard (online indicator)
    public async Task SetUserOnline(string status = "online")
    {
        var userId = Context.User?.FindFirst("sub")?.Value;
        if (userId != null)
        {
            await Clients.All.SendAsync("UserStatusChanged", new { userId, status });
        }
    }
}
```

---

## Frontend Architecture

### New Blazor Components/Pages

#### 1. **Real-time Notifications** (P0)
```
Components/
  ├─ NotificationBell.razor (AppBar component)
  │  ├─ Dropdown list of latest 10 notifications
  │  ├─ "Mark all as read" button
  │  └─ NotificationItem.razor (child component)
  │
Pages/
  ├─ Notifications.razor
  │  ├─ Tabbed interface (All, Unread, Friends, System)
  │  ├─ Pagination (20 per page)
  │  └─ Filters (date range, type)
```

**SignalR Integration in Blazor:**
```csharp
@implements IAsyncDisposable
@inject NavigationManager Navigation

private HubConnection? hubConnection;
private List<NotificationDto> notifications = [];

protected override async Task OnInitializedAsync()
{
    hubConnection = new HubConnectionBuilder()
        .WithUrl(Navigation.ToAbsoluteUri("/hubs/notification"), options =>
        {
            options.AccessTokenProvider = async () => await GetToken();
        })
        .WithAutomaticReconnect()
        .Build();

    hubConnection.On<NotificationDto>("ReceiveNotification", 
        notification => OnNotificationReceived(notification));

    await hubConnection.StartAsync();
}

private void OnNotificationReceived(NotificationDto notification)
{
    notifications.Insert(0, notification);
    StateHasChanged();
}

async ValueTask IAsyncDisposable.DisposeAsync()
{
    if (hubConnection is not null)
    {
        await hubConnection.DisposeAsync();
    }
}
```

#### 2. **Advanced Analytics Pages** (P1)
```
Pages/
  ├─ Analytics.razor (enhanced with new data)
  │  ├─ Yearly summary section
  │  ├─ Mood trends chart (6-month)
  │  ├─ Top 10 artists + songs
  │  └─ Mood vs artist correlation
  │
  ├─ YearlyReport.razor (NEW)
  │  ├─ Year selector
  │  ├─ Monthly breakdown
  │  ├─ PDF export button
  │  └─ Download report
```

**Chart Libraries:**
- **ChartJS** (via NuGet package) for interactive charts
- **PDFSharp** for PDF export

#### 3. **Offline Support (PWA)** (P2)
```
wwwroot/
  ├─ manifest.json (PWA manifest)
  ├─ service-worker.js (offline caching)
  ├─ service-worker-assets.js (asset list)
  │
index.html
  ├─ <link rel="manifest" href="manifest.json" />
  ├─ <script src="service-worker.js"></script>
```

**Service Worker Strategy:**
- Cache-first for static assets (CSS, JS, images)
- Network-first for API calls (with fallback to cache)
- Background sync for queued entries (sync when online)

#### 4. **Export/Import UI** (P2)
```
Pages/
  ├─ DataManagement.razor
  │  ├─ Export section
  │  │  ├─ Format selector (JSON / CSV)
  │  │  ├─ Date range picker
  │  │  └─ Export button → download
  │  │
  │  ├─ Import section
  │  │  ├─ File upload input
  │  │  ├─ Preview table
  │  │  └─ "Confirm import" button
  │  │
  │  └─ Data retention policy
```

#### 5. **Enhanced Dark Mode** (P2)
```css
/* Additional color palette variables */
:root {
  --color-primary-dark: #1e1e2e;
  --color-surface-dark-1: #313244;
  --color-surface-dark-2: #45475a;
  --color-text-dark-primary: #cdd6f4;
  --color-text-dark-secondary: #a6adc8;
  
  /* Accent colors (vibrant but accessible) */
  --color-accent-blue: #89b4fa;
  --color-accent-green: #a6e3a1;
  --color-accent-purple: #cba6f7;
  --color-accent-peach: #fab387;
}

@media (prefers-color-scheme: dark) {
  /* Automatically apply dark theme */
}
```

#### 6. **Social Features Pages** (P3)
```
Pages/
  ├─ Friends.razor
  │  ├─ Friend list (with online/offline indicators)
  │  ├─ Friend requests (pending)
  │  └─ Search & add friends
  │
  ├─ Profile.razor (user public profile)
  │  ├─ User info
  │  ├─ Shared entries (public)
  │  ├─ Top artists/songs
  │  └─ Send friend request button
  │
  ├─ SharedEntries.razor
  │  ├─ Entries shared with me
  │  ├─ Who shared (filter by friend)
  │  └─ Comment thread (if enabled)
```

#### 7. **Admin Panel** (P3)
```
Pages/Admin/
  ├─ AdminDashboard.razor
  │  ├─ Key metrics (users, entries, reports)
  │  ├─ System health status
  │  └─ Recent audit logs
  │
  ├─ UserManagement.razor
  │  ├─ User table (paginated, sortable)
  │  ├─ Ban/Unban actions
  │  ├─ Email verification status
  │  └─ Last activity timestamp
  │
  ├─ ContentModeration.razor
  │  ├─ Reported entries (if reporting feature added)
  │  ├─ Actions (approve/delete/warn)
  │  └─ Moderation notes
  │
  ├─ AuditLogs.razor
  │  ├─ Timeline of admin actions
  │  ├─ Filters (action type, date, admin)
  │  └─ Export logs
```

---

## Implementation Timeline

### Sprint 4.1 (2 weeks: Week 1-2 of Feb)
**Focus:** Real-time + Dark Mode + Analytics Foundation

**Tasks:**
1. ✅ Create `Notification` entity + migration
2. ✅ Implement `NotificationHub` (SignalR)
3. ✅ Build `NotificationService` + `NotificationController`
4. ✅ Create `NotificationBell.razor` component
5. ✅ Create `Notifications.razor` page
6. ✅ Enhance `AnalyticsService` with yearly reports
7. ✅ Enhance `Analytics.razor` page with new charts
8. ✅ Implement enhanced dark mode color palette
9. ✅ Update `ThemeService` with new color tokens

**Deliverables:**
- Real-time notifications working end-to-end
- Advanced analytics with yearly report
- Dark mode palette improvements
- Build: 0 warnings ✅

---

### Sprint 4.2 (2 weeks: Week 3-4 of Feb)
**Focus:** Offline + Export/Import + Settings

**Tasks:**
1. ✅ Create `UserSettings` entity + migration
2. ✅ Implement PWA manifest + service worker
3. ✅ Implement `ExportService` (JSON/CSV)
4. ✅ Implement `ImportService` (JSON/CSV validation)
5. ✅ Create `ExportController` + `ExportService`
6. ✅ Create `DataManagement.razor` page
7. ✅ Create `Settings.razor` page (feature toggles)
8. ✅ Implement offline fallback pages
9. ✅ Test service worker caching

**Deliverables:**
- PWA fully functional (installable, offline fallback)
- Export/Import working with validation
- User settings panel
- All 7 features have feature flags

---

### Sprint 4.3 (2 weeks: Week 5-6 of March)
**Focus:** Social + Admin

**Tasks:**
1. ✅ Create `UserFriendship`, `SharedEntry` entities + migrations
2. ✅ Create `UserRole` entity + authorization (Admin, User)
3. ✅ Implement `SocialService` + `SocialController`
4. ✅ Implement `AdminService` + `AdminController`
5. ✅ Create `AdminAuditLog` entity
6. ✅ Create social pages (Friends, Profile, SharedEntries)
7. ✅ Create admin pages (Dashboard, UserManagement, Moderation, AuditLogs)
8. ✅ Implement role-based authorization middleware
9. ✅ Add friend notifications to SignalR

**Deliverables:**
- Complete social feature set
- Admin panel fully functional
- Role-based access control (RBAC)
- Friend request flow end-to-end
- Shared entries visibility rules

---

### Sprint 4.4 (2 weeks: Week 7-8 of March)
**Focus:** Integration, Testing, Hardening

**Tasks:**
1. ✅ E2E tests for all 7 features
2. ✅ Performance testing (SignalR load testing)
3. ✅ Security audit (JWT with roles, CORS for admin)
4. ✅ Accessibility audit (new pages WCAG AA)
5. ✅ Load testing (notifications, concurrent users)
6. ✅ Database query optimization
7. ✅ User documentation
8. ✅ Release notes + migration guide

**Deliverables:**
- All 7 features fully tested
- Documentation complete
- Release ready for Phase 4 beta

---

## Risk Assessment

### High Risk Items

| Risk | Impact | Mitigation |
|------|--------|-----------|
| **SignalR scalability** (many users online) | API overload | Implement message batching, connection pooling |
| **Real-time DB conflicts** (concurrent entry creation) | Data corruption | Optimistic locking + conflict resolution |
| **PWA offline data sync** (queued changes) | Lost data | Queue with timestamps, exponential backoff retry |
| **Export/Import validation** (malicious CSV) | Security | Strict schema validation, file size limits, virus scan |

### Medium Risk Items

| Risk | Impact | Mitigation |
|------|--------|-----------|
| **Admin panel access control** | Unauthorized actions | Role-based middleware, audit logging |
| **Friend sharing privacy** | Data leakage | Explicit permissions, audit trail |
| **Mobile offline experience** | Bad UX | Progressive enhancement, UI feedback |

### Low Risk Items
- Dark mode palette (visual only)
- Analytics calculations (read-only)
- Admin audit logging (append-only)

---

## Success Criteria

✅ **Phase 4 is complete when:**

1. All 7 features implemented per specification
2. **Build:** 0 errors, 0 warnings
3. **Tests:** ≥95% code coverage on new features
4. **E2E:** All test scenarios pass (from E2E_TESTING_GUIDE.md)
5. **Performance:** API response time <500ms (p95), SignalR <100ms latency
6. **Accessibility:** WCAG 2.1 AA on all new pages
7. **Security:** No OWASP Top 10 vulnerabilities
8. **Documentation:** 
   - API docs (OpenAPI/Swagger)
   - Component documentation (Storybook-style comments)
   - User guide for new features
   - Admin guide for management features

---

## Dependencies & Prerequisites

### NuGet Packages to Add

```xml
<!-- Real-time -->
<PackageReference Include="Microsoft.AspNetCore.SignalR" Version="10.0.0" />
<PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="10.0.0" />

<!-- Charts & PDF -->
<PackageReference Include="SkiaSharp" Version="2.88.0" />
<PackageReference Include="iText7" Version="8.0.0" />

<!-- Data export -->
<PackageReference Include="CsvHelper" Version="30.0.0" />

<!-- Background jobs (for export processing) -->
<PackageReference Include="Hangfire.Core" Version="1.8.0" />
<PackageReference Include="Hangfire.AspNetCore" Version="1.8.0" />
```

### Database Migrations
All 6 new entities require migrations:
```bash
dotnet ef migrations add Phase4_Notifications -p src/MIMM.Backend
dotnet ef migrations add Phase4_UserFriendships -p src/MIMM.Backend
dotnet ef migrations add Phase4_SharedEntries -p src/MIMM.Backend
dotnet ef migrations add Phase4_AnalyticsSnapshot -p src/MIMM.Backend
dotnet ef migrations add Phase4_UserSettings -p src/MIMM.Backend
dotnet ef migrations add Phase4_AdminAuditLog -p src/MIMM.Backend
```

---

## Next Steps

1. **Review & Approval** → Confirm feature scope with stakeholders
2. **Setup Database** → Create migrations for all 6 new entities
3. **Setup SignalR** → Verify hub configuration in Program.cs
4. **Begin Sprint 4.1** → Start with notifications + analytics
5. **Weekly Reviews** → Check progress, adjust as needed

---

**Document Status:** Ready for Development  
**Last Updated:** January 26, 2026  
**Next Review:** End of Sprint 4.1
