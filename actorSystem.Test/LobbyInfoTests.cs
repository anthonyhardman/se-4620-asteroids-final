using System;
using System.Numerics;
using Xunit;
using shared.Models;

namespace actorSystem.Test;

public class LobbyInfoTests
{
  [Fact]
  public void HandleCollision_NoStateChangeDuringCooldown()
  {
    var lobby = SetupLobby();
    var player = lobby.Players["player1"];
    var asteroid = SetupAsteroid(player.Position);
    lobby.Asteroids.Add(asteroid);
    float timestep = 16.667f;

    lobby.HandleCollision(timestep);
    float initialHealth = player.Health;
    Vector2 initialVelocity = player.Velocity;

    lobby.HandleCollision(timestep);

    Assert.Equal(initialHealth, player.Health);
    Assert.Equal(initialVelocity, player.Velocity);
  }

  [Fact]
  public void HandleCollision_StateChangesAfterCooldownExpires()
  {
    var lobby = SetupLobby();
    var player = lobby.Players["player1"];
    var asteroid = SetupAsteroid(player.Position);
    lobby.Asteroids.Add(asteroid);
    float timestep = 16.667f;

    lobby.HandleCollision(timestep);

    while (player.CollisionCooldown > 0)
    {
      lobby.UpdatePlayers(player.CollisionCooldown);
    }

    lobby.Asteroids.Clear();
    var newAsteroid = SetupAsteroid(player.Position);
    lobby.Asteroids.Add(newAsteroid);

    float initialHealthAfterCooldown = player.Health;
    Vector2 initialVelocityAfterCooldown = player.Velocity;

    lobby.HandleCollision(timestep);

    Assert.True(player.Health < initialHealthAfterCooldown);
    Assert.NotEqual(initialVelocityAfterCooldown, player.Velocity);
  }

  [Fact]
  public void BulletKillsAsteroid()
  {
    var lobby = SetupLobby();
    var player = lobby.Players["player1"];
    var asteroid = SetupAsteroid(player.Position, 2);
    lobby.Asteroids.Add(asteroid);
    float timestep = 16.667f;

    player.Fire();
    lobby.HandleCollision(timestep);

    Assert.True(lobby.Asteroids.Count == 0);
  }

  [Fact]
  public void GameEnds()
  {
    var lobby = SetupLobby();
    var player = lobby.Players["player1"];
    Assert.NotEqual(LobbyState.GameOver, lobby.State);
    player.TakeDamage(100, 1000);
    lobby.EndGameIfAllPlayersDead();
    Assert.Equal(LobbyState.GameOver, lobby.State);
  }

  [Fact]
  public void CountdownStarts()
  {
    var lobby = SetupLobby();
    Assert.Equal(LobbyState.Joining, lobby.State);
    lobby.StartCountdown();
    Assert.Equal(LobbyState.Countdown, lobby.State);
  }

  [Fact]
  public void CountdownDoesntStartIfNotJoining()
  {
    var lobby = SetupLobby();
    Assert.Equal(LobbyState.Joining, lobby.State);
    lobby.StartCountdown();
    Assert.Throws<InvalidOperationException>(lobby.StartCountdown);
  }

  [Fact]
  public void UpdateCountdownTime_UpdatesTime_WhenStateIsCountdown()
  {
    var lobby = SetupLobby();
    lobby.StartCountdown();
    var expectedTime = 9000;
    lobby.UpdateCountdownTime(expectedTime);
    Assert.Equal(9, lobby.CountdownTime);
  }

  [Fact]
  public void UpdateCountdownTime_DoesNotUpdateTime_WhenStateIsNotCountdown()
  {
    var lobby = SetupLobby();
    var initialTime = lobby.CountdownTime;
    lobby.UpdateCountdownTime(10000);
    Assert.Equal(initialTime, lobby.CountdownTime);
  }


  private LobbyInfo SetupLobby()
  {
    var lobby = new LobbyInfo("test_user", 5);
    lobby.Players.Add("player1", new PlayerShip(new Vector2(100, 100), new Vector2(1, 0), new Vector2(1, 0), [], 100, "blue", 1200, 900));
    return lobby;
  }

  private Asteroid SetupAsteroid(Vector2 position, int health = 3)
  {
    return new Asteroid(position, new Vector2(-0.1f, 0), new Vector2(-1, 0), 2, health);
  }
}
