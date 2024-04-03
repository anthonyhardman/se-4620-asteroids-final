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

  public async Task SendLobbyList(LobbyList message, string connectionId)
  {
    using var activity = message.Activity("Lobby list is in the signalr hub");
    // logger.LogInformation("processing signalR request going to: " + connectionId);
    var client = Clients.Client(connectionId);
    await client
      .SendAsync(SignalRMessages.ReceiveLobbyList, message);
  }

  public async Task LeaveGroup(string groupName)
  {
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
  }

  public async Task JoinGroup(string groupName)
  {
    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
  }

  public async Task RegisterClient(string username)
  {
    logger.LogInformation("Registering client: " + username);
    await Clients.All.SendAsync("RegisterClient", username);
  }
}