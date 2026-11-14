namespace uoProjects.Models;

public sealed record Project(
    Guid Id = default,
    Guid SupervisorId = default,
    string Title = "",
    string Description = "",
    string? ExternalUrl = null,
    ProjectStatus Status = ProjectStatus.Available,
    DateTime CreatedAt = default,
    DateTime UpdatedAt = default,
    List<Topic>? Topics = null
);
