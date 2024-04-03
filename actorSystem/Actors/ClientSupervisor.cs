using System.Text.RegularExpressions;
using Akka.Actor;
using Akka.Event;

namespace actorSystem;

public record RegisterClientCommand(string ConnectionId, string Username);
public record ClientRegistered(string ConnectionId, string ClientActorPath);

public class ClientSupervisor : ReceiveActor
{
  private readonly Dictionary<string, IActorRef> Clients = [];

  public ClientSupervisor()
  {
    Receive<RegisterClientCommand>(m => RegisterClient(m));
  }

  private void RegisterClient(RegisterClientCommand command)
  {
    var actorPath = GetClientActorPath(command.Username);
    if (Clients.ContainsKey(actorPath))
    {
      Log.Info($"User session for {command.Username} already exists.");
      Sender.Tell(new ClientRegistered(command.ConnectionId, Clients[actorPath].Path.ToStringWithAddress()));
    }
    else
    {
      Log.Info($"Creating user session for {command.Username}");

      var clientActor = Context.ActorOf(ClientActor.Props(command.ConnectionId, command.Username), actorPath);

      Clients.Add(actorPath, clientActor);
      Sender.Tell(new ClientRegistered(command.ConnectionId, clientActor.Path.ToStringWithAddress()));
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