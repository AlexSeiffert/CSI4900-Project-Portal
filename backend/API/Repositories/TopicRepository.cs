using API.Exceptions;
using API.Models;
using API.Models.Data;
using API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class TopicRepository(ApplicationDbContext db) : ITopicRepository
{
    private readonly ApplicationDbContext _db = db;

    public async Task<List<Topic>> GetTopics(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _db.Topics.AsNoTracking().OrderBy(topic => topic.Name).ToListAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(GetTopics), exception);
        }
    }

    public async Task<List<Topic>> GetTopicsByIds(IReadOnlyCollection<Guid> topicIds, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _db.Topics.AsNoTracking().Where(topic => topicIds.Contains(topic.Id)).ToListAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(GetTopicsByIds), exception);
        }
    }
}
