using API.DTOs.Request;
using API.Exceptions;
using API.Models;
using API.Models.Data;
using API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class ProjectRepository(ApplicationDbContext db) : IProjectRepository
{
    private readonly ApplicationDbContext _db = db;

    public async Task<API.DTOs.ProjectPage> GetPage(ProjectPageQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_db.Database.IsRelational())
            {
                return await ReadPage(request, cancellationToken);
            }

            await _db.Database.OpenConnectionAsync(cancellationToken);
            try
            {
                return await ReadPage(request, cancellationToken);
            }
            finally
            {
                await _db.Database.CloseConnectionAsync();
            }
        }
        catch (Exception exception) when (exception is not RepositoryException and not OperationCanceledException)
        {
            throw new RepositoryException(nameof(GetPage), exception);
        }
    }

    private async Task<API.DTOs.ProjectPage> ReadPage(ProjectPageQuery request, CancellationToken cancellationToken)
    {
        var projects = _db.Projects.AsNoTracking();
        var statusCounts = await projects.GroupBy(project => project.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToDictionaryAsync(group => group.Status, group => group.Count, cancellationToken);

        if (request.Status is { } status)
        {
            projects = projects.Where(project => project.Status == status);
        }
        if (request.TopicId is { } topicId)
        {
            projects = projects.Where(project => project.Topics.Any(topic => topic.Id == topicId));
        }
        var search = request.Search?.Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(search))
        {
            projects = projects.Where(project => project.Title.ToLower().Contains(search) ||
                project.Supervisor.DisplayName.ToLower().Contains(search));
        }

        var total = request.TopicId.HasValue || !string.IsNullOrEmpty(search)
            ? await projects.CountAsync(cancellationToken)
            : request.Status is { } selectedStatus
                ? statusCounts.GetValueOrDefault(selectedStatus)
                : statusCounts.Values.Sum();
        var saves = _db.Set<Dictionary<string, object>>("student_saved_projects");
        var rows = projects.Select(project => new
        {
            Project = project,
            Supervisor = project.Supervisor.DisplayName,
            Inquiries = project.Inquiries.Count,
            Saves = saves.Count(saved => EF.Property<Guid>(saved, "project_id") == project.Id)
        });

        var ordered = request.Sort switch
        {
            ProjectSort.Supervisor => Order(rows, row => row.Supervisor.ToLower(), request.Descending),
            ProjectSort.Status => Order(rows, row => row.Project.Status, request.Descending),
            ProjectSort.Inquiries => Order(rows, row => row.Inquiries, request.Descending),
            ProjectSort.Saves => Order(rows, row => row.Saves, request.Descending),
            ProjectSort.Updated => Order(rows, row => row.Project.UpdatedAt, request.Descending),
            _ => Order(rows, row => row.Project.Title.ToLower(), request.Descending)
        };

        var items = await ordered.ThenBy(row => row.Project.Id)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .Select(row => new API.DTOs.ProjectPageRow(new API.DTOs.Project
            {
                Id = row.Project.Id,
                SupervisorId = row.Project.SupervisorId,
                Title = row.Project.Title,
                Description = row.Project.Description,
                ExternalUrl = row.Project.ExternalUrl,
                Status = row.Project.Status,
                CreatedAt = row.Project.CreatedAt,
                UpdatedAt = row.Project.UpdatedAt,
                Topics = row.Project.Topics.Select(topic => new API.DTOs.Topic { Id = topic.Id, Name = topic.Name }).ToList()
            }, row.Supervisor, row.Inquiries, row.Saves))
            .ToListAsync(cancellationToken);

        return new API.DTOs.ProjectPage(items, total, statusCounts);
    }

    private static IOrderedQueryable<T> Order<T, TKey>(IQueryable<T> query,
        System.Linq.Expressions.Expression<Func<T, TKey>> key, bool descending)
    {
        return descending ? query.OrderByDescending(key) : query.OrderBy(key);
    }

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

    public async Task<List<Project>> GetProjectsWithTopics(CancellationToken cancellationToken = default)
    {
        try
        {
            return await ProjectQuery().Include(project => project.Topics).ToListAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not RepositoryException and not OperationCanceledException)
        {
            throw new RepositoryException(nameof(GetProjectsWithTopics), exception);
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
            await _db.ReplaceTopics(project.Topics, topicIds, nameof(CreateProject), cancellationToken);
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
            await _db.ReplaceTopics(project.Topics, topicIds, nameof(UpdateProject), cancellationToken);
            // Returns Forbidden if the database blocks this user from editing the project.
            await _db.SaveChangesOrForbiddenAsync(nameof(UpdateProject), cancellationToken);
            await _db.Entry(project).Reference(x => x.Supervisor).LoadAsync(cancellationToken);
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
            // Returns Forbidden if the database blocks this user from deleting the project.
            await _db.SaveChangesOrForbiddenAsync(nameof(DeleteProject), cancellationToken);
            return true;
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(DeleteProject), exception);
        }
    }

    private IQueryable<Project> ProjectQuery()
    {
        return _db.Projects.AsNoTracking()
            .OrderByDescending(project => project.CreatedAt).ThenBy(project => project.Id);
    }

    private IQueryable<Project> ProjectWithRelations()
    {
        return _db.Projects.Include(project => project.Supervisor)
            .Include(project => project.Topics)
            .Include(project => project.Inquiries)
            .AsSplitQuery();
    }
}
