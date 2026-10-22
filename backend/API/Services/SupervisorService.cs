using API.DTOs.Mapping;
using API.DTOs.Request;
using API.Exceptions;
using API.Repositories.Interfaces;
using API.Services.Interfaces;

namespace API.Services;

public class SupervisorService(ISupervisorRepository repository) : ISupervisorService
{
    private readonly ISupervisorRepository _repository = repository;

    public async Task<IEnumerable<DTOs.Supervisor>> GetSupervisors(CancellationToken cancellationToken = default)
    {
        var profiles = await _repository.GetSupervisors(cancellationToken);
        return profiles.Select(profile => profile.ToDto()).ToList();
    }

    public async Task<DTOs.Supervisor> GetSupervisorById(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await _repository.GetSupervisorById(userId, cancellationToken)
            ?? throw ServiceException.NotFound(nameof(GetSupervisorById), $"Supervisor '{userId}' was not found.");
        return profile.ToDtoWithRelations();
    }

    public async Task<DTOs.Supervisor> UpdateSupervisor(Guid userId, UpdateSupervisor request, CancellationToken cancellationToken = default)
    {
        var profile = await _repository.GetSupervisorById(userId, cancellationToken)
            ?? throw ServiceException.NotFound(nameof(UpdateSupervisor), $"Supervisor '{userId}' was not found.");
        profile.DisplayName = request.DisplayName;
        profile.ContactEmail = request.ContactEmail;
        profile.AboutMe = request.AboutMe;
        profile.ExternalUrl = request.ExternalUrl;
        profile.AcceptingInquiries = request.AcceptingInquiries;
        return (await _repository.UpdateSupervisor(profile, request.TopicIds, cancellationToken)).ToDtoWithRelations();
    }
}
