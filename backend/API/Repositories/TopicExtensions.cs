using API.Exceptions;
using API.Models;
using API.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public static class TopicExtensions
{
    // Replaces a project's tags or a profile's interests with the topics in topicIds.
    // If any id doesn't match a topic (for example it was just deleted), nothing changes and it fails with Conflict (409).
    public static async Task ReplaceTopics(this ApplicationDbContext db, ICollection<Topic> current, IReadOnlyCollection<Guid> topicIds,
        string operation, CancellationToken cancellationToken)
    {
        var topics = await db.Topics.Where(topic => topicIds.Contains(topic.Id)).ToListAsync(cancellationToken);
        if (topics.Count != topicIds.Distinct().Count())
        {
            throw new RepositoryException(operation, new DbUpdateConcurrencyException("The selected topics changed before they could be saved."));
        }
        current.Clear();
        foreach (var topic in topics)
        {
            current.Add(topic);
        }
    }
}
