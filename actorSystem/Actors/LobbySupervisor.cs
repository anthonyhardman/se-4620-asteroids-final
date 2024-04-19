using System.Diagnostics.Metrics;
using Akka.Actor;
using Akka.DependencyInjection;
using shared.Models;

namespace actorSystem;

public record CreateLobbyCommand(string Username);
public record LobbyCreated(LobbyInfo Info, string ActorPath);
public record GetLobbiesQuery();
public record UpdatePlayerInputStateCommand(string Username, Guid LobbyId, InputState InputState);
public record KillLobbyCommand(Guid LobbyId);

public class LobbySupervisor : ReceiveActor
{
  private readonly ILogger<LobbySupervisor> logger;
  private readonly IServiceScope scope;

  public Dictionary<Guid, IActorRef> Lobbies { get; set; } = [];
  public IActorRef RaftActor { get; set; }



  public static readonly Meter meter = new("LobbySupervisor");

  public LobbySupervisor(IServiceProvider serviceProvider, IActorRef? raftActor = null)
  {
    Receive<CreateLobbyCommand>(CreateLobby);
    Receive<JoinLobbyCommand>(JoinLobby);
    ReceiveAsync<GetLobbiesQuery>(async _ => await GetLobbies());
    Receive<StartGameCommand>(StartGame);
    Receive<Guid>(GetLobby);
    Receive<UpdatePlayerInputStateCommand>(UpdatePlayerInputState);
    Receive<KillLobbyCommand>(KillLobby);
    ReceiveAsync<Terminated>(async (t) => await RehydrateLobby(t.ActorRef));
    RaftActor = raftActor ?? Context.ActorSelection("/user/raft-actor").ResolveOne(TimeSpan.FromSeconds(3)).Result;

    scope = serviceProvider.CreateScope();
    this.logger = scope.ServiceProvider.GetRequiredService<ILogger<LobbySupervisor>>();

    meter.CreateObservableGauge<int>("LobbyCount", () => Lobbies.Count, "Number of lobbies");
    meter.CreateObservableGauge<int>("JoiningLobbies", () =>
    {
      return Lobbies.Values.Count(x => x.Ask<LobbyInfo>(new GetLobbyInfoQuery()).Result.State == LobbyState.Joining);
    }, "Number of Lobbies in the Joining State");
    meter.CreateObservableGauge<int>("PlayingLobbies", () =>
    {
      return Lobbies.Values.Count(x => x.Ask<LobbyInfo>(new GetLobbyInfoQuery()).Result.State == LobbyState.Playing);
    }, "Number of Lobbies in the Playing State");
    meter.CreateObservableGauge<int>("GameOverLobbies", () =>
    {
      return Lobbies.Values.Count(x => x.Ask<LobbyInfo>(new GetLobbyInfoQuery()).Result.State == LobbyState.GameOver);
    }, "Number of Lobbies in the GameOver State");
    meter.CreateObservableGauge<int>("StoppedLobbies", () =>
    {
      return Lobbies.Values.Count(x => x.Ask<LobbyInfo>(new GetLobbyInfoQuery()).Result.State == LobbyState.Stopped);
    }, "Number of Lobbies in the Stopped State");
    meter.CreateObservableGauge<int>("CountdownLobbies", () =>
    {
      return Lobbies.Values.Count(x => x.Ask<LobbyInfo>(new GetLobbyInfoQuery()).Result.State == LobbyState.Countdown);
    }, "Number of Lobbies in the Countdown State");
    meter.CreateObservableGauge<int>("PlayerCount", () =>
    {
      return Lobbies.Values.Select(x => x.Ask<LobbyInfo>(new GetLobbyInfoQuery()).Result.Players.Count).Sum();
    }, "Number of Players in Lobbies");
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

  private void KillLobby(KillLobbyCommand command)
  {
    if (Lobbies.TryGetValue(command.LobbyId, out var lobby))
    {
      logger.LogInformation($"Killing lobby {command.LobbyId}");
      lobby.Tell(PoisonPill.Instance);
    }
    else
    {
      logger.LogError($"Lobby Supervisor: Failed to kill lobby. Lobby {command.LobbyId} not found.");
      Sender.Tell(new Status.Failure(new KeyNotFoundException($"Failed to kill lobby. Lobby {command.LobbyId} not found.")));
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
    logger.LogInformation("Creating lobby via lobby supervisor");
    var lobbyInfo = new LobbyInfo(command.Username);
    var props = DependencyResolver.For(Context.System).Props<LobbyActor>(lobbyInfo, RaftActor);
    var lobbyActor = Context.ActorOf(props, $"lobby_{lobbyInfo.Id}");
    Context.Watch(lobbyActor);
    Lobbies.Add(lobbyInfo.Id, lobbyActor);
    Sender.Tell(new LobbyCreated(lobbyInfo, lobbyActor.Path.ToString()));
    logger.LogInformation($"Lobby Supervisor: Lobby created: {lobbyActor.Path}");
  }

  private async Task RehydrateLobby(IActorRef oldLobby)
  {
    logger.LogInformation("RehydrateLobby");
    var lobby = Lobbies.FirstOrDefault(x => x.Value == oldLobby);
    var lobbyInfo = (LobbyInfo)await RaftActor.Ask(new GetLobbyCommand(lobby.Key));
    if (lobbyInfo.PlayerCount > 0)
    {
      logger.LogInformation($"Rehyrdating lobby {lobby.Key}");
      var lobbyProps = DependencyResolver.For(Context.System).Props<LobbyActor>(lobbyInfo, RaftActor);
      var newLobbyActor = Context.ActorOf(lobbyProps, $"lobby_{lobbyInfo.Id}");
      Context.Watch(newLobbyActor);
      Lobbies[lobbyInfo.Id] = newLobbyActor;
    }
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

  public static Props Props(IServiceProvider serviceProvider, IActorRef? raftActor = null)
  {
    return Akka.Actor.Props.Create<LobbySupervisor>(() => new LobbySupervisor(serviceProvider, raftActor));
  }
}
