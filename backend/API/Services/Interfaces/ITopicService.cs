using API.DTOs.Request;

namespace API.Services.Interfaces;

public interface ITopicService
{
    Task<IEnumerable<DTOs.Topic>> GetTopics(CancellationToken cancellationToken = default);
    Task ValidateTopicsExist(IReadOnlyCollection<Guid> topicIds, CancellationToken cancellationToken = default);
}
