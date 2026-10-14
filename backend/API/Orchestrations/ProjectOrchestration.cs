using API.DTOs.Request;
using API.Exceptions;
using API.Orchestrations.Interfaces;
using API.Services.Interfaces;

namespace API.Orchestrations;

public class ProjectOrchestration(IProjectService projects, ISupervisorService supervisors, ITopicService topics) : IProjectOrchestration
{
    private readonly IProjectService _projects = projects;
    private readonly ISupervisorService _supervisors = supervisors;
    private readonly ITopicService _topics = topics;

    public async Task<DTOs.Project> CreateProject(CreateProject request, CancellationToken cancellationToken = default)
    {
        try
        {
            await _supervisors.ValidateSupervisorExists(request.SupervisorId, cancellationToken);
            await _topics.ValidateTopicsExist(request.TopicIds, cancellationToken);
            return await _projects.CreateProject(request, cancellationToken);
        }
        catch (ServiceException exception)
        {
            throw new OrchestrationException(nameof(CreateProject), exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new OrchestrationException(nameof(CreateProject), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not OrchestrationException)
        {
            throw new OrchestrationException(nameof(CreateProject), ErrorType.Unexpected, "The operation could not be completed.", exception);
        }
    }

    public async Task<DTOs.Project> UpdateProject(Guid projectId, UpdateProject request, CancellationToken cancellationToken = default)
    {
        try
        {
            await _topics.ValidateTopicsExist(request.TopicIds, cancellationToken);
            return await _projects.UpdateProject(projectId, request, cancellationToken);
        }
        catch (ServiceException exception)
        {
            throw new OrchestrationException(nameof(UpdateProject), exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new OrchestrationException(nameof(UpdateProject), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not OrchestrationException)
        {
            throw new OrchestrationException(nameof(UpdateProject), ErrorType.Unexpected, "The operation could not be completed.", exception);
        }
    }
}
