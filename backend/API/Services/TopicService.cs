using System.ComponentModel.DataAnnotations;
using API.DTOs.Mapping;
using API.Exceptions;
using API.Repositories.Interfaces;
using API.Services.Interfaces;

namespace API.Services;

public class TopicService(ITopicRepository repository) : ITopicService
{
    private readonly ITopicRepository _repository = repository;

    public async Task<IEnumerable<DTOs.Topic>> GetTopics(CancellationToken cancellationToken = default)
    {
        try
        {
            var topics = await _repository.GetTopics(cancellationToken);
            return topics.Select(topic => topic.ToDto()).ToList();
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(GetTopics), exception.Type, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(GetTopics), ErrorType.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(GetTopics), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(GetTopics), ErrorType.Unexpected, "The operation could not be completed.", exception);
        }
    }
}
