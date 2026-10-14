using System.ComponentModel.DataAnnotations;
using API.DTOs.Mapping;
using API.DTOs.Request;
using API.Exceptions;
using API.Repositories.Interfaces;
using API.Services.Interfaces;

namespace API.Services;

public class SavedProjectService(ISavedProjectRepository repository) : ISavedProjectService
{
    private readonly ISavedProjectRepository _repository = repository;

    public async Task<IEnumerable<DTOs.Project>> GetSavedProjects(Guid studentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var projects = await _repository.GetSavedProjects(studentId, cancellationToken);
            return projects.Select(project => project.ToDto()).ToList();
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(GetSavedProjects), exception.Kind, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(GetSavedProjects), ErrorType.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(GetSavedProjects), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(GetSavedProjects), ErrorType.Unexpected, "The operation could not be completed.", exception);
        }
    }

    public async Task SaveProject(Guid studentId, Guid projectId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _repository.SaveProject(studentId, projectId, cancellationToken);
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(SaveProject), exception.Kind, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(SaveProject), ErrorType.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(SaveProject), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(SaveProject), ErrorType.Unexpected, "The operation could not be completed.", exception);
        }
    }

    public async Task RemoveSavedProject(Guid studentId, Guid projectId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _repository.RemoveSavedProject(studentId, projectId, cancellationToken);
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(RemoveSavedProject), exception.Kind, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(RemoveSavedProject), ErrorType.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(RemoveSavedProject), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(RemoveSavedProject), ErrorType.Unexpected, "The operation could not be completed.", exception);
        }
    }
}
