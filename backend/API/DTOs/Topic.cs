namespace API.DTOs;

public class Topic : BaseDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public List<Project> Projects { get; set; } = [];
    public List<Student> Students { get; set; } = [];
    public List<Supervisor> Supervisors { get; set; } = [];
}
