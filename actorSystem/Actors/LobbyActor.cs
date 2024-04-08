using System.Text.Json;
using actorSystem.Services;
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
public record StopGameCommand(string Username, Guid LobbyId);
public record PlayerInput(string Username, InputState InputState);

public class LobbyActor : ReceiveActor
{
  public LobbyInfo Info { get; set; }
  private ICancelable? _gameLoop;
  private ICommunicationService _communicationService;
  private const float _timeStep = 16.667f;
  private float countdown = 10000;

  public LobbyActor(LobbyInfo info, ICommunicationService communicationService)
  {
    Info = info;
    Info.AddPlayer(info.CreatedBy);
    Log.Info($"{info.CreatedBy} created and joined lobby {Info.Id}");
    _communicationService = communicationService;

    Receive<JoinLobbyCommand>(JoinLobby);
    Receive<GetLobbyInfoQuery>(_ => Sender.Tell(Info));
    Receive<Tick>(_ => UpdateGame());
    Receive<StartGameCommand>(StartGame);
    Receive<StopGameCommand>(StopGame);
    Receive<PlayerInput>(UpdatePlayerInput);
  }

  public void JoinLobby(JoinLobbyCommand command)
  {
    if (Info.State == LobbyState.Joining)
    {
      try
      {
        Info.AddPlayer(command.Username);
        Sender.Tell(new UserJoined(command.Username));
        Log.Info($"{command.Username} joined lobby {Info.Id}");
        _communicationService.SendLobbyInfo(Info);
      }
      catch (InvalidOperationException exception)
      {
        Log.Error(exception.Message);
        Sender.Tell(new Status.Failure(new InvalidOperationException(exception.Message)));
      }
    }
    else
    {
      Sender.Tell(new Status.Failure(new InvalidOperationException("Cannot join game. Wrong state.")));
    }
  }

  protected ILoggingAdapter Log { get; } = Context.GetLogger();

  public void StartGame(StartGameCommand command)
  {
    if (Info.State == LobbyState.Joining)
    {
      try
      {
        Log.Info($"Starting game {command.LobbyId}");
        countdown = 10000;
        Info.StartCountdown();
        _gameLoop = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(
          TimeSpan.FromMilliseconds(_timeStep),
          TimeSpan.FromMicroseconds(_timeStep),
          Self,
          new Tick(),
          Self
        );
        Sender.Tell(new Status.Success("Game started."));
        Log.Info($"Started game {command.LobbyId}");
      }
      catch (InvalidOperationException exception)
      {
        Log.Error(exception.Message);
        Sender.Tell(new Status.Failure(new InvalidOperationException(exception.Message)));
      }
    }
    else
    {
      Sender.Tell(new Status.Failure(new InvalidOperationException("Cannot start game. Wrong state.")));
    }
  }

  public void StopGame(StopGameCommand command)
  {
    _gameLoop?.Cancel();
    Sender.Tell(new Status.Success("Game stopped."));
  }

  public void UpdateGame()
  {
    if (Info.State == LobbyState.Countdown)
    {
      countdown -= _timeStep;
      Info.UpdateCountdownTime(countdown);
      if (countdown <= 0)
      {
        Info.StartPlaying();
      }
    }
    else if (Info.State == LobbyState.Playing)
    {
      foreach (var player in Info.Players)
      {
        player.Value.Update(_timeStep);
      }
    }
    _communicationService.SendLobbyInfo(Info);
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

  public static Props Props(LobbyInfo info, ICommunicationService communicationService)
  {
    return Akka.Actor.Props.Create<LobbyActor>(() => new LobbyActor(info, communicationService));
  }
}
