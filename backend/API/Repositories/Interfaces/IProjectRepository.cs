using API.Models;

namespace API.Repositories.Interfaces;

public interface IProjectRepository
{
    Task<List<Project>> GetProjects(CancellationToken cancellationToken = default);
    Task<List<Project>> GetProjectsBySupervisorId(Guid supervisorId, CancellationToken cancellationToken = default);
    Task<List<Project>> GetProjectsByTopicId(Guid topicId, CancellationToken cancellationToken = default);
    Task<List<Project>> GetProjectsByStatus(API.Models.ProjectStatus status, CancellationToken cancellationToken = default);
    Task<Project?> GetProjectById(Guid projectId, CancellationToken cancellationToken = default);
    Task<Project> CreateProject(Project project, IReadOnlyCollection<Guid> topicIds, CancellationToken cancellationToken = default);
    Task<Project> UpdateProject(Project project, IReadOnlyCollection<Guid> topicIds, CancellationToken cancellationToken = default);
    Task<bool> DeleteProject(Guid projectId, CancellationToken cancellationToken = default);
}
