namespace API.Models;

public class InquiryAttachment
{
    public Guid Id { get; set; }
    public Guid InquiryId { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public int SizeBytes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Inquiry Inquiry { get; set; } = null!;
}
