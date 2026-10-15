namespace API.DTOs.Request;

public class CreateProject : UpdateProject
{
    // The client does not choose this. If a value is sent, it is ignored (JsonIgnore),
    // and the controller sets it to the id of the logged-in supervisor.
    [System.Text.Json.Serialization.JsonIgnore]
    public Guid SupervisorId { get; set; }
}
