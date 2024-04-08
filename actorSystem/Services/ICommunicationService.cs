using shared.Models;

namespace actorSystem.Services;

public interface ICommunicationService
{
  Task<Guid> CreateLobby(string username);
  void JoinLobby(string username, Guid lobbyId);
  Task<LobbyList> GetLobbies();
  void StartGame(StartGameCommand command);
  Task<LobbyInfo> GetLobbyInfo(Guid lobbyId);
  void SendLobbyInfo(LobbyInfo info);
}
