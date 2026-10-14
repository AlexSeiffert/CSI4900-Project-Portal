using API.Exceptions;
using API.Models;
using API.Models.Data;
using API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace API.Repositories;

public class SavedProjectRepository(ApplicationDbContext db) : ISavedProjectRepository
{
    private readonly ApplicationDbContext _db = db;

    public async Task<List<Project>> GetSavedProjects(Guid studentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var student = await _db.Students.AsNoTrackingWithIdentityResolution()
                .Include(student => student.SavedProjects).ThenInclude(project => project.Supervisor)
                .Include(student => student.SavedProjects).ThenInclude(project => project.Topics)
                .Include(student => student.SavedProjects).ThenInclude(project => project.Inquiries)
                .AsSplitQuery()
                .SingleOrDefaultAsync(student => student.UserId == studentId, cancellationToken);
            return student?.SavedProjects.OrderBy(project => project.Title).ToList() ?? [];
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(GetSavedProjects), exception);
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
            try
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (ArgumentException exception) when (_db.Database.IsInMemory())
            {
                // A concurrent InMemory request may have inserted this exact relationship first.
                _db.ChangeTracker.Entries().Single(entry => ReferenceEquals(entry.Entity, relationship)).State = EntityState.Detached;
                if (!await savedProjects.AnyAsync(saved => EF.Property<Guid>(saved, "student_id") == studentId
                        && EF.Property<Guid>(saved, "project_id") == projectId, cancellationToken))
                {
                    throw new RepositoryException(nameof(SaveProject), exception);
                }
            }
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
