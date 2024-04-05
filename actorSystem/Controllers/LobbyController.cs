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
    var lobbyId = await _communicationService.CreateLobby(command.Username);
    return Ok(lobbyId);
  }
}
