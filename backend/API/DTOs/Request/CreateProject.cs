namespace API.DTOs.Request;

// Same fields as an update. SupervisorId (on UpdateProject) is required here for an admin; for a
// supervisor the controller always sets it to their own id.
public class CreateProject : UpdateProject
{
}
