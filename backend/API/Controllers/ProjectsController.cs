using API.Orchestrations.Interfaces;
using API.DTOs;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using API.DTOs.Request;

using System.ComponentModel.DataAnnotations;
using ProjectStatus = API.Models.ProjectStatus;

namespace API.Controllers;

[ApiController]
[Route("api/v1/projects")]
public class ProjectsController(IProjectService service, IProjectOrchestration orchestration) : ControllerBase
{
    private readonly IProjectService _service = service;
    private readonly IProjectOrchestration _orchestration = orchestration;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Project>>> GetProjects(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetProjects(cancellationToken));
    }

    [HttpGet("supervisors/{supervisorId:guid}")]
    public async Task<ActionResult<IEnumerable<Project>>> GetProjectsBySupervisorId(Guid supervisorId, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetProjectsBySupervisorId(supervisorId, cancellationToken));
    }

    [HttpGet("topics/{topicId:guid}")]
    public async Task<ActionResult<IEnumerable<Project>>> GetProjectsByTopicId(Guid topicId, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetProjectsByTopicId(topicId, cancellationToken));
    }

    [HttpGet("statuses/{status}")]
    public async Task<ActionResult<IEnumerable<Project>>> GetProjectsByStatus(
        [EnumDataType(typeof(ProjectStatus))] ProjectStatus status, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetProjectsByStatus(status, cancellationToken));
    }

    [HttpGet("{projectId:guid}")]
    [ProducesResponseType<Project>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Project>> GetProjectById(Guid projectId, CancellationToken cancellationToken)
    {
        return await _service.GetProjectById(projectId, cancellationToken);
    }

    [HttpPost]
    [ProducesResponseType<Project>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Project>> CreateProject(CreateProject request, CancellationToken cancellationToken)
    {
        var result = await _orchestration.CreateProject(request, cancellationToken);
        return CreatedAtAction(nameof(GetProjectById), new { projectId = result.Id }, result);
    }

    [HttpPut("{projectId:guid}")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProject(Guid projectId, UpdateProject request, CancellationToken cancellationToken)
    {
        await _orchestration.UpdateProject(projectId, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{projectId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProject(Guid projectId, CancellationToken cancellationToken)
    {
        await _service.DeleteProject(projectId, cancellationToken);
        return NoContent();
    }
}
