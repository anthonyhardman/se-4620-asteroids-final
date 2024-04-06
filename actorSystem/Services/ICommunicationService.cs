using shared.Models;

namespace actorSystem.Services;

public interface ICommunicationService
{
  Task<Guid> CreateLobby(string username);
  void JoinLobby(string username, Guid lobbyId);
  Task<LobbyList> GetLobbies();
  Task StartGame(StartGameCommand command);
  public Task<LobbyInfo> GetLobbyInfo(Guid lobbyId);
  public void SendLobbyInfo(LobbyInfo info);
}
