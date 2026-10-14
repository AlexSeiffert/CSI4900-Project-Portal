using API.Exceptions;
using API.Models;
using API.Models.Data;
using API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class StudentRepository(ApplicationDbContext db) : IStudentRepository
{
    private readonly ApplicationDbContext _db = db;

    public async Task<List<Student>> GetStudents(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _db.Students.AsNoTracking().Include(profile => profile.TopicInterests)
                .OrderBy(profile => profile.DisplayName).ToListAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(GetStudents), exception);
        }
    }

    public async Task<Student?> GetStudentById(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _db.Students.Include(profile => profile.TopicInterests)
                .SingleOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(GetStudentById), exception);
        }
    }

    public async Task<Student> UpdateStudent(Student profile, IReadOnlyCollection<Guid> topicIds, CancellationToken cancellationToken = default)
    {
        try
        {
            var topics = await _db.Topics.Where(topic => topicIds.Contains(topic.Id)).ToListAsync(cancellationToken);
            if (topics.Count != topicIds.Distinct().Count())
            {
                throw new RepositoryException(nameof(UpdateStudent), new DbUpdateConcurrencyException("The selected topics changed before they could be saved."));
            }
            profile.TopicInterests.Clear();
            foreach (var topic in topics)
            {
                profile.TopicInterests.Add(topic);
            }
            await _db.SaveChangesAsync(cancellationToken);
            return profile;
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(UpdateStudent), exception);
        }
    }
}
