using System.Text.Json.Serialization;

namespace shared.Models;

public enum LobbyState
{
    Joining,
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

    [JsonConstructor]
    public LobbyInfo(Guid id, string createdBy, int maxPlayers, Dictionary<string, PlayerShip> players, LobbyState state)
    {
        Id = id;
        CreatedBy = createdBy;
        MaxPlayers = maxPlayers;
        Players = players;
        State = state;
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

    public void Start(string username)
    {
        if (username == CreatedBy)
        {
            if (State == LobbyState.Joining)
            {
                State = LobbyState.Playing;
            }
            else
            {
                throw new InvalidOperationException("Cannot start game. Wrong state.");
            }
        }
        else
        {
            throw new InvalidOperationException("Cannot start game. Only the creator can start the game.");
        }
    }
}
