
namespace actorSystem;

public interface IActorBridge
{
  void Tell(object message);
  Task<T> Ask<T>(object message);

  public void RegisterClient(RegisterClientCommand command);
}
