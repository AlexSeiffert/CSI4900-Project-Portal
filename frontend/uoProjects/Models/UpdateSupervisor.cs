namespace uoProjects.Models;

public sealed record UpdateSupervisor(
    string DisplayName,
    string ContactEmail,
    string? AboutMe,
    string? ExternalUrl,
    bool AcceptingInquiries,
    List<Guid> TopicIds
);
