using System.Net.Http.Headers;
using System.Net.Http.Json;
using MIMM.Shared.Dtos;

namespace MIMM.Frontend.Services;

/// <summary>
/// Service for authentication API calls
/// </summary>
public interface IAuthApiService
{
    Task<AuthenticationResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthenticationResponse?> LoginAsync(LoginRequest request);
    Task<AuthenticationResponse?> RefreshTokenAsync(string refreshToken);
    Task<UserDto?> GetCurrentUserAsync();
    Task<bool> VerifyTokenAsync(string token);
}

public class AuthApiService : IAuthApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthApiService> _logger;

    public AuthApiService(HttpClient httpClient, ILogger<AuthApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AuthenticationResponse?> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/register", request);
            
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    return await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deserializing registration response");
                    return null;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Registration failed: {StatusCode}, {ErrorContent}", response.StatusCode, errorContent);
            return null;
        }
        catch (HttpRequestException hex)
        {
            _logger.LogError(hex, "HTTP error during registration");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration");
            return null;
        }
    }

    public async Task<AuthenticationResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);
            
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    return await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deserializing login response");
                    return null;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Login failed: {StatusCode}, {ErrorContent}", response.StatusCode, errorContent);
            return null;
        }
        catch (HttpRequestException hex)
        {
            _logger.LogError(hex, "HTTP error during login");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login");
            return null;
        }
    }

    public async Task<AuthenticationResponse?> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var request = new RefreshTokenRequest { RefreshToken = refreshToken };
            var response = await _httpClient.PostAsJsonAsync("/api/auth/refresh", request);
            
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    return await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deserializing refresh response");
                    return null;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Token refresh failed: {StatusCode}, {ErrorContent}", response.StatusCode, errorContent);
            return null;
        }
        catch (HttpRequestException hex)
        {
            _logger.LogError(hex, "HTTP error during token refresh");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token refresh");
            return null;
        }
    }

    public async Task<UserDto?> GetCurrentUserAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/auth/me");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserDto>();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user");
            return null;
        }
    }

    public async Task<bool> VerifyTokenAsync(string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/verify");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying token");
            return false;
        }
    }
}
