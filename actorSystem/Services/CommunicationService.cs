
using Microsoft.AspNetCore.SignalR.Client;

namespace actorSystem.Services;

public class CommunicationService : ICommunicationService
{
  private readonly HubConnection _hubConnection;
  private readonly IActorBridge _akkaService;

  public CommunicationService(IActorBridge akkaService)
  {
    _hubConnection = new HubConnectionBuilder()
        .WithUrl(Environment.GetEnvironmentVariable("SIGNALR_URL") ?? "http://asteroids_signalr:8080/ws")
        .WithAutomaticReconnect()
        .Build();
    
    _akkaService = akkaService;
  }

  public async Task ConnectAsync()
  {
    if (_hubConnection.State == HubConnectionState.Disconnected)
    {
      await _hubConnection.StartAsync();
    }
  }

  p8
}
