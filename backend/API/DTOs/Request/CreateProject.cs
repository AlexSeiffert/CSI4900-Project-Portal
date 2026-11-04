namespace API.DTOs.Request;

// Temporary development identity. Replace with the authenticated supervisor at the HTTP boundary.
public class CreateProject : UpdateProject
{
    public Guid SupervisorId { get; set; }
}
