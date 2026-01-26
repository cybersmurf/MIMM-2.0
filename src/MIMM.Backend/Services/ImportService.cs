using System.Globalization;
using System.Text;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MIMM.Backend.Data;
using MIMM.Shared.Dtos;
using MIMM.Shared.Entities;

namespace MIMM.Backend.Services;

/// <summary>
/// Service for importing journal entries from various formats (CSV, JSON)
/// </summary>
public interface IImportService
{
    /// <summary>
    /// Import entries from CSV file
    /// </summary>
    Task<ImportResponse> ImportFromCsvAsync(
        Guid userId,
        IFormFile file,
        bool continueOnError = true,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Import entries from JSON file
    /// </summary>
    Task<ImportResponse> ImportFromJsonAsync(
        Guid userId,
        IFormFile file,
        bool continueOnError = true,
        CancellationToken cancellationToken = default
    );
}

public class ImportService : IImportService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ImportService> _logger;

    public ImportService(ApplicationDbContext dbContext, ILogger<ImportService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ImportResponse> ImportFromCsvAsync(
        Guid userId,
        IFormFile file,
        bool continueOnError = true,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Starting CSV import for user {UserId} from file {FileName}", userId, file.FileName);

        var response = new ImportResponse();
        var errors = new List<ImportError>();

        // Validate user exists
        var userExists = await _dbContext.Users
            .AnyAsync(u => u.Id == userId && u.DeletedAt == null, cancellationToken);

        if (!userExists)
        {
            return response with
            {
                Errors = [new ImportError { ErrorMessage = "User not found" }]
            };
        }

        try
        {
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);

            var entriesToImport = new List<JournalEntry>();
            var lineNumber = 2; // Skip header

            try
            {
                await csvReader.ReadAsync();
                csvReader.ReadHeader();

                while (await csvReader.ReadAsync())
                {
                    response = response with { TotalProcessed = response.TotalProcessed + 1 };

                    try
                    {
                        var songTitle = csvReader.GetField("SongTitle");
                        
                        if (string.IsNullOrWhiteSpace(songTitle))
                        {
                            errors.Add(new ImportError
                            {
                                LineNumber = lineNumber,
                                ErrorMessage = "Song title is required"
                            });

                            if (!continueOnError)
                                throw new InvalidOperationException("Song title is required");

                            lineNumber++;
                            continue;
                        }

                        var entry = new JournalEntry
                        {
                            Id = Guid.NewGuid(),
                            UserId = userId,
                            SongTitle = songTitle.Trim(),
                            ArtistName = GetOptionalField(csvReader, "ArtistName"),
                            AlbumName = GetOptionalField(csvReader, "AlbumName") ?? "Unknown Album",
                            SongId = GetOptionalField(csvReader, "SongId"),
                            CoverUrl = GetOptionalField(csvReader, "CoverUrl"),
                            Source = GetOptionalField(csvReader, "Source") ?? "imported",
                            Valence = float.TryParse(
                                csvReader.GetField("Valence"),
                                System.Globalization.NumberStyles.Any,
                                CultureInfo.InvariantCulture,
                                out var valence
                            ) ? valence : 0f,
                            Arousal = float.TryParse(
                                csvReader.GetField("Arousal"),
                                System.Globalization.NumberStyles.Any,
                                CultureInfo.InvariantCulture,
                                out var arousal
                            ) ? arousal : 0f,
                            TensionLevel = int.TryParse(csvReader.GetField("TensionLevel") ?? string.Empty, out int tensionValue) ? (int?)tensionValue : null,
                            SomaticTags = ParseArray(csvReader.GetField("SomaticTags")) ?? [],
                            Notes = GetOptionalField(csvReader, "Notes"),
                            ScrobbledToLastFm = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = null,
                            DeletedAt = null
                        };

                        entriesToImport.Add(entry);
                    }
                    catch (Exception ex)
                    {
                        errors.Add(new ImportError
                        {
                            LineNumber = lineNumber,
                            ErrorMessage = ex.Message
                        });

                        if (!continueOnError)
                            throw;
                    }

                    lineNumber++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing CSV for user {UserId}", userId);
                errors.Add(new ImportError { ErrorMessage = $"CSV parsing error: {ex.Message}" });

                if (!continueOnError)
                    throw;
            }

            // Import successful entries
            if (entriesToImport.Count > 0)
            {
                _dbContext.Entries.AddRange(entriesToImport);
                await _dbContext.SaveChangesAsync(cancellationToken);
                response = response with { SuccessfullyImported = entriesToImport.Count };
            }

            response = response with
            {
                FailedCount = errors.Count,
                Errors = errors
            };

            _logger.LogInformation(
                "CSV import completed for user {UserId}: {Successful} successful, {Failed} failed",
                userId, response.SuccessfullyImported, response.FailedCount
            );

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error during CSV import for user {UserId}", userId);
            errors.Add(new ImportError { ErrorMessage = $"Fatal error: {ex.Message}" });
            return response with { Errors = errors };
        }
    }

    public async Task<ImportResponse> ImportFromJsonAsync(
        Guid userId,
        IFormFile file,
        bool continueOnError = true,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Starting JSON import for user {UserId} from file {FileName}", userId, file.FileName);

        var response = new ImportResponse();
        var errors = new List<ImportError>();

        // Validate user exists
        var userExists = await _dbContext.Users
            .AnyAsync(u => u.Id == userId && u.DeletedAt == null, cancellationToken);

        if (!userExists)
        {
            return response with
            {
                Errors = [new ImportError { ErrorMessage = "User not found" }]
            };
        }

        try
        {
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync(cancellationToken);

            var importData = System.Text.Json.JsonSerializer.Deserialize<JsonImportData>(
                json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (importData?.Entries == null || importData.Entries.Count == 0)
            {
                return response with
                {
                    Errors = [new ImportError { ErrorMessage = "No entries found in JSON file" }]
                };
            }

            var entriesToImport = new List<JournalEntry>();

            for (int i = 0; i < importData.Entries.Count; i++)
            {
                response = response with { TotalProcessed = response.TotalProcessed + 1 };

                try
                {
                    var jsonEntry = importData.Entries[i];

                    if (string.IsNullOrWhiteSpace(jsonEntry.SongTitle))
                    {
                        errors.Add(new ImportError
                        {
                            LineNumber = i + 1,
                            ErrorMessage = "Song title is required"
                        });

                        if (!continueOnError)
                            throw new InvalidOperationException("Song title is required");

                        continue;
                    }

                    var entry = new JournalEntry
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        SongTitle = jsonEntry.SongTitle.Trim(),
                        ArtistName = jsonEntry.ArtistName,
                        AlbumName = jsonEntry.AlbumName ?? "Unknown Album",
                        SongId = jsonEntry.SongId,
                        CoverUrl = jsonEntry.CoverUrl,
                        Source = jsonEntry.Source ?? "imported",
                        Valence = (float)jsonEntry.Valence,
                        Arousal = (float)jsonEntry.Arousal,
                        TensionLevel = jsonEntry.TensionLevel,
                        SomaticTags = jsonEntry.SomaticTags ?? [],
                        Notes = jsonEntry.Notes,
                        ScrobbledToLastFm = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = null,
                        DeletedAt = null
                    };

                    entriesToImport.Add(entry);
                }
                catch (Exception ex)
                {
                    errors.Add(new ImportError
                    {
                        LineNumber = i + 1,
                        ErrorMessage = ex.Message
                    });

                    if (!continueOnError)
                        throw;
                }
            }

            // Import successful entries
            if (entriesToImport.Count > 0)
            {
                _dbContext.Entries.AddRange(entriesToImport);
                await _dbContext.SaveChangesAsync(cancellationToken);
                response = response with { SuccessfullyImported = entriesToImport.Count };
            }

            response = response with
            {
                FailedCount = errors.Count,
                Errors = errors
            };

            _logger.LogInformation(
                "JSON import completed for user {UserId}: {Successful} successful, {Failed} failed",
                userId, response.SuccessfullyImported, response.FailedCount
            );

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error during JSON import for user {UserId}", userId);
            errors.Add(new ImportError { ErrorMessage = $"Fatal error: {ex.Message}" });
            return response with { Errors = errors };
        }
    }

    // ==================== PRIVATE HELPERS ====================

    private static string? GetOptionalField(CsvReader reader, string fieldName)
    {
        try
        {
            var value = reader.GetField(fieldName);
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        catch
        {
            return null;
        }
    }

    private static string[]? ParseArray(string? arrayString)
    {
        if (string.IsNullOrWhiteSpace(arrayString))
            return null;

        return arrayString
            .Trim('[', ']')
            .Split(',')
            .Select(s => s.Trim('"').Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToArray();
    }

    // DTO for JSON deserialization
    private class JsonImportData
    {
        public List<JsonEntryData> Entries { get; set; } = [];
        public DateTime ExportedAt { get; set; }
        public string Version { get; set; } = "1.0";
    }

    private class JsonEntryData
    {
        public string SongTitle { get; set; } = string.Empty;
        public string? ArtistName { get; set; }
        public string? AlbumName { get; set; }
        public string? SongId { get; set; }
        public string? CoverUrl { get; set; }
        public string? Source { get; set; }
        public double Valence { get; set; }
        public double Arousal { get; set; }
        public int? TensionLevel { get; set; }
        public string[]? SomaticTags { get; set; }
        public string? Notes { get; set; }
        public bool ScrobbledToLastFm { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
