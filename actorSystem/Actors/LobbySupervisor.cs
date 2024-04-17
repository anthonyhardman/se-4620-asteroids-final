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
  private readonly ILogger<LobbySupervisor> logger;

  public Dictionary<Guid, IActorRef> Lobbies { get; set; } = [];
  public IActorRef RaftActor { get; set; }

  public LobbySupervisor(ILogger<LobbySupervisor> logger, IActorRef? raftActor = null)
  {
    Receive<CreateLobbyCommand>(CreateLobby);
    Receive<JoinLobbyCommand>(JoinLobby);
    ReceiveAsync<GetLobbiesQuery>(async _ => await GetLobbies());
    Receive<StartGameCommand>(StartGame);
    Receive<Guid>(GetLobby);
    Receive<UpdatePlayerInputStateCommand>(UpdatePlayerInputState);
    this.logger = logger;
    RaftActor = raftActor ?? Context.ActorSelection("/user/raft-actor").ResolveOne(TimeSpan.FromSeconds(3)).Result;
  }

  private void GetLobby(Guid lobbyId)
  {
    if (Lobbies.TryGetValue(lobbyId, out var lobby))
    {
      lobby.Forward(new GetLobbyInfoQuery());
    }
    else
    {
      logger.LogError($"Lobby Supervisor: Failed to get lobby. Lobby {lobbyId} not found.");
      Sender.Tell(new Status.Failure(new KeyNotFoundException($"Failed to get lobby. Lobby {lobbyId} not found.")));
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

    var lobbies = (await Task.WhenAll(lobbiesTasks)).Where(lobby => lobby.State != LobbyState.GameOver).ToList();
    var lobbyList = new LobbyList();
    lobbyList.AddRange(lobbies);
    logger.LogInformation("Lobby Supervisor: Got lobbies");
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
      logger.LogError($"Lobby Supervisor: Failed to join. Lobby {command.LobbyId} not found.");
      Sender.Tell(new Status.Failure(new KeyNotFoundException($"Failed to join. Lobby {command.LobbyId} not found.")));
    }
  }


  private void CreateLobby(CreateLobbyCommand command)
  {
    var lobbyInfo = new LobbyInfo(command.Username);
    var props = DependencyResolver.For(Context.System).Props<LobbyActor>(lobbyInfo, RaftActor);
    var lobbyActor = Context.ActorOf(props, $"lobby_{lobbyInfo.Id}");
    Lobbies.Add(lobbyInfo.Id, lobbyActor);
    Sender.Tell(new LobbyCreated(lobbyInfo, lobbyActor.Path.ToString()));
    logger.LogInformation($"Lobby Supervisor: Lobby created: {lobbyActor.Path}");
  }

  private void StartGame(StartGameCommand command)
  {
    if (Lobbies.TryGetValue(command.LobbyId, out var lobby))
    {
      lobby.Forward(command);
    }
    else
    {
      logger.LogError($"Lobby Supervisor: Unable to start game. Lobby {command.LobbyId} not found.");
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
      logger.LogError($"Lobby Supervisor: Unable to update player input state. Lobby {command.LobbyId} not found.");
      Sender.Tell(new Status.Failure(new KeyNotFoundException($"Unable to update player input state. Lobby {command.LobbyId} not found.")));
    }
  }

  public static Props Props(ILogger<LobbySupervisor> logger, IActorRef? raftActor = null)
  {
    return Akka.Actor.Props.Create(() => new LobbySupervisor(logger, raftActor));
  }
}
