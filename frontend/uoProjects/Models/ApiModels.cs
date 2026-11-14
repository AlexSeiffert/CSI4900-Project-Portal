namespace uoProjects.Models;

public enum ProjectStatus { Available, Unavailable, Archived }

public enum InquiryStatus { Sent, Matched, Closed }

public sealed record Topic(Guid Id, string Name);

public sealed record Supervisor(
    Guid UserId, string DisplayName, string ContactEmail, string? AboutMe, string? ExternalUrl,
    bool AcceptingInquiries, DateTime UpdatedAt, List<Topic>? TopicInterests);

public sealed record Student(Guid UserId, string DisplayName, DateTime UpdatedAt, List<Topic>? TopicInterests, string? Program = null);

public sealed record Inquiry(
    Guid Id, Guid StudentId, Guid SupervisorId, Guid? ProjectId, string? Message,
    InquiryStatus Status, DateTime CreatedAt, DateTime UpdatedAt);

public sealed record UpdateProject(
    string Title, string Description, string? ExternalUrl, ProjectStatus Status, List<Guid> TopicIds, Guid? SupervisorId = null);

public sealed record UpdateSupervisor(
    string DisplayName, string ContactEmail, string? AboutMe, string? ExternalUrl, bool AcceptingInquiries, List<Guid> TopicIds);

public sealed record UpdateStudent(string DisplayName, List<Guid> TopicIds, string? Program);
