namespace API.Services.Interfaces;

public interface ITopicService
{
    Task<IEnumerable<DTOs.Topic>> GetTopics(CancellationToken cancellationToken = default);
}
