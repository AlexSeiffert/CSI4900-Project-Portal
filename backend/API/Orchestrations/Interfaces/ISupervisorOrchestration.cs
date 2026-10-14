using API.DTOs.Request;

namespace API.Orchestrations.Interfaces;

public interface ISupervisorOrchestration
{
    Task<DTOs.Supervisor> UpdateSupervisor(Guid userId, UpdateSupervisor request, CancellationToken cancellationToken = default);
}
