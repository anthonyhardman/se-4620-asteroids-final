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

  private LobbyInfo SetupLobby()
  {
    var lobby = new LobbyInfo("test_user", 5);
    lobby.Players.Add("player1", new PlayerShip(new Vector2(100, 100), new Vector2(1, 0), new Vector2(1, 0), [], 100, "blue", 1200, 900));
    return lobby;
  }

  private Asteroid SetupAsteroid(Vector2 position)
  {
    return new Asteroid(position, new Vector2(-0.1f, 0), new Vector2(-1, 0), 2, 3);
  }
}
