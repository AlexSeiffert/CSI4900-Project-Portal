using API.DTOs.Request;

namespace API.Orchestrations.Interfaces;

public interface ISavedProjectOrchestration
{
    Task<IEnumerable<DTOs.Project>> GetSavedProjects(Guid studentId, CancellationToken cancellationToken = default);
    Task SaveProject(Guid studentId, Guid projectId, CancellationToken cancellationToken = default);
    Task RemoveSavedProject(Guid studentId, Guid projectId, CancellationToken cancellationToken = default);
}
