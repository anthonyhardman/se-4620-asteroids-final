namespace shared.Models;

// Secure Coding DDD demonstrating immutability
public class LobbyInfo
{
    public Guid Id { get; }
    public string CreatedBy { get; }
    public int PlayerCount { get; private set; }
    public int MaxPlayers { get; }

    public LobbyInfo(string createdBy, int playerCount = 0, int maxPlayers = 5)
    {
        Id = Guid.NewGuid();
        CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
        PlayerCount = playerCount;
        MaxPlayers = maxPlayers;
    }

    public LobbyInfo AddPlayer()
    {
        if (PlayerCount >= MaxPlayers)
        {
            throw new InvalidOperationException("Cannot add more players. The lobby is full.");
        }

        return new LobbyInfo(Id, CreatedBy, PlayerCount + 1, MaxPlayers);
    }

    public LobbyInfo RemovePlayer()
    {
        if (PlayerCount <= 0)
        {
            throw new InvalidOperationException("Cannot remove players. The lobby is empty.");
        }

        return new LobbyInfo(Id, CreatedBy, PlayerCount - 1, MaxPlayers);
    }

    protected LobbyInfo(Guid id, string createdBy, int playerCount, int maxPlayers) : this(createdBy, playerCount, maxPlayers)
    {
        Id = id;
    }
}
