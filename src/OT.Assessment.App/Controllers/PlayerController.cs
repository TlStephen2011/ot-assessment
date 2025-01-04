using Microsoft.AspNetCore.Mvc;
using OT.Assessment.App.Contracts;
using OT.Assessment.Domain.Models;

namespace OT.Assessment.App.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class PlayerController(ICasinoWagerPublishService casinoWagerPublishService, ICasinoService casinoService) : Controller
{

    //POST api/player/casinowager

    [HttpPost("casinowager")]
    public IActionResult CreateCasinoWager([FromBody] CasinoWager wager)
    {
        casinoWagerPublishService.PublishCasinoWagerAsync(wager);
        return Ok();
    }

    //GET api/player/{playerId}/wagers

    [HttpGet("{playerId:guid}/casino")]
    public async Task<IActionResult> GetLatestCasinoWagersForPlayer(Guid playerId, [FromQuery] int pageSize, [FromQuery] int page)
    {
        var response = await casinoService.GetLatestCasinoWagersAsync(playerId, pageSize, page);

        if (response is null)
            return NotFound();

        return Ok(response);
    }

    //GET api/player/topSpenders?count=10
    [HttpGet("topSpenders")]
    public async Task<IActionResult> GetTopSpenders([FromQuery] int count)
    {
        var response = await casinoService.GetTopSpenders(count);

        if (response is null)
            return NotFound();

        return Ok(response);
    }

}