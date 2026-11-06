namespace ProjectPortal.Web.Api;

// Shapes of the API's JSON. List endpoints leave the nested lists (Topics, TopicInterests) out;
// the single-record endpoints include them.

public enum ProjectStatus { Available, Unavailable, Archived }

public enum InquiryStatus { Pending, Viewed, Responded, Closed }

public sealed record Topic(Guid Id, string Name);

public sealed record Project(
    Guid Id, Guid SupervisorId, string Title, string Description, string? ExternalUrl,
    ProjectStatus Status, DateTime CreatedAt, DateTime UpdatedAt, List<Topic>? Topics);

public sealed record Supervisor(
    Guid UserId, string DisplayName, string ContactEmail, string? AboutMe, string? ExternalUrl,
    bool AcceptingInquiries, DateTime UpdatedAt, List<Topic>? TopicInterests);

public sealed record Student(Guid UserId, string DisplayName, string? Program, DateTime UpdatedAt, List<Topic>? TopicInterests);

public sealed record Inquiry(
    Guid Id, Guid StudentId, Guid SupervisorId, Guid? ProjectId, string? Message,
    InquiryStatus Status, DateTime CreatedAt, DateTime UpdatedAt);

// Request bodies. TopicIds replaces the whole list.

// Also the body for creating a project. SupervisorId is only read from an admin: the owner of a new
// project, or the supervisor to hand an existing one to. A supervisor's own requests ignore it.
public sealed record UpdateProject(
    string Title, string Description, string? ExternalUrl, ProjectStatus Status, List<Guid> TopicIds, Guid? SupervisorId = null);

public sealed record UpdateSupervisor(
    string DisplayName, string ContactEmail, string? AboutMe, string? ExternalUrl, bool AcceptingInquiries, List<Guid> TopicIds);

public sealed record UpdateStudent(string DisplayName, string? Program, List<Guid> TopicIds);
