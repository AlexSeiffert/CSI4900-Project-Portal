namespace API.DTOs;

public class Inquiry : AuditableDto<Guid>
{
    public Guid StudentId { get; set; }
    public Guid SupervisorId { get; set; }
    public Guid? ProjectId { get; set; }
    public string? Message { get; set; }
    public Student? Student { get; set; }
    public Supervisor? Supervisor { get; set; }
    public Project? Project { get; set; }
}
