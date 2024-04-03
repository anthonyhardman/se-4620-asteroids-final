using Akka.Actor;
using Akka.Event;

namespace actorSystem;

public record CreateLobbyCommand(string Username);

public class LobbySupervisor : ReceiveActor
{
  public LobbySupervisor()
  {

  }

  private void CreateLobby(CreateLobbyCommand command)
  {

  }

  protected ILoggingAdapter Log { get; } = Context.GetLogger();

  public static Props Props()
  {
    return Akka.Actor.Props.Create<LobbySupervisor>();
  }
}
