using System.Numerics;
using System.Text.Json.Serialization;

namespace shared.Models;

public enum LobbyState
{
  Joining,
  Countdown,
  Playing,
  Stopped
}

// Secure Coding DDD demonstrating immutability
public class LobbyInfo
{
  public Guid Id { get; }
  public string CreatedBy { get; } = string.Empty;
  public int PlayerCount => Players.Count;
  public int MaxPlayers { get; }
  public Dictionary<string, PlayerShip> Players { get; init; } = [];
  public List<Asteroid> Asteroids { get; init; } = [];
  public LobbyState State { get; private set; }
  public int CountdownTime { get; private set; }
  private const int maxX = 400 * 3;
  private const int maxY = 300 * 3;
  private static readonly Random random = new();
  private float elapsedTime = 0.0f;

  [JsonConstructor]
  public LobbyInfo(Guid id, string createdBy, int maxPlayers, Dictionary<string, PlayerShip> players, LobbyState state, int countdownTime, List<Asteroid> asteroids)
  {
    Id = id;
    CreatedBy = createdBy;
    MaxPlayers = maxPlayers;
    Players = players;
    State = state;
    CountdownTime = countdownTime;
    Asteroids = asteroids;
  }

  public LobbyInfo(string createdBy, int maxPlayers = 5)
  {
    Id = Guid.NewGuid();
    CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
    MaxPlayers = maxPlayers;
    State = LobbyState.Joining;
  }

  public void AddPlayer(string username)
  {
    if (PlayerCount >= MaxPlayers && State == LobbyState.Joining)
    {
      throw new InvalidOperationException("Cannot add more players. The lobby is full.");
    }

    Players.Add(username, new PlayerShip(maxX, maxY));
  }

  public void RemovePlayer(string username)
  {
    if (PlayerCount <= 0)
    {
      throw new InvalidOperationException("Cannot remove players. The lobby is empty.");
    }

    Players.Remove(username);
  }

  public void UpdatePlayers(float timeStep)
  {
    foreach (var player in Players)
    {
      player.Value.Update(timeStep);
    }
  }

  public void HandleAsteroids(float timeStep)
  {
    SpawnAsteroid(timeStep);

    foreach (var asteroid in Asteroids)
    {
      asteroid.Update(timeStep); // Ensure TimeStep is in seconds if using velocity in units per second
    }

    // Remove asteroids that are off the screen
    Asteroids.RemoveAll(asteroid =>
        asteroid.Position.X < -maxX - 1000 || asteroid.Position.X > maxX + 1000 ||
        asteroid.Position.Y < -maxY - 1000 || asteroid.Position.Y > maxY + 1000);
  }

  private void SpawnAsteroid(float timeStep)
  {
    elapsedTime += timeStep / 1000.0f;
    float spawnProbability = 0.002f + (elapsedTime / 10000.0f);
    int desiredAsteroidCount = 2 + (int)(elapsedTime / 180);

    if (Asteroids.Count < desiredAsteroidCount)
    {
      spawnProbability *= 1.1f;
    }

    if (Asteroids.Count < 2 || (Asteroids.Count < 30 && random.NextDouble() < spawnProbability))
    {
      Asteroids.Add(new Asteroid(maxX, maxY));
    }
  }

  public void StartPlaying()
  {
    if (State == LobbyState.Joining || State == LobbyState.Countdown)
    {
      State = LobbyState.Playing;
    }
    else
    {
      throw new InvalidOperationException("Cannot start game. Wrong state.");
    }
  }

  public void StartCountdown()
  {
    if (State == LobbyState.Joining)
    {
      State = LobbyState.Countdown;
    }
    else
    {
      throw new InvalidOperationException("Cannot start countdown. Wrong state.");
    }
  }

  public void UpdateCountdownTime(float time)
  {
    if (State == LobbyState.Countdown)
    {
      CountdownTime = (int)Math.Round(time / 1000);
    }
  }

  public void HandleCollision()
  {
    foreach (var player in Players.Values)
    {
      foreach (var asteroid in Asteroids)
      {
        if (Vector2.Distance(player.Position, asteroid.Position) < 20)
        {
          player.TakeDamage(asteroid.Damage);
          player.HandleCollision(asteroid);
          asteroid.HandleCollision(player);
          asteroid.TakeDamage(1);
        }
        if (asteroid.Health < 0)
        {
          Asteroids.Remove(asteroid);
        }
      }
      if (player.Health < 0)
      {

      }
    }

    // Players = Players.Where(player => player.Value.Health > 0).ToDictionary(player => player.Key, player => player.Value);
    // Asteroids = Asteroids.Where(asteroid => asteroid.Health > 0).ToList();
  }
}
