using API.Models;

namespace API.Repositories.Interfaces;

public interface ITopicRepository
{
    Task<List<Topic>> GetTopics(CancellationToken cancellationToken = default);
}
