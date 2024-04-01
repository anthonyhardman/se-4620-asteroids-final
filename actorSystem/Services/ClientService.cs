
using Microsoft.AspNetCore.SignalR.Client;

namespace actorSystem.Services;

public class ClientService : IClientService
{
  private readonly HubConnection _hubConnection;

  public ClientService()
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
}
