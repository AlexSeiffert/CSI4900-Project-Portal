namespace API.Models;

public class Inquiry
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public Guid SupervisorId { get; set; }

    public Guid? ProjectId { get; set; }

    public string? Message { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<InquiryAttachment> Attachments { get; set; } = new List<InquiryAttachment>();

    // Relationships
    public Student Student { get; set; } = null!;

    public Supervisor Supervisor { get; set; } = null!;

    public Project? Project { get; set; }
}
