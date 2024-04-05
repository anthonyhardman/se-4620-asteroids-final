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

  // public async Task SendLobbyList(LobbyList message, string connectionId)
  // {
  //   // using var activity = message.Activity("Lobby list is in the signalr hub");
  //   logger.LogInformation("Sending lobby list to: " + connectionId);
  //   var client = Clients.Client(connectionId);
  //   await client
  //     .SendAsync(SignalRMessages.ReceiveLobbyList, message);
  // }

  // Sent by frontend
  public async Task RequestLobbies()
  {
    logger.LogInformation("Requesting lobbies for: " + Context.ConnectionId);
    // Only Akka listening to this one
    await Clients.All.SendAsync("SendLobbies");
  }

  // Sent by Akka
  public async Task SendLobbies(LobbyList lobbies)
  {
    logger.LogInformation("Sending lobbies to: " + Context.ConnectionId);
    // Only frontend listening to this one
    await Clients.All.SendAsync("ReceiveLobbies", lobbies);
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

  public async Task LobbyCreated()
  {
    logger.LogInformation("Lobby created");
    await Clients.All.SendAsync("LobbyCreated");
  }
}