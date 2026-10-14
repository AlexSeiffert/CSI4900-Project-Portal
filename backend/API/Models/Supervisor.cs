namespace API.Models;

public class Supervisor
{
    public Guid UserId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string ContactEmail { get; set; } = string.Empty;

    public string? AboutMe { get; set; }

    public string? ExternalUrl { get; set; }

    public bool AcceptingInquiries { get; set; } = true;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relationships
    public ICollection<Topic> TopicInterests { get; set; } = new List<Topic>();

    public ICollection<Project> Projects { get; set; } = new List<Project>();

    public ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();
}
