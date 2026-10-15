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
            return await _db.Students.AsNoTracking()
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
            return await StudentWithRelations()
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
            // Returns Forbidden if the database blocks this user from editing this profile.
            await _db.SaveChangesOrForbiddenAsync(nameof(UpdateStudent), cancellationToken);
            await _db.Entry(profile).Collection(x => x.Inquiries).LoadAsync(cancellationToken);
            await _db.Entry(profile).Collection(x => x.SavedProjects).LoadAsync(cancellationToken);
            return profile;
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(UpdateStudent), exception);
        }
    }

    private IQueryable<Student> StudentWithRelations()
    {
        return _db.Students.Include(student => student.TopicInterests)
            .Include(student => student.Inquiries)
            .Include(student => student.SavedProjects)
            .AsSplitQuery();
    }
}
