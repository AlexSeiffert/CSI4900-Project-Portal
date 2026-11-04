using System.Text.Json.Serialization;

namespace API.DTOs;

public class Project : AuditableDto<Guid>
{
    public Guid SupervisorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ExternalUrl { get; set; }
    public Models.ProjectStatus Status { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Supervisor? Supervisor { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Topic>? Topics { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Inquiry>? Inquiries { get; set; }
}
