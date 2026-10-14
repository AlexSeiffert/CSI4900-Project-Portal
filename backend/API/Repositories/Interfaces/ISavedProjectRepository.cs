using API.Models;

namespace API.Repositories.Interfaces;

public interface ISavedProjectRepository
{
    Task<List<Project>> GetSavedProjects(Guid studentId, CancellationToken cancellationToken = default);
    Task SaveProject(Guid studentId, Guid projectId, CancellationToken cancellationToken = default);
    Task RemoveSavedProject(Guid studentId, Guid projectId, CancellationToken cancellationToken = default);
}
