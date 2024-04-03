using Akka.Actor;
using Akka.Event;
using shared.Models;

namespace actorSystem;

public record JoinLobbyCommand(string Username, Guid LobbyId);
public record UserJoined(string Username);

public class LobbyActor : ReceiveActor
{
  public LobbyInfo Info { get; set; }
  public Dictionary<string, PlayerShip> Players { get; set; } = new Dictionary<string, PlayerShip>();

  public LobbyActor(LobbyInfo info)
  {
    Info = info;
    Info.AddPlayer();
    Players.Add(info.CreatedBy, new PlayerShip());

    Receive<JoinLobbyCommand>(command => JoinLobby(command));
  }

  public void JoinLobby(JoinLobbyCommand command)
  {
    Info.AddPlayer();
    Players.Add(command.Username, new PlayerShip());
    Sender.Tell(new UserJoined(command.Username));
  }

  protected ILoggingAdapter Log { get; } = Context.GetLogger();

  

  public static Props Props(LobbyInfo info)
  {
    return Akka.Actor.Props.Create<LobbyActor>(info);
  }

}
