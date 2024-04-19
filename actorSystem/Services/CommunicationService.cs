
using System.Text.Json;
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
    _hubConnection.On<string, Guid, InputState>("UpdatePlayerInputState", UpdatePlayerInputState);
  }

  private async Task EnsureConnectedAsync()
  {
    if (_hubConnection.State == HubConnectionState.Disconnected)
    {
      try
      {
        await _hubConnection.StartAsync();
        logger.LogInformation("SignalR hub connection established.");
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Error establishing SignalR hub connection.");
        throw;
      }
    }
  }

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    if (_hubConnection.State != HubConnectionState.Disconnected)
    {
      await _hubConnection.StartAsync(cancellationToken);
    }
  }

  public async Task StopAsync(CancellationToken cancellationToken)
  {
    await _hubConnection.StopAsync(cancellationToken);
  }

  public async Task<Guid> CreateLobby(string username)
  {
    logger.LogInformation("Creating lobby via communication service.");
    var lobbyId = await _akkaService.CreateLobby(username);
    var lobbies = await _akkaService.GetLobbies();
    await EnsureConnectedAsync();
    await _hubConnection.SendAsync("LobbyCreated", lobbyId, lobbies);
    return lobbyId;
  }

  public void JoinLobby(string username, Guid lobbyId)
  {
    _akkaService.JoinLobby(username, lobbyId);
  }

  public async Task<LobbyList> GetLobbies()
  {
    return await _akkaService.GetLobbies();
  }

  public void StartGame(StartGameCommand command)
  {
    _akkaService.StartGame(command);
  }

  public async Task<LobbyInfo> GetLobbyInfo(Guid lobbyId)
  {
    var result = await _akkaService.GetLobbyInfo(lobbyId);
    return result;
  }

  public async Task SendLobbyInfo(LobbyInfo info)
  {
    await EnsureConnectedAsync();
    await _hubConnection.SendAsync("UpdateLobbyInfo", info);
  }

  public void UpdatePlayerInputState(string username, Guid lobbyId, InputState inputState)
  {
    _akkaService.UpdatePlayerInputState(username, lobbyId, inputState);
  }

  public void KillLobby(Guid lobbyId)
  {
    _akkaService.KillLobby(lobbyId);
  }
}
