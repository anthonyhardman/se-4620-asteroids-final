using Microsoft.AspNetCore.SignalR.Client;

namespace actorSystem.Services;

public class SignalRService
{
    private readonly HubConnection _hubConnection;

    public SignalRService()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(Environment.GetEnvironmentVariable("SIGNALR_URL") ?? "http://asteroids_signalr:8080/ws")
            .WithAutomaticReconnect()
            .Build();
    }

    public async Task ConnectAsync()
    {
        if (_hubConnection.State == HubConnectionState.Disconnected)
        {
            await _hubConnection.StartAsync();
        }
    }

    public async Task SendNewMessageNotificationAsync()
    {
        await ConnectAsync();
        await _hubConnection.SendAsync("RequestNewMessages");
    }
}
