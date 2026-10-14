using System.Text.Json.Serialization;

namespace API.DTOs;

public class Inquiry : AuditableDto<Guid>
{
    public Guid StudentId { get; set; }
    public Guid SupervisorId { get; set; }
    public Guid? ProjectId { get; set; }
    public string? Message { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Student? Student { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Supervisor? Supervisor { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Project? Project { get; set; }
}
