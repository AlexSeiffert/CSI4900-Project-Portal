namespace API.DTOs;

public class InquiryAttachment : BaseDto<Guid>
{
    public Guid InquiryId { get; set; }
    public Guid StudentId { get; set; }
    public Models.AttachmentKind Kind { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public int SizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
}
