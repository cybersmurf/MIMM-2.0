using System.Net.Http.Headers;

namespace MIMM.Frontend.Services;

/// <summary>
/// DelegatingHandler that adds JWT Bearer token to outgoing HTTP requests
/// </summary>
public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly IAuthStateService _authStateService;

    public AuthorizationMessageHandler(IAuthStateService authStateService)
    {
        _authStateService = authStateService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        // Get access token from localStorage
        var token = await _authStateService.GetAccessTokenAsync();

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
