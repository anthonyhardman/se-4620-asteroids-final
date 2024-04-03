using Akka.Actor;
using Akka.Dispatch.SysMsg;
using Akka.Event;

namespace actorSystem;

// This actor is in charge of managing the correlation between
// websocket connection ID and username
// We'll need that for the lobby, notification actor, etc.
public class ClientActor : ReceiveActor
{
  private string connectionId;
  private readonly string username;
  private string _lobbyId;

  public ClientActor(string connectionId, string username)
  {
    this.connectionId = connectionId;
    this.username = username;
    Receive<CreateLobbyCommand>(command => CreateLobby(command));
    Receive<JoinLobbyCommand>(command => JoinLobby(command));
  }

  public void CreateLobby(CreateLobbyCommand command)
  {
    Context.ActorSelection("/user/lobbySupervisor").Tell(command, Sender);
  }

  public void JoinLobby(JoinLobbyCommand command)
  {
    _lobbyId = command.LobbyId.ToString();
    Context.ActorSelection($"/user/lobbySupervisor/lobby_{command.LobbyId}").Tell(command, Sender);
  }

  protected ILoggingAdapter Log { get; } = Context.GetLogger();
  public static Props Props(string connectionId, string username)
  {
    return Akka.Actor.Props.Create<ClientActor>(connectionId, username);
  }

}