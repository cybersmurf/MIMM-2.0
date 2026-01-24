using System.Net.Http.Json;
using MIMM.Shared.Dtos;

namespace MIMM.Frontend.Services;

/// <summary>
/// Service for journal entry API calls
/// </summary>
public interface IEntryApiService
{
    Task<PagedResponse<EntryDto>?> GetEntriesAsync(PaginationRequest request);
    Task<EntryDto?> GetEntryByIdAsync(Guid id);
    Task<EntryDto?> CreateEntryAsync(CreateEntryRequest request);
    Task<EntryDto?> UpdateEntryAsync(Guid id, UpdateEntryRequest request);
    Task<bool> DeleteEntryAsync(Guid id);
    Task<PagedResponse<EntryDto>?> SearchEntriesAsync(SearchEntriesRequest request);
}

public class EntryApiService : IEntryApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EntryApiService> _logger;

    public EntryApiService(HttpClient httpClient, ILogger<EntryApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PagedResponse<EntryDto>?> GetEntriesAsync(PaginationRequest request)
    {
        try
        {
            var url = $"/api/entries?pageNumber={request.PageNumber}&pageSize={request.PageSize}&sortBy={request.SortBy}&sortDirection={request.SortDirection}";
            return await _httpClient.GetFromJsonAsync<PagedResponse<EntryDto>>(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching entries");
            return null;
        }
    }

    public async Task<EntryDto?> GetEntryByIdAsync(Guid id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<EntryDto>($"/api/entries/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching entry {Id}", id);
            return null;
        }
    }

    public async Task<EntryDto?> CreateEntryAsync(CreateEntryRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/entries", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<EntryDto>();
            }

            _logger.LogWarning("Failed to create entry: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating entry");
            return null;
        }
    }

    public async Task<EntryDto?> UpdateEntryAsync(Guid id, UpdateEntryRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/entries/{id}", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<EntryDto>();
            }

            _logger.LogWarning("Failed to update entry {Id}: {StatusCode}", id, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entry {Id}", id);
            return null;
        }
    }

    public async Task<bool> DeleteEntryAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/entries/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entry {Id}", id);
            return false;
        }
    }

    public async Task<PagedResponse<EntryDto>?> SearchEntriesAsync(SearchEntriesRequest request)
    {
        try
        {
            var url = $"/api/entries/search?query={Uri.EscapeDataString(request.Query ?? "")}&pageNumber={request.PageNumber}&pageSize={request.PageSize}&sortBy={request.SortBy}&sortDirection={request.SortDirection}";
            
            if (request.DateFrom.HasValue)
            {
                url += $"&dateFrom={request.DateFrom.Value:yyyy-MM-dd}";
            }
            
            if (request.DateTo.HasValue)
            {
                url += $"&dateTo={request.DateTo.Value:yyyy-MM-dd}";
            }

            return await _httpClient.GetFromJsonAsync<PagedResponse<EntryDto>>(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching entries");
            return null;
        }
    }
}
