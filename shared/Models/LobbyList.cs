namespace shared.Models;

public class LobbyList
{
    public List<LobbyInfo> Lobbies { get; set; } = new List<LobbyInfo>();

    // Assuming you have an Activity method for logging or other purposes
    public IDisposable Activity(string message)
    {
        // Implement your logging or tracking logic here
        // For simplicity, this example just returns a disposable no-op
        return new NoOpDisposable();
    }

    // A helper class for the above example, to fulfill the using statement requirement
    private class NoOpDisposable : IDisposable
    {
        public void Dispose()
        {
            // No operation
        }
    }
}
