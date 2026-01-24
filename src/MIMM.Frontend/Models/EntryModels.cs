namespace MIMM.Frontend.Models;

/// <summary>
/// Editable model for creating journal entries
/// </summary>
public class CreateEntryModel
{
    public string SongTitle { get; set; } = string.Empty;
    public string? ArtistName { get; set; }
    public string? AlbumName { get; set; }
    public double Valence { get; set; } = 0.0;
    public double Arousal { get; set; } = 0.0;
    public int TensionLevel { get; set; } = 50;
    public string[]? SomaticTags { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Editable model for updating journal entries
/// </summary>
public class UpdateEntryModel
{
    public string? SongTitle { get; set; }
    public string? ArtistName { get; set; }
    public string? AlbumName { get; set; }
    public double Valence { get; set; } = 0.0;
    public double Arousal { get; set; } = 0.0;
    public int TensionLevel { get; set; } = 50;
    public string[]? SomaticTags { get; set; }
    public string? Notes { get; set; }
}
