namespace API.Models;

public class Project
{
    public Guid Id { get; set; }

    public Guid SupervisorId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? ExternalUrl { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.Available;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Supervisor Supervisor { get; set; } = null!;

    public ICollection<Topic> Topics { get; set; } = new List<Topic>();

    public ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();
}
