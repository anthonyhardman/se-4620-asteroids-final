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
    public float TimeStep { get; set; } = 16.667f;
    private const int maxX = 400 * 3;
    private const int maxY = 300 * 3;
    private static readonly Random random = new();

    [JsonConstructor]
    public LobbyInfo(Guid id, string createdBy, int maxPlayers, Dictionary<string, PlayerShip> players, LobbyState state, int countdownTime, List<Asteroid> asteroids, float timeStep = 16.667f)
    {
        Id = id;
        CreatedBy = createdBy;
        MaxPlayers = maxPlayers;
        Players = players;
        State = state;
        CountdownTime = countdownTime;
        Asteroids = asteroids;
        TimeStep = timeStep;
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

    public void HandleAsteroids()
    {
        if (random.Next(200) == 1) // 1% chance to add a new asteroid
        {
            Asteroids.Add(new Asteroid(maxX, maxY));
        }

        foreach (var asteroid in Asteroids)
        {
            asteroid.Update(TimeStep);
        }

        var asteroidCountBefore = Asteroids.Count;
        // Remove asteroids that are off the screen, with an extra 100 units of flexibility
        Asteroids.RemoveAll(asteroid =>
            asteroid.Position.X < -maxX - 100 || asteroid.Position.X > maxX + 100 ||
            asteroid.Position.Y < -maxY - 100 || asteroid.Position.Y > maxY + 100);
        var asteroidCountAfter = Asteroids.Count;
        if (asteroidCountBefore != asteroidCountAfter)
        {
            Console.WriteLine($"Removed {asteroidCountBefore - asteroidCountAfter} asteroids");
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
}
