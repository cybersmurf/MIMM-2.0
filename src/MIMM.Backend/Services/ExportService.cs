using System.Globalization;
using System.Text;
using CsvHelper;
using Microsoft.Extensions.Logging;
using MIMM.Backend.Data;
using MIMM.Shared.Dtos;

namespace MIMM.Backend.Services;

/// <summary>
/// Service for exporting journal entries to various formats (CSV, JSON)
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Export entries as CSV file
    /// </summary>
    Task<byte[]> ExportAsCsvAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Export entries as JSON file
    /// </summary>
    Task<string> ExportAsJsonAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class ExportService : IExportService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ExportService> _logger;

    public ExportService(ApplicationDbContext dbContext, ILogger<ExportService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<byte[]> ExportAsCsvAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting entries as CSV for user {UserId}", userId);

        // Get all non-deleted entries for user
        var entries = _dbContext.Entries
            .Where(e => e.UserId == userId && e.DeletedAt == null)
            .OrderByDescending(e => e.CreatedAt)
            .ToList(); // Force evaluation before mapping

        var exportDtos = entries.Select(MapToExportDto).ToList();

        try
        {
            using var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                // Write CSV header
                csvWriter.WriteHeader<ExportEntryDto>();
                await csvWriter.NextRecordAsync();

                // Write records
                foreach (var entry in exportDtos)
                {
                    csvWriter.WriteRecord(entry);
                    await csvWriter.NextRecordAsync();
                }

                await csvWriter.FlushAsync();
            }

            var csv = memoryStream.ToArray();
            _logger.LogInformation("Exported {Count} entries as CSV for user {UserId}", entries.Count, userId);
            return csv;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting entries as CSV for user {UserId}", userId);
            throw;
        }
    }

    public Task<string> ExportAsJsonAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting entries as JSON for user {UserId}", userId);

        // Get all non-deleted entries for user
        var entries = _dbContext.Entries
            .Where(e => e.UserId == userId && e.DeletedAt == null)
            .OrderByDescending(e => e.CreatedAt)
            .ToList();

        var exportDtos = entries.Select(MapToExportDto).ToList();

        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(
                new { entries = exportDtos, exportedAt = DateTime.UtcNow, version = "1.0" },
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            );

            _logger.LogInformation("Exported {Count} entries as JSON for user {UserId}", entries.Count, userId);
            return Task.FromResult(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting entries as JSON for user {UserId}", userId);
            throw;
        }
    }

    private static ExportEntryDto MapToExportDto(Shared.Entities.JournalEntry entry)
    {
        return new ExportEntryDto
        {
            SongTitle = entry.SongTitle,
            ArtistName = entry.ArtistName,
            AlbumName = entry.AlbumName,
            SongId = entry.SongId,
            CoverUrl = entry.CoverUrl,
            Source = entry.Source,
            Valence = (double)entry.Valence,
            Arousal = (double)entry.Arousal,
            TensionLevel = entry.TensionLevel,
            SomaticTags = entry.SomaticTags,
            Notes = entry.Notes,
            ScrobbledToLastFm = entry.ScrobbledToLastFm,
            CreatedAt = entry.CreatedAt,
            UpdatedAt = entry.UpdatedAt
        };
    }
}
