using System.Net.Http.Json;
using MIMM.Shared.Dtos;

namespace MIMM.Frontend.Services;

public interface INotificationApiService
{
    Task<PagedResponse<NotificationDto>?> GetNotificationsAsync(int pageNumber = 1, int pageSize = 10);
    Task<bool> MarkAsReadAsync(Guid notificationId);
    Task<int?> MarkAllAsReadAsync();
    Task<bool> DeleteAsync(Guid notificationId);
}

public class NotificationApiService : INotificationApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationApiService> _logger;

    public NotificationApiService(HttpClient httpClient, ILogger<NotificationApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PagedResponse<NotificationDto>?> GetNotificationsAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var url = $"/api/notification?pageNumber={pageNumber}&pageSize={pageSize}";
            return await _httpClient.GetFromJsonAsync<PagedResponse<NotificationDto>>(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching notifications");
            return null;
        }
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"/api/notification/{notificationId}/read", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
            return false;
        }
    }

    public async Task<int?> MarkAllAsReadAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync("/api/notification/read-all", null);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return payload != null && payload.TryGetValue("updated", out var count) ? count : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read");
            return null;
        }
    }

    public async Task<bool> DeleteAsync(Guid notificationId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/notification/{notificationId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification {NotificationId}", notificationId);
            return false;
        }
    }
}
