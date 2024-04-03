namespace actorSystem.Services;

public interface ICommunicationService
{
  public void CreateLobby(string username);
  public void JoinLobby(string username, Guid lobbyId);
}
