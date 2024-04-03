
using Microsoft.AspNetCore.SignalR.Client;
using shared.Models;

namespace actorSystem.Services;

public class CommunicationService : ICommunicationService, IHostedService
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
    });
    _hubConnection.On<string>("CreateLobby", CreateLobby);
    _hubConnection.On<string, Guid>("JoinLobby", JoinLobby);
    _hubConnection.On("SendLobbies", SendLobbies);
  }


  public async Task StartAsync(CancellationToken cancellationToken)
  {
    if (_hubConnection.State == HubConnectionState.Disconnected)
    {
      await _hubConnection.StartAsync();
    }
  }

  public async Task StopAsync(CancellationToken cancellationToken)
  {
    await _hubConnection.StopAsync();
  }

  public void CreateLobby(string username)
  {
    _akkaService.CreateLobby(username);
  }

  public void JoinLobby(string username, Guid lobbyId)
  {
    _akkaService.JoinLobby(username, lobbyId);
  }

  public async Task SendLobbies()
  {
    var lobbyList = await _akkaService.GetLobbies();
    await _hubConnection.SendAsync("SendLobbies", lobbyList);
  }

}
