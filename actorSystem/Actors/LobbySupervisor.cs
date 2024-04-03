using Akka.Actor;
using Akka.Event;

namespace actorSystem;

public class LobbySupervisor : ReceiveActor
{
  protected ILoggingAdapter Log { get; } = Context.GetLogger();

  public static Props Props()
  {
    return Akka.Actor.Props.Create<LobbySupervisor>();
  }
}
