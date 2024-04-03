
namespace actorSystem;

public interface IActorBridge
{
  void Tell(object message);
  Task<T> Ask<T>(object message);
  public void CreateLobby(string username);
  public void JoinLobby(string username, Guid lobbyId);

}
