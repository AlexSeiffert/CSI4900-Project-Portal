namespace uoProjects.Models;

public sealed record UpdateProject(
    string Title,
    string Description,
    string? ExternalUrl,
    ProjectStatus Status,
    List<Guid> TopicIds,
    Guid? SupervisorId = null
);
