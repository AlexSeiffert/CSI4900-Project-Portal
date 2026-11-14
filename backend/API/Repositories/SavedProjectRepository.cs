using API.Exceptions;
using API.Models;
using API.Models.Data;
using API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class SavedProjectRepository(ApplicationDbContext db) : ISavedProjectRepository
{
    private readonly ApplicationDbContext _db = db;

    public async Task<List<Project>> GetSavedProjects(Guid studentId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _db.Students.AsNoTracking()
                .Where(student => student.UserId == studentId)
                .SelectMany(student => student.SavedProjects)
                .OrderBy(project => project.Title)
                .ToListAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(GetSavedProjects), exception);
        }
    }

    public async Task<Dictionary<Guid, int>> GetSaveCounts(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _db.Set<Dictionary<string, object>>("student_saved_projects").AsNoTracking()
                .GroupBy(saved => EF.Property<Guid>(saved, "project_id"))
                .Select(group => new { ProjectId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(group => group.ProjectId, group => group.Count, cancellationToken);
        }
        catch (Exception exception) when (exception is not RepositoryException and not OperationCanceledException)
        {
            throw new RepositoryException(nameof(GetSaveCounts), exception);
        }
    }

    public async Task SaveProject(Guid studentId, Guid projectId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_db.Database.IsNpgsql())
            {
                await _db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO public.student_saved_projects (student_id, project_id) VALUES ({studentId}, {projectId}) ON CONFLICT (student_id, project_id) DO NOTHING", cancellationToken);
                return;
            }
            // Saving twice is fine: Postgres uses ON CONFLICT above, and the fake database checks first.
            var savedProjects = _db.Set<Dictionary<string, object>>("student_saved_projects");
            if (await savedProjects.AnyAsync(saved => EF.Property<Guid>(saved, "student_id") == studentId
                    && EF.Property<Guid>(saved, "project_id") == projectId, cancellationToken))
            {
                return;
            }
            var relationship = new Dictionary<string, object>
            {
                ["student_id"] = studentId,
                ["project_id"] = projectId,
                ["saved_at"] = DateTime.UtcNow
            };
            savedProjects.Add(relationship);
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(SaveProject), exception);
        }
    }

    public async Task RemoveSavedProject(Guid studentId, Guid projectId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_db.Database.IsNpgsql())
            {
                await _db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM public.student_saved_projects WHERE student_id = {studentId} AND project_id = {projectId}", cancellationToken);
                return;
            }
            var savedProjects = _db.Set<Dictionary<string, object>>("student_saved_projects");
            var relationship = await savedProjects.SingleOrDefaultAsync(saved => EF.Property<Guid>(saved, "student_id") == studentId
                && EF.Property<Guid>(saved, "project_id") == projectId, cancellationToken);
            if (relationship != null)
            {
                savedProjects.Remove(relationship);
                await _db.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(RemoveSavedProject), exception);
        }
    }
}
