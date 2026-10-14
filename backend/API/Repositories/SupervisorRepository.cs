using API.Exceptions;
using API.Models;
using API.Models.Data;
using API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class SupervisorRepository(ApplicationDbContext db) : ISupervisorRepository
{
    private readonly ApplicationDbContext _db = db;

    public async Task<List<Supervisor>> GetSupervisors(CancellationToken cancellationToken = default)
    {
        try
        {
            return await SupervisorWithRelations().AsNoTrackingWithIdentityResolution()
                .OrderBy(profile => profile.DisplayName).ToListAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(GetSupervisors), exception);
        }
    }

    public async Task<Supervisor?> GetSupervisorById(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await SupervisorWithRelations()
                .SingleOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(GetSupervisorById), exception);
        }
    }

    public async Task<Supervisor> UpdateSupervisor(Supervisor profile, IReadOnlyCollection<Guid> topicIds, CancellationToken cancellationToken = default)
    {
        try
        {
            var topics = await _db.Topics.Where(topic => topicIds.Contains(topic.Id)).ToListAsync(cancellationToken);
            if (topics.Count != topicIds.Distinct().Count())
            {
                throw new RepositoryException(nameof(UpdateSupervisor), new DbUpdateConcurrencyException("The selected topics changed before they could be saved."));
            }
            profile.TopicInterests.Clear();
            foreach (var topic in topics)
            {
                profile.TopicInterests.Add(topic);
            }
            await _db.SaveChangesAsync(cancellationToken);
            await _db.Entry(profile).Collection(x => x.Projects).LoadAsync(cancellationToken);
            await _db.Entry(profile).Collection(x => x.Inquiries).LoadAsync(cancellationToken);
            return profile;
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(UpdateSupervisor), exception);
        }
    }

    private IQueryable<Supervisor> SupervisorWithRelations()
    {
        return _db.Supervisors.Include(supervisor => supervisor.TopicInterests)
            .Include(supervisor => supervisor.Projects)
            .Include(supervisor => supervisor.Inquiries)
            .AsSplitQuery();
    }
}
