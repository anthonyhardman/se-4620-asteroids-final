using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Event;
using Microsoft.AspNet.SignalR.Messaging;
using shared.Models;

namespace actorSystem;

public record CreateLobbyCommand(string Username);
public record LobbyCreated(LobbyInfo Info, string ActorPath);
public record GetLobbiesQuery();
public record UpdatePlayerInputStateCommand(string Username, Guid LobbyId, InputState InputState);


public class LobbySupervisor : ReceiveActor
{
  public Dictionary<Guid, IActorRef> Lobbies { get; set; } = new Dictionary<Guid, IActorRef>();

  public LobbySupervisor()
  {
    Receive<CreateLobbyCommand>(CreateLobby);
    Receive<JoinLobbyCommand>(JoinLobby);
    ReceiveAsync<GetLobbiesQuery>(async _ => await GetLobbies());
    Receive<StartGameCommand>(StartGame);
    Receive<Guid>(GetLobby);
    Receive<UpdatePlayerInputStateCommand>(UpdatePlayerInputState);
  }

  private void GetLobby(Guid lobbyId)
  {
    if (Lobbies.TryGetValue(lobbyId, out var lobby))
    {
      lobby.Forward(new GetLobbyInfoQuery());
    }
    else
    {
      Sender.Tell(new Status.Failure(new KeyNotFoundException($"Lobby {lobbyId} not found.")));
    }
  }

  private async Task GetLobbies()
  {
    var lobbiesTasks = Lobbies.Values.Select(lobby => lobby.Ask<LobbyInfo>(new GetLobbyInfoQuery()));
    if (!lobbiesTasks.Any())
    {
      Sender.Tell(new LobbyList());
      return;
    }

    var lobbies = (await Task.WhenAll(lobbiesTasks)).ToList();
    var lobbyList = new LobbyList();
    lobbyList.AddRange(lobbies);
    Sender.Tell(lobbyList);
  }

  private void JoinLobby(JoinLobbyCommand command)
  {
    if (Lobbies.TryGetValue(command.LobbyId, out var lobby))
    {
      lobby.Forward(command);
    }
    else
    {
      Sender.Tell(new Status.Failure(new KeyNotFoundException($"Lobby {command.LobbyId} not found.")));
    }
  }


  private void CreateLobby(CreateLobbyCommand command)
  {
    var lobbyInfo = new LobbyInfo(command.Username);
    var props = DependencyResolver.For(Context.System).Props<LobbyActor>(lobbyInfo);
    var lobbyActor = Context.ActorOf(props, $"lobby_{lobbyInfo.Id}");
    Lobbies.Add(lobbyInfo.Id, lobbyActor);
    Sender.Tell(new LobbyCreated(lobbyInfo, lobbyActor.Path.ToString()));
    Log.Info($"Lobby created: {lobbyActor.Path}");
  }

  private void StartGame(StartGameCommand command)
  {
    if (Lobbies.TryGetValue(command.LobbyId, out var lobby))
    {
      lobby.Forward(command);
    }
    else
    {
      Sender.Tell(new Status.Failure(new KeyNotFoundException($"Unable to start game. Lobby {command.LobbyId} not found.")));
    }
  }

  public void UpdatePlayerInputState(UpdatePlayerInputStateCommand command)
  {
    if (Lobbies.TryGetValue(command.LobbyId, out var lobby))
    {
      lobby.Tell(new PlayerInput(command.Username, command.InputState));
    }
    else
    {
      Sender.Tell(new Status.Failure(new KeyNotFoundException($"Unable to update player input state. Lobby {command.LobbyId} not found.")));
    }
  }


  protected ILoggingAdapter Log { get; } = Context.GetLogger();

  public static Props Props()
  {
    return Akka.Actor.Props.Create<LobbySupervisor>();
  }
}
