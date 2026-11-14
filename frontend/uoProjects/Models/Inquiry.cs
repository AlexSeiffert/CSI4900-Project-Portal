namespace uoProjects.Models;

public sealed record Inquiry(
    Guid Id,
    Guid StudentId,
    Guid SupervisorId,
    Guid? ProjectId,
    string? Message,
    InquiryStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
