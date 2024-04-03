using Akka.Actor;
using Akka.Event;

namespace actorSystem;

// This actor is in charge of managing the correlation between
// websocket connection ID and username
// We'll need that for the lobby, notification actor, etc.
public class ClientActor : ReceiveActor
{
  private string connectionId;
  private readonly string username;

  public ClientActor(string connectionId, string username)
  {
    this.connectionId = connectionId;
    this.username = username;
  } 

  protected ILoggingAdapter Log { get; } = Context.GetLogger();
  public static Props Props(string connectionId, string username)
  {
    return Akka.Actor.Props.Create<ClientActor>(connectionId, username);
  }
}