﻿using System.Text.Json;
using actorSystem.Services;
using Akka.Actor;
using Akka.Dispatch.SysMsg;
using Akka.Event;
using shared.Models;

namespace actorSystem;

public record JoinLobbyCommand(string Username, Guid LobbyId);
public record UserJoined(string Username);
public record GetLobbyInfoQuery();
public record Tick();
public record StartGameCommand(string Username, Guid LobbyId);
public record GameStartedCommand(DateTime StartedAt);
public record PlayerInput(string Username, InputState InputState);

public class LobbyActor : ReceiveActor
{
  public LobbyInfo Info { get; set; }
  private ICancelable? _gameLoop;
  private ICommunicationService _communicationService;
  private const float _timeStep = 16.667f;
  private float countdown = 10000;
  public IActorRef RaftActor { get; set; }
  public DateTime LastPersisted { get; set; }


  public LobbyActor(LobbyInfo info, ICommunicationService communicationService, IActorRef? raftActor = null)
  {
    Info = info;
    Info.AddPlayer(info.CreatedBy);
    Log.Info($"{info.CreatedBy} created and joined lobby {Info.Id}");
    _communicationService = communicationService;

    Receive<JoinLobbyCommand>(JoinLobby);
    Receive<GetLobbyInfoQuery>(_ => Sender.Tell(Info));
    Receive<Tick>(_ => UpdateGame());
    Receive<StartGameCommand>(StartGame);
    Receive<PlayerInput>(UpdatePlayerInput);
    RaftActor = raftActor ?? Context.ActorSelection("/user/raft-actor").ResolveOne(TimeSpan.FromSeconds(3)).Result;
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
      Log.Error("Lobby Actor: Cannot join game. Wrong state.");
      Sender.Tell(new Status.Failure(new InvalidOperationException("Cannot join game. Wrong state.")));
    }
  }

  protected ILoggingAdapter Log { get; } = Context.GetLogger();

  public void StartGame(StartGameCommand command)
  {
    if (Info.State == LobbyState.Joining && command.Username == Info.CreatedBy)
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
        Log.Info($"Started game {command.LobbyId}");
        Sender.Tell(new Status.Success("Game started."));
      }
      catch (InvalidOperationException exception)
      {
        Log.Error(exception.Message);
        Sender.Tell(new Status.Failure(new InvalidOperationException(exception.Message)));
      }
    }
    else
    {
      Log.Error("Lobby Actor: Cannot start game. Not joining or not creator.");
      Sender.Tell(new Status.Failure(new InvalidOperationException("Cannot start game.")));
    }
  }

  public void StopGame()
  {
    _gameLoop?.Cancel();
    Info.StopGame();
    Log.Info("Lobby Actor: Game Stopped.");
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
      Info.UpdatePlayers(_timeStep);
      Info.UpdateBullets(_timeStep);
      Info.HandleAsteroids(_timeStep);
      Info.HandleCollision(_timeStep);
      if (Info.PlayerCount == 0)
      {
        StopGame();
      }
    }
    Info.EndGameIfAllPlayersDead();
    _communicationService.SendLobbyInfo(Info);

    if (DateTime.Now - LastPersisted > TimeSpan.FromSeconds(3))
    {
      RaftActor.Tell(new StoreLobbyCommand(Info));
      LastPersisted = DateTime.Now;
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

  public static Props Props(LobbyInfo info, ICommunicationService communicationService, IActorRef? raftActor = null)
  {
    return Akka.Actor.Props.Create<LobbyActor>(() => new LobbyActor(info, communicationService, raftActor));
  }
}
