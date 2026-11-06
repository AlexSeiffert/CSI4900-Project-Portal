using API.DTOs.Mapping;
using API.Repositories.Interfaces;
using API.Services.Interfaces;

namespace API.Services;

public class SavedProjectService(ISavedProjectRepository repository) : ISavedProjectService
{
    private readonly ISavedProjectRepository _repository = repository;

    public async Task<IEnumerable<DTOs.Project>> GetSavedProjects(Guid studentId, CancellationToken cancellationToken = default)
    {
        var projects = await _repository.GetSavedProjects(studentId, cancellationToken);
        return projects.Select(project => project.ToDto()).ToList();
    }

    public Task<Dictionary<Guid, int>> GetSaveCounts(CancellationToken cancellationToken = default) =>
        _repository.GetSaveCounts(cancellationToken);

    public Task SaveProject(Guid studentId, Guid projectId, CancellationToken cancellationToken = default) =>
        _repository.SaveProject(studentId, projectId, cancellationToken);

    public Task RemoveSavedProject(Guid studentId, Guid projectId, CancellationToken cancellationToken = default) =>
        _repository.RemoveSavedProject(studentId, projectId, cancellationToken);
}
