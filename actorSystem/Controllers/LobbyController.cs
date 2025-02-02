using actorSystem.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using shared.Models;

namespace actorSystem;

[Route("api/[controller]")]
[ApiController]
public class LobbyController : ControllerBase
{
  private readonly ICommunicationService _communicationService;

  public LobbyController(ICommunicationService communicationService)
  {
    _communicationService = communicationService;
  }

  [HttpGet]
  public async Task<ActionResult<LobbyList>> GetLobbies()
  {
    var lobbies = await _communicationService.GetLobbies();
    return Ok(lobbies);
  }

  [HttpPost]
  public async Task<ActionResult<string>> CreateLobby([FromBody] CreateLobbyCommand command)
  {
    Console.WriteLine("Creating lobby");
    var lobbyId = await _communicationService.CreateLobby(command.Username);
    return Ok(lobbyId);
  }

  [HttpPut]
  public void StartGame([FromBody] StartGameCommand command)
  {
    _communicationService.StartGame(command);
  }

  [HttpGet("{lobbyId}")]
  public async Task<LobbyInfo> GetLobbyInfo(Guid lobbyId)
  {
    Console.WriteLine("Getting lobby info");
    var result = await _communicationService.GetLobbyInfo(lobbyId);
    Console.WriteLine("Got lobby info");
    return result;
  }

  [HttpPut("join")]
  public IActionResult JoinLobby([FromBody] JoinLobbyCommand command)
  {
    _communicationService.JoinLobby(command.Username, command.LobbyId);
    return Ok();
  }

  [HttpPut("kill/{lobbyId}")]
  public void KillLobby(Guid lobbyId)
  {
    _communicationService.KillLobby(lobbyId);
  }

  [HttpPut("color")]
  public void UpdatePlayerColor([FromBody] UpdateLobbiesPlayerColorCommand command)
  {
    _communicationService.UpdatePlayerColor(command);
  }
}
