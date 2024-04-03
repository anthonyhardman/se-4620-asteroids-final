using Akka.Actor;
using Akka.Event;

namespace actorSystem;

public class LobbyActor : ReceiveActor
{
  public LobbyInfo info { get; set; }
  
  public LobbyActor()
  {
    
  }
  protected ILoggingAdapter Log { get; } = Context.GetLogger();

  public static Props Props()
  {
    return Akka.Actor.Props.Create<LobbyActor>();
  }
}
