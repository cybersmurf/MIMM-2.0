using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using MIMM.Shared.Dtos;

namespace MIMM.Frontend.Services;

/// <summary>
/// Service for export/import API calls
/// </summary>
public interface IExportImportApiService
{
    Task<byte[]?> ExportAsCSVAsync();
    Task<string?> ExportAsJsonAsync();
    Task<ImportResponse?> ImportFromCsvAsync(IBrowserFile file, bool continueOnError = true);
    Task<ImportResponse?> ImportFromJsonAsync(IBrowserFile file, bool continueOnError = true);
}

public class ExportImportApiService : IExportImportApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExportImportApiService> _logger;

    public ExportImportApiService(HttpClient httpClient, ILogger<ExportImportApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<byte[]?> ExportAsCSVAsync()
    {
        try
        {
            _logger.LogInformation("Exporting entries as CSV");
            var response = await _httpClient.PostAsync("api/exportimport/csv", null);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to export CSV: {StatusCode}", response.StatusCode);
                return null;
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting entries as CSV");
            return null;
        }
    }

    public async Task<string?> ExportAsJsonAsync()
    {
        try
        {
            _logger.LogInformation("Exporting entries as JSON");
            var response = await _httpClient.PostAsync("api/exportimport/json", null);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to export JSON: {StatusCode}", response.StatusCode);
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting entries as JSON");
            return null;
        }
    }

    public async Task<ImportResponse?> ImportFromCsvAsync(IBrowserFile file, bool continueOnError = true)
    {
        try
        {
            _logger.LogInformation("Importing entries from CSV file: {FileName}", file.Name);

            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream(file.Size);
            using var streamContent = new StreamContent(fileStream);
            
            content.Add(streamContent, "file", file.Name);

            var response = await _httpClient.PostAsync(
                $"api/exportimport/csv/import?continueOnError={continueOnError}",
                content
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to import CSV: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<ImportResponse>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing entries from CSV");
            return null;
        }
    }

    public async Task<ImportResponse?> ImportFromJsonAsync(IBrowserFile file, bool continueOnError = true)
    {
        try
        {
            _logger.LogInformation("Importing entries from JSON file: {FileName}", file.Name);

            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream(file.Size);
            using var streamContent = new StreamContent(fileStream);
            
            content.Add(streamContent, "file", file.Name);

            var response = await _httpClient.PostAsync(
                $"api/exportimport/json/import?continueOnError={continueOnError}",
                content
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to import JSON: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<ImportResponse>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing entries from JSON");
            return null;
        }
    }
}
