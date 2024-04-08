using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using shared.Models;

namespace signalR;

public class AsteroidsHub : Hub
{
  private readonly ILogger<AsteroidsHub> logger;

  public AsteroidsHub(ILogger<AsteroidsHub> logger)
  {
    this.logger = logger;
  }

  public async Task LeaveGroup(string groupName)
  {
    logger.LogInformation($"Left group {groupName}");
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
  }

  public async Task JoinGroup(string groupName)
  {
    logger.LogInformation($"Joined group {groupName}");
    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
  }

  public async Task CreateLobby(string username)
  {
    logger.LogInformation("Creating lobby for client: " + username);
    await Clients.All.SendAsync("CreateLobby", username);
  }

  public async Task JoinLobby(string username, Guid lobbyId)
  {
    logger.LogInformation("Joining lobby for client: " + username);
    await Clients.All.SendAsync("JoinLobby", username, lobbyId);
  }

  public async Task LobbyCreated(Guid lobbyId, LobbyList lobbies)
  {
    logger.LogInformation($"Lobby created {lobbyId}");
    await Clients.All.SendAsync("LobbyCreated", lobbies);
  }

  public async Task GameStarted(Guid lobbyId)
  {
    logger.LogInformation($"Game {lobbyId} started");
    await Clients.Group(lobbyId.ToString()).SendAsync("GameStarted");
  }

  public async Task GameStarting(Guid lobbyId)
  {
    DateTime startedAt = DateTime.UtcNow;
    logger.LogInformation($"Starting game countdown for lobby {lobbyId}");
    await Clients.Group(lobbyId.ToString()).SendAsync("GameStarting", startedAt);
  }

  public async Task UpdateLobbyInfo(LobbyInfo info)
  {
    // logger.LogInformation($"Update lobby {info.Id}");
    await Clients.Group(info.Id.ToString()).SendAsync("UpdateLobbyInfo", info);
  }

  public async Task UpdatePlayerInputState(string username, Guid lobbyId, InputState inputState)
  {
    logger.LogInformation($"Update player input state {username}");
    await Clients.All.SendAsync("UpdatePlayerInputState", username, lobbyId, inputState);
  }
}