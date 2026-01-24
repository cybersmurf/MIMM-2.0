// ============================================================================
// MIMM 1.0 → 2.0 Data Export Script
// ============================================================================
// Purpose: Export journal entries from localStorage to JSON file
// Usage: Run this in Developer Console (F12) while on MIMM 1.0 page
// Output: Downloads mimm-export-YYYY-MM-DD.json file
// ============================================================================

(function() {
    'use strict';
    
    console.log('🎵 MIMM 1.0 Data Export Script v1.0');
    console.log('=====================================\n');
    
    // ========== Step 1: Read from localStorage ==========
    console.log('📖 Reading data from localStorage...');
    
    const entriesJson = localStorage.getItem('mimm_entries');
    const preferencesJson = localStorage.getItem('mimm_preferences');
    
    if (!entriesJson) {
        console.error('❌ ERROR: No entries found in localStorage');
        alert('⚠️ No MIMM data found!\n\nPossible reasons:\n1. No entries created yet\n2. Data was cleared\n3. Wrong page (must be on MIMM 1.0)');
        return;
    }
    
    let entries;
    try {
        entries = JSON.parse(entriesJson);
    } catch (e) {
        console.error('❌ ERROR: Failed to parse entries JSON', e);
        alert('❌ Error: localStorage data is corrupted');
        return;
    }
    
    const preferences = preferencesJson ? JSON.parse(preferencesJson) : {};
    
    console.log(`✅ Found ${entries.length} journal entries`);
    console.log(`✅ User language: ${preferences.language || 'en'}`);
    console.log('');
    
    // ========== Step 2: Transform to MIMM 2.0 format ==========
    console.log('🔄 Transforming data to MIMM 2.0 format...');
    
    const transformedEntries = entries.map((entry, index) => {
        // Handle different MIMM 1.0 data structures
        const song = entry.song || entry;
        const mood = entry.mood || {};
        const body = entry.body || {};
        
        return {
            // Song information
            songTitle: song.title || entry.songTitle || 'Unknown Song',
            artistName: song.artist || entry.artistName || 'Unknown Artist',
            albumName: song.album || entry.albumName || 'Unknown Album',
            songId: song.id || entry.songId || null,
            coverUrl: song.coverUrl || entry.coverUrl || null,
            source: song.source || entry.source || 'manual',
            
            // Mood (Russell Circumplex Model)
            valence: parseFloat(mood.valence ?? mood.x ?? 0),
            arousal: parseFloat(mood.arousal ?? mood.y ?? 0),
            
            // Body sensations
            tensionLevel: parseInt(body.tension ?? body.tensionLevel ?? 50, 10),
            somaticTags: Array.isArray(body.tags) ? body.tags : (body.somaticTags || []),
            notes: body.notes || entry.notes || '',
            
            // Metadata
            createdAt: entry.createdAt || entry.timestamp || entry.date || new Date().toISOString(),
            scrobbledToLastFm: false
        };
    });
    
    console.log(`✅ Transformed ${transformedEntries.length} entries`);
    console.log('');
    
    // ========== Step 3: Create export object ==========
    const exportData = {
        version: '1.0',
        exportDate: new Date().toISOString(),
        sourceApp: 'MIMM 1.0',
        user: {
            language: preferences.language || 'en',
            timezone: Intl.DateTimeFormat().resolvedOptions().timeZone,
            entryCount: transformedEntries.length
        },
        entries: transformedEntries
    };
    
    // ========== Step 4: Generate statistics ==========
    console.log('📊 Export Statistics:');
    console.log('   - Total entries:', transformedEntries.length);
    console.log('   - Date range:', 
        transformedEntries.length > 0 
            ? `${new Date(transformedEntries[transformedEntries.length - 1].createdAt).toLocaleDateString()} to ${new Date(transformedEntries[0].createdAt).toLocaleDateString()}`
            : 'N/A'
    );
    
    const sources = {};
    transformedEntries.forEach(e => {
        sources[e.source] = (sources[e.source] || 0) + 1;
    });
    console.log('   - Sources:', sources);
    console.log('');
    
    // ========== Step 5: Create downloadable file ==========
    console.log('💾 Creating download file...');
    
    const jsonString = JSON.stringify(exportData, null, 2);
    const blob = new Blob([jsonString], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    
    const filename = `mimm-export-${new Date().toISOString().split('T')[0]}.json`;
    
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.style.display = 'none';
    document.body.appendChild(a);
    a.click();
    
    // Cleanup
    setTimeout(() => {
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    }, 100);
    
    // ========== Step 6: Success message ==========
    console.log('✅ Export successful!');
    console.log(`📦 File: ${filename}`);
    console.log(`📏 Size: ${(blob.size / 1024).toFixed(2)} KB`);
    console.log('');
    console.log('📝 Next steps:');
    console.log('   1. Locate the downloaded JSON file');
    console.log('   2. Register account in MIMM 2.0');
    console.log('   3. Import via Settings → Data Import');
    console.log('   4. Verify all entries imported correctly');
    console.log('');
    console.log('📚 For help, see: MIGRATION_GUIDE.md');
    
    alert(`✅ Export Complete!\n\n` +
          `${transformedEntries.length} entries saved to:\n${filename}\n\n` +
          `Next: Register in MIMM 2.0 and import this file.`);
    
})();

// ============================================================================
// End of Export Script
// ============================================================================
