namespace API.DTOs;

public class Project : AuditableDto<Guid>
{
    public Guid SupervisorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ExternalUrl { get; set; }
    public Models.ProjectStatus Status { get; set; }
    public Supervisor? Supervisor { get; set; }
    public List<Topic> Topics { get; set; } = [];
    public List<Inquiry> Inquiries { get; set; } = [];
}
