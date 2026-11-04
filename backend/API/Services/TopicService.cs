using API.DTOs.Mapping;
using API.Repositories.Interfaces;
using API.Services.Interfaces;

namespace API.Services;

public class TopicService(ITopicRepository repository) : ITopicService
{
    private readonly ITopicRepository _repository = repository;

    public async Task<IEnumerable<DTOs.Topic>> GetTopics(CancellationToken cancellationToken = default)
    {
        var topics = await _repository.GetTopics(cancellationToken);
        return topics.Select(topic => topic.ToDto()).ToList();
    }
}
