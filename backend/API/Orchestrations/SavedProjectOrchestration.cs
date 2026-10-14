using API.DTOs.Request;
using API.Exceptions;
using API.Orchestrations.Interfaces;
using API.Services.Interfaces;

namespace API.Orchestrations;

public class SavedProjectOrchestration(ISavedProjectService savedProjects, IStudentService students, IProjectService projects) : ISavedProjectOrchestration
{
    private readonly ISavedProjectService _savedProjects = savedProjects;
    private readonly IStudentService _students = students;
    private readonly IProjectService _projects = projects;

    public async Task<IEnumerable<DTOs.Project>> GetSavedProjects(Guid studentId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _students.GetStudentById(studentId, cancellationToken);
            return await _savedProjects.GetSavedProjects(studentId, cancellationToken);
        }
        catch (ServiceException exception)
        {
            throw new OrchestrationException(nameof(GetSavedProjects), exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new OrchestrationException(nameof(GetSavedProjects), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not OrchestrationException)
        {
            throw new OrchestrationException(nameof(GetSavedProjects), ErrorType.Unexpected, "The operation could not be completed.", exception);
        }
    }

    public async Task SaveProject(Guid studentId, Guid projectId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _students.GetStudentById(studentId, cancellationToken);
            await _projects.GetProjectById(projectId, cancellationToken);
            await _savedProjects.SaveProject(studentId, projectId, cancellationToken);
        }
        catch (ServiceException exception)
        {
            throw new OrchestrationException(nameof(SaveProject), exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new OrchestrationException(nameof(SaveProject), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not OrchestrationException)
        {
            throw new OrchestrationException(nameof(SaveProject), ErrorType.Unexpected, "The operation could not be completed.", exception);
        }
    }

    public async Task RemoveSavedProject(Guid studentId, Guid projectId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _students.GetStudentById(studentId, cancellationToken);
            await _savedProjects.RemoveSavedProject(studentId, projectId, cancellationToken);
        }
        catch (ServiceException exception)
        {
            throw new OrchestrationException(nameof(RemoveSavedProject), exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new OrchestrationException(nameof(RemoveSavedProject), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not OrchestrationException)
        {
            throw new OrchestrationException(nameof(RemoveSavedProject), ErrorType.Unexpected, "The operation could not be completed.", exception);
        }
    }
}
