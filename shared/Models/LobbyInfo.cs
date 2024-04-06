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
    public LobbyState State { get; private set; }
    public int CountdownTime { get; private set; }

    [JsonConstructor]
    public LobbyInfo(Guid id, string createdBy, int maxPlayers, Dictionary<string, PlayerShip> players, LobbyState state, int countdownTime)
    {
        Id = id;
        CreatedBy = createdBy;
        MaxPlayers = maxPlayers;
        Players = players;
        State = state;
        CountdownTime = countdownTime;
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

        Players.Add(username, new PlayerShip());
    }

    public void RemovePlayer(string username)
    {
        if (PlayerCount <= 0)
        {
            throw new InvalidOperationException("Cannot remove players. The lobby is empty.");
        }

        Players.Remove(username);
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
