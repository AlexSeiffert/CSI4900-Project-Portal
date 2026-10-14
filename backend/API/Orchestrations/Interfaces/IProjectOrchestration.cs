using API.DTOs.Request;

namespace API.Orchestrations.Interfaces;

public interface IProjectOrchestration
{
    Task<DTOs.Project> CreateProject(CreateProject request, CancellationToken cancellationToken = default);
    Task<DTOs.Project> UpdateProject(Guid projectId, UpdateProject request, CancellationToken cancellationToken = default);
}
