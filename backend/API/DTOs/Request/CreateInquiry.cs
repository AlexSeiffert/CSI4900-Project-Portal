namespace API.DTOs.Request;

public class CreateInquiry
{
    // The client does not choose this. If a value is sent, it is ignored (JsonIgnore),
    // and the controller sets it to the id of the logged-in student.
    [System.Text.Json.Serialization.JsonIgnore]
    public Guid StudentId { get; set; }
    public Guid SupervisorId { get; set; }
    public Guid? ProjectId { get; set; }
    public string? Message { get; set; }
}
