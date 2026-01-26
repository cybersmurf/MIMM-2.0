using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MIMM.Shared.Dtos;

namespace MIMM.Frontend.Services;

public interface INotificationHubService : IAsyncDisposable
{
    event Action<NotificationDto>? NotificationReceived;
    Task StartAsync();
    bool IsConnected { get; }
}

public class NotificationHubService : INotificationHubService
{
    private readonly NavigationManager _navigationManager;
    private readonly HttpClient _httpClient;
    private readonly IAuthStateService _authStateService;
    private HubConnection? _hubConnection;

    public event Action<NotificationDto>? NotificationReceived;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public NotificationHubService(
        NavigationManager navigationManager,
        HttpClient httpClient,
        IAuthStateService authStateService)
    {
        _navigationManager = navigationManager;
        _httpClient = httpClient;
        _authStateService = authStateService;
    }

    public async Task StartAsync()
    {
        if (_hubConnection is { State: HubConnectionState.Connected })
        {
            return;
        }

        var token = await _authStateService.GetAccessTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        var hubUrl = _httpClient.BaseAddress != null
            ? new Uri(_httpClient.BaseAddress, "/hubs/notification")
            : new Uri(new Uri(_navigationManager.BaseUri), "/hubs/notification");

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(token);
            })
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<NotificationDto>("ReceiveNotification", notification =>
        {
            NotificationReceived?.Invoke(notification);
        });

        await _hubConnection.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
