namespace uoProjects.Models;

public sealed record UpdateStudent(
    string DisplayName,
    List<Guid> TopicIds,
    string? Program
);
