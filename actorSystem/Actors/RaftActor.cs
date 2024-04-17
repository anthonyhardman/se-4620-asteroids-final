using Akka.Actor;
using shared.Models;
using System.Text;
using System.Text.Json;
using Raft.Shared.Models;
using Akka.Event;

namespace actorSystem;

public record StoreLobbyCommand(LobbyInfo Info);
public record GetLobbyCommand(Guid LobbyId);
public record OperationFailed(string Reason);

public class RaftActor : ReceiveActor
{
  private readonly HttpClient _httpClient;
  private readonly Dictionary<Guid, (string Value, int Version)> _lobbyCache = new();

  public RaftActor(HttpClient httpClient)
  {
    _httpClient = httpClient;

    ReceiveAsync<StoreLobbyCommand>(HandleStoreLobbyCommand);
    ReceiveAsync<GetLobbyCommand>(HandleGetLobbyCommand);
  }

  private async Task HandleStoreLobbyCommand(StoreLobbyCommand command)
  {
    var key = command.Info.Id.ToString();
    var newValue = JsonSerializer.Serialize(command.Info);
    var casUri = "/api/storage/compare-and-swap";

    try
    {
      if (!_lobbyCache.TryGetValue(command.Info.Id, out var cachedData))
      {
        cachedData = await FetchCurrentData(key);
        _lobbyCache[command.Info.Id] = cachedData;
      }

      var casRequest = new CompareAndSwapRequest
      {
        Key = key,
        NewValue = newValue,
        ExpectedValue = cachedData.Value ?? "null",
        Version = cachedData.Version
      };

      var casResult = await TryCompareAndSwap(casUri, casRequest);
      if (!casResult.Success && casResult.Version == -1)
      {
        cachedData = await FetchCurrentData(key);
        _lobbyCache[command.Info.Id] = cachedData;

        casRequest.ExpectedValue = cachedData.Value;
        casRequest.Version = cachedData.Version;
        casResult = await TryCompareAndSwap(casUri, casRequest);
      }

      if (casResult.Success)
        Log.Info($"Store operation completed for Lobby {command.Info.Id}, Version: {casResult.Version}");
      else
        Log.Error($"Store operation failed for Lobby {command.Info.Id}, Reason: {casResult.Value}");
    }
    catch (Exception e)
    {
      Log.Error($"Error communicating with storage API: {e.Message}");
    }
  }

  private async Task<(string Value, int Version)> FetchCurrentData(string key)
  {
    var getUri = $"/api/storage/strong?key={Uri.EscapeDataString(key)}";
    var response = await _httpClient.GetAsync(getUri);
    if (response.IsSuccessStatusCode)
    {
      var responseData = await JsonSerializer.DeserializeAsync<StrongGetResponse>(await response.Content.ReadAsStreamAsync());
      return (responseData.Value, responseData.Version);
    }

    Log.Error("Failed to retrieve current state.");
    return (null, -1);
  }

  private async Task<CompareAndSwapResponse> TryCompareAndSwap(string uri, CompareAndSwapRequest request)
  {
    var requestContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync(uri, requestContent);
    if (response.IsSuccessStatusCode)
    {
      return await JsonSerializer.DeserializeAsync<CompareAndSwapResponse>(await response.Content.ReadAsStreamAsync());
    }
    return new CompareAndSwapResponse { Success = false, Value = $"HTTP error: {response.StatusCode}" };
  }

  private async Task HandleGetLobbyCommand(GetLobbyCommand command)
  {
    var key = command.LobbyId.ToString();

    if (!_lobbyCache.TryGetValue(command.LobbyId, out var cachedData) || cachedData.Value == null)
    {
      cachedData = await FetchCurrentData(key);
      _lobbyCache[command.LobbyId] = cachedData;
    }

    if (cachedData.Value == null)
    {
      Sender.Tell(new OperationFailed("Lobby not found."));
      return;
    }

    Sender.Tell(JsonSerializer.Deserialize<LobbyInfo>(cachedData.Value));
  }
  protected ILoggingAdapter Log { get; } = Context.GetLogger();

  public static Props Props(HttpClient httpClient)
  {
    return Akka.Actor.Props.Create<RaftActor>();
  }
}
