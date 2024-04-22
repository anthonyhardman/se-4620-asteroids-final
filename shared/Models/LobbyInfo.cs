using System.Numerics;
using System.Text.Json.Serialization;

namespace shared.Models;

public enum LobbyState
{
  Joining,
  Countdown,
  Playing,
  Stopped,
  GameOver
}

// Secure Coding DDD demonstrating immutability
public class LobbyInfo
{
  public Guid Id { get; }
  public string CreatedBy { get; } = string.Empty;
  public int PlayerCount => Players.Count;
  public int MaxPlayers { get; }
  public Dictionary<string, PlayerShip> Players { get; init; } = [];
  [JsonIgnore]
  [Newtonsoft.Json.JsonIgnore]
  public IEnumerable<PlayerShip> PlayersThatArentDead => Players.Where(player => player.Value.Health > 0).Select(player => player.Value);
  public List<Asteroid> Asteroids { get; init; } = [];
  public LobbyState State { get; private set; }
  public int CountdownTime { get; private set; }
  private const int maxX = 400 * 3;
  private const int maxY = 300 * 3;
  private static readonly Random random = new();
  private float elapsedTime = 0.0f;
  private const int screenAdjustment = 1000;
  private readonly SpatialHash spatialHash = new(200);

  [JsonConstructor]
  [Newtonsoft.Json.JsonConstructor]
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

  public void UpdateBullets(float timeStep)
  {
    foreach (var player in Players)
    {
      player.Value.UpdateBullets(timeStep);
      player.Value.Bullets.RemoveAll(b => CheckBounds(b.Position));
    }
  }

  private static bool CheckBounds(Vector2 position)
  {
    return position.X < -maxX - screenAdjustment || position.X > maxX + screenAdjustment ||
           position.Y < -maxY - screenAdjustment || position.Y > maxY + screenAdjustment;
  }


  public void UpdatePlayers(float timeStep)
  {
    foreach (var player in PlayersThatArentDead)
    {
      player.Update(timeStep);
    }
  }

  public void HandleAsteroids(float timeStep)
  {
    SpawnAsteroid(timeStep);

    foreach (var asteroid in Asteroids)
    {
      asteroid.Update(timeStep);
    }

    Asteroids.RemoveAll(a => CheckBounds(a.Position));
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

  public void StopGame()
  {
    State = LobbyState.Stopped;
    Asteroids.Clear();
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

  public void HandleCollision(float timeStep)
  {
    HashSet<Asteroid> asteroidsToRemove = [];
    RebuildSpatialHash();
    CheckPlayerAsteroidCollisions(asteroidsToRemove, timeStep);
    CheckBulletAsteroidCollisions(asteroidsToRemove);
    Asteroids.RemoveAll(asteroidsToRemove.Contains);
  }

  private void RebuildSpatialHash()
  {
    spatialHash.Clear();
    foreach (var player in Players.Values)
    {
      spatialHash.Insert(player.Position, player);
      foreach (var bullet in player.Bullets)
      {
        spatialHash.Insert(bullet.Position, bullet);
      }
    }
    foreach (var asteroid in Asteroids)
    {
      spatialHash.Insert(asteroid.Position, asteroid);
    }
  }

  private void CheckPlayerAsteroidCollisions(HashSet<Asteroid> asteroidsToRemove, float timeStep)
  {
    foreach (var player in PlayersThatArentDead)
    {
      if (player.CollisionCooldown <= 0)
      {
        var nearbyObjects = spatialHash.Query(player.Position);
        foreach (var obj in nearbyObjects)
        {
          if (obj is Asteroid asteroid && Vector2.Distance(player.Position, asteroid.Position) < 50 * asteroid.Size)
          {
            player.TakeDamage(asteroid.Damage, timeStep);
            player.HandleCollision(asteroid);
            asteroid.HandleCollision();
            asteroid.TakeDamage();
            if (asteroid.Health <= 0)
            {
              asteroidsToRemove.Add(asteroid);
            }
          }
        }
      }
    }
  }

  private void CheckBulletAsteroidCollisions(HashSet<Asteroid> asteroidsToRemove)
  {
    foreach (var player in Players.Values)
    {
      List<Bullet> bulletsToRemove = [];
      foreach (var bullet in player.Bullets)
      {
        var nearbyObjects = spatialHash.Query(bullet.Position);
        foreach (var obj in nearbyObjects)
        {
          if (obj is Asteroid asteroid && Vector2.Distance(bullet.Position, asteroid.Position) < asteroid.Size * 50)
          {
            asteroid.TakeDamage();
            bulletsToRemove.Add(bullet);

            if (asteroid.Health <= 0)
            {
              asteroidsToRemove.Add(asteroid);
              player.Points += (int)asteroid.Size * 100;
            }
          }
        }
      }
      player.Bullets.RemoveAll(bulletsToRemove.Contains);
    }
  }

  public void EndGameIfAllPlayersDead()
  {
    var allPlayersAreDead = Players.All(player => player.Value.Health <= 0);
    if (allPlayersAreDead)
    {
      State = LobbyState.GameOver;
    }
  }
}
