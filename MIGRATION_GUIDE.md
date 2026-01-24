# MIMM 1.0 → 2.0 Migration Guide

**Version**: 1.0  
**Date**: 24. ledna 2026  
**Purpose**: Guide users through data migration from localStorage-based MIMM 1.0 to database-backed MIMM 2.0

---

## Overview

MIMM 2.0 introduces a significant architectural change: from **localStorage (browser-based)** to **PostgreSQL database (server-based)** with user authentication. This guide helps users migrate their existing journal entries.

### What Changes?

| Aspect | MIMM 1.0 | MIMM 2.0 |
|--------|----------|----------|
| **Data Storage** | Browser localStorage | PostgreSQL database |
| **Authentication** | None (single user) | Email + password (JWT) |
| **Accessibility** | Single device only | Multiple devices |
| **Data Limit** | ~5MB (browser limit) | Unlimited |
| **Backup** | Manual export | Automatic backups |
| **Sharing** | Not possible | Optional (future) |

---

## Part 1: Export Data from MIMM 1.0

### Step 1: Run Export Script

Open MIMM 1.0 in your browser and run this script in the **Developer Console** (F12):

```javascript
// MIMM 1.0 Data Export Script
(function() {
    console.log('🎵 MIMM 1.0 Data Export Starting...');
    
    // Read from localStorage
    const entriesJson = localStorage.getItem('mimm_entries');
    const preferencesJson = localStorage.getItem('mimm_preferences');
    
    if (!entriesJson) {
        console.error('❌ No entries found in localStorage');
        alert('No MIMM data found. Have you created any entries?');
        return;
    }
    
    const entries = JSON.parse(entriesJson);
    const preferences = preferencesJson ? JSON.parse(preferencesJson) : {};
    
    console.log(`✅ Found ${entries.length} journal entries`);
    
    // Transform data to MIMM 2.0 format
    const exportData = {
        version: '1.0',
        exportDate: new Date().toISOString(),
        user: {
            language: preferences.language || 'en',
            timezone: Intl.DateTimeFormat().resolvedOptions().timeZone
        },
        entries: entries.map(entry => ({
            // Song info
            songTitle: entry.song?.title || entry.songTitle || 'Unknown',
            artistName: entry.song?.artist || entry.artistName || 'Unknown',
            albumName: entry.song?.album || entry.albumName || 'Unknown',
            songId: entry.song?.id || entry.songId || null,
            coverUrl: entry.song?.coverUrl || entry.coverUrl || null,
            source: entry.song?.source || entry.source || 'manual',
            
            // Mood (Russell Circumplex)
            valence: entry.mood?.valence ?? 0,
            arousal: entry.mood?.arousal ?? 0,
            
            // Body sensations
            tensionLevel: entry.body?.tension ?? 50,
            somaticTags: entry.body?.tags || [],
            notes: entry.body?.notes || '',
            
            // Metadata
            createdAt: entry.createdAt || entry.timestamp || new Date().toISOString(),
            scrobbledToLastFm: false
        }))
    };
    
    // Create downloadable JSON file
    const blob = new Blob([JSON.stringify(exportData, null, 2)], { 
        type: 'application/json' 
    });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `mimm-export-${new Date().toISOString().split('T')[0]}.json`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
    
    console.log('✅ Export successful!');
    console.log(`📦 File: mimm-export-${new Date().toISOString().split('T')[0]}.json`);
    alert(`✅ Export complete! ${entries.length} entries saved.`);
})();
```

### Step 2: Verify Export File

1. Open the downloaded `mimm-export-YYYY-MM-DD.json` file
2. Verify structure:

```json
{
  "version": "1.0",
  "exportDate": "2026-01-24T...",
  "user": {
    "language": "cs",
    "timezone": "Europe/Prague"
  },
  "entries": [
    {
      "songTitle": "...",
      "artistName": "...",
      "valence": 0.5,
      "arousal": 0.8,
      "createdAt": "..."
    }
  ]
}
```

---

## Part 2: Import Data to MIMM 2.0

### Step 1: Register Account

1. Navigate to MIMM 2.0: `https://mimm.example.com` (or your deployment URL)
2. Click **"Register"**
3. Enter:
   - Email address
   - Password (minimum 6 characters)
   - Display name (optional)
4. Click **"Create Account"**
5. *(Optional)* Verify email if enabled

### Step 2: Upload Export File

1. Log in to MIMM 2.0
2. Navigate to **Settings** → **Data Import**
3. Click **"Import from MIMM 1.0"**
4. Select your `mimm-export-YYYY-MM-DD.json` file
5. Click **"Upload"**

### Step 3: Review Import

The system will:

- Validate JSON structure
- Check for duplicate entries (by date + song)
- Transform data to MIMM 2.0 format
- Insert entries into database

You'll see a summary:

```python
✅ Import Complete

- Total entries in file: 127
- Successfully imported: 127
- Duplicates skipped: 0
- Errors: 0

Your journal is now available in MIMM 2.0!
```

---

## Part 3: Manual Import (Alternative via API)

If the web UI import doesn't work, use the API directly:

### Step 1: Get JWT Token

```bash
# Login via API
curl -X POST https://mimm.example.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "your-email@example.com",
    "password": "your-password"
  }'

# Response: { "accessToken": "eyJ..." }
```

### Step 2: Import via API

```bash
# Replace YOUR_JWT_TOKEN with token from Step 1
curl -X POST https://mimm.example.com/api/v1-migration/import \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d @mimm-export-2026-01-24.json
```

### Response

```json
{
  "success": true,
  "totalEntries": 127,
  "imported": 127,
  "duplicates": 0,
  "errors": []
}
```

---

## Part 4: Post-Migration Checklist

### Verify Data Integrity

- [ ] All entries visible in dashboard
- [ ] Entry count matches export file
- [ ] Mood coordinates preserved (valence, arousal)
- [ ] Somatic tags intact
- [ ] Song metadata (title, artist, album) correct
- [ ] Timestamps preserved

### Cleanup Old Data

**Option A: Keep MIMM 1.0 as backup**

- Leave localStorage data intact
- Bookmark old Vercel URL

**Option B: Delete MIMM 1.0 data**

- Open Developer Console (F12)
- Run: `localStorage.removeItem('mimm_entries')`
- Verify: `localStorage.getItem('mimm_entries')` → `null`

### Update Bookmarks

- Remove old MIMM 1.0 URL
- Bookmark new MIMM 2.0 URL
- Install PWA on mobile (if applicable)

---

## Part 5: Troubleshooting

### Problem: Export script doesn't work

**Solution**:

1. Ensure you're on the MIMM 1.0 page (not 2.0)
2. Check browser console for errors
3. Verify localStorage is not empty: `localStorage.getItem('mimm_entries')`

### Problem: Import fails with "Invalid format"

**Solution**:

1. Open JSON file in text editor
2. Validate JSON syntax: <https://jsonlint.com/>
3. Ensure `version` field exists: `"version": "1.0"`
4. Check `entries` is an array

### Problem: Some entries missing after import

**Solution**:

1. Check import summary for errors
2. Look for duplicate timestamps (system skips exact duplicates)
3. Manually add missing entries via UI

### Problem: Mood coordinates wrong

**Solution**:

- MIMM 1.0 used different scale? Check original values
- Valence/Arousal should be between -1.0 and 1.0
- Contact support with sample entry

---

## Part 6: Migration Script for Developers

If you're self-hosting MIMM 2.0, add this endpoint to your backend:

```csharp
// Controllers/MigrationController.cs
[ApiController]
[Route("api/v1-migration")]
[Authorize]
public class MigrationController : ControllerBase
{
    private readonly IEntryService _entryService;
    private readonly ILogger<MigrationController> _logger;

    public MigrationController(IEntryService entryService, ILogger<MigrationController> logger)
    {
        _entryService = entryService;
        _logger = logger;
    }

    [HttpPost("import")]
    public async Task<IActionResult> ImportFromV1([FromBody] MigrationData data)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        try
        {
            var imported = 0;
            var duplicates = 0;
            var errors = new List<string>();

            foreach (var entry in data.Entries)
            {
                try
                {
                    // Check for duplicate (same song + similar timestamp)
                    var exists = await _entryService.ExistsAsync(
                        Guid.Parse(userId), 
                        entry.SongTitle, 
                        entry.CreatedAt
                    );

                    if (exists)
                    {
                        duplicates++;
                        continue;
                    }

                    // Create entry
                    await _entryService.CreateAsync(Guid.Parse(userId), new EntryCreateRequest
                    {
                        SongTitle = entry.SongTitle,
                        ArtistName = entry.ArtistName,
                        AlbumName = entry.AlbumName,
                        SongId = entry.SongId,
                        CoverUrl = entry.CoverUrl,
                        Source = entry.Source,
                        Valence = entry.Valence,
                        Arousal = entry.Arousal,
                        TensionLevel = entry.TensionLevel,
                        SomaticTags = entry.SomaticTags,
                        Notes = entry.Notes,
                        CreatedAt = entry.CreatedAt
                    });

                    imported++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to import entry: {Title}", entry.SongTitle);
                    errors.Add($"{entry.SongTitle}: {ex.Message}");
                }
            }

            return Ok(new
            {
                success = true,
                totalEntries = data.Entries.Count,
                imported,
                duplicates,
                errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Migration import failed");
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class MigrationData
{
    public string Version { get; set; } = null!;
    public DateTime ExportDate { get; set; }
    public UserData User { get; set; } = null!;
    public List<MigrationEntry> Entries { get; set; } = new();
}

public class UserData
{
    public string Language { get; set; } = "en";
    public string? Timezone { get; set; }
}

public class MigrationEntry
{
    public string SongTitle { get; set; } = null!;
    public string ArtistName { get; set; } = null!;
    public string AlbumName { get; set; } = null!;
    public string? SongId { get; set; }
    public string? CoverUrl { get; set; }
    public string Source { get; set; } = "manual";
    public float Valence { get; set; }
    public float Arousal { get; set; }
    public int TensionLevel { get; set; }
    public string[] SomaticTags { get; set; } = Array.Empty<string>();
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

## FAQ

**Q: Will my MIMM 1.0 data be deleted?**  
A: No. The export script only reads from localStorage, it doesn't modify or delete anything.

**Q: Can I use both MIMM 1.0 and 2.0 simultaneously?**  
A: Yes, but they won't sync. New entries in 2.0 won't appear in 1.0 and vice versa.

**Q: Do I need to delete MIMM 1.0 after migration?**  
A: No, but it's recommended to avoid confusion. You can keep it as a backup.

**Q: What if I have duplicate entries?**  
A: The import system automatically skips duplicates (same song + date within 1 hour).

**Q: Can I re-import if I made a mistake?**  
A: Yes, but the system will skip duplicates. To force re-import, delete entries first via UI.

**Q: Will my Last.fm connection transfer?**  
A: No. Last.fm uses OAuth which can't be exported. You'll need to reconnect in MIMM 2.0.

---

**Support**: If you encounter issues, open an issue on GitHub or contact <support@mimm.example.com>

**Document Version**: 1.0  
**Last Updated**: 24. ledna 2026
