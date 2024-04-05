using Akka.Actor;
using Akka.Event;
using shared.Models;

namespace actorSystem;

public record JoinLobbyCommand(string Username, Guid LobbyId);
public record UserJoined(string Username);
public record GetLobbyInfoQuery();
public record Tick();
public record StartGameCommand(string Username, Guid LobbyId);
public record GameStartedCommand(DateTime StartedAt);
public record StopGameCommand(string Username);
public record PlayerInput(string Username, InputState InputState);

public class LobbyActor : ReceiveActor
{
  public LobbyInfo Info { get; set; }
  private ICancelable? _gameLoop;
  private const float _timeStep = 16.667f;

  public LobbyActor(LobbyInfo info)
  {
    Info = info;
    Info.AddPlayer(info.CreatedBy);
    Log.Info($"{info.CreatedBy} created and joined lobby {Info.Id}");

    Receive<JoinLobbyCommand>(JoinLobby);
    Receive<GetLobbyInfoQuery>(_ => Sender.Tell(Info));
    Receive<Tick>(_ => UpdateGame());
    Receive<StartGameCommand>(StartGame);
    Receive<StopGameCommand>(StopGame);
  }

  public void JoinLobby(JoinLobbyCommand command)
  {
    try
    {
      Info.AddPlayer(command.Username);
      Sender.Tell(new UserJoined(command.Username));
      Log.Info($"{command.Username} joined lobby {Info.Id}");
    }
    catch (InvalidOperationException exception)
    {
      Log.Error(exception.Message);
      Sender.Tell(new Status.Failure(new InvalidOperationException(exception.Message)));
    }
  }

  protected ILoggingAdapter Log { get; } = Context.GetLogger();

  public void StartGame(StartGameCommand command)
  {
    try
    {
      Log.Info($"Starting game {command.LobbyId}");
      Info.Start(command.Username);
      _gameLoop = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(
        TimeSpan.FromMilliseconds(_timeStep),
        TimeSpan.FromMicroseconds(_timeStep),
        Self,
        new Tick(),
        Self
      );
      Sender.Tell(new GameStartedCommand(DateTime.UtcNow));
    }
    catch (InvalidOperationException exception)
    {
      Log.Error(exception.Message);
      Sender.Tell(new Status.Failure(new InvalidOperationException(exception.Message)));
    }
  }

  public void StopGame(StopGameCommand command)
  {
    _gameLoop?.Cancel();
  }

  public void UpdateGame()
  {
    foreach (var player in Info.Players)
    {
      player.Value.Update(_timeStep);
    }
  }

  public void UpdatePlayerInput(PlayerInput input)
  {
    if (Info.Players.TryGetValue(input.Username, out PlayerShip? value))
    {
      value.InputState = input.InputState;
    }
  }

  public static Props Props(LobbyInfo info)
  {
    return Akka.Actor.Props.Create<LobbyActor>(info);
  }

}
