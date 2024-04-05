using Akka.Actor;
using Akka.Event;
using shared.Models;

namespace actorSystem;

public record JoinLobbyCommand(string Username, Guid LobbyId);
public record UserJoined(string Username);
public record GetLobbyInfoQuery();
public record Tick();
public record StartGameCommand();
public record StopGameCommand();
public record PlayerInput(string Username, InputState InputState);

public class LobbyActor : ReceiveActor
{
  public LobbyInfo Info { get; set; }
  public Dictionary<string, PlayerShip> Players { get; set; } = new Dictionary<string, PlayerShip>();
  private ICancelable? _gameLoop;
  private const float _timeStep = 16.667f;

  public LobbyActor(LobbyInfo info)
  {
    Info = info;
    Info = Info.AddPlayer();
    Players.Add(info.CreatedBy, new PlayerShip());

    Receive<JoinLobbyCommand>(command => JoinLobby(command));
    Receive<GetLobbyInfoQuery>(_ => Sender.Tell(Info));
    Receive<Tick>(_ => UpdateGame());
    Receive<StartGameCommand>(_ => StartGame());
    Receive<StopGameCommand>(_ => StopGame());
  }

  public void JoinLobby(JoinLobbyCommand command)
  {
    try
    {
      Info.AddPlayer();
      Players.Add(command.Username, new PlayerShip());
      Sender.Tell(new UserJoined(command.Username));
    }
    catch (InvalidOperationException exception)
    {
      Sender.Tell(new Status.Failure(new KeyNotFoundException(exception.Message)));
    }
  }

  protected ILoggingAdapter Log { get; } = Context.GetLogger();

  public void StartGame()
  {
    _gameLoop = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(
      TimeSpan.FromMilliseconds(_timeStep),
      TimeSpan.FromMicroseconds(_timeStep),
      Self,
      new Tick(),
      Self
    );
  }

  public void StopGame()
  {
    _gameLoop?.Cancel();
  }

  public void UpdateGame()
  {
    foreach (var player in Players)
    {
      player.Value.Update(_timeStep);
    }
  }

  public void UpdatePlayerInput(PlayerInput input)
  {
    if (Players.TryGetValue(input.Username, out PlayerShip? value))
    {
      value.InputState = input.InputState;
    }
  }

  public static Props Props(LobbyInfo info)
  {
    return Akka.Actor.Props.Create<LobbyActor>(info);
  }

}
