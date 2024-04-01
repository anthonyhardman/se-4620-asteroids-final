using Microsoft.AspNetCore.SignalR;

namespace signalR;

public class AsteroidsHub : Hub
{
  public async Task LeaveGroup(string groupName)
  {
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
  }

  public async Task JoinGroup(string groupName)
  {
    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
  }
}