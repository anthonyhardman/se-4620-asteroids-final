
using shared.Models;

namespace actorSystem;

public interface IActorBridge
{
  void Tell(object message);
  Task<T> Ask<T>(object message);
  public Task<Guid> CreateLobby(string username);
  public void JoinLobby(string username, Guid lobbyId);
  public Task<LobbyList> GetLobbies();
  public void StartGame(StartGameCommand command);
  public Task<LobbyInfo> GetLobbyInfo(Guid lobbyId);
  public void UpdatePlayerInputState(string username, Guid lobbyId, InputState inputState);
  void KillLobby(Guid lobbyId);

}
