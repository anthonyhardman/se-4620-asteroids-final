using shared.Models;

namespace actorSystem.Services;

public interface ICommunicationService
{
  Task<string> CreateLobby(string username);
  void JoinLobby(string username, Guid lobbyId);
  Task<LobbyList> GetLobbies();
}
