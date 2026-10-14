using API.DTOs.Request;

namespace API.Services.Interfaces;

public interface ISupervisorService
{
    Task<IEnumerable<DTOs.Supervisor>> GetSupervisors(CancellationToken cancellationToken = default);
    Task<DTOs.Supervisor> GetSupervisorById(Guid userId, CancellationToken cancellationToken = default);
    Task<DTOs.Supervisor> UpdateSupervisor(Guid userId, UpdateSupervisor request, CancellationToken cancellationToken = default);
    Task ValidateSupervisorExists(Guid supervisorId, CancellationToken cancellationToken = default);
}
