using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace MIMM.Frontend.Services;

/// <summary>
/// DelegatingHandler that adds JWT Bearer token to outgoing HTTP requests
/// </summary>
public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly IAuthStateService _authStateService;
    private readonly ILogger<AuthorizationMessageHandler>? _logger;

    public AuthorizationMessageHandler(IAuthStateService authStateService, ILogger<AuthorizationMessageHandler>? logger = null)
    {
        _authStateService = authStateService;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        // Get access token from localStorage
        var token = await _authStateService.GetAccessTokenAsync();
        
        _logger?.LogInformation("AuthHandler: Token present = {HasToken}, URL = {Url}", !string.IsNullOrEmpty(token), request.RequestUri);

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _logger?.LogInformation("AuthHandler: Added Bearer token");
        }
        else
        {
            _logger?.LogWarning("AuthHandler: No token found!");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
