using API.DTOs;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/v1/students/{studentId:guid}/saved-projects")]
public class SavedProjectsController(ISavedProjectService service) : ControllerBase
{
    private readonly ISavedProjectService _service = service;

    [HttpGet]
    [ProducesResponseType<IEnumerable<Project>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Project>>> GetSavedProjects(Guid studentId, CancellationToken cancellationToken)
    {
        var projects = await _service.GetSavedProjects(studentId, cancellationToken);
        return Ok(projects);
    }

    [HttpGet("/api/v1/saved-projects/counts")]
    [ProducesResponseType<Dictionary<Guid, int>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<Guid, int>>> GetSaveCounts(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetSaveCounts(cancellationToken));
    }

    [HttpPut("{projectId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SaveProject(Guid studentId, Guid projectId, CancellationToken cancellationToken)
    {
        await _service.SaveProject(studentId, projectId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{projectId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveSavedProject(Guid studentId, Guid projectId, CancellationToken cancellationToken)
    {
        await _service.RemoveSavedProject(studentId, projectId, cancellationToken);
        return NoContent();
    }
}
