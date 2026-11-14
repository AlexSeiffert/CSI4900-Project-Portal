namespace uoProjects.Models;

public sealed record Supervisor(
    Guid UserId,
    string DisplayName,
    string ContactEmail,
    string? AboutMe,
    string? ExternalUrl,
    bool AcceptingInquiries,
    DateTime UpdatedAt,
    List<Topic>? TopicInterests
);
