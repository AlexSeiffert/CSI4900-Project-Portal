using API.Models;

namespace API.Repositories.Interfaces;

public interface ISupervisorRepository
{
    Task<List<Supervisor>> GetSupervisors(CancellationToken cancellationToken = default);
    Task<Supervisor?> GetSupervisorById(Guid userId, CancellationToken cancellationToken = default);
    Task<Supervisor> UpdateSupervisor(Supervisor profile, IReadOnlyCollection<Guid> topicIds, CancellationToken cancellationToken = default);
}
