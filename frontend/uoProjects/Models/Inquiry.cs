namespace uoProjects.Models;

public sealed record Inquiry(
    Guid Id,
    Guid StudentId,
    Guid SupervisorId,
    Guid? ProjectId,
    string? Message,
    InquiryStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<InquiryMember>? Members = null,
    Supervisor? Supervisor = null,
    Project? Project = null,
    List<InquiryAttachment>? Attachments = null
)
{
    public const int MessageMaxLength = 2000;
    public const int MaxMembers = 3;
}

public sealed record InquiryMember(Guid StudentId, InquiryResponse Response, string? DisplayName = null, string? Email = null);

public sealed record InquiryAttachment(Guid Id, Guid InquiryId, Guid StudentId, AttachmentKind Kind, string StoragePath, string MimeType, int SizeBytes, DateTime CreatedAt);

public sealed record RespondResult(InquiryStatus Status, List<Guid> DroppedInquiryIds);

public sealed record SignedUrl(string Url, string? StoragePath = null);
