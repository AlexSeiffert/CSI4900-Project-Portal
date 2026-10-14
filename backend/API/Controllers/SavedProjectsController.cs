using API.Orchestrations.Interfaces;
using API.DTOs;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/v1/students/{studentId:guid}/saved-projects")]
public class SavedProjectsController(ISavedProjectOrchestration orchestration) : ControllerBase
{
    private readonly ISavedProjectOrchestration _orchestration = orchestration;

    [HttpGet]
    [ProducesResponseType<IEnumerable<Project>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<Project>>> GetSavedProjects(Guid studentId, CancellationToken cancellationToken)
    {
        var projects = await _orchestration.GetSavedProjects(studentId, cancellationToken);
        return Ok(projects);
    }

    [HttpPut("{projectId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SaveProject(Guid studentId, Guid projectId, CancellationToken cancellationToken)
    {
        await _orchestration.SaveProject(studentId, projectId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{projectId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveSavedProject(Guid studentId, Guid projectId, CancellationToken cancellationToken)
    {
        await _orchestration.RemoveSavedProject(studentId, projectId, cancellationToken);
        return NoContent();
    }
}
