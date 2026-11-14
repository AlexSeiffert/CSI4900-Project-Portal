namespace uoProjects.Models;

public sealed record Student(
    Guid UserId,
    string DisplayName,
    DateTime UpdatedAt,
    List<Topic>? TopicInterests,
    string? Program = null
);
