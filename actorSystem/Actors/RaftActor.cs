using Akka.Actor;
using shared.Models;
using System.Text.Json;
using Raft.Shared.Models;
using Raft.Shared;

namespace actorSystem;

public record StoreLobbyCommand(LobbyInfo Info);
public record GetLobbyCommand(Guid LobbyId);
public record OperationFailed(string Reason);

public class RaftActor : ReceiveActor
{
  private readonly ILogger<RaftActor> _logger;
  private readonly IRaftService _raftService;
  public RaftActor(IRaftService raftService, ILogger<RaftActor> logger)
  {
    _raftService = raftService;
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
      var response = await _raftService.StrongGet(key);

      return (response.value, response.version);
    }
    catch
    {
      _logger.LogError("Failed to retrieve current state.");
      return (null, -1);
    }
  }

  private async Task<CompareAndSwapResponse> TryCompareAndSwap(string uri, CompareAndSwapRequest request)
  {
    var response = await _raftService.CompareAndSwap(request.Key, request.NewValue, request.ExpectedValue, request.Version);
    return response.ToRaft();
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

  public static Props Props(IRaftService raftService, ILogger<RaftActor> logger)
  {
    return Akka.Actor.Props.Create<RaftActor>(() => new RaftActor(raftService, logger));
  }
}
