namespace MIMM.Shared.Dtos;

public record YearlyReportDto
{
    public required int Year { get; init; }
    public required int TotalEntries { get; init; }
    public required List<MonthlyStatsDto> MonthlyStats { get; init; }
    public required double AverageValence { get; init; }
    public required double AverageArousal { get; init; }
    public required List<TopArtistDto> TopArtists { get; init; }
    public required List<TopSongDto> TopSongs { get; init; }
    public required MoodDistributionDto MoodDistribution { get; init; }
    public required DateTime GeneratedAt { get; init; }
}

public record MonthlyStatsDto
{
    public required int Month { get; init; }
    public required string MonthName { get; init; }
    public required int EntryCount { get; init; }
    public required double AverageValence { get; init; }
    public required double AverageArousal { get; init; }
    public required List<TopArtistDto> TopArtists { get; init; }
}

public record TopArtistDto
{
    public required string Name { get; init; }
    public required int Count { get; init; }
    public required double AverageMoodValence { get; init; }
}

public record TopSongDto
{
    public required string Title { get; init; }
    public required string? Artist { get; init; }
    public required int Count { get; init; }
    public required double AverageMoodValence { get; init; }
}

public record MoodCorrelationDto
{
    public required List<ArtistMoodDto> ArtistMoods { get; init; }
    public required List<SourceMoodDto> SourceMoods { get; init; }
}

public record ArtistMoodDto
{
    public required string Artist { get; init; }
    public required int EntryCount { get; init; }
    public required double AverageValence { get; init; }
    public required double AverageArousal { get; init; }
}

public record SourceMoodDto
{
    public required string Source { get; init; }
    public required int EntryCount { get; init; }
    public required double AverageValence { get; init; }
    public required double AverageArousal { get; init; }
}
