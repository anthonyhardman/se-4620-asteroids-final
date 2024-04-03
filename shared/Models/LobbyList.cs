
namespace shared.Models;

public class LobbyList
{
  public LobbyList(List<LobbyInfo> lobbies)
  {
    Lobbies = lobbies;
  }

  public LobbyList()
  {

  }

  private List<LobbyInfo> Lobbies { get; set; } = [];
  public int LobbyCount => Lobbies.Count;
}
