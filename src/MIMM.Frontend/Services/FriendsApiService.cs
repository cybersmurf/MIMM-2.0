using MIMM.Shared.Dtos;

namespace MIMM.Frontend.Services;

public interface IFriendsApiService
{
    Task<FriendsListResponse> GetFriendsAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<FriendOperationResponse> AddFriendAsync(string friendEmail, CancellationToken cancellationToken = default);
    Task<FriendOperationResponse> RemoveFriendAsync(Guid friendshipId, CancellationToken cancellationToken = default);
    Task<List<FriendDto>> GetPendingRequestsAsync(CancellationToken cancellationToken = default);
    Task<FriendOperationResponse> RespondFriendRequestAsync(Guid friendshipId, string action, string? reason = null, CancellationToken cancellationToken = default);
}

public class FriendsApiService : IFriendsApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FriendsApiService> _logger;

    public FriendsApiService(HttpClient httpClient, ILogger<FriendsApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<FriendsListResponse> GetFriendsAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/friends?page={page}&pageSize={pageSize}", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return System.Text.Json.JsonSerializer.Deserialize<FriendsListResponse>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new FriendsListResponse { Friends = [], Total = 0, Page = page, PageSize = pageSize };
            }
            
            _logger.LogError("Failed to fetch friends: {StatusCode}", response.StatusCode);
            return new FriendsListResponse { Friends = [], Total = 0, Page = page, PageSize = pageSize };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching friends");
            return new FriendsListResponse { Friends = [], Total = 0, Page = page, PageSize = pageSize };
        }
    }

    public async Task<FriendOperationResponse> AddFriendAsync(string friendEmail, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new AddFriendRequest { FriendEmail = friendEmail };
            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(request),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("api/friends", content, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            
            return System.Text.Json.JsonSerializer.Deserialize<FriendOperationResponse>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new FriendOperationResponse { Success = false, Message = "Unknown error" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding friend");
            return new FriendOperationResponse { Success = false, Message = ex.Message };
        }
    }

    public async Task<FriendOperationResponse> RemoveFriendAsync(Guid friendshipId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/friends/{friendshipId}", cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            
            return System.Text.Json.JsonSerializer.Deserialize<FriendOperationResponse>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new FriendOperationResponse { Success = false, Message = "Unknown error" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing friend");
            return new FriendOperationResponse { Success = false, Message = ex.Message };
        }
    }

    public async Task<List<FriendDto>> GetPendingRequestsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/friends/pending", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return System.Text.Json.JsonSerializer.Deserialize<List<FriendDto>>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? [];
            }
            
            _logger.LogError("Failed to fetch pending requests: {StatusCode}", response.StatusCode);
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pending requests");
            return [];
        }
    }

    public async Task<FriendOperationResponse> RespondFriendRequestAsync(Guid friendshipId, string action, string? reason = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new RespondFriendRequestRequest { FriendshipId = friendshipId, Action = action, Reason = reason };
            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(request),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync($"api/friends/{friendshipId}/respond", content, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            
            return System.Text.Json.JsonSerializer.Deserialize<FriendOperationResponse>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new FriendOperationResponse { Success = false, Message = "Unknown error" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error responding to friend request");
            return new FriendOperationResponse { Success = false, Message = ex.Message };
        }
    }
}
