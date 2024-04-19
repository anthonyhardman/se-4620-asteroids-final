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
  private readonly ILogger<RaftActor> _logger;
  public RaftActor(HttpClient httpClient, ILogger<RaftActor> logger)
  {
    _httpClient = httpClient;
    _logger = logger;

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
      var data = await FetchCurrentData(key);
      var casRequest = new CompareAndSwapRequest
      {
        Key = key,
        NewValue = newValue,
        ExpectedValue = data.Value ?? "null",
        Version = data.Version
      };

      var casResult = await TryCompareAndSwap(casUri, casRequest);
    }
    catch (Exception e)
    {
      _logger.LogError(e, "Error communicating with storage API");
    }
  }

  private async Task<(string? Value, int Version)> FetchCurrentData(string key)
  {
    try
    {
      var getUri = $"/api/Storage/strong?key={Uri.EscapeDataString(key)}";
      var response = await _httpClient.GetFromJsonAsync<StrongGetResponse>(getUri);

      return (response?.Value, response?.Version ?? -1);
    }
    catch
    {
      _logger.LogError("Failed to retrieve current state.");
      return (null, -1);
    }
  }

  private async Task<CompareAndSwapResponse> TryCompareAndSwap(string uri, CompareAndSwapRequest request)
  {
    var response = await _httpClient.PostAsJsonAsync(uri, request);
    if (response.IsSuccessStatusCode)
    {
      var result = await JsonSerializer.DeserializeAsync<CompareAndSwapResponse>(await response.Content.ReadAsStreamAsync());
      if (result is null || result.Success == false)
        return new CompareAndSwapResponse { Success = false, Value = $"CAS failed: Version or Expected value must not have matched" };
      return result;
    }
    return new CompareAndSwapResponse { Success = false, Value = $"HTTP error: {response.StatusCode}" };
  }

  public async Task HandleGetLobbyCommand(GetLobbyCommand command)
  {
    var key = command.LobbyId.ToString();
    var data = await FetchCurrentData(key);
    if (data.Value == null)
    {
      _logger.LogError("Raft Actor: Lobby not found");
      Sender.Tell(new OperationFailed("Lobby not found."));
      return;
    }

    Sender.Tell(JsonSerializer.Deserialize<LobbyInfo>(data.Value));
  }

  public static Props Props(HttpClient httpClient, ILogger<RaftActor> logger)
  {
    return Akka.Actor.Props.Create<RaftActor>(() => new RaftActor(httpClient, logger));
  }
}
