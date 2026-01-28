using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MIMM.Frontend.Services;

/// <summary>
/// Service for managing authentication state and tokens in browser localStorage
/// </summary>
public interface IAuthStateService
{
    Task<string?> GetAccessTokenAsync();
    Task<string?> GetRefreshTokenAsync();
    Task SetTokensAsync(string accessToken, string? refreshToken);
    Task ClearTokensAsync();
    Task<bool> IsAuthenticatedAsync();
}

public class AuthStateService : IAuthStateService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<AuthStateService>? _logger;
    private const string AccessTokenKey = "mimm_access_token";
    private const string RefreshTokenKey = "mimm_refresh_token";

    public AuthStateService(IJSRuntime jsRuntime, ILogger<AuthStateService>? logger = null)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", AccessTokenKey);
            _logger?.LogDebug("AuthStateService: Retrieved token, present = {HasToken}", !string.IsNullOrEmpty(token));
            return token;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "AuthStateService: Error retrieving access token from localStorage");
            return null;
        }
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", RefreshTokenKey);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "AuthStateService: Error retrieving refresh token from localStorage");
            return null;
        }
    }

    public async Task SetTokensAsync(string accessToken, string? refreshToken)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", AccessTokenKey, accessToken);
        
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", RefreshTokenKey, refreshToken);
        }
    }

    public async Task ClearTokensAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", AccessTokenKey);
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", RefreshTokenKey);
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetAccessTokenAsync();
        return !string.IsNullOrEmpty(token);
    }
}
