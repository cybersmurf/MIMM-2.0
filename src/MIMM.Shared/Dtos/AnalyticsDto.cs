namespace MIMM.Shared.Dtos;

public record MoodTrendDto
{
    public int DaysAnalyzed { get; set; }
    public int TotalEntries { get; set; }
    public double AverageValence { get; set; }
    public double AverageArousal { get; set; }
    public List<DailyMoodDto> DailyAverages { get; set; } = [];
    public MoodDistributionDto MoodDistribution { get; set; } = new();
}

public record DailyMoodDto
{
    public DateTime Date { get; set; }
    public double AverageValence { get; set; }
    public double AverageArousal { get; set; }
    public int EntryCount { get; set; }
}

public record MoodDistributionDto
{
    public double HappyPercent { get; set; }
    public double CalmPercent { get; set; }
    public double AngryPercent { get; set; }
    public double SadPercent { get; set; }
}

public record MusicStatisticsDto
{
    public int TotalEntriesLogged { get; set; }
    public int TotalScrobbled { get; set; }
    public double ScrobbleRate { get; set; }
    public List<ArtistStatDto> TopArtists { get; set; } = [];
    public List<SongStatDto> TopSongs { get; set; } = [];
    public Dictionary<string, int> EntriesBySource { get; set; } = [];
}

public record ArtistStatDto
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public record SongStatDto
{
    public string Title { get; set; } = string.Empty;
    public string? Artist { get; set; }
    public int Count { get; set; }
}
