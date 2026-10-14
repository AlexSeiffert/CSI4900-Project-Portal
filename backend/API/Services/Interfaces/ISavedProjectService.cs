using API.DTOs.Request;

namespace API.Services.Interfaces;

public interface ISavedProjectService
{
    Task<IEnumerable<DTOs.Project>> GetSavedProjects(Guid studentId, CancellationToken cancellationToken = default);
    Task SaveProject(Guid studentId, Guid projectId, CancellationToken cancellationToken = default);
    Task RemoveSavedProject(Guid studentId, Guid projectId, CancellationToken cancellationToken = default);
}
