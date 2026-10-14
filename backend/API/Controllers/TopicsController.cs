using API.DTOs;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/v1/topics")]
public class TopicsController(ITopicService service) : ControllerBase
{
    private readonly ITopicService _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Topic>>> GetTopics(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetTopics(cancellationToken));
    }
}
