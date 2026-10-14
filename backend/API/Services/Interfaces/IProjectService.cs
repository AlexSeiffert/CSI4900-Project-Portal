using API.DTOs.Request;

namespace API.Services.Interfaces;

public interface IProjectService
{
    Task<IEnumerable<DTOs.Project>> GetProjects(CancellationToken cancellationToken = default);
    Task<IEnumerable<DTOs.Project>> GetProjectsBySupervisorId(Guid supervisorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DTOs.Project>> GetProjectsByTopicId(Guid topicId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DTOs.Project>> GetProjectsByStatus(Models.ProjectStatus status, CancellationToken cancellationToken = default);
    Task<DTOs.Project> GetProjectById(Guid projectId, CancellationToken cancellationToken = default);
    Task<DTOs.Project> CreateProject(CreateProject request, CancellationToken cancellationToken = default);
    Task<DTOs.Project> UpdateProject(Guid projectId, UpdateProject request, CancellationToken cancellationToken = default);
    Task DeleteProject(Guid projectId, CancellationToken cancellationToken = default);
    Task ValidateProjectSupervisor(Guid? projectId, Guid supervisorId, CancellationToken cancellationToken = default);
}
