namespace API.DTOs.Request;

public class CreateInquiry
{
    public Guid StudentId { get; set; }
    public Guid SupervisorId { get; set; }
    public Guid? ProjectId { get; set; }
    public string? Message { get; set; }
}
