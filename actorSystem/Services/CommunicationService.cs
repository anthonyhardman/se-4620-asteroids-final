
using Microsoft.AspNetCore.SignalR.Client;

namespace actorSystem.Services;

public class CommunicationService : ICommunicationService
{
  private readonly HubConnection _hubConnection;
  private readonly IActorBridge _akkaService;
  private readonly ILogger<CommunicationService> logger;

  public CommunicationService(IActorBridge akkaService, ILogger<CommunicationService> logger)
  {
    _hubConnection = new HubConnectionBuilder()
        .WithUrl(Environment.GetEnvironmentVariable("SIGNALR_URL") ?? "http://asteroids_signalr:8080/ws")
        .WithAutomaticReconnect()
        .Build();

    _akkaService = akkaService;
    this.logger = logger;
    _hubConnection.On<string>("RegisterClient", (username) =>
    {
      Console.WriteLine("Registering client: " + username);
      if (string.IsNullOrEmpty(username))
      {
        throw new ArgumentException("Username cannot be null or empty.", nameof(username));
      }

      if (_hubConnection.ConnectionId == null)
      {
        throw new InvalidOperationException("ConnectionId cannot be null.");
      }
      
      RegisterClient(username, _hubConnection.ConnectionId);
    });

    ConnectAsync().Wait();
  }

  public async Task ConnectAsync()
  {
    if (_hubConnection.State == HubConnectionState.Disconnected)
    {
      Console.WriteLine("Connecting to SignalR hub...");
      logger.LogInformation("Connecting to SignalR hub...");
      await _hubConnection.StartAsync();
    }
  }

  public void RegisterClient(string username, string connectionId)
  {
    _akkaService.RegisterClient(new RegisterClientCommand(connectionId, username));
  }

}
