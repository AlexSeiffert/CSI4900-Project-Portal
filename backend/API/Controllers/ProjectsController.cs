using Microsoft.AspNetCore.Authorization;
using API.Auth;
using API.DTOs;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using API.DTOs.Request;

using System.ComponentModel.DataAnnotations;
using ProjectStatus = API.Models.ProjectStatus;

namespace API.Controllers;

[ApiController]
[Route("api/v1/projects")]
public class ProjectsController(IProjectService service, ISupervisorService supervisors) : ControllerBase
{
    private readonly IProjectService _service = service;

    [Authorize(Policy = "Admin")]
    [HttpGet("page")]
    public async Task<ActionResult<ProjectPage>> GetPage([FromQuery] ProjectPageQuery query, CancellationToken cancellationToken)
    {
        return await _service.GetPage(query, cancellationToken);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Project>>> GetProjects(CancellationToken cancellationToken, [FromQuery] bool includeTopics = false)
    {
        var projects = includeTopics
            ? await _service.GetProjectsWithTopics(cancellationToken)
            : await _service.GetProjects(cancellationToken);
        return Ok(projects);
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
        if (!User.IsAdmin())
        {
            request.SupervisorId = User.GetRequiredUserId();
        }
        else if (!await ValidateSupervisorAsync(request.SupervisorId, cancellationToken))
        {
            return ValidationProblem(ModelState);
        }
        var result = await _service.CreateProject(request, cancellationToken);
        return CreatedAtAction(nameof(GetProjectById), new { projectId = result.Id }, result);
    }

    [HttpPut("{projectId:guid}")]
    [ProducesResponseType<Project>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Project>> UpdateProject(Guid projectId, UpdateProject request, CancellationToken cancellationToken)
    {
        if (!User.IsAdmin())
        {
            request.SupervisorId = null;
        }
        else if (request.SupervisorId.HasValue && !await ValidateSupervisorAsync(request.SupervisorId, cancellationToken))
        {
            return ValidationProblem(ModelState);
        }

        return await _service.UpdateProject(projectId, request, cancellationToken);
    }

    [HttpDelete("{projectId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProject(Guid projectId, CancellationToken cancellationToken)
    {
        await _service.DeleteProject(projectId, cancellationToken);
        return NoContent();
    }

    private async Task<bool> ValidateSupervisorAsync(Guid? id, CancellationToken cancellationToken)
    {
        if (id is { } supervisorId && supervisorId != Guid.Empty)
        {
            try
            {
                await supervisors.GetSupervisorById(supervisorId, cancellationToken);
                return true;
            }
            catch (API.Exceptions.ServiceException exception) when (exception.Type == API.Exceptions.ErrorType.NotFound)
            {
            }
        }

        ModelState.AddModelError(nameof(API.DTOs.Request.UpdateProject.SupervisorId), "Choose an existing supervisor for the project.");
        return false;
    }
}
