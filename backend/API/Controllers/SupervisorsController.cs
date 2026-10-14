using API.Orchestrations.Interfaces;
using API.DTOs;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using API.DTOs.Request;

namespace API.Controllers;

[ApiController]
[Route("api/v1/supervisors")]
public class SupervisorsController(ISupervisorService service, ISupervisorOrchestration orchestration) : ControllerBase
{
    private readonly ISupervisorService _service = service;
    private readonly ISupervisorOrchestration _orchestration = orchestration;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Supervisor>>> GetSupervisors(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetSupervisors(cancellationToken));
    }

    [HttpGet("{userId:guid}")]
    [ProducesResponseType<Supervisor>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Supervisor>> GetSupervisorById(Guid userId, CancellationToken cancellationToken)
    {
        return await _service.GetSupervisorById(userId, cancellationToken);
    }

    [HttpPut("{userId:guid}")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSupervisor(Guid userId, UpdateSupervisor request, CancellationToken cancellationToken)
    {
        await _orchestration.UpdateSupervisor(userId, request, cancellationToken);
        return NoContent();
    }
}
