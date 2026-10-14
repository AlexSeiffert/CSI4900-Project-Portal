using API.DTOs.Request;
using API.Exceptions;
using API.Orchestrations.Interfaces;
using API.Services.Interfaces;

namespace API.Orchestrations;

public class StudentOrchestration(IStudentService students, ITopicService topics) : IStudentOrchestration
{
    private readonly IStudentService _students = students;
    private readonly ITopicService _topics = topics;

    public async Task<DTOs.Student> UpdateStudent(Guid userId, UpdateStudent request, CancellationToken cancellationToken = default)
    {
        try
        {
            await _topics.ValidateTopicsExist(request.TopicIds, cancellationToken);
            return await _students.UpdateStudent(userId, request, cancellationToken);
        }
        catch (ServiceException exception)
        {
            throw new OrchestrationException(nameof(UpdateStudent), exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new OrchestrationException(nameof(UpdateStudent), ErrorKind.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not OrchestrationException)
        {
            throw new OrchestrationException(nameof(UpdateStudent), ErrorKind.Unexpected, "The operation could not be completed.", exception);
        }
    }
}
