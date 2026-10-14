using API.DTOs.Request;
using API.Exceptions;
using API.Orchestrations.Interfaces;
using API.Services.Interfaces;

namespace API.Orchestrations;

public class SupervisorOrchestration(ISupervisorService supervisors, ITopicService topics) : ISupervisorOrchestration
{
    private readonly ISupervisorService _supervisors = supervisors;
    private readonly ITopicService _topics = topics;

    public async Task<DTOs.Supervisor> UpdateSupervisor(Guid userId, UpdateSupervisor request, CancellationToken cancellationToken = default)
    {
        try
        {
            await _topics.ValidateTopicsExist(request.TopicIds, cancellationToken);
            return await _supervisors.UpdateSupervisor(userId, request, cancellationToken);
        }
        catch (ServiceException exception)
        {
            throw new OrchestrationException(nameof(UpdateSupervisor), exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new OrchestrationException(nameof(UpdateSupervisor), ErrorKind.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not OrchestrationException)
        {
            throw new OrchestrationException(nameof(UpdateSupervisor), ErrorKind.Unexpected, "The operation could not be completed.", exception);
        }
    }
}
