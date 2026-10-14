using API.Exceptions;
using API.Models;
using API.Models.Data;
using API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class ProjectRepository(ApplicationDbContext db) : IProjectRepository
{
    private readonly ApplicationDbContext _db = db;

    public async Task<List<Project>> GetProjects(CancellationToken cancellationToken = default)
    {
        try
        {
            return await ProjectQuery().ToListAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(GetProjects), exception);
        }
    }

    public async Task<List<Project>> GetProjectsBySupervisorId(Guid supervisorId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await ProjectQuery().Where(project => project.SupervisorId == supervisorId).ToListAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(GetProjectsBySupervisorId), exception);
        }
    }

    public async Task<List<Project>> GetProjectsByTopicId(Guid topicId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await ProjectQuery().Where(project => project.Topics.Any(topic => topic.Id == topicId)).ToListAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(GetProjectsByTopicId), exception);
        }
    }

    public async Task<List<Project>> GetProjectsByStatus(API.Models.ProjectStatus status, CancellationToken cancellationToken = default)
    {
        try
        {
            return await ProjectQuery().Where(project => project.Status == status).ToListAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(GetProjectsByStatus), exception);
        }
    }

    public async Task<Project?> GetProjectById(Guid projectId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await ProjectWithRelations().SingleOrDefaultAsync(project => project.Id == projectId, cancellationToken);
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(GetProjectById), exception);
        }
    }

    public async Task<Project> CreateProject(Project project, IReadOnlyCollection<Guid> topicIds, CancellationToken cancellationToken = default)
    {
        try
        {
            await SetTopics(project, topicIds, cancellationToken);
            _db.Projects.Add(project);
            await _db.SaveChangesAsync(cancellationToken);
            await _db.Entry(project).Reference(x => x.Supervisor).LoadAsync(cancellationToken);
            return project;
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(CreateProject), exception);
        }
    }

    public async Task<Project> UpdateProject(Project project, IReadOnlyCollection<Guid> topicIds, CancellationToken cancellationToken = default)
    {
        try
        {
            await SetTopics(project, topicIds, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return project;
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(UpdateProject), exception);
        }
    }

    public async Task<bool> DeleteProject(Guid projectId, CancellationToken cancellationToken = default)
    {
        try
        {
            var project = await _db.Projects.Include(project => project.Topics).Include(project => project.Inquiries)
                .SingleOrDefaultAsync(project => project.Id == projectId, cancellationToken);
            if (project == null)
            {
                return false;
            }
            // Load dependents so InMemory mirrors PostgreSQL's CASCADE and SET NULL behavior.
            await _db.Students.Include(student => student.SavedProjects)
                .Where(student => student.SavedProjects.Any(saved => saved.Id == projectId)).LoadAsync(cancellationToken);
            _db.Projects.Remove(project);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(DeleteProject), exception);
        }
    }

    private IQueryable<Project> ProjectQuery()
    {
        return ProjectWithRelations().AsNoTrackingWithIdentityResolution()
            .OrderByDescending(project => project.CreatedAt).ThenBy(project => project.Id);
    }

    private IQueryable<Project> ProjectWithRelations()
    {
        return _db.Projects.Include(project => project.Supervisor)
            .Include(project => project.Topics)
            .Include(project => project.Inquiries)
            .AsSplitQuery();
    }

    private async Task SetTopics(Project project, IReadOnlyCollection<Guid> topicIds, CancellationToken cancellationToken)
    {
        var topics = await _db.Topics.Where(topic => topicIds.Contains(topic.Id)).ToListAsync(cancellationToken);
        if (topics.Count != topicIds.Distinct().Count())
        {
            throw new RepositoryException(nameof(SetTopics), new DbUpdateConcurrencyException("The selected topics changed before they could be saved."));
        }
        project.Topics.Clear();
        foreach (var topic in topics)
        {
            project.Topics.Add(topic);
        }
    }
}
