using System.ComponentModel.DataAnnotations;
using API.DTOs.Mapping;
using API.DTOs.Request;
using API.Exceptions;
using API.Repositories.Interfaces;
using API.Services.Interfaces;

namespace API.Services;

public class ProjectService(IProjectRepository repository) : IProjectService
{
    private readonly IProjectRepository _repository = repository;

    public async Task<IEnumerable<DTOs.Project>> GetProjects(CancellationToken cancellationToken = default)
    {
        try
        {
            var projects = await _repository.GetProjects(cancellationToken);
            return projects.Select(project => project.ToDto()).ToList();
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(GetProjects), exception.Kind, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(GetProjects), ErrorType.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(GetProjects), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(GetProjects), ErrorType.Unexpected, "The operation could not be completed.", exception);
        }
    }

    public async Task<IEnumerable<DTOs.Project>> GetProjectsBySupervisorId(Guid supervisorId, CancellationToken cancellationToken = default)
    {
        try
        {
            var projects = await _repository.GetProjectsBySupervisorId(supervisorId, cancellationToken);
            return projects.Select(project => project.ToDto()).ToList();
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(GetProjectsBySupervisorId), exception.Kind, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(GetProjectsBySupervisorId), ErrorType.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(GetProjectsBySupervisorId), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(GetProjectsBySupervisorId), ErrorType.Unexpected, "The operation could not be completed.", exception);
        }
    }

    public async Task<IEnumerable<DTOs.Project>> GetProjectsByTopicId(Guid topicId, CancellationToken cancellationToken = default)
    {
        try
        {
            var projects = await _repository.GetProjectsByTopicId(topicId, cancellationToken);
            return projects.Select(project => project.ToDto()).ToList();
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(GetProjectsByTopicId), exception.Kind, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(GetProjectsByTopicId), ErrorType.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(GetProjectsByTopicId), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(GetProjectsByTopicId), ErrorType.Unexpected, "The operation could not be completed.", exception);
        }
    }

    public async Task<IEnumerable<DTOs.Project>> GetProjectsByStatus(API.Models.ProjectStatus status, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Enum.IsDefined(status))
            {
                var exception = new ValidationException("Project status is invalid.");
                throw new ServiceException(nameof(GetProjectsByStatus), ErrorType.Validation, exception.Message, exception);
            }
            var projects = await _repository.GetProjectsByStatus(status, cancellationToken);
            return projects.Select(project => project.ToDto()).ToList();
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(GetProjectsByStatus), exception.Kind, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(GetProjectsByStatus), ErrorType.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(GetProjectsByStatus), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(GetProjectsByStatus), ErrorType.Unexpected, "The operation could not be completed.", exception);
        }
    }

    public async Task<DTOs.Project> GetProjectById(Guid projectId, CancellationToken cancellationToken = default)
    {
        try
        {
            var project = await _repository.GetProjectById(projectId, cancellationToken);
            if (project == null)
            {
                var exception = new KeyNotFoundException($"Project '{projectId}' was not found.");
                throw new ServiceException(nameof(GetProjectById), ErrorType.NotFound, exception.Message, exception);
            }
            return project.ToDto();
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(GetProjectById), exception.Kind, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(GetProjectById), ErrorType.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(GetProjectById), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(GetProjectById), ErrorType.Unexpected, "The operation could not be completed.", exception);
        }
    }

    public async Task<DTOs.Project> CreateProject(CreateProject request, CancellationToken cancellationToken = default)
    {
        try
        {
            Validator.ValidateObject(request, new ValidationContext(request), true);
            var project = new API.Models.Project
            {
                Id = Guid.NewGuid(),
                SupervisorId = request.SupervisorId,
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                ExternalUrl = request.ExternalUrl,
                Status = request.Status
            };
            return (await _repository.CreateProject(project, request.TopicIds, cancellationToken)).ToDto();
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(CreateProject), exception.Kind, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(CreateProject), ErrorType.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(CreateProject), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(CreateProject), ErrorType.Unexpected, "The operation could not be completed.", exception);
        }
    }

    public async Task<DTOs.Project> UpdateProject(Guid projectId, UpdateProject request, CancellationToken cancellationToken = default)
    {
        try
        {
            Validator.ValidateObject(request, new ValidationContext(request), true);
            var project = await _repository.GetProjectById(projectId, cancellationToken);
            if (project == null)
            {
                var exception = new KeyNotFoundException($"Project '{projectId}' was not found.");
                throw new ServiceException(nameof(UpdateProject), ErrorType.NotFound, exception.Message, exception);
            }
            project.Title = request.Title.Trim();
            project.Description = request.Description.Trim();
            project.ExternalUrl = request.ExternalUrl;
            project.Status = request.Status;
            project.UpdatedAt = DateTime.UtcNow;
            return (await _repository.UpdateProject(project, request.TopicIds, cancellationToken)).ToDto();
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(UpdateProject), exception.Kind, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(UpdateProject), ErrorType.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(UpdateProject), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(UpdateProject), ErrorType.Unexpected, "The operation could not be completed.", exception);
        }
    }

    public async Task DeleteProject(Guid projectId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await _repository.DeleteProject(projectId, cancellationToken))
            {
                var exception = new KeyNotFoundException($"Project '{projectId}' was not found.");
                throw new ServiceException(nameof(DeleteProject), ErrorType.NotFound, exception.Message, exception);
            }
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(DeleteProject), exception.Kind, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(DeleteProject), ErrorType.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(DeleteProject), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(DeleteProject), ErrorType.Unexpected, "The operation could not be completed.", exception);
        }
    }

    public async Task ValidateProjectSupervisor(Guid? projectId, Guid supervisorId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!projectId.HasValue)
            {
                return;
            }
            var project = await _repository.GetProjectById(projectId.Value, cancellationToken);
            if (project == null || project.SupervisorId != supervisorId)
            {
                var exception = new ValidationException("Project does not exist or belongs to another supervisor.");
                throw new ServiceException(nameof(ValidateProjectSupervisor), ErrorType.Validation, exception.Message, exception);
            }
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(ValidateProjectSupervisor), exception.Kind, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(ValidateProjectSupervisor), ErrorType.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(ValidateProjectSupervisor), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(ValidateProjectSupervisor), ErrorType.Unexpected, "The operation could not be completed.", exception);
        }
    }
}
