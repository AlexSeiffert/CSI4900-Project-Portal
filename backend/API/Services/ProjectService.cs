using API.DTOs.Mapping;
using API.DTOs.Request;
using API.Exceptions;
using API.Repositories.Interfaces;
using API.Services.Interfaces;

namespace API.Services;

// Repository errors are not caught here: ApiExceptionHandler turns RepositoryException into the right status.
// Request bodies are validated by [ApiController] before they reach a service.
public class ProjectService(IProjectRepository repository) : IProjectService
{
    private readonly IProjectRepository _repository = repository;

    public async Task<IEnumerable<DTOs.Project>> GetProjects(CancellationToken cancellationToken = default)
    {
        var projects = await _repository.GetProjects(cancellationToken);
        return projects.Select(project => project.ToDto()).ToList();
    }

    public async Task<IEnumerable<DTOs.Project>> GetProjectsBySupervisorId(Guid supervisorId, CancellationToken cancellationToken = default)
    {
        var projects = await _repository.GetProjectsBySupervisorId(supervisorId, cancellationToken);
        return projects.Select(project => project.ToDto()).ToList();
    }

    public async Task<IEnumerable<DTOs.Project>> GetProjectsByTopicId(Guid topicId, CancellationToken cancellationToken = default)
    {
        var projects = await _repository.GetProjectsByTopicId(topicId, cancellationToken);
        return projects.Select(project => project.ToDto()).ToList();
    }

    public async Task<IEnumerable<DTOs.Project>> GetProjectsByStatus(API.Models.ProjectStatus status, CancellationToken cancellationToken = default)
    {
        var projects = await _repository.GetProjectsByStatus(status, cancellationToken);
        return projects.Select(project => project.ToDto()).ToList();
    }

    public async Task<DTOs.Project> GetProjectById(Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await _repository.GetProjectById(projectId, cancellationToken)
            ?? throw ServiceException.NotFound(nameof(GetProjectById), $"Project '{projectId}' was not found.");
        return project.ToDtoWithRelations();
    }

    public async Task<DTOs.Project> CreateProject(CreateProject request, CancellationToken cancellationToken = default)
    {
        var project = new API.Models.Project
        {
            Id = Guid.NewGuid(),
            SupervisorId = request.SupervisorId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            ExternalUrl = request.ExternalUrl,
            Status = request.Status
        };
        return (await _repository.CreateProject(project, request.TopicIds, cancellationToken)).ToDtoWithRelations();
    }

    public async Task<DTOs.Project> UpdateProject(Guid projectId, UpdateProject request, CancellationToken cancellationToken = default)
    {
        var project = await _repository.GetProjectById(projectId, cancellationToken)
            ?? throw ServiceException.NotFound(nameof(UpdateProject), $"Project '{projectId}' was not found.");
        project.Title = request.Title.Trim();
        project.Description = request.Description.Trim();
        project.ExternalUrl = request.ExternalUrl;
        project.Status = request.Status;
        return (await _repository.UpdateProject(project, request.TopicIds, cancellationToken)).ToDtoWithRelations();
    }

    public async Task DeleteProject(Guid projectId, CancellationToken cancellationToken = default)
    {
        if (!await _repository.DeleteProject(projectId, cancellationToken))
        {
            throw ServiceException.NotFound(nameof(DeleteProject), $"Project '{projectId}' was not found.");
        }
    }
}
