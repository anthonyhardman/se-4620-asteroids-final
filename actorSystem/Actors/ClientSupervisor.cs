using System.Text.RegularExpressions;
using Akka.Actor;
using Akka.Event;

namespace actorSystem;

public record RegisterClientCommand(string connectionId, string username);
public record ClientRegistered(string connectionId, string clientActorPath);

public class ClientSupervisor : ReceiveActor
{
  private Dictionary<string, IActorRef> clients = new();

  public ClientSupervisor()
  {
    Receive<RegisterClientCommand>(m => RegisterClient(m));
  }

  private void RegisterClient(RegisterClientCommand command)
  {
    var actorPath = GetClientActorPath(command.username);
    if (clients.ContainsKey(actorPath))
    {
      Log.Info($"User session for {command.username} already exists.");
      // Sender.Tell(new ClientRegistered(command.connectionId, clients[command.username].Path.ToStringWithAddress()));
    }
    else
    {
      Log.Info($"Creating user session for {command.username}");

      var clientActor = Context.ActorOf(ClientActor.Props(command.connectionId, command.username), actorPath);

      clients.Add(actorPath, clientActor);
      // Sender.Tell(new ClientRegistered(command.connectionId, clientActor.Path.ToStringWithAddress()));
    }
  }

  private string GetClientActorPath(string username)
  {
    var validActorPath = UsernameToActorPath(username);
    return $"client_{validActorPath}";
  }

  private static string UsernameToActorPath(string username)
  {
    if (string.IsNullOrEmpty(username))
    {
      throw new ArgumentException("Username cannot be null or empty.", nameof(username));
    }

    string validActorPath = Regex.Replace(username, "[\\$\\/\\#\\s]+", "_").ToLower();

    return validActorPath;
  }

  protected ILoggingAdapter Log { get; } = Context.GetLogger();
  public static Props Props()
  {
    return Akka.Actor.Props.Create<ClientSupervisor>();
  }
}